namespace Model.Interfaces
{
    /// <summary>
    /// Represents an object that belongs to a certain Side
    /// </summary>
    public interface ISidedObject
    {
        public Side Side { get; }
    }
}
