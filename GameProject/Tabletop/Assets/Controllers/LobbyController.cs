using Controllers.Data;
using Controllers.Objects.Lobby;
using Model;
using Model.Lobby;
using Model.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Controllers
{
    public class LobbyController : MenuControllerBase<LobbyController>
    {
        #region Constant values
        private const float TEXT_FADE_RATE = 1.0f;
        private const string DECK_TEXT_BASE = "Your side: ";
        #endregion

        #region Serializations
        [SerializeField] private GameObject networkManagerPrefab;
        [SerializeField] private LobbySlotController playerSlotPrefab;

        [SerializeField] private GameObject playerSlotsParent;
        [SerializeField] private GameObject clientViewBlocker;
        [SerializeField] private TMP_Text hostNameDisplay;
        [SerializeField] private TMP_Text messageBox;

        [SerializeField] private GameObject deckParent;
        [SerializeField] private TMP_Text deckSideText;
        [SerializeField] private UnitAdder adderObject;
        #endregion

        #region Fields

        private List<LobbySlotController> clientSlots;
        private LobbyModel<ulong> lobbyModel;
        private List<UnitAdder> unitAdders;

        private Coroutine currentErrorMessage;

        #endregion

        #region Lobby setup - Create, Join, Leave

        /// <summary>
        /// Creates a network for the upcoming game, creates the model used by the lobby and assigns event handlers
        /// </summary>
        public void CreateSession()
        {
            // The port 35420 is, by default, unassigned
            // By setting the listening port to 0.0.0.0, it will listen to every IP
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 35420, "0.0.0.0");

            if (!NetworkManager.Singleton.StartHost())
            {
                SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
            }

            
            clientSlots = new ();
            hostNameDisplay.text = ProfileController.Instance.DisplayName + "'s Lobby";

            for (int i = 0; i < LobbyModel<ulong>.LOBBY_SIZE; i++)
            {
                // 150 is the height
                LobbySlotController slotobj = Instantiate(playerSlotPrefab);
                slotobj.NetworkObject.Spawn(true);
                slotobj.transform.SetParent(playerSlotsParent.transform);
                clientSlots.Add(slotobj);
                slotobj.SetupInitialData(i, i % 2 == 0 ? Side.Imperium : Side.Chaos);
            }

            lobbyModel = new();

            foreach (LobbySlotController obj in clientSlots)
            {
                // Add slot to model
                lobbyModel.LobbySlots.Add(obj.SlotModel);
            }

            // Assign Host data to slot
            // This adds the Player to the model and assigns it to the first slot
            lobbyModel.ReserveEmptySlot();
            lobbyModel.AddNewPlayer(NetworkManager.Singleton.LocalClientId, ProfileController.Instance.DisplayName);
            // Change ownership of the slot object, just in case. It can be source for problems
            clientSlots.First().GetComponent<NetworkObject>().ChangeOwnership(NetworkManager.Singleton.LocalClientId);
            SideDataDelivery_ClientRpc(clientSlots.First().SlotModel.Side, RpcTarget.Single(NetworkManager.Singleton.LocalClientId, RpcTargetUse.Temp));

            // Bind the connected and disconnected handler only to the server
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // Assign an approval callback
            NetworkManager.Singleton.ConnectionApprovalCallback += ClientApproval;
        }

        /// <summary>
        /// Joins into an already existing network. If it is not possible, redirects to the main menu.
        /// </summary>
        /// <param name="ipAddress">The address to join to</param>
        public void JoinSession(string ipAddress)
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, 35420);
            if (!NetworkManager.Singleton.StartClient())
            {
                SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
            }
            SetClientViewBlocker(true);
        }

        /// <summary>
        /// Starts the leaving process of a client, or destroys the network if host calls it
        /// </summary>
        public void StartLeaveProcess()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.ConnectionApprovalCallback -= ClientApproval;
            }
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                ClientDisconnect_ServerRpc(NetworkManager.Singleton.LocalClientId);
            } else
            {
                NetworkManager.Singleton.Shutdown(true);
                SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
            }

        }

        #region Event Handlers

        /// <summary>
        /// Event handler that gets invoked when a new user is connected to the network. Starts the conversation between the server and the new client.
        /// </summary>
        /// <param name="clientId">The ID of the new user</param>
        private void OnClientConnected(ulong clientId)
        {
            // Send hostname
            string hosterName = ProfileController.Instance.DisplayName;
            HosterNameDelivery_ClientRpc(hosterName, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        /// <summary>
        /// Handles client disconnection before finishing the joining process, also prevents getting stuck in the lobby after disconnecting.
        /// </summary>
        private void OnClientStopped(bool wasHost)
        {
            SetClientViewBlocker(false);
            StartLeaveProcess();
        }
        #endregion

        #region For Connection

        /// <summary>
        /// Handles incoming connection requests, reserves a slot for them, and based on the result of the reservation approves or rejects the connection.
        /// </summary>
        private void ClientApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (lobbyModel.ReserveEmptySlot())
            {
                response.CreatePlayerObject = false;
                response.Approved = true;
            }
            else
            {
                response.Reason = "There are no open slots left in the lobby";
                response.Approved = false;
            }
        }

        /// <summary>
        /// Shows or hides a darkening image in order to prevent users from pressing stuff until the connection is finished.
        /// </summary>
        /// <param name="assign">If we want to turn on or off the image.</param>
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

        #endregion

        #region RPCs

        /// <summary>
        /// Notifies the server about a client's disconnection, and handles it's server-side logic.
        /// </summary>
        /// <param name="clientId">Which client disconnected</param>
        [Rpc(SendTo.Server)]
        private void ClientDisconnect_ServerRpc(ulong clientId)
        {
            // Server side:
            LobbySlot slotOfClient = lobbyModel.GetSlotOfPlayer(clientId);
            if (slotOfClient != null) // which shouldnt be
            {
                LobbySlotController obj = clientSlots.Single(slot => slot.SlotModel == slotOfClient);
                obj.GetComponent<NetworkObject>().RemoveOwnership(); // give back ownership to host
                lobbyModel.RemovePlayer(clientId);
                ClientDisconnectFinish_ClientRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
            }
        }

        /// <summary>
        /// After <see cref="ClientDisconnect_ServerRpc(ulong)"/> finished, this method will be called. Destroys client's part of the network and redirects to the main menu.
        /// </summary>
        /// <param name="param"></param>
        [Rpc(SendTo.SpecifiedInParams)]
        private void ClientDisconnectFinish_ClientRpc(RpcParams param)
        {
            // Client side:
            NetworkManager.Singleton.Shutdown(true);
            SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
        }

        /// <summary>
        /// Recieves the hostname from the server and displays it
        /// </summary>
        [Rpc(SendTo.SpecifiedInParams)]
        private void HosterNameDelivery_ClientRpc(string hostname, RpcParams rpcParams)
        {
            // This all happens client-side:
            // Turn off blocker screen
            SetClientViewBlocker(false);
            hostNameDisplay.text = hostname + "'s Lobby";
            // Send client name to server
            string clientName = ProfileController.Instance.DisplayName;
            SendClientName_ServerRpc(NetworkManager.Singleton.LocalClientId, clientName);
        }

        /// <summary>
        /// Gets called after <see cref="HosterNameDelivery_ClientRpc(string, RpcParams)"/>.
        /// Sends the connected client's ID and Name to the server.
        /// </summary>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SendClientName_ServerRpc(ulong clientId, string clientName)
        {
            // This happens server-side:
            int slotIndex = lobbyModel.FindReservedSlot();
            if (slotIndex != -1) // which it shouldnt be here, under no circumstances
            {
                LobbySlotController reserved = clientSlots[slotIndex];

                reserved.GetComponent<NetworkObject>().ChangeOwnership(clientId);
                reserved.ForceRefresh();

                lobbyModel.AddNewPlayer(clientId, clientName, slotIndex);
                SideDataDelivery_ClientRpc(reserved.SlotModel.Side, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            }
        }
        #endregion

        #endregion

        #region Switching Slots

        /// <summary>
        /// This method gets called on the client-side when wanting to switch to a different slot.
        /// </summary>
        /// <param name="callerSlotId">The new slot that the client tries to switch to</param>
        public void SwitchToSlot(int callerSlotId)
        {
            ulong callerClientId = NetworkManager.Singleton.LocalClientId;
            SwitchPlayerSlot_ServerRpc(callerClientId, callerSlotId);
        }

        /// <summary>
        /// Notifies the server about a client's intention to change slots and handles it's server-side logic
        /// </summary>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SwitchPlayerSlot_ServerRpc(ulong clientId, int targetSlotId)
        {
            LobbySlotController targetSlot = clientSlots[targetSlotId];
            LobbySlotController currentSlot = clientSlots.Single(obj => obj.SlotModel == lobbyModel.GetSlotOfPlayer(clientId));

            currentSlot.GetComponent<NetworkObject>().RemoveOwnership();
            targetSlot.GetComponent<NetworkObject>().ChangeOwnership(clientId);

            lobbyModel.ReassignPlayerToSlot(clientId, targetSlotId);

            SideDataDelivery_ClientRpc(targetSlot.SlotModel.Side, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        #endregion

        #region Ready function for Clients

        /// <summary>
        /// This method will get called when a client changes his readiness value.
        /// </summary>
        public void ClientReadyChange(int readyValue)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            ClientReady_ServerRpc(clientId, readyValue);
        }

        /// <summary>
        /// Notifies the server about a client's readiness change, and delegates the change to the server-side equivalent of the target object.
        /// </summary>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void ClientReady_ServerRpc(ulong clientId, int readyValue)
        {
            LobbySlotController obj = clientSlots.Single(slot => slot.SlotModel == lobbyModel.GetSlotOfPlayer(clientId));
            obj.OnReadinessInput(readyValue);
        }
        #endregion

        #region Deck functions

        /// <summary>
        /// Changes to the deck window
        /// </summary>
        public void ViewDeck()
        {
            ChangeScreen(ScreenType.Deck);
        }

        /// <summary>
        /// When changing slots, relists the possible units for the side that the slot represents.
        /// </summary>
        private void ListUnitsForSide(Side current)
        {
            unitAdders.Clear();
            foreach (Transform child in deckParent.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (var item in Defines.UnitValues)
            {
                UnitIdentifier target = item.Key;
                if (Defines.UnitValues[target].Side == current)
                {
                    UnitAdder obj = Instantiate(adderObject);
                    unitAdders.Add(obj);
                    obj.transform.SetParent(deckParent.transform);
                    obj.transform.position = Vector3.zero;
                    obj.transform.rotation = Quaternion.identity;
                    obj.transform.localScale = Vector3.one;
                    obj.Setup(target);
                    GetLimit_ServerRpc(NetworkManager.Singleton.LocalClientId, target);
                }
            }
        }

        #region RPCs

        /// <summary>
        /// When changing sides, delivers the new Side's value to the client, rewrites the main header on the deck's page and relists the units.
        /// </summary>
        [Rpc(SendTo.SpecifiedInParams)]
        private void SideDataDelivery_ClientRpc(Side own, RpcParams rpcParams)
        {
            deckSideText.text = DECK_TEXT_BASE;
            deckSideText.text += own == Side.Imperium ? $"<color=#4242ff>{own.ToString()}</color>" : $"<color=#ff4242>{own.ToString()}</color>";
            ListUnitsForSide(own);
        }

        /// <summary>
        /// Starts the getting process for a limit of a specific unit's amount
        /// </summary>
        /// <param name="clientId">The client that requests it</param>
        /// <param name="identity">For what unit does the client wants to know</param>
        [Rpc(SendTo.Server)]
        private void GetLimit_ServerRpc(ulong clientId, UnitIdentifier identity)
        {
            int limit = Defines.UnitValues[identity].LimitInDeck;
            LimitDelivery_ClientRpc(limit, identity, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        /// <summary>
        /// Returns the limitation for a unit's amount to the client that asked for it.
        /// </summary>
        [Rpc(SendTo.SpecifiedInParams)]
        private void LimitDelivery_ClientRpc(int limit, UnitIdentifier identity, RpcParams param)
        {
            UnitAdder adder = unitAdders.Single(ad => ad.Identity == identity);
            adder.UnitLimit = limit;
        }

        /// <summary>
        /// Notifies the server about a client adding a unit to his deck
        /// </summary>
        [Rpc(SendTo.Server)]
        public void AddUnitToDeck_ServerRpc(ulong clientId, UnitIdentifier identity)
        {
            // Server-side:
            int amount = lobbyModel.ConnectedClients[clientId].Deck.Add(identity);
            AmountDelivery_ClientRpc(amount, identity, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        /// <summary>
        /// Notifies the server about a client removing a unit from his deck
        /// </summary>
        [Rpc(SendTo.Server)]
        public void RemoveUnitFromDeck_ServerRpc(ulong clientId, UnitIdentifier identity)
        {
            int remaining = lobbyModel.ConnectedClients[clientId].Deck.RemoveOne(identity);
            AmountDelivery_ClientRpc(remaining, identity, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }
        
        /// <summary>
        /// Refreshes the client's view about how many are there of a certain unit
        /// </summary>
        [Rpc(SendTo.SpecifiedInParams)]
        private void AmountDelivery_ClientRpc(int amount, UnitIdentifier identity, RpcParams param)
        {
            UnitAdder adder = unitAdders.Single(ad => ad.Identity == identity);
            adder.UnitAmount = amount;
        }
        #endregion

        #endregion

        #region Starting game / ending lobby

        /// <summary>
        /// Tries to start a game and switch to the game scene. Shows an error message if unsuccessful
        /// </summary>
        public void TryStartGame()
        {
            if (IsServer)
            {
                try
                {
                    if (lobbyModel.CanStart())
                    {
                        InterSceneData.ConvertLobbyData(lobbyModel.ConnectedClients);
                        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
                    }
                }
                catch (TabletopException e)
                {
                    if (currentErrorMessage != null)
                    {
                        StopCoroutine(currentErrorMessage);
                    }
                    currentErrorMessage = StartCoroutine(DisplayErrorMessage(e.Message));
                }
            } else
            {
                if (currentErrorMessage != null)
                {
                    StopCoroutine(currentErrorMessage);
                }
                currentErrorMessage = StartCoroutine(DisplayErrorMessage("Only the host can start the game"));
            }
        }

        /// <summary>
        /// Displays an error message accross multiple frames, and after a given time hides it.
        /// For displaying and hiding it uses a fade-in fade-out effect.
        /// </summary>
        private IEnumerator DisplayErrorMessage(string message)
        {
            messageBox.text = message;
            messageBox.alpha = 0;
            messageBox.gameObject.SetActive(true);
            while (messageBox.alpha < 1.0f)
            {
                messageBox.alpha = Mathf.Clamp(messageBox.alpha + (Time.deltaTime * TEXT_FADE_RATE), 0f, 1.0f);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(4);

            while (messageBox.alpha > 0.0f)
            {
                messageBox.alpha = Mathf.Clamp(messageBox.alpha - (Time.deltaTime * TEXT_FADE_RATE), 0f, 1.0f);
                yield return null;
            }

            messageBox.gameObject.SetActive(false);
            currentErrorMessage = null;
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
                Instantiate(networkManagerPrefab);
            }
        }
        private void Start()
        {
            currentErrorMessage = null;
            unitAdders = new List<UnitAdder>();
            if (InterSceneData.ShouldHost)
            {
                CreateSession();
            } else
            {
                JoinSession(InterSceneData.ConnectionAddress);
            }
            InterSceneData.Reset();
            screenStack.Push(ScreenType.Lobby);
        }

        #endregion
    }
}
