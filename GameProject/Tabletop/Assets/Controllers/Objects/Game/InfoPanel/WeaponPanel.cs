using Controllers.Data;
using Model;
using Model.Weapons;
using TMPro;
using UnityEngine;

namespace Controllers.Objects.Game.InfoPanel
{
    public class WeaponPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameField;
        [SerializeField] private TMP_Text range;
        [SerializeField] private TMP_Text attacks;
        [SerializeField] private TMP_Text ballistics;
        [SerializeField] private TMP_Text ap;
        [SerializeField] private TMP_Text damage;
        [SerializeField] private TMP_Text canUse;

        public void UpdateValues(WeaponInfoData data)
        {
            WeaponIdentifier id = (WeaponIdentifier)data.WeaponIdentifier;
            nameField.text = Defines.Weapons[id].Name;
            if (data.Count > 1)
            {
                nameField.text += $"(x{data.Count})";
            }
            range.text = Defines.Weapons[id].Range.ToString();
            ballistics.text = Defines.Weapons[id].BallisticSkill.ToString() + "+";
            attacks.text = Defines.Weapons[id].Attacks.ToString();
            ap.text = Defines.Weapons[id].ArmorPiercing.ToString();
            damage.text = Defines.Weapons[id].Damage.ToString();
            canUse.text = data.CanShoot ? "<color=green>Yes</color>" : "<color=red>No</color>";
        }
    }
}
