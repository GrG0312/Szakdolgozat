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
        private const string READY_RICH_TEXT = "<color=green>Ready</color>";
        private const string NOT_READY_RICH_TEXT = "<color=orange>Not Ready</color>";
        private const string CLOSED_RICH_TEXT = "<color=red>Closed</color>";
        #endregion

        #region Serializations and network variables
        [SerializeField] private TMP_Text nameField;
        public NetworkVariable<FixedString32Bytes> nameFieldValue = 
            new NetworkVariable<FixedString32Bytes>(string.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [SerializeField] private TMP_Dropdown readinessDropdown;
        public NetworkVariable<int> readinessDropDownValue =
            new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<bool> readinessDropDownVisibility =
            new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [SerializeField] private TMP_Dropdown emptyDropdown;
        public NetworkVariable<int> emptyDropDownValue =
            new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<bool> emptyDropDownVisibility =
            new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [SerializeField] private TMP_Text statusLabel;
        public NetworkVariable<FixedString32Bytes> statusLabelValue =
            new NetworkVariable<FixedString32Bytes>(string.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<bool> statusLabelVisibility =
            new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        #endregion

        #region Initial data setup
        public void SetupInitialData(int id, Side side)
        {
            SlotModel = new LobbySlot<ulong>(id, side);
            SlotModel.SlotDataChanged += SlotModel_SlotDataChanged;

            readinessDropdown.onValueChanged.AddListener(OnReadinessChange);
            emptyDropdown.onValueChanged.AddListener(OnOccupantChange);
        }
        #endregion

        #region Model
        public LobbySlot<ulong> SlotModel { get; private set; }
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
        private void OnReadinessChange(int value)
        {
            
        }
        private void OnOccupantChange(int value)
        {
            switch (value)
            {
                case 0: // open
                case 2: // closed
                    Debug.Log($"<color=orange>Trying to set Slot#{SlotModel.SlotId} status to {value}</color>");
                    try
                    {
                        int slotId = SlotModel.SlotId;
                        LobbyController.Instance.SetSlotStatus(slotId, value);
                    }
                    catch (Exception) { /* If exception, then caller was not host. Dont need to do anything. */ Debug.Log("<color=yellow>Tried to change slot occupant.</color>"); }
                    break;
                case 1: // switch
                    LobbyController.Instance.SwitchToSlot(SlotModel.SlotId);
                    break;
                default:
                    Debug.LogError($"LobbyPlayerObject #{SlotModel.SlotId} : What are you on about?");
                    break;
            }
        }
        #endregion

        #region Unity Messages

        #endregion

        #region Endpoint - Recieving updates from model / controller
        private void SlotModel_SlotDataChanged(object sender, EventArgs e)
        {
            nameField.text = SlotModel.PlayerData.Name;
            Debug.Log($"<color=orange>[Client] Slot#{SlotModel.SlotId} Occupant status: {SlotModel.OccupantStatus}");
            if (SlotModel.OccupantStatus.ToString() == "Open")
            {
                // The slot is open
                emptyDropdown.SetValueWithoutNotify(0);
                SwitchDropdowns(true);
            } else
            {
                // The slot is closed
                emptyDropdown.SetValueWithoutNotify(2);
                if (SlotModel.PlayerData.Name != string.Empty) // Only switch if a player is assigned
                {
                    SwitchDropdowns(false);
                }
            }
            readinessDropdown.SetValueWithoutNotify(SlotModel.IsPlayerReady() ? 1 : 0);
        }
        private void SwitchDropdowns(bool param)
        {
            emptyDropdown.gameObject.SetActive(param);
            readinessDropdown.gameObject.SetActive(!param);
        }
        #endregion
    }
}
