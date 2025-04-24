using System.Collections.Generic;
using System.Linq;
using Model.Units;

namespace Model.Deck
{
    /// <summary>
    /// An object that represent a single deck. Contains the entries of units and implements logic for modifying the storing list.
    /// </summary>
    public class DeckObject
    {
        public List<DeckEntry> Entries { get; private set; }

        public DeckObject()
        {
            Entries = new List<DeckEntry>();
        }

        /// <summary>
        /// Checks if it is possible to add an other unit to the deck. 
        /// If so, then creates a new entry if the unit is not already added,
        /// or increments the existing entry's <see cref="DeckEntry.Amount"/> property.
        /// </summary>
        /// <returns>The final number of units</returns>
        public int Add(UnitIdentifier identity)
        {
            // Lin Search
            int index = 0;
            while (index < Entries.Count && Entries[index].TargetUnit != identity)
            {
                index++;
            }

            DeckEntry entry;
            // If found, then use its value
            if (index < Entries.Count)
            {
                entry = Entries[index];
            }
            // If not found, insert a new one
            else
            {
                entry = new DeckEntry(identity);
                Entries.Add(entry);
            }

            // If the unit fits
            if (Defines.UnitValues[identity].LimitInDeck > entry.Amount)
            {
                entry.Amount++;
            }
            return entry.Amount;
        }

        /// <summary>
        /// Tries to remove a unit from the deck.
        /// If the deck contains the unit, it decrements the existing entry's <see cref="DeckEntry.Amount"/> property.
        /// Otherwise returns zero.
        /// </summary>
        /// <returns>The remaining number of units in the deck.</returns>
        public int RemoveOne(UnitIdentifier identity)
        {
            // Lin Search
            int index = 0;
            while (index < Entries.Count && Entries[index].TargetUnit != identity)
            {
                index++;
            }

            // If we found the matching entry, we should decrease the amount
            if (index < Entries.Count)
            {
                Entries[index].Amount--;
                if (Entries[index].Amount == 0)
                {
                    Entries.RemoveAt(index);
                    return 0;
                }
                return Entries[index].Amount;
            }
            // If there is none, then all good, return zero
            else
            {
                return 0;
            }
        }

        public void Clear()
        {
            Entries.Clear();
        }

        public bool IsEmpty()
        {
            return Entries.Count == 0;
        }
    }
}
