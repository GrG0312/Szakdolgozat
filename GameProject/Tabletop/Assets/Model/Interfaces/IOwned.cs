namespace Model.Interfaces
{
    public interface IOwned<IdType>
    {
        public IdType Owner { get; }
    }
}
