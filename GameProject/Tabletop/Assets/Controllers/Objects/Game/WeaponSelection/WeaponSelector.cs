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

        public void AddButton(WeaponInfoData data)
        {
            if (!data.CanShoot)
            {
                return;
            }
            WeaponButton btn = Instantiate(weaponButtonPrefab, this.transform);
            btn.SetupValues(data);
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
            Debug.Log($"User clicked the button of {e}");
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            WeaponIdentifier id = e;
            GameController.Instance.AttackingWeaponSelected_ServerRpc(clientId, id);
            this.gameObject.SetActive(false);
        }


        private IEnumerator HideAtEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
            //
        }
    }
}
