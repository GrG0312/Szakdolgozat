using System;
using System.Linq;
using Model;
using Model.Lobby;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Controllers.Objects.Lobby
{
    public class LobbyPlayerObject : NetworkBehaviour
    {
        #region Constants
        private const string CLOSED_OPTION = "Closed";
        private const string SWITCH_OPTION = "Switch";

        private const string READY_RICH_TEXT = "<color=green>Ready";
        private const string NOT_READY_RICH_TEXT = "<color=orange>Not Ready";
        private const string CLOSED_RICH_TEXT = "<color=red>Closed";

        private readonly IReadOnlyDictionary<Side, string> sideColors = new Dictionary<Side, string>()
        {
            { Side.Imperium, "#4242ff" },
            { Side.Chaos, "#ff4242" }
        };
        #endregion

        #region Serializations and network variables
        [SerializeField] private Image slotBg;
        private NetworkVariable<FixedString32Bytes> slotColorNetworkVar = 
            new NetworkVariable<FixedString32Bytes>("#FFFFF");

        [SerializeField] private TMP_Text nameField;
        private NetworkVariable<FixedString32Bytes> nameFieldNetworkVar = 
            new NetworkVariable<FixedString32Bytes>(string.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [SerializeField] private TMP_Dropdown readinessDropdown;
        private NetworkVariable<int> readinessDropdownNetworkVar =
            new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [SerializeField] private TMP_Dropdown emptyDropdown;
        private NetworkVariable<int> emptyDropdownNetworkVar =
            new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [SerializeField] private TMP_Text statusLabel;
        private NetworkVariable<FixedString32Bytes> statusLabelNetworkVar =
            new NetworkVariable<FixedString32Bytes>(string.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public int SlotId { get { return slotIdNetworkVar.Value; } }
        private NetworkVariable<int> slotIdNetworkVar =
            new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        #endregion

        #region Initial data setup
        public void SetupInitialData(int id, Side side)
        {
            slotIdNetworkVar.Value = id;
            slotColorNetworkVar.Value = sideColors[side];
            SlotModel = new LobbySlot(side);
            SlotModel.OccupantStatusChanged += OnOccupantChange;
            SlotModel.PlayerChanged += OnPlayerChanged;

            if (IsServer)
            {
                // Add closing option only to server
                emptyDropdown.options.Add(new TMP_Dropdown.OptionData(CLOSED_OPTION));
            }
        }

        private void OnSlotColorNetVarValueChanged(FixedString32Bytes oldvalue, FixedString32Bytes newvalue)
        {
            ColorUtility.TryParseHtmlString(newvalue.ToString(), out Color c);
            slotBg.color = c;
        }
        #endregion

        #region Model
        public LobbySlot SlotModel { get; private set; }
        #endregion

        #region Unity Messages
        private void Awake()
        {
            nameFieldNetworkVar.OnValueChanged += OnNameNetVarValueChanged;
            emptyDropdownNetworkVar.OnValueChanged += OnEmptyNetVarValueChanged;
            readinessDropdownNetworkVar.OnValueChanged += OnReadyNetVarValueChanged;
            statusLabelNetworkVar.OnValueChanged += OnStatusNetVarValueChanged;
            slotColorNetworkVar.OnValueChanged += OnSlotColorNetVarValueChanged;

            readinessDropdown.onValueChanged.AddListener(OnReadinessInput);
            emptyDropdown.onValueChanged.AddListener(OnOccupantInput);
        }
        private void Start()
        {
            transform.localPosition = new Vector3(0, 225 - (SlotId * this.gameObject.GetComponent<RectTransform>().sizeDelta.y), 0);
            transform.localScale = Vector3.one;
        }
        public override void OnNetworkSpawn()
        {
            // Updating the contents upon creation
            ForceRefresh();
        }
        #endregion

        #region Startpoint - Calling controller methods
        public void OnReadinessInput(int value)
        {
            if (IsServer)
            {
                readinessDropdownNetworkVar.Value = value; // 0 - not ready, 1 - ready
                statusLabelNetworkVar.Value = value == 1 ? READY_RICH_TEXT : NOT_READY_RICH_TEXT;
            } else
            {
                LobbyController.Instance.ClientReadyChange(value);
            }
        }

        private void OnOccupantInput(int value)
        {
            switch (value)
            {
                case 0: // OPEN
                    if (IsServer)
                    {
                        SlotModel.OccupantStatus = SlotOccupantStatus.Open;
                        emptyDropdownNetworkVar.Value = (int)SlotModel.OccupantStatus;
                        RemoveDropdownOption(CLOSED_OPTION); // remove if possible, should be last option
                        RemoveDropdownOption(SWITCH_OPTION); // remove if possible
                        emptyDropdown.options.Add(new TMP_Dropdown.OptionData(SWITCH_OPTION));
                        emptyDropdown.options.Add(new TMP_Dropdown.OptionData(CLOSED_OPTION));
                    }
                    break;
                case 1: // SWITCH / RESERVE
                    if (IsServer && emptyDropdown.options.Count < 3) // if there is only Open and Closed, then Closed would operate as Switch, we dont want that
                    {
                        break;
                    }
                    LobbyController.Instance.SwitchToSlot(SlotId);
                    break;
                case 2: // CLOSED
                    if (IsServer)
                    {
                        SlotModel.OccupantStatus = SlotOccupantStatus.Closed;
                        emptyDropdownNetworkVar.Value = (int)SlotModel.OccupantStatus;
                        RemoveDropdownOption(SWITCH_OPTION); // remove possibility of switching to a closed slot
                    }
                    break;
                // OCCUPIED option doesnt exist
                default:
                    break;
            }
        }

        private void RemoveDropdownOption(string text)
        {
            try
            {
                TMP_Dropdown.OptionData asd = emptyDropdown.options.Single(option => option.text == text);
                emptyDropdown.options.Remove(asd);

            } catch (Exception) { /* Couldn't find a matching option. No problem. */ }
        }
        #endregion

        #region Endpoint - Recieving updates from model / network

        #region SlotModel event handlers

        private void OnPlayerChanged(object sender, bool isPlayerNotNull)
        {
            // This can only happen on server-side:
            nameFieldNetworkVar.Value = SlotModel.GetPlayerName();
            readinessDropdownNetworkVar.Value = SlotModel.IsPlayerReady() ? 1 : 0;
            statusLabelNetworkVar.Value = SlotModel.IsPlayerReady() ? READY_RICH_TEXT : NOT_READY_RICH_TEXT;
        }

        private void OnOccupantChange(object sender, EventArgs e)
        {
            emptyDropdownNetworkVar.Value = (int)SlotModel.OccupantStatus;
        }

        #endregion

        #region Network variable changes

        private void OnNameNetVarValueChanged(FixedString32Bytes oldvalue, FixedString32Bytes newvalue)
        {
            nameField.text = newvalue.ToString();
        }

        private void OnEmptyNetVarValueChanged(int oldValue, int newValue)
        {
            switch (newValue)
            {
                case 0: // OPEN
                    emptyDropdown.SetValueWithoutNotify(0);
                    emptyDropdown.gameObject.SetActive(true);
                    readinessDropdown.gameObject.SetActive(false);
                    statusLabel.gameObject.SetActive(false);
                    break;
                case 1: // RESERVED / SWITCH TO
                    // no need to do anything because this is not an important change from a displaying viewpont
                    break;
                case 2: // CLOSED / !!!
                    if (IsServer)
                    {
                        statusLabelNetworkVar.Value = CLOSED_RICH_TEXT;
                        emptyDropdown.SetValueWithoutNotify(2);
                    } else
                    {
                        emptyDropdown.gameObject.SetActive(false);
                        statusLabel.gameObject.SetActive(true);
                    }
                    break;
                case 3: // OCCUPIED / !!!
                    emptyDropdown.gameObject.SetActive(false);
                    if (IsOwner)
                    {
                        readinessDropdown.gameObject.SetActive(true);
                    } else
                    {
                        statusLabel.gameObject.SetActive(true);
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnReadyNetVarValueChanged(int oldValue, int newValue)
        {
            if (IsServer)
            {
                try
                {
                    SlotModel.PlayerData.IsReady = newValue == 1;
                }
                catch (Exception) 
                { /* This is here because upon spawning, the host tries to set this value, but it is not possible to initialize PlayerData before spawning */ }
            }
            if (IsOwner)
            {
                readinessDropdown.SetValueWithoutNotify(newValue);
            }
        }

        private void OnStatusNetVarValueChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
        {
            statusLabel.text = newValue.ToString();
        }

        #endregion

        #endregion

        public void ForceRefresh()
        {
            OnNameNetVarValueChanged(default, nameFieldNetworkVar.Value);
            OnEmptyNetVarValueChanged(default, emptyDropdownNetworkVar.Value);
            OnReadyNetVarValueChanged(default, readinessDropdownNetworkVar.Value);
            OnStatusNetVarValueChanged(default, statusLabelNetworkVar.Value);
            OnSlotColorNetVarValueChanged(default, slotColorNetworkVar.Value);
        }
    }
}
