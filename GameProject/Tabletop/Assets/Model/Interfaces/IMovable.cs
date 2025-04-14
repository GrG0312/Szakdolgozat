namespace Model.Interfaces
{
    /// <summary>
    /// Represents an object which can move in the world
    /// </summary>
    /// <typeparam name="WorldPositionType">The type used by the implementing platform to identify a position in the world</typeparam>
    public interface IMovable<WorldPositionType>
    {
        public bool CanMove { get; set; }
        /// <summary>
        /// The current position of the object.
        /// </summary>
        public WorldPositionType Position { get; set; }
        /// <summary>
        /// Executes movement to the targeted position
        /// </summary>
        /// <param name="targetPosition">Where to move to</param>
        public void MoveTo(WorldPositionType targetPosition);
    }
}
