using Model.Units.Interfaces;
using Model.Units;

namespace Model.Deck
{
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
