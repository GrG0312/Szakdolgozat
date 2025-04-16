using Controllers.Data;
using Controllers.Objects.Game;
using Controllers.Objects.Game.InfoPanel;
using Controllers.Objects.Game.Purchase;
using Controllers.Objects.Game.WeaponSelection;
using Model;
using Model.Deck;
using Model.GameModel;
using Model.GameModel.Commands;
using Model.Units;
using Model.UnityDependant;
using Model.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


namespace Controllers
{
    public sealed class GameController : MenuControllerBase<GameController>
    {
        private const int MAX_CLICK_DISTANCE = 100;

        #region Serializations

        [SerializeField] private GamePlayerObject gamePlayerObjectPrefab;
        [SerializeField] private List<GameObject> spawnpoints;
        [SerializeField] private UnitModel unitPrefab;
        
        [SerializeField] private TMP_Text pointsNow;
        [SerializeField] private TMP_Text pointPerTurn;
        [SerializeField] private GameObject purchaserMenuButton;
        [SerializeField] private PurchaserWindow purchaserMenuParent;

        [SerializeField] private TMP_Text activePlayerText;
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private TMP_Text turnText;

        [SerializeField] private LayerMask unitLayer;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask uiLayer;

        [SerializeField] private InputActionAsset inputActionAsset;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference selectAction;
        [SerializeField] private InputActionReference orderAction;
        [SerializeField] private InputActionReference rangeAction;
        [SerializeField] private InputActionReference escape;
        [SerializeField] private InputActionReference doneAction;

        [SerializeField] private WeaponSelector selector;
        [SerializeField] private DiceRoller diceRoller;
        [SerializeField] private UnitInfoPanel infopanel;

        [SerializeField] private GameObject buttonControlPanel;
        [SerializeField] private ThrowStatPanel throwPanel;

        [SerializeField] private RangeIndicator rangeIndicatorObject;

        #endregion

        #region Fields

        private bool wasSpacePressed = false;
        private bool purchaseToggled = false;
        private bool leaveToggled = false;

        private GamePlayerObject gamePlayerObject;
        private GameModel<ulong> gameModel;

        private TaskCompletionSource<WeaponIdentifier> orderTaskSource;

        public Dictionary<ulong, string> UserColors { get; private set; }

        #endregion

        #region Network Variables

        NetworkVariable<FixedString64Bytes> PlayerNameNetVar = new NetworkVariable<FixedString64Bytes>(string.Empty);
        NetworkVariable<int> TurnCounterNetVar = new NetworkVariable<int>(0);
        NetworkVariable<int> PhaseNetVar = new NetworkVariable<int>(-1);

        NetworkVariable<int> ShownUI = new NetworkVariable<int>(-1);

        NetworkVariable<SelectedUnitData> SelectedUnitNetVar = new NetworkVariable<SelectedUnitData>(SelectedUnitData.Empty);
        NetworkList<WeaponInfoData> SelectedWeaponsNetVar = new NetworkList<WeaponInfoData>();
        NetworkList<int> AttackingWeaponsNetVar = new NetworkList<int>(); 

        #endregion

