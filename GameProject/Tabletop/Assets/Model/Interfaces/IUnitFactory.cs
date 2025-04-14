using Model.Units;

namespace Model.Interfaces
{
    public interface IUnitFactory<T>
    {
        public IUnit Produce(T owner, UnitIdentifier identity, Side s);
    }
}
