using Model.Weapons;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Controllers.Data;
using System.Collections;

namespace Controllers.Objects.Game.WeaponSelection
{
    public class WeaponSelector : MonoBehaviour
    {
        [SerializeField] private WeaponButton weaponButtonPrefab;

        private List<WeaponButton> buttons = new List<WeaponButton>();

        public void Start()
        {
            this.gameObject.SetActive(false);
        }

        public void AddButton(WeaponIdentifier id)
        {
            WeaponButton btn = Instantiate(weaponButtonPrefab, this.transform);
            btn.SetupValues(id);
            btn.Clicked += ButtonClicked;
            buttons.Add(btn);
        }

        public void ClearButtons()
        {
            foreach (WeaponButton btn in buttons)
            {
                Destroy(btn.gameObject);
            }
            buttons.Clear();
        }

        private void ButtonClicked(object sender, WeaponIdentifier e)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            WeaponIdentifier id = e;
            GameController.Instance.AttackingWeaponSelected_ServerRpc(clientId, id);
            this.gameObject.SetActive(false);
        }
    }
}
