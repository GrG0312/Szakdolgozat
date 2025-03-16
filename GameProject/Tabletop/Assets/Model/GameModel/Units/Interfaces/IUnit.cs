namespace Model.Units.Interfaces
{
    public interface IUnit
    {
        /// <summary>
        /// Base values for the statistics of this unit
        /// </summary>
        public UnitStat Base { get; }
    }
}
