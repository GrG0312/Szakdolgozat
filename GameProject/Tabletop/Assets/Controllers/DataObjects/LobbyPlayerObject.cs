using System;
using Model;
using Model.Lobby;
using TMPro;
using UnityEngine;

namespace Controllers.DataObjects
{
    public class LobbyPlayerObject : MonoBehaviour
    {
        #region Serializations
        [SerializeField] private TMP_Text nameField;
        [SerializeField] private TMP_Dropdown readinessDropdown;
        [SerializeField] private TMP_Dropdown emptyDropdown;
        [SerializeField] private Side slotSide;
        [field: SerializeField] public int SlotId { get; private set; }
        #endregion

        public LobbySlot SlotModel { get; private set; }

        private void OnReadinessChange(int value)
        {
            //SlotModel.SetReadiness(value == 1);
            // LobbyController instance
        }
        private void OnOccupantChange(int value)
        {
            // 0 : open
            // 1 : switch
            // 2 : closed
            switch (value)
            {
                case 0:
                case 2:
                    Debug.Log($"<color=orange>Trying to set Slot#{SlotId} status to {value}</color>");
                    try
                    {
                        int slotId = SlotId;
                        LobbyController.Instance.SetSlotStatus(slotId, value);
                    }
                    catch (Exception) { /* If exception, then caller was not host. Dont need to do anything. */ Debug.Log("<color=yellow>Tried to change slot occupant.</color>"); }
                    break;
                case 1:
                    LobbyController.Instance.SwitchToSlot(SlotId);
                    break;
                default:
                    break;
            }
        }

        private void Awake()
        {
            SlotModel = new LobbySlot(slotSide);
            SlotModel.SlotDataChanged += SlotModel_SlotDataChanged;

            readinessDropdown.onValueChanged.AddListener(OnReadinessChange);
            emptyDropdown.onValueChanged.AddListener(OnOccupantChange);
        }

        private void SlotModel_SlotDataChanged(object sender, EventArgs e)
        {
            nameField.text = SlotModel.DisplayName;
            Debug.Log($"<color=magenta>[Client] Slot#{SlotId} Occupant status: {SlotModel.OccupantStatus}");
            if (SlotModel.OccupantStatus.ToString() == "OpenModel")
            {
                // The slot is open
                emptyDropdown.SetValueWithoutNotify(0);
                SwitchDropdowns(true);
            } else
            {
                // The slot is closed
                emptyDropdown.SetValueWithoutNotify(2);
                if (SlotModel.DisplayName != string.Empty) // Only switch if a player is assigned
                {
                    SwitchDropdowns(false);
                }
            }
            readinessDropdown.SetValueWithoutNotify(SlotModel.IsReady ? 1 : 0);
        }

        private void SwitchDropdowns(bool param)
        {
            emptyDropdown.gameObject.SetActive(param);
            readinessDropdown.gameObject.SetActive(!param);
        }
    }
}
