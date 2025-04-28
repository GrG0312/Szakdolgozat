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
    }
}
