using Model;
using Model.Units;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
    public class UnitAdder : MonoBehaviour
    {
        [SerializeField] private TMP_Text unitName;
        [SerializeField] private TMP_Text unitAmount;
        [SerializeField] private TMP_Text unitLimit;
        [SerializeField] private Image image;

        public UnitIdentifier Identity { get; private set; }
        public int UnitLimit 
        { 
            get { return int.Parse(unitLimit.text); }
            set
            {
                unitLimit.text = value.ToString();
            }
        }
        public int UnitAmount
        {
            get { return int.Parse(unitAmount.text); }
            set
            {
                unitAmount.text = value.ToString();
            }
        }

        public void Setup(UnitIdentifier i)
        {
            Identity = i;
            UnitVisualData data = Defines.UnitVisuals[Identity];
            unitName.text = data.UnitName;
            image.sprite = Resources.Load<Sprite>(data.UnitProfileSprite);
            unitAmount.text = "0";
            unitLimit.text = "99"; // something
        }
        public void AddUnit()
        {
            LobbyController.Instance.AddUnitToDeck_ServerRpc(NetworkManager.Singleton.LocalClientId, Identity);
        }
        public void RemoveUnit()
        {
            LobbyController.Instance.RemoveUnitFromDeck_ServerRpc(NetworkManager.Singleton.LocalClientId, Identity);
        }
    }
}