        #region Unity messages

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"Multiple {nameof(GameController)} instances found. Deleting duplicate...");
                Destroy(Instance.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            ChangeScreen(ScreenType.Game);

            foreach (InputActionMap map in inputActionAsset.actionMaps)
            {
                map.Disable();
            }

            PlayerNameNetVar.OnValueChanged += PlayerNameChanged;
            TurnCounterNetVar.OnValueChanged += TurnValueChanged;
            PhaseNetVar.OnValueChanged += PhaseValueChanged;
            SelectedUnitNetVar.OnValueChanged += SelectedUnitValueChanged;
            SelectedWeaponsNetVar.OnListChanged += SelectedWeaponsValueChanged;
            AttackingWeaponsNetVar.OnListChanged += AttackingWeaponsValueChanged;
            ShownUI.OnValueChanged += NetworkScreenChange;

            moveAction.action.performed += MoveAction_Performed;
            moveAction.action.canceled += MoveAction_Canceled;

            selectAction.action.performed += SelectAction_Performed;
            orderAction.action.performed += OrderAction_Performed;

            rangeAction.action.started += RangeAction_Started;
            rangeAction.action.canceled += RangeAction_Canceled;

            escape.action.performed += EscapeAction_Performed;

            doneAction.action.performed += DoneAction_Performed;

            if (NetworkManager.Singleton.IsHost)
            {
                gamePlayerObject = Instantiate(gamePlayerObjectPrefab);
                gamePlayerObject.NetworkObject.Spawn(true);

                throwPanel.RegisterThrower(diceRoller);

                Dictionary<Side, GameObject> sp = new Dictionary<Side, GameObject>();
                
                // I have to do it this way because unity cannot display Dictionary type,
                // so I can't setup it in the editor
                int index = 0;
                foreach (Side side in Enum.GetValues(typeof(Side)))
                {
                    sp.Add(side, spawnpoints.ElementAt(index));
                    index++;
                }
                gameModel = 
                    new GameModel<ulong>(InterSceneData.Players, new UnityUnitFactory(sp, unitPrefab), new UnityCommandFactory(), diceRoller);
                gameModel.PlayerPointsChanged += GameModel_PlayerPointsChanged;
                gameModel.ActivePlayerChanged += GameModel_ActivePlayerChanged;
                gameModel.PhaseChanged += GameModel_PhaseChanged;
                gameModel.TurnChanged += GameModel_TurnChanged;
                gameModel.SelectedUnitChanged += GameModel_SelectedUnitChanged;
                gameModel.GameOver += GameModel_GameOver;
                gameModel.UnitCycled += GameModel_UnitCycled;

                SetupPurchasers();
                SetupColors();

                gamePlayerObject.NetworkObject.ChangeOwnership(gameModel.ActivePlayerId);

                gameModel.StartGame();
                ForceUpdate_ClientRpc(PlayerNameNetVar.Value, TurnCounterNetVar.Value, PhaseNetVar.Value);
            }

            inputActionAsset.FindActionMap("Game").Enable();
            inputActionAsset.FindActionMap("Always").Enable();
        }

        public override void OnDestroy()
        {
            moveAction.action.performed -= MoveAction_Performed;
            moveAction.action.canceled -= MoveAction_Canceled;

            selectAction.action.performed -= SelectAction_Performed;
            orderAction.action.performed -= OrderAction_Performed;

            PlayerNameNetVar.OnValueChanged -= PlayerNameChanged;
            TurnCounterNetVar.OnValueChanged -= TurnValueChanged;
            PhaseNetVar.OnValueChanged -= PhaseValueChanged;
        }
        #endregion

        #region Setup methods
        [Rpc(SendTo.NotServer)]
        private void ForceUpdate_ClientRpc(FixedString64Bytes name, int turncount, int current)
        {
            PlayerNameChanged(name, name);
            TurnValueChanged(turncount, turncount);
            PhaseValueChanged(current, current);
        }

        private void SetupColors()
        {
            UserColors = new Dictionary<ulong, string>();
            int blue = 0;
            int red = 0;
            foreach (KeyValuePair<ulong, GamePlayerData> kvp in gameModel.ConnectedPlayers)
            {
                switch (kvp.Value.Side)
                {
                    case Side.Imperium:
                        UserColors.Add(kvp.Key, Defines.BlueColors.ElementAt(blue));
                        blue++;
                        break;
                    case Side.Chaos:
                        UserColors.Add(kvp.Key, Defines.RedColors.ElementAt(red));
                        red++;
                        break;
                    default:
                        break;
                }
            }
        }

        private void SetupPurchasers()
        {
            foreach (KeyValuePair<ulong, GamePlayerData> kvp in gameModel.ConnectedPlayers)
            {
                ulong clientid = kvp.Key;
                GamePlayerData data = kvp.Value;
                foreach (DeckEntry item in data.Deck.Entries)
                {
                    AddUnitPurchaser_ClientRpc(
                        item.TargetUnit,
                        item.Amount,
                        item.Constants.Price,
                        RpcTarget.Single(clientid, RpcTargetUse.Temp));
                }
            }
        }


