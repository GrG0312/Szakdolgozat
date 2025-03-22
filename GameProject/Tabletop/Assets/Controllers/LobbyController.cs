using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Controllers.DataObjects;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using Model.Lobby;
using Controllers.Data;
using System.Linq;
using System;
using Model;

namespace Controllers
{
    public class LobbyController : BaseController<LobbyController>
    {
        private const int LOBBY_SIZE = 4;

        #region Serializations

        [SerializeField] private GameObject networkManagerPrefab;
        [SerializeField] private LobbyPlayerObject playerObjectPrefab;

        [SerializeField] private GameObject playerNamesParent;
        [SerializeField] private GameObject clientViewBlocker;
        [SerializeField] private TMP_Text hostNameDisplay;

        #endregion

        private List<LobbyPlayerObject> clientSlots;

        private LobbyModel<ulong> lobbyModel;

        #region Lobby setup - Create, Join, Leave
        public void CreateSession()
        {
            // The port 35420 is, by default, unassigned
            // By setting the listening port to 0.0.0.0, it will listen to every IP
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 35420, "0.0.0.0");

            if (!NetworkManager.Singleton.StartHost())
            {
                Debug.LogError("<color=red>Could not start a host. Navigating back to Main Menu</color>");
                SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
            }

            
            clientSlots = new ();

            for (int i = 0; i < LOBBY_SIZE; i++)
            {
                // 150 is the height
                LobbyPlayerObject slotobj = Instantiate(playerObjectPrefab);
                slotobj.NetworkObject.Spawn(true);
                slotobj.transform.SetParent(playerNamesParent.transform);
                // fhu te
                // de nem szeretlek
                // sokkal tovább tartott mint kellett volna -.-
                slotobj.transform.localPosition = new Vector3(0, 225 - (i * 150), 0);
                slotobj.transform.localScale = Vector3.one;
                clientSlots.Add(slotobj);
                slotobj.SetupInitialData(i, i % 2 == 0 ? Side.Blue : Side.Red);
            }

            lobbyModel = new();

            foreach (LobbyPlayerObject obj in clientSlots)
            {
                // Add slot to model
                lobbyModel.LobbySlots.Add(obj.SlotModel);
                // Assign event handler to data changed
                //obj.SlotModel.SlotDataChanged += UpdateSlotDataForConnectedClients;
            }

            // Assign Host data to slot
            LobbyPlayerData<ulong> hostData = new LobbyPlayerData<ulong>(NetworkManager.Singleton.LocalClientId, ProfileController.Instance.DisplayName);
            lobbyModel.ConnectedClients.Add(hostData);
            clientSlots.First().GetComponent<NetworkObject>().ChangeOwnership(NetworkManager.Singleton.LocalClientId);
            clientSlots.First().SlotModel.PlayerData = hostData;

            // Bind the connected handler only to the server
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // Assign an approval callback
            NetworkManager.Singleton.ConnectionApprovalCallback += ClientApproval;
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
            // TODO clear slot of client
        }

        private void OnClientStopped(bool wasHost)
        {
            SetClientViewBlocker(false);
            DestroySession();
        }
        #endregion

        #region For Connection
        private void ClientApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (lobbyModel.ReserveEmptySlot())
            {
                response.CreatePlayerObject = false;
                response.Approved = true;
                Debug.Log("<color=green>Found empty slot. Approving connection.</color>");
            }
            else
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
            }
            else
            {
                NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
            }
            clientViewBlocker.SetActive(assign);
        }

        private void AssignPlayerToSlot(int slotid, ulong clientId, string clientName)
        {
            LobbyPlayerObject obj = clientSlots[slotid];
            LobbyPlayerData<ulong> newPlayerData = new (clientId, clientName);
            lobbyModel.ConnectedClients.Add(newPlayerData);
            obj.SlotModel.PlayerData = newPlayerData;
        }

        #region RPCs for connection

        /// <summary>
        /// Gets the Hoster's name from the server, then sends the LocalClientId and local client's clientName pair.
        /// </summary>
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
            // This happens server-side:
            int id = lobbyModel.FindReservedSlot();
            if (id != -1) // which it shouldnt be here, under no circumstances
            {
                LobbyPlayerObject reserved = clientSlots[id];
                LobbyPlayerData<ulong> newlyJoinedData = new LobbyPlayerData<ulong>(clientId, clientName);

                lobbyModel.ConnectedClients.Add(newlyJoinedData);
                reserved.NetworkObject.ChangeOwnership(clientId);
                reserved.SlotModel.PlayerData = newlyJoinedData;
            }
            // Update every slot's data for the connected client
            for (int i = 0; i < clientSlots.Count; i++)
            {
                id = clientSlots[i].SlotId;
                string name = clientSlots[i].SlotModel.GetPlayerName();
                //ulong ownerId = clientSlots[i].SlotModel.GetPlayerId();
                string slotstatus = clientSlots[i].SlotModel.OccupantStatus.ToString();
                //bool isReady = clientSlots[i].SlotModel.IsReady;
                //UpdateSlotData_ClientsRpc(id, name, slotstatus, isReady);
            }
        }
        #endregion

        #endregion

        #endregion

        #region Switching Slots
        public void SwitchToSlot(int callerSlotId)
        {
            ulong callerClientId = NetworkManager.Singleton.LocalClientId;
            SwitchPlayerSlot_ServerRpc(callerClientId, callerSlotId);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SwitchPlayerSlot_ServerRpc(ulong callerClientId, int callerSlotId)
        {
            LobbyPlayerObject targetSlot = clientSlots[callerSlotId];
            LobbyPlayerData<ulong> playerData = lobbyModel.ConnectedClients.Single(player => player.ID == callerClientId);
            LobbyPlayerObject currentSlot = clientSlots.Single(obj => obj.SlotModel.PlayerData == playerData);

            currentSlot.GetComponent<NetworkObject>().RemoveOwnership();
            targetSlot.GetComponent<NetworkObject>().ChangeOwnership(callerClientId);

            currentSlot.SlotModel.PlayerData = null;
            targetSlot.SlotModel.PlayerData = playerData;

        }

        #endregion

        #region Ready function for Clients
        public void ClientReadyChange(int readyValue)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            ClientReady_ServerRpc(clientId, readyValue);
        }
        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void ClientReady_ServerRpc(ulong clientId, int readyValue)
        {
            LobbyPlayerObject obj = 
                clientSlots
                .Where(slot => slot.SlotModel.PlayerData != null)
                .Single(slot => slot.SlotModel.PlayerData.ID == clientId);
            obj.OnReadinessChangeInput(readyValue);
        }
        #endregion

        #region Unity Messages
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
        #endregion
    }
}
