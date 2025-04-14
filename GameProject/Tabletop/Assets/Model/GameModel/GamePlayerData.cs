using Model.Deck;
using Model.Units;
using Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model.GameModel
{
    public class GamePlayerData
    {
        #region Data coming from lobby
        public string Name { get; }
        public DeckObject Deck { get; }
        public Side Side { get; }
        #endregion

        #region Units in play

        private List<IUnit> unitsInPlay;
        public IReadOnlyList<IUnit> UnitsInPlay { get; }

        #endregion

        public bool IsDefeated { get; private set; }

        public GamePlayerData(string name, DeckObject obj, Side s)
        {
            this.Name = name;
            this.Deck = obj;
            this.Side = s;
            IsDefeated = false;

            Currency = 0;
            CapturedPoints = 0;

            unitsInPlay = new List<IUnit>();

            UnitsInPlay = unitsInPlay;
        }

        public void ResetUnits()
        {
            List<IUnit> toRemove = new List<IUnit>();
            foreach (IUnit unit in unitsInPlay)
            {
                if (unit is IDamagable d && !d.Alive)
                {
                    d.Delete();
                    toRemove.Add(unit);
                    Debug.Log($"<color=orange>Unit deleted</color>");
                } else
                {
                    unit.SetStartValues();
                }
            }
            foreach (IUnit unit in toRemove)
            {
                unitsInPlay.Remove(unit);
                Debug.Log($"<color=orange>Unit removed</color>");
            }
        }

        #region Cycle
        #endregion

        #region Points

        public event EventHandler? PointsChanged;

        private int currency;
        public int Currency 
        { 
            get => currency;
            private set
            {
                currency = value;
                PointsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private int cp;
        public int CapturedPoints 
        { 
            get => cp;
            private set
            {
                cp = value;
                PointsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int PointsGainedPerTurn { get { return Defines.POINTS_PER_TURN + CapturedPoints * Defines.POINTS_PER_CAP; } }

        public void AddPoints()
        {
            Currency += PointsGainedPerTurn;
        }
        public void AddPoints(int amount)
        {
            Currency += amount;
        }
        public void CapturePoint(bool isLost = false)
        {
            if (isLost)
            {
                CapturedPoints--;
            } else
            {
                CapturedPoints++;
            }
        }

        #endregion

        #region Unit purchase

        public bool BuyUnit(UnitIdentifier identity)
        {
            int price = Defines.UnitValues[identity].Price;
            DeckEntry entry = Deck.Entries.Single(entry => entry.TargetUnit == identity);
            if (Currency >= price && entry.Amount > 0)
            {
                Currency -= price;
                entry.Amount--;
                if (entry.Amount == 0)
                {
                    Deck.Entries.Remove(entry);
                }
                return true;
            }
            return false;
        }

        public void GetUnit(IUnit unit)
        {
            IDamagable d = unit as IDamagable;
            d.UnitDestroyed += UnitDestroyed;
            unitsInPlay.Add(unit);
        }

        #endregion

        private void UnitDestroyed(object sender, EventArgs e)
        {
            if (Deck.Entries.Count == 0 && unitsInPlay.Count == 0)
            {
                IsDefeated = true;
            }
        }

        public void Forfeit()
        {
            IsDefeated = true;
            foreach (IUnit unit in unitsInPlay)
            {
                IDamagable d = unit as IDamagable;
                d.Die();
            }
        }
    }
}
