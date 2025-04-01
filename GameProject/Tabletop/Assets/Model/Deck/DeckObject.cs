using System.Collections.Generic;
using System.Linq;
using Model.Units;

namespace Model.Deck
{
    public class DeckObject
    {
        public List<DeckEntry> Entries { get; private set; }

        public DeckObject()
        {
            Entries = new List<DeckEntry>();
        }

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

        public int Remove(UnitIdentifier identity)
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

        public void Reset()
        {
            Entries.Clear();
        }
    }
}
