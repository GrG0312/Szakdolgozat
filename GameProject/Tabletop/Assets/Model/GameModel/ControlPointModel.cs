using Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (contester is ISidedObject s)
            {
                if (didArrive)
                {
                    sideValues[s.Side] += contester.Constants.ObjectiveControl;
                } else
                {
                    sideValues[s.Side] -= contester.Constants.ObjectiveControl;
                }
                CalculateControl();
            }
        }

        public void CalculateControl()
        {
            int max = 0; // zero because there is no guarantee to have units capping the point
            int owner = -1;
            for (int i = 0; i < sideValues.Count; i++)
            {
                // If max is lower
                if (max < sideValues.ElementAt(i).Value)
                {
                    max = sideValues.ElementAt(i).Value;
                    owner = (int)sideValues.ElementAt(i).Key;
                }
            }

            if (max == 0)
            {
                owner = Owner;
            }

            Owner = owner;
        }
    }
}
