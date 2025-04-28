using Model.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Controllers.Objects.Game.Purchase
{
    public class PurchaserWindow : MonoBehaviour
    {
        [SerializeField] private GameObject content;
        [SerializeField] private UnitPurchaser purchaserPrefab;

        private List<UnitPurchaser> purchasers;

        public void AddPurchaser(UnitIdentifier id, int amount, int price)
        {
            if (purchasers == null)
            {
                purchasers = new List<UnitPurchaser>();
            }
            UnitPurchaser p = Instantiate(purchaserPrefab);
            Sprite sprite = Resources.Load<Sprite>(ViewDefines.UnitVisuals[id].UnitProfileSprite);
            p.Setup(id, sprite, amount, price);
            purchasers.Add(p);
            p.transform.SetParent(content.transform);
        }

        public void UpdatePurchaser(UnitIdentifier id, int amount)
        {
            UnitPurchaser pur = purchasers.Single(p => p.Identity == id);
            pur.Amount = amount;
        }

        public void RemovePurchaser(UnitIdentifier id)
        {
            UnitPurchaser pur = purchasers.Single(p => p.Identity == id);
            purchasers.Remove(pur);
            Destroy(pur.gameObject);
        }
    }
}
