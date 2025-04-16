using Model.Deck;
using Model.Interfaces;
using Model.Units;
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

        #region Units

        private List<IUnit> toBeDestroyed = new List<IUnit>();

        private List<IUnit> unitsInPlay;
        public IReadOnlyList<IUnit> UnitsInPlay { get; }

        #endregion

        public bool IsDefeated { get; private set; }
        public bool IsConnected { get; private set; }

        public GamePlayerData(string name, DeckObject obj, Side s)
        {
            this.Name = name;
            this.Deck = obj;
            this.Side = s;
            IsDefeated = false;
            IsConnected = true;

            Currency = 0;
            CapturedPoints = 0;

            unitsInPlay = new List<IUnit>();

            UnitsInPlay = unitsInPlay;
        }

        public void ResetUnits()
        {
            foreach (IUnit unit in unitsInPlay)
            {
                if (unit is IDamageable d && !d.Alive)
                {
                    toBeDestroyed.Add(unit);
                } else
                {
                    unit.SetStartValues();
                }
            }
            DeleteUnits();
        }

        public void DeleteUnits(bool forceDelete = false)
        {
            foreach (IUnit unit in toBeDestroyed)
            {
                if (unitsInPlay.Contains(unit))
                {
                    unitsInPlay.Remove(unit);
                }
                IDamageable d = unit as IDamageable;
                d.Delete();
            }
            toBeDestroyed.Clear();

            if (forceDelete)
            {
                foreach (IUnit unit in unitsInPlay)
                {
                    IDamageable d = unit as IDamageable;
                    d.Delete();
                }
                unitsInPlay.Clear();
            }
        }

        #region Cycle

        public IUnit? Cycle(Phase phase, IUnit selected)
        {
            int selectedIndex = 0;
            bool found = false;
            while (!found && selectedIndex < unitsInPlay.Count)
            {
                if (selected == unitsInPlay[selectedIndex])
                {
                    found = true;
                } else
                {
                    selectedIndex++;
                }
            }

            if (!found)
            {
                throw new Exception("Could not find selected unit in the list!");
            }

            if (CollectionHelper.LoopbackSearch(
                unitsInPlay, 
                unit => unit is IUsable u && u.IsUsable(phase) && unit is IDamageable d && d.Alive, 
                selectedIndex, 
                out int foundIndex))
            {
                return unitsInPlay[foundIndex];
            } else
            {
                return null;
            }
        }

        public IUnit? Cycle(Phase phase)
        {
            for (int i = 0; i < unitsInPlay.Count; i++)
            {
                if (unitsInPlay[i] is IUsable u && u.IsUsable(phase) && unitsInPlay[i] is IDamageable d && d.Alive)
                {
                    return unitsInPlay[i];
                }
            }
            return null;
        }

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

        #region Units

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
            IDamageable d = unit as IDamageable;
            d.UnitDestroyed += UnitDestroyed;
            unitsInPlay.Add(unit);
        }

        private void UnitDestroyed(object sender, EventArgs e)
        {
            if (Deck.Entries.Count == 0 && unitsInPlay.Count(u => u is IDamageable d && d.Alive) == 0)
            {
                IsDefeated = true;
            }
        }

        #endregion

        public void Forfeit()
        {
            IsDefeated = true;
            IsConnected = false;
            foreach (IUnit unit in unitsInPlay)
            {
                IDamageable d = unit as IDamageable;
                d.Die();
            }
        }
    }
}
