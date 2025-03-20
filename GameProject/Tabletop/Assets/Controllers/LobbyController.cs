using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Controllers.DataObjects;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Text;
using Model.Lobby;
using Controllers.Data;
using System.Linq;

namespace Controllers
{
    public class LobbyController : BaseController<LobbyController>
    {
        [SerializeField] private GameObject networkManagerPrefab;

        [SerializeField] private GameObject clientViewBlocker;
        [SerializeField] private TMP_Text hostNameDisplay;
        [SerializeField] private List<LobbyPlayerObject> clientSlots;

        private LobbyModel<ulong> lobbyModel;

        public void CreateSession()
        {
            lobbyModel = new ();
            // The port 35420 is, by default, unassigned
            // By setting the listening port to 0.0.0.0, it will listen to every IP
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 35420, "0.0.0.0");

            // Bind the connected handler only to the server
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // Assign an approval callback
            NetworkManager.Singleton.ConnectionApprovalCallback += ClientApproval;
            
            if (!NetworkManager.Singleton.StartHost())
            {
                Debug.LogError("<color=red>Could not start a host. Navigating back to Main Menu</color>");
                SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
            }

        }
        public void JoinSession(string ipAddress)
        {
            Debug.Log($"<color=magenta>Connecting to address {ipAddress}...</color>");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, 35420);
            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("<color=red>Could not start a client. Navigating back to Main Menu</color>");
                SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
            }
            SetClientViewBlocker(true);
        }
        public void DestroySession()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.ConnectionApprovalCallback -= ClientApproval;
            }

            NetworkManager.Singleton.Shutdown(true);
            SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
        }

        #region Event Handlers
        private void OnClientConnected(ulong clientId)
        {
            // Send hostname
            string hosterName = ProfileController.Instance.DisplayName;
            GetHosterName_ClientRpc(hosterName, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        private void OnClientDisconnect(ulong clientId)
        {

        }

        private void OnClientStopped(bool wasHost)
        {
            SetClientViewBlocker(false);
            DestroySession();
        }
        #endregion


        #region RPC

        [Rpc(SendTo.SpecifiedInParams)]
        private void GetHosterName_ClientRpc(string hostname, RpcParams rpcParams)
        {
            // This all happens client-side:
            // Turn off blocker screen
            SetClientViewBlocker(false);
            hostNameDisplay.text = hostname + "'s Lobby";
            // Send client name to server
            string clientName = ProfileController.Instance.DisplayName;
            SendClientName_ServerRpc(NetworkManager.Singleton.LocalClientId, clientName);
        }


        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SendClientName_ServerRpc(ulong clientId, string clientName)
        {
            // This all happens server-side:
            LobbyPlayerObject? reserved = FindReservedSlot();
            if (reserved != null)
            {
                lobbyModel.ConnectedClients.Add(new LobbyPlayerData<ulong>(clientId, clientName, reserved.SlotModel));
                reserved.SlotModel.ChangeDisplayedData(clientName, SlotOccupantStatus.OccupiedModel);
                Debug.Log($"<color=green>[Server] Added a new client: {clientId} | {clientName} on position {reserved.SlotId}</color>");
                PrintClientNames("[Server] Client names");
            }
            foreach (LobbyPlayerObject obj in clientSlots)
            {
                Debug.Log($"<color=orange>[Server] Sending slot update {obj.SlotId}: \nName: {clientName} \nStatus: {obj.SlotModel.OccupantStatus.ToString()}</color>");
                int id = obj.SlotId;
                string name = obj.SlotModel.DisplayName;
                string slotstatus = obj.SlotModel.OccupantStatus.ToString();
                UpdateSlotData_Rpc(id, name, slotstatus);
            }
        }

        [Rpc(SendTo.NotServer)]
        private void UpdateSlotData_Rpc(int slotId, string clientName, string slotstatus)
        {
            LobbyPlayerObject obj = clientSlots.Single(slot => slot.SlotId == slotId);
            Debug.Log($"<color=magenta>[Client] Updating slot {slotId}: \nName: {clientName} \nStatus: {slotstatus}</color>");
            obj.SlotModel.ChangeDisplayedData(name, SlotOccupantStatus.ConvertFromString(slotstatus));
        }
        #endregion

        #region Other methods
        private LobbyPlayerObject? FindReservedSlot()
        {
            foreach (LobbyPlayerObject slot in clientSlots)
            {
                if (slot.SlotModel.OccupantStatus == SlotOccupantStatus.ReservedModel)
                {
                    return slot;
                }
            }
            return null;
        }
        private bool IsThereEmptySlot()
        {
            foreach (LobbyPlayerObject slot in clientSlots)
            {
                if (slot.SlotModel.OccupantStatus == SlotOccupantStatus.OpenModel)
                {
                    slot.SlotModel.ChangeDisplayedData("", SlotOccupantStatus.ReservedModel);
                    return true;
                }
            }
            return false;
        }
        private void ClientApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            Debug.Log("<color=yellow>Searching for empty slot...</color>");
            if (IsThereEmptySlot())
            {
                response.CreatePlayerObject = false;
                response.Approved = true;
                Debug.Log("<color=green>Found empty slot. Approving connection.</color>");
            } else
            {
                response.Reason = "There are no open slots left in the lobby";
                response.Approved = false;
                Debug.Log("<color=red>Couldn't find empty spot. Connection refused.</color>");
            }
        }
        private void SetClientViewBlocker(bool assign)
        {
            if (assign)
            {
                NetworkManager.Singleton.OnClientStopped += OnClientStopped;
            } else
            {
                NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
            }
            clientViewBlocker.SetActive(assign);
        }
        #endregion

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"Multiple {nameof(LobbyController)} instances found. Deleting duplicate...");
                Destroy(Instance.gameObject);
            }
            else
            {
                Instance = this;
            }

            if (NetworkManager.Singleton == null)
            {
                Debug.LogWarning("<color=yellow>NetworkManager object's instance is null!</color>");
                Instantiate(networkManagerPrefab);
                Debug.Log("<color=green>NetworkManager instantiated.</color>");
            }
        }
        private void Start()
        {
            if (InterSceneData.ShouldHost)
            {
                CreateSession();
            } else
            {
                JoinSession(InterSceneData.ConnectionAddress);
            }
            InterSceneData.Reset();
        }

        // Temporary
        private void PrintClientNames(string header = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{header}:");
            sb.Append("[ ");
            foreach (var item in lobbyModel.ConnectedClients)
            {
                sb.Append($"({item.ID} : {item.Name}) ");
            }
            sb.Append("]");
            Debug.Log(sb.ToString());
        }
    }
}
