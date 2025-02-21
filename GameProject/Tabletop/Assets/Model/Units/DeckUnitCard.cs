using Model.Units.Interfaces;

namespace Model.Units
{
    public class DeckUnitCard
    {
        protected DeckUnitCard(IUnit target, int price, int amount) { TargetUnit = target; Price = price; Amount = amount; }
        /// <summary>
        /// How many points it costs to spawn in a unit
        /// </summary>
        public int Price { get; }
        /// <summary>
        /// How many units can you spawn in with one card
        /// </summary>
        public int Amount { get; set; }
        /// <summary>
        /// The unit that you can spawn in with this card
        /// </summary>
        public IUnit TargetUnit { get; }
    }
}
