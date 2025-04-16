using Controllers.Data;
using Model;
using Model.Weapons;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.Objects.Game.WeaponSelection
{
    public class WeaponButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text nameText;

        public event EventHandler<WeaponIdentifier>? Clicked;

        public WeaponIdentifier Id { get; private set; }

        public void SetupValues(WeaponIdentifier id)
        {
            Id = id;
            nameText.text = Defines.Weapons[Id].Name;
        }

        private void Start()
        {
            button.onClick.AddListener(() => Clicked?.Invoke(this, Id));
        }
    }
}
