using Model.Units;

namespace Model.Deck
{
    /// <summary>
    /// An entry for a certain unit. 
    /// Only one entry will should exist for one unit because this includes the <see cref="Amount"/> property,
    /// which tells how many of this unit have been added to the deck.
    /// </summary>
    public class DeckEntry
    {
        public DeckEntry(UnitIdentifier target) 
        { 
            TargetUnit = target; 
            Amount = 0;
            Constants = Defines.UnitValues[target];
        }
        /// <summary>
        /// How many units can you spawn in with one card
        /// </summary>
        public int Amount { get; set; }
        /// <summary>
        /// The unit that you can spawn in with this card
        /// </summary>
        public UnitIdentifier TargetUnit { get; }
        /// <summary>
        /// Constant values for this unit, stored in Defines
        /// </summary>
        public UnitConstants Constants { get; }
    }
}
