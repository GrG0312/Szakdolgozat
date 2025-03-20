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
            SlotModel.IsReady = value == 1;
        }
        private void OnOccupantChange(int value)
        {
            Debug.LogWarning("Not implemented yet");
        }

        private void Awake()
        {
            SlotModel = new LobbySlot(slotSide);
            SlotModel.SlotDataChanged += SlotModel_SlotDataChanged;
            SlotModel.SlotReadinessChanged += SlotModel_SlotReadinessChanged;

            readinessDropdown.onValueChanged.AddListener(OnReadinessChange);
            emptyDropdown.onValueChanged.AddListener(OnOccupantChange);
        }

        private void SlotModel_SlotReadinessChanged(object sender, EventArgs e)
        {
            readinessDropdown.value = SlotModel.IsReady ? 1 : 0;
        }

        private void SlotModel_SlotDataChanged(object sender, EventArgs e)
        {
            nameField.text = SlotModel.DisplayName;
            if (SlotModel.OccupantStatus.ToString() == "Open")
            {
                // The slot is open
                emptyDropdown.value = 0;
                SwitchDropdowns(true);
            } else
            {
                // The slot is closed
                emptyDropdown.value = 2;
                SwitchDropdowns(false);
            }
        }

        private void SwitchDropdowns(bool param)
        {
            emptyDropdown.gameObject.SetActive(param);
            readinessDropdown.gameObject.SetActive(!param);
        }
    }
}
