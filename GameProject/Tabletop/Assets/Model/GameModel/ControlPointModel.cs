using Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model.GameModel
{
#nullable enable
    /// <summary>
    /// Model class for a capturable point on the map. 
    /// Keeps track of, and determines it's owner based based on the entered units' ObjectiveControl value.
    /// </summary>
    public class ControlPointModel
    {
        // Integer because if I'd add an extra value to the enum then it would mess up the loops
        private int owner;
        public int Owner
        {
            get => owner;
            set
            {
                int oldValue = owner;
                owner = value;
                OwnerChanged?.Invoke(this, oldValue);
            }
        }

        public event EventHandler<int>? OwnerChanged;

        private Dictionary<Side, int> sideValues;

        public ControlPointModel()
        {
            owner = -1; // -1 means noone owns the point
            sideValues = new Dictionary<Side, int>();
            foreach (Side item in Enum.GetValues(typeof(Side)))
            {
                sideValues.Add(item, 0);
            }
        }

        public void ContesterChanged(IUnit contester, bool didArrive)
        {
            Debug.Log($"<color=aqua>Contester changed, did arrive? {didArrive}</color>");
            if (contester is ISidedObject s)
            {
                if (didArrive)
                {
                    Debug.Log($"<color=aqua>Side's value before the adding: {sideValues[s.Side]}</color>");
                    sideValues[s.Side] += contester.Constants.ObjectiveControl;
                    Debug.Log($"<color=aqua>Side's value after the adding: {sideValues[s.Side]}</color>");
                } else
                {
                    Debug.Log($"<color=aqua>Side's value before the subtracting: {sideValues[s.Side]}</color>");
                    sideValues[s.Side] -= contester.Constants.ObjectiveControl;
                    Debug.Log($"<color=aqua>Side's value after the subtracting: {sideValues[s.Side]}</color>");
                }
                CalculateControl();
            }
        }

        public void CalculateControl()
        {
            Debug.Log($"<color=aqua>Current owner: {Owner}</color>");
            int max = 0; // zero because there is no guarantee to have units capping the point
            int owner = -1;
            for (int i = 0; i < sideValues.Count; i++)
            {
                Debug.Log($"<color=aqua>Now evaluating: {sideValues.ElementAt(i).Key}</color>");
                // If max is lower
                if (max < sideValues.ElementAt(i).Value)
                {
                    Debug.Log($"<color=aqua>Currently selected max: {sideValues.ElementAt(i).Key}</color>");
                    max = sideValues.ElementAt(i).Value;
                    owner = (int)sideValues.ElementAt(i).Key;
                }
            }

            if (max == 0)
            {
                owner = Owner;
            }

            Debug.Log($"<color=aqua>Changed owner: {owner}</color>");
            Owner = owner;
        }
    }
}
