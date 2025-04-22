using Model.Units;

namespace Model.Interfaces
{
    /// <summary>
    /// A factory object that is used to produce classes of units
    /// </summary>
    /// <typeparam name="T">The type of the owner of units</typeparam>
    public interface IUnitFactory<T>
    {
        /// <summary>
        /// Creates an instance of a unit from the given parameters
        /// </summary>
        public IUnit Produce(T owner, UnitIdentifier identity, Side s);
    }
}
