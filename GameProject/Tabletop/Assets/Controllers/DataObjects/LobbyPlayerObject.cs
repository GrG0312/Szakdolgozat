using Controllers.Data;
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

        }

        private void Awake()
        {
            SlotModel = new LobbySlot(slotSide);
            readinessDropdown.onValueChanged.AddListener(OnReadinessChange);
            emptyDropdown.onValueChanged.AddListener(OnOccupantChange);
        }
    }
}
