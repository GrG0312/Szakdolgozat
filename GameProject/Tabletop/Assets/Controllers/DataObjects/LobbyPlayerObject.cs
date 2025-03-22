using System;
using System.Linq;
using Model;
using Model.Lobby;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

namespace Controllers.DataObjects
{
    public class LobbyPlayerObject : NetworkBehaviour
    {
        #region Constants
        private const string READY_RICH_TEXT = "<color=green>Ready";
        private const string NOT_READY_RICH_TEXT = "<color=orange>Not Ready";
        private const string CLOSED_RICH_TEXT = "<color=red>Closed";
        #endregion

        #region Serializations and network variables
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
            SlotModel = new LobbySlot<ulong>(side);
            SlotModel.OccupantStatusChanged += OnOccupantChange;
            SlotModel.PlayerChanged += OnPlayerChanged;
        }
        #endregion

        #region Model
        public LobbySlot<ulong> SlotModel { get; private set; }
        #endregion

        #region Unity Messages
        private void Awake()
        {
            nameFieldNetworkVar.OnValueChanged += OnNameFieldValueChanged;
            emptyDropdownNetworkVar.OnValueChanged += OnEmptyDropdownValueChanged;
            readinessDropdownNetworkVar.OnValueChanged += OnReadyDropdownValueChanged;

            readinessDropdown.onValueChanged.AddListener(OnReadinessChangeInput);
            emptyDropdown.onValueChanged.AddListener(OnOccupantChangeInput);
        }
        #endregion

        #region Display - Host options, Owned logic
        private bool isHostDisplay;
        public void SetHostDisplay(bool isDisplayedOnHost)
        {
            isHostDisplay = isDisplayedOnHost;
            if (isDisplayedOnHost)
            {
                emptyDropdown.options.Add(new TMP_Dropdown.OptionData("Closed"));
            } else
            {
                try
                {
                    TMP_Dropdown.OptionData data = emptyDropdown.options.Single(data => data.text == "Closed");
                    emptyDropdown.options.Remove(data);
                }
                catch(InvalidOperationException) { /* No element was found with Close, no need to do anything */ }
            }
        }
        #endregion

        #region Startpoint - Calling controller methods
        public void OnReadinessChangeInput(int value)
        {
            if (IsServer)
            {
                readinessDropdownNetworkVar.Value = value; // 0 - not ready, 1 - ready
            } else
            {
                LobbyController.Instance.ClientReadyChange(value);
            }
        }
        private void OnOccupantChangeInput(int value)
        {
            switch (value)
            {
                case 0: // OPEN
                    if (IsServer)
                    {
                        SlotModel.OccupantStatus = SlotOccupantStatus.Open;
                        emptyDropdownNetworkVar.Value = (int)SlotModel.OccupantStatus;
                    }
                    break;
                case 1: // SWITCH / RESERVE
                    LobbyController.Instance.SwitchToSlot(SlotId);
                    break;
                case 2: // CLOSED
                    if (IsServer)
                    {
                        SlotModel.OccupantStatus = SlotOccupantStatus.Closed;
                        emptyDropdownNetworkVar.Value = (int)SlotModel.OccupantStatus;
                    }
                    break;
                // OCCUPIED option doesnt exist
                default:
                    Debug.LogError($"LobbyPlayerObject #{SlotId} : What are you on about?");
                    break;
            }
        }
        #endregion

        #region Endpoint - Recieving updates from model / network

        #region SlotModel parts

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

        #region Network parts

        private void OnNameFieldValueChanged(FixedString32Bytes oldvalue, FixedString32Bytes newvalue)
        {
            nameField.text = newvalue.ToString();
        }

        private void OnEmptyDropdownValueChanged(int oldValue, int newValue)
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

        private void OnReadyDropdownValueChanged(int oldValue, int newValue)
        {
            if (IsServer)
            {
                SlotModel.PlayerData.IsReady = newValue == 1;
            }
            if (IsOwner)
            {
                readinessDropdown.SetValueWithoutNotify(newValue);
            }
            else
            {
                statusLabel.text = newValue == 1 ? READY_RICH_TEXT : NOT_READY_RICH_TEXT;
            }
        }

        #endregion

        #endregion
    }
}
