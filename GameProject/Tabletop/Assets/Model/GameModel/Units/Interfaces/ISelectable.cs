namespace Model.Units.Interfaces
{
    public interface ISelectable
    {
        public int Owner { get; }
        /// <summary>
        /// Contains logic for when a unit is selected.
        /// </summary>
        public void Selected();
    }
}
