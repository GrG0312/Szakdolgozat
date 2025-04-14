using Controllers.Data;
using Model;
using Model.Units;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Controllers.Objects.Game.InfoPanel
{
    public class UnitInfoPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text unitName;
        [SerializeField] private TMP_Text canMove;

        [SerializeField] private TMP_Text movement;
        [SerializeField] private TMP_Text armorsave;
        [SerializeField] private TMP_Text wound;
        [SerializeField] private TMP_Text objcontr;

        [SerializeField] private GameObject weaponContainer;
        [SerializeField] private WeaponPanel weaponPanelPrefab;

        private List<WeaponPanel> weaponPanels = new List<WeaponPanel>();

        public void UpdateValues(SelectedUnitData data)
        {
            UnitIdentifier id = (UnitIdentifier)data.UnitIdentifier;
            unitName.text = Defines.UnitVisuals[id].UnitName;
            canMove.text = data.CanUnitMove ? "<color=green>Yes</color>" : "<color=red>No</color>";
            movement.text = Defines.UnitValues[id].Movement.ToString();
            armorsave.text = Defines.UnitValues[id].ArmorSave.ToString() + "+";
            wound.text = $"{data.WoundNow}/{Defines.UnitValues[id].Wound}";
            objcontr.text = Defines.UnitValues[id].ObjectiveControl.ToString();
        }

        public void ClearWeapons()
        {
            foreach (WeaponPanel panel in weaponPanels)
            {
                Destroy(panel.gameObject);
            }
            weaponPanels.Clear();
        }

        public void AddWeapon(WeaponInfoData data)
        {
            WeaponPanel wp = Instantiate(weaponPanelPrefab, weaponContainer.transform);
            wp.UpdateValues(data);
            weaponPanels.Add(wp);
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }
    }
}
