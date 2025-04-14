using Model.Units;

namespace Model.Interfaces
{
    public interface IUnit
    {
        /// <summary>
        /// The Unit's identifier
        /// </summary>
        public UnitIdentifier Identity { get; }
        /// <summary>
        /// Base values for the statistics of this unit
        /// </summary>
        public UnitConstants Constants { get; }

        /// <summary>
        /// Reset the unit's values to their original values at the beginning of a turn
        /// </summary>
        public void SetStartValues();
    }
}
