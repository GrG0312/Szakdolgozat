using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Controllers.DataObjects;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Text;
using Model.Lobby;
using System.Linq;
using Controllers.Data;

namespace Controllers
{
    public class LobbyController : BaseController<LobbyController>
    {
        [SerializeField] private GameObject networkManagerPrefab;

        [SerializeField] private GameObject clientViewBlocker;
        [SerializeField] private TMP_Text hostNameDisplay;
        [SerializeField] private Button exitButton;
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
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
        }

        #region Event Handlers
        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"[ClientConnected] A client has successfully connected: {clientId}");

            // Send hostname
            string hosterName = ProfileController.Instance.DisplayName;
            Debug.Log($"Sending hoster name: {hosterName}");
            GetHosterName_ClientRpc(hosterName, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        private void OnClientDisconnect(ulong clientId)
        {

        }

        private void OnClientStopped(bool wasHost)
        {
            SetClientViewBlocker(false);
            Debug.LogError("<color=red>Could not connect to the IP. Navigating back to Main Menu</color>");
            SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
        }
        #endregion


        #region RPC

        [Rpc(SendTo.SpecifiedInParams)]
        private void GetHosterName_ClientRpc(string hostname, RpcParams rpcParams)
        {
            // This all happens client-side:
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
            LobbyPlayerObject? obj = FindEmptySlot();
            if (obj != null)
            {
                lobbyModel.ConnectedClients.Add(new LobbyPlayerData<ulong>(clientId, clientName, obj.SlotModel));
                Debug.Log($"[Server] Added a new client: {clientId} | {clientName} on position {obj.SlotId}");
                PrintClientNames("[Server] Client names");
            }

        }
        #endregion

        #region Other methods

        private LobbyPlayerObject? FindEmptySlot()
        {
            foreach (LobbyPlayerObject slot in clientSlots)
            {
                if (slot.SlotModel.IsOpen)
                {
                    return slot;
                }
            }
            return null;
        }

        private void ClientApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (FindEmptySlot() != null)
            {
                response.CreatePlayerObject = false;
                response.Approved = true;
            } else
            {
                response.Reason = "There are no open slots left in the lobby";
                response.Approved = false;
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
            if (Instance is null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning($"Multiple {nameof(GameController)} instances found. Deleting duplicate...");
                Destroy(this.gameObject);
            }

            if (NetworkManager.Singleton is null)
            {
                Debug.LogWarning("<color=yellow>NetworkManager object's instance is null!</color>");
                Instantiate(networkManagerPrefab);
                Debug.Log("<color=green>NetworkManager instantiated.</color>");
            }
        }
        private void Start()
        {
            clientSlots = new List<LobbyPlayerObject>();
            if (InterSceneData.ShouldHost)
            {
                CreateSession();
            } else
            {
                JoinSession(InterSceneData.ConnectionAddress);
            }
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
