using Model.Units;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.Objects.Game.Purchase
{
    public class UnitPurchaser : MonoBehaviour
    {
        [SerializeField] private TMP_Text amount;
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text price;
        [SerializeField] private Button button;

        #region Fields

        public UnitIdentifier Identity { get; private set; }
        public Sprite ImageSource
        {
            get { return image.sprite; }
            private set { image.sprite = value; }
        }

        public int Amount
        {
            get { return int.Parse(amount.text); }
            set { amount.text = value.ToString(); }
        }

        public int Price
        {
            get { return int.Parse(price.text); }
            private set { price.text = value.ToString(); }
        }
        
        #endregion
        public void Setup(UnitIdentifier id, Sprite sprite, int amount, int price)
        {
            Identity = id;
            ImageSource = sprite;
            Amount = amount;
            Price = price;
        }

        private void PurchaseBegin()
        {
            GameController.Instance.PurchaseUnitBegin_ServerRpc(NetworkManager.Singleton.LocalClientId, Identity);
        }

        private void Start()
        {
            
            button.onClick.AddListener(PurchaseBegin);
        }
    }
}
