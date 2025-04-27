using Model.Units;

namespace Model.Interfaces
{
    /// <summary>
    /// Represents a Unit of the game. The Unit has an Identifier and a Constants class which stores it's default values.
    /// </summary>
    public interface IUnit
    {
        /// <summary>
        /// The Unit's identifier.
        /// </summary>
        public UnitIdentifier Identity { get; }

        /// <summary>
        /// Base values of this unit.
        /// </summary>
        public UnitConstants Constants { get; }

        /// <summary>
        /// Reset the unit's values to their original values at the beginning of a turn.
        /// </summary>
        public void ResetToStartValues();
    }
}