        [Rpc(SendTo.SpecifiedInParams)]
        private void AddUnitPurchaser_ClientRpc(UnitIdentifier id, int amount, int price, RpcParams param)
        {
            purchaserMenuParent.AddPurchaser(id, amount, price);
        }

        #endregion

        #region Moving camera

        private void MoveAction_Performed(InputAction.CallbackContext obj)
        {
            Vector2 v2 = obj.ReadValue<Vector2>();
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            MoveCamera_ServerRpc(clientId, v2.x, v2.y);
        }


        [Rpc(SendTo.Server)]
        private void MoveCamera_ServerRpc(ulong clientId, float x, float y)
        {
            if (gameModel.ActivePlayerId != clientId)
            {
                return;
            }
            gamePlayerObject.StartMoving(x, y);
        }


        private void MoveAction_Canceled(InputAction.CallbackContext obj)
        {
            StopCamera_ServerRpc(NetworkManager.Singleton.LocalClientId);
        }


        [Rpc(SendTo.Server)]
        private void StopCamera_ServerRpc(ulong clientId)
        {
            if (gameModel.ActivePlayerId != clientId)
            {
                return;
            }
            gamePlayerObject.StopMoving();
        }

        #endregion

        #region Purchasing units

        public void TogglePurchaseTab()
        {
            purchaseToggled = !purchaseToggled; // false -> true | true -> false
            purchaserMenuParent.gameObject.SetActive(purchaseToggled);
        }


        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void PurchaseUnitBegin_ServerRpc(ulong clientId, UnitIdentifier id)
        {
            // Server side:
            if (gameModel.BuyUnit(clientId, id))
            {
                int amount = 0;
                try
                {
                    amount = gameModel.ConnectedPlayers[clientId].Deck.Entries.Single(e => e.TargetUnit == id).Amount;
                }
                catch (InvalidOperationException) { /*If the amount reaches zero, then the enrty is taken out*/ }
                PurchaseUnitEnd_ClientRpc(id, amount, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            }
        }


        [Rpc(SendTo.SpecifiedInParams)]
        private void PurchaseUnitEnd_ClientRpc(UnitIdentifier id, int amount, RpcParams param)
        {
            // Client side:
            if (amount != 0)
            {
                purchaserMenuParent.UpdatePurchaser(id, amount);
            }
            else
            {
                purchaserMenuParent.RemovePurchaser(id);
            }
        }


        [Rpc(SendTo.SpecifiedInParams)]
        private void CurrencyDataDelivery_ClientRpc(int currency, int ppt, RpcParams param)
        {
            //Client side:
            pointsNow.text = currency.ToString();
            pointPerTurn.text = ppt.ToString();
        }

        #endregion

        #region Selecting units

        private void SelectAction_Performed(InputAction.CallbackContext obj)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Vector3 v = Input.mousePosition;
                SelectUnit_ServerRpc(NetworkManager.Singleton.LocalClientId, v);
            }
        }


        [Rpc(SendTo.Server)]
        private void SelectUnit_ServerRpc(ulong clientId, Vector3 mousepos)
        {
            Ray ray = gamePlayerObject.AttachedCamera.ScreenPointToRay(mousepos);

            // If the click hit a Unit
            if (Physics.Raycast(ray, out RaycastHit hit, MAX_CLICK_DISTANCE, unitLayer))
            {
                GameObject obi = hit.collider.gameObject;
                UnitModel unitModel = obi.GetComponent<UnitModel>();
                if (unitModel != null)
                {
                    gameModel.SelectUnit(clientId, unitModel);
                }
            }
            else
            {
                gameModel.SelectUnit(clientId, null);
            }
            ShowWeaponSelector_ClientRpc(false, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        }

        private void GameModel_SelectedUnitChanged(object sender, EventArgs e)
        {
            if (gameModel.SelectedUnit != null)
            {
                UnitModel m = gameModel.SelectedUnit as UnitModel;
                SelectedWeaponsNetVar.Clear();
                foreach (UsableWeapon weapon in m.UsableWeapons)
                {
                    SelectedWeaponsNetVar.Add(new WeaponInfoData((int)weapon.Weapon.Identity, weapon.Weapon.Count, weapon.CanDamage, m.GetInstanceID()));
                }
                SelectedUnitNetVar.Value = new SelectedUnitData((int)m.Identity, m.CanMove, m.CurrentHP, m.GetInstanceID());
                ShowInfoPanel_ClientRpc(true);
            } else
            {
                ShowInfoPanel_ClientRpc(false);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void ShowInfoPanel_ClientRpc(bool shouldShow)
        {
            infopanel.gameObject.SetActive(shouldShow);
        }

        private void SelectedUnitValueChanged(SelectedUnitData oldvalue, SelectedUnitData newvalue)
        {
            infopanel.gameObject.SetActive(true);
            infopanel.UpdateValues(newvalue);
        }

        private void SelectedWeaponsValueChanged(NetworkListEvent<WeaponInfoData> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<WeaponInfoData>.EventType.Add:
                    infopanel.AddWeapon(changeEvent.Value);
                    break;
                case NetworkListEvent<WeaponInfoData>.EventType.Clear:
                    infopanel.ClearWeapons();
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Ordering units

        private void OrderAction_Performed(InputAction.CallbackContext obj)
        {
            selector.gameObject.SetActive(false);
            Vector3 v = Input.mousePosition;
            OrderUnit_ServerRpc(NetworkManager.Singleton.LocalClientId, v);
        }


        [Rpc(SendTo.Server)]
        private void OrderUnit_ServerRpc(ulong clientId, Vector3 mousepos)
        {
            ShowWeaponSelector_ClientRpc(false, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            Ray ray = gamePlayerObject.AttachedCamera.ScreenPointToRay(mousepos);
            if (Physics.Raycast(ray, out RaycastHit hit, MAX_CLICK_DISTANCE, unitLayer))
            {
                if (gameModel.CurrentPhase != Phase.Fighting)
                {
                    return;
                }
                GameObject obi = hit.collider.gameObject;
                UnitModel? attacked = obi.GetComponent<UnitModel>();
                if (attacked != null)
                {
                    // Register the defender's stats on the throw sidepanel
                    throwPanel.RegisterDefender(attacked.Constants.ArmorSave);
                    // Create the still incomplete AttackCommand
                    gameModel.CreateCommand<AttackCommand<Vector3>>(clientId, attacked);
                    AttackCommand<Vector3> cmd = gameModel.PendingCommand as AttackCommand<Vector3>;
                    if (cmd.UsableWeapons.Count == 0)
                    {
                        gameModel.AbortCommand();
                        return;
                    }

                    // Set up the weapon selector window with only the weapons that can reach the target
                    AttackingWeaponsNetVar.Clear();
                    foreach (UsableWeapon weapon in cmd.UsableWeapons)
                    {
                        AttackingWeaponsNetVar.Add((int)weapon.Weapon.Identity);
                    }

                    AttackCommandProcess(clientId, attacked);
                }
            }
            else if (Physics.Raycast(ray, out hit, MAX_CLICK_DISTANCE, groundLayer))
            {
                if (gameModel.CurrentPhase != Phase.Movement)
                {
                    return;
                }
                Vector3 hitLocation = hit.point;
                // I know this is wacky but as far as I know this is the only way to keep the model clean of any types depending on unity
                gameModel.CreateCommand<MoveCommand<Vector3>>(clientId, hitLocation);
                gameModel.ExecuteCommand();
            }
        }

        private void AttackingWeaponsValueChanged(NetworkListEvent<int> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<int>.EventType.Add:
                    selector.AddButton((WeaponIdentifier)changeEvent.Value);
                    break;
                case NetworkListEvent<int>.EventType.Clear:
                    selector.ClearButtons();
                    break;
                default:
                    break;
            }
        }

        private async void AttackCommandProcess(ulong clientId, UnitModel defender)
        {
            WeaponIdentifier id = await WaitForAttackingWeapon(clientId);
            WeaponConstants used = Defines.Weapons[id];
            AttackCommand<Vector3> cmd = gameModel.PendingCommand as AttackCommand<Vector3>;
            cmd.RegisterUsedWeapon(id);
            int weaponCount = cmd.UsableWeapons.Single(w => w.Weapon.Identity == id).Weapon.Count;
            throwPanel.RegisterAttacker(used.Attacks * weaponCount, used.BallisticSkill, used.ArmorPiercing, used.Damage);

            UnitModel m = gameModel.SelectedUnit as UnitModel;

            wasSpacePressed = false;

            SwitchActionMap_ClientRpc(false);
            ShownUI.Value = (int)ScreenType.Throw;

            await gamePlayerObject.MoveToArena();
            await gameModel.ExecuteCommand();
            ShowSpaceMessage_ClientRpc(true, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            await WaitUntilSpacePressed();
            ShowSpaceMessage_ClientRpc(false, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            await gamePlayerObject.MoveToPrevious();

            // Refresh the usable weapons' list. (I cant modify a single element so I have to reload the whole list)
            SelectedWeaponsNetVar.Clear();
            foreach (UsableWeapon weapon in m.UsableWeapons)
            {
                SelectedWeaponsNetVar.Add(new WeaponInfoData((int)weapon.Weapon.Identity, weapon.Weapon.Count, weapon.CanDamage, m.GetInstanceID()));
            }

            SwitchActionMap_ClientRpc(true);
            ShownUI.Value = (int)ScreenType.Game;
            
        }

        private Task<WeaponIdentifier> WaitForAttackingWeapon(ulong clientId)
        {
            orderTaskSource = new TaskCompletionSource<WeaponIdentifier>();
            ShowWeaponSelector_ClientRpc(true, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            return orderTaskSource.Task;
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void ShowWeaponSelector_ClientRpc(bool isShown, RpcParams param)
        {
            selector.transform.position = Input.mousePosition;
            selector.gameObject.SetActive(isShown);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void AttackingWeaponSelected_ServerRpc(ulong clientId, WeaponIdentifier id)
        {
            orderTaskSource.SetResult(id);
        }

        private void NetworkScreenChange(int oldvalue, int newvalue)
        {
            ChangeScreen((ScreenType)newvalue);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void SwitchActionMap_ClientRpc(bool defaultcontrols)
        {
            if (defaultcontrols)
            {
                inputActionAsset.FindActionMap("Throw").Disable();
                inputActionAsset.FindActionMap("Game").Enable();
            } else
            {
                inputActionAsset.FindActionMap("Game").Disable();
                inputActionAsset.FindActionMap("Throw").Enable();
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void ShowSpaceMessage_ClientRpc(bool v, RpcParams param)
        {
            throwPanel.ShowMessage(v);
        }

        private async Task WaitUntilSpacePressed()
        {
            while (!wasSpacePressed)
            {
                await Task.Yield();
            }
        }

        private void DoneAction_Performed(InputAction.CallbackContext obj)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            SpacePressed_ServerRpc(clientId);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void SpacePressed_ServerRpc(ulong clientId)
        {
            if (clientId != gameModel.ActivePlayerId)
            {
                return;
            }
            wasSpacePressed = true;
        }

        #endregion

        #region Finishing turn
        public void TurnFinished()
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            TurnFinished_ServerRpc(clientId);
        }


        [Rpc(SendTo.Server)]
        private void TurnFinished_ServerRpc(ulong clientId)
        {
            ShowWeaponSelector_ClientRpc(false, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            gameModel.PlayerDone(clientId);
        }

        #region Model events

        private void GameModel_ActivePlayerChanged(object sender, EventArgs e)
        {
            PlayerNameNetVar.Value = $"<color={UserColors[gameModel.ActivePlayerId]}>{gameModel.ActivePlayerData.Name}</color>";
            gamePlayerObject.NetworkObject.ChangeOwnership(gameModel.ActivePlayerId);
        }

        private void GameModel_PhaseChanged(object sender, EventArgs e)
        {
            PhaseNetVar.Value = (int)gameModel.CurrentPhase;
        }

        private void GameModel_TurnChanged(object sender, EventArgs e)
        {
            TurnCounterNetVar.Value = gameModel.TurnCounter;
        }

        private void GameModel_PlayerPointsChanged(object sender, PlayerPointsChangedEventArgs<ulong> e)
        {
            CurrencyDataDelivery_ClientRpc(e.Current, e.PerTurn, RpcTarget.Single(e.Owner, RpcTargetUse.Temp));
        }

        #endregion

        #region Network Variable events
        private void PlayerNameChanged(FixedString64Bytes oldvalue, FixedString64Bytes newvalue)
        {
            // Because of this setting, it's important to always set the color before the name!!
            activePlayerText.text = newvalue.ToString();
        }
        private void TurnValueChanged(int oldvalue, int newvalue)
        {
            turnText.text = newvalue.ToString();
        }
        private void PhaseValueChanged(int oldvalue, int newvalue)
        {
            phaseText.text = ((Phase)newvalue).ToString();
        }
        #endregion

        #endregion

        #region Cycle between units

        public void CycleWithButton()
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            NextCycle_ServerRpc(clientId);
        }


        [Rpc(SendTo.Server)]
        private void NextCycle_ServerRpc(ulong clientId)
        {
            gameModel.CycleUnits(clientId);
        }

        private void GameModel_UnitCycled(object sender, EventArgs e)
        {
            if (gameModel.SelectedUnit == null)
            {
                return;
            }
            UnitModel m = gameModel.SelectedUnit as UnitModel;
            gamePlayerObject.transform.position = m.Position;
        }

        #endregion

        #region Undo commands

        public void UndoWithButton()
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;

        }

        [Rpc(SendTo.Server)]
        private void UndoCommand_ServerRpc(ulong clientId)
        {
            gameModel.UndoCommand(clientId);
        }

        #endregion

        #region Range display

        private void RangeAction_Started(InputAction.CallbackContext obj)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            RangeIndicatorShow_ServerRpc(clientId, true);
        }

        private void RangeAction_Canceled(InputAction.CallbackContext obj)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            RangeIndicatorShow_ServerRpc(clientId, false);
        }

        [Rpc(SendTo.Server)]
        private void RangeIndicatorShow_ServerRpc(ulong clientId, bool shouldShow)
        {
            if (clientId != gameModel.ActivePlayerId)
            {
                return;
            }
            if (shouldShow)
            {
                if (gameModel.SelectedUnit != null)
                {
                    UnitModel m = gameModel.SelectedUnit as UnitModel;
                    Vector3 pos = m.Position;
                    RangeIndicatorPosition_ClientRpc(pos);
                }
            } else
            {
                RangeIndicatorHide_ClientRpc();
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void RangeIndicatorPosition_ClientRpc(Vector3 selectedPos)
        {
            rangeIndicatorObject.transform.position = selectedPos;
            rangeIndicatorObject.gameObject.SetActive(true);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void RangeIndicatorHide_ClientRpc()
        {
            rangeIndicatorObject.gameObject.SetActive(false);
        }
        #endregion

        #region Leaving

        private void EscapeAction_Performed(InputAction.CallbackContext obj)
        {
            if (leaveToggled)
            {
                ChangeScreen(ScreenType.Game);
            } else
            {
                ChangeScreen(ScreenType.Leave);
            }
            leaveToggled = !leaveToggled;
        }

        public void LeaveGame()
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            ClientLeaves_ServerRpc(clientId);
            BackToMenu(false);
        }

        [Rpc(SendTo.Server)]
        private void ClientLeaves_ServerRpc(ulong clientId)
        {
            gameModel.Forfeit(clientId);
        }

        private void BackToMenu(bool didWin)
        {
            ProfileController.Instance.GameFinished(didWin);
            StartCoroutine(ShutdownRoutine());
        }

        private IEnumerator ShutdownRoutine()
        {
            if (IsHost)
            {
                yield return new WaitUntil(() => NetworkManager.Singleton.ConnectedClients.Count == 1);
            }
            NetworkManager.Singleton.Shutdown(true);
            yield return new WaitForSeconds(0.5f);
            Destroy(NetworkManager.Singleton.gameObject);
            SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
        }
        #endregion

        #region Game over
        private void GameModel_GameOver(object sender, Side e)
        {
            // Server side:
            foreach (KeyValuePair<ulong, GamePlayerData> kvp in gameModel.ConnectedPlayers)
            {
                GameOver_ClientRpc(kvp.Value.Side == e);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void GameOver_ClientRpc(bool didWin)
        {
            BackToMenu(didWin);
        }
        #endregion
    }
}
