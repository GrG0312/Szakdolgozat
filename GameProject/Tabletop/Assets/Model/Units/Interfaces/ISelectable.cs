namespace Model.Units.Interfaces
{
    public interface ISelectable
    {
        /// <summary>
        /// Contains logic for when a unit is selected.
        /// </summary>
        public void Selected();
    }
    public interface ISelectable<IdType> : IOwned<IdType>, ISelectable { }
}
