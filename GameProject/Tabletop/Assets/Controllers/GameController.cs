using Controllers.Data;
using Controllers.Objects;
using Controllers.Objects.Game;
using Controllers.Objects.Game.InfoPanel;
using Controllers.Objects.Game.WeaponSelection;
using Model;
using Model.Deck;
using Model.GameModel;
using Model.GameModel.Commands;
using Model.Interfaces;
using Model.Units;
using Model.UnityDependant;
using Model.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


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
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference selectAction;
        [SerializeField] private InputActionReference orderAction;
        [SerializeField] private InputActionReference cycleAction;

        [SerializeField] private WeaponSelector selector;
        [SerializeField] private DiceRoller diceRoller;
        [SerializeField] private UnitInfoPanel infopanel;

        [SerializeField] private GameObject buttonControlPanel;
        [SerializeField] private ThrowStatPanel throwPanel;

        #endregion

        #region Fields

        private bool movingEnabled;
        private bool purchaseToggled;
        private GamePlayerObject gamePlayerObject;
        private GameModel<ulong> gameModel;
        private UnitModel? attackedModel;
        public Dictionary<ulong, string> UserColors { get; private set; }

        #endregion

        #region Network Variables

        NetworkVariable<FixedString64Bytes> PlayerNameNetVar = new NetworkVariable<FixedString64Bytes>(string.Empty);
        NetworkVariable<int> TurnCounterNetVar = new NetworkVariable<int>(0);
        NetworkVariable<int> PhaseNetVar = new NetworkVariable<int>(-1);

        NetworkVariable<int> ShownUI = new NetworkVariable<int>(-1);

        NetworkVariable<SelectedUnitData> SelectedUnitNetVar = new NetworkVariable<SelectedUnitData>(SelectedUnitData.Empty);
        NetworkList<WeaponInfoData> SelectedWeaponsNetVar = new NetworkList<WeaponInfoData>();

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
            purchaseToggled = false;
            movingEnabled = true;

            PlayerNameNetVar.OnValueChanged += PlayerNameChanged;
            TurnCounterNetVar.OnValueChanged += TurnValueChanged;
            PhaseNetVar.OnValueChanged += PhaseValueChanged;
            SelectedUnitNetVar.OnValueChanged += SelectedUnitValueChanged;
            SelectedWeaponsNetVar.OnListChanged += SelectedWeaponsValueChanged;
            ShownUI.OnValueChanged += NetworkScreenChange;

            moveAction.action.performed += MoveAction_Performed;
            moveAction.action.canceled += MoveAction_Canceled;

            selectAction.action.performed += SelectAction_Performed;
            orderAction.action.performed += OrderAction_Performed;

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
                gameModel.RequestMoreData += GameModel_RequestMoreData;
                gameModel.SelectedUnitChanged += GameModel_SelectedUnitChanged;
                gameModel.GameOver += GameModel_GameOver;

                SetupPurchasers();
                SetupColors();

                gamePlayerObject.NetworkObject.ChangeOwnership(gameModel.ActivePlayerId);

                gameModel.StartGame();
                ForceUpdate_ClientRpc(PlayerNameNetVar.Value, TurnCounterNetVar.Value, PhaseNetVar.Value);
            }
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
            if (gameModel.ActivePlayerId != clientId || !movingEnabled)
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
                ShowWeaponSelector_ClientRpc(false, RpcTarget.Single(clientId, RpcTargetUse.Temp));
                gameModel.SelectUnit(clientId, null);
            }
        }

        private void GameModel_SelectedUnitChanged(object sender, EventArgs e)
        {
            Debug.Log($"<color=magenta>Entering event handler...</color>");
            Debug.Log($"<color=magenta>Is value null? {gameModel.SelectedUnit == null}</color>");
            if (gameModel.SelectedUnit != null)
            {
                Debug.Log($"<color=magenta>Value is not null, starting selection...</color>");
                UnitModel m = gameModel.SelectedUnit as UnitModel;
                SelectedWeaponsNetVar.Clear();
                Debug.Log($"<color=magenta>Previous weapons have been cleared...</color>");
                foreach (UsableWeapon weapon in m.EquippedWeapons)
                {
                    Debug.Log($"<color=magenta>New weapon added: {weapon.Weapon.Constants.Name}</color>");
                    SelectedWeaponsNetVar.Add(new WeaponInfoData((int)weapon.Weapon.Identity, weapon.Weapon.Count, weapon.CanDamage, m.GetInstanceID()));
                }
                Debug.Log($"<color=magenta>Network value set. Selection ended on server.</color>");
                SelectedUnitNetVar.Value = new SelectedUnitData((int)m.Identity, m.CanMove, m.CurrentHP, m.GetInstanceID());
                ShowInfoPanel_ClientRpc(true);
            } else
            {
                Debug.Log($"<color=magenta>New value is null. Hiding panel, selection ended.</color>");
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
                    Debug.Log($"<color=yellow>A weapon has been added: {changeEvent.Value}</color>");
                    infopanel.AddWeapon(changeEvent.Value);
                    selector.AddButton(changeEvent.Value);
                    break;
                case NetworkListEvent<WeaponInfoData>.EventType.Clear:
                    Debug.Log($"<color=yellow>Weapons list has been cleared.</color>");
                    infopanel.ClearWeapons();
                    selector.ClearButtons();
                    break;
                default:
                    Debug.Log("Default branch");
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
            Ray ray = gamePlayerObject.AttachedCamera.ScreenPointToRay(mousepos);
            if (Physics.Raycast(ray, out RaycastHit hit, MAX_CLICK_DISTANCE, unitLayer))
            {
                GameObject obi = hit.collider.gameObject;
                attackedModel = obi.GetComponent<UnitModel>();
                if (attackedModel != null)
                {
                    Debug.Log("<color=aqua>Invoking attack order creation...</color>");
                    gameModel.CreateCommand<AttackCommand<ulong>>(clientId, attackedModel);
                    throwPanel.RegisterDefender(attackedModel.Constants.ArmorSave);
                }
            }
            else if (Physics.Raycast(ray, out hit, MAX_CLICK_DISTANCE, groundLayer))
            {
                Vector3 hitLocation = hit.point;
                Debug.Log("<color=aqua>Invoking move order creation...</color>");
                // I know this is wacky but as far as I know this is the only way to keep the model clean of any types depending on unity
                gameModel.CreateCommand<MoveCommand<Vector3>>(clientId, hitLocation);
            }
        }


        private void GameModel_RequestMoreData(object sender, EventArgs e)
        {
            ShowWeaponSelector_ClientRpc(true, RpcTarget.Single(gameModel.ActivePlayerId, RpcTargetUse.Temp));
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
            Debug.Log($"<color=red>Got attacking weapon: {id}</color>");
            AttackCommand<ulong> c = gameModel.PendingCommand as AttackCommand<ulong>;

            WeaponConstants con = Defines.Weapons[id];
            IWeaponUser m = c.Initiator;
            int weaponCount = m.EquippedWeapons.Single(w => w.Weapon.Identity == id).Weapon.Count;
            c.SetUsedWeapon(id);
            throwPanel.RegisterAttacker(con.Attacks * weaponCount, con.BallisticSkill, con.ArmorPiercing, con.Damage);

            ExecuteAttackCommandAsync();
            Debug.Log("<color=red>AttackWeaponSelected method is over...</color>");
        }

        private async void ExecuteAttackCommandAsync()
        {
            Debug.Log($"<color=red>Starting of execution...</color>");
            Debug.Log($"<color=red>Saving the initiator...</color>");
            AttackCommand<ulong> ac = gameModel.PendingCommand as AttackCommand<ulong>;
            UnitModel m = ac.Initiator as UnitModel;
            Debug.Log($"<color=red>Is initiator null? {m == null}</color>");

            Debug.Log($"<color=red>Is button control panel enabled? {buttonControlPanel.activeSelf}</color>");
            ShownUI.Value = (int)ScreenType.Throw;
            Debug.Log($"<color=red>Is button control panel enabled? {buttonControlPanel.activeSelf}</color>");

            Debug.Log($"<color=red>Is moving enabled? {buttonControlPanel.activeSelf}</color>");
            movingEnabled = false;
            Debug.Log($"<color=red>Is moving enabled? {buttonControlPanel.activeSelf}</color>");

            await gamePlayerObject.MoveToArena();
            await gameModel.ExecutePendingCommand();
            await gamePlayerObject.MoveToPrevious();

            // Refresh the usable weapons' list. (I cant modify a single element so I have to reload the whole list)
            SelectedWeaponsNetVar.Clear();
            foreach (UsableWeapon weapon in m.EquippedWeapons)
            {
                SelectedWeaponsNetVar.Add(new WeaponInfoData((int)weapon.Weapon.Identity, weapon.Weapon.Count, weapon.CanDamage, m.GetInstanceID()));
            }

            Debug.Log($"<color=red>Is button control panel enabled? {buttonControlPanel.activeSelf}</color>");
            ShownUI.Value = (int)ScreenType.Game;
            Debug.Log($"<color=red>Is button control panel enabled? {buttonControlPanel.activeSelf}</color>");

            Debug.Log($"<color=red>Is moving enabled? {buttonControlPanel.activeSelf}</color>");
            movingEnabled = true;
            Debug.Log($"<color=red>Is moving enabled? {buttonControlPanel.activeSelf}</color>");
        }

        private void NetworkScreenChange(int oldvalue, int newvalue)
        {
            ChangeScreen((ScreenType)newvalue);
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
            NextCycle_ServerRpc(clientId, 1);
        }


        [Rpc(SendTo.Server)]
        private void NextCycle_ServerRpc(ulong clientId, int value)
        {
            Debug.LogError("Not finished function!");
        }

        #endregion

        #region Game over
        private void GameModel_GameOver(object sender, Side e)
        {
            Debug.LogError("GAMOE OVER EVENT RECIEVED");
        }
        #endregion
    }
}
