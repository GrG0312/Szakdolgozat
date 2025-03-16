using System;
using System.Collections.Generic;
using System.Linq;
namespace Model.Units.Interfaces
{
    /// <summary>
    /// Represents an object which can move in the world
    /// </summary>
    /// <typeparam name="WorldPositionType">The type used by the implementing platform to identify a position in the world</typeparam>
    public interface IMovable<WorldPositionType>
    {
        /// <summary>
        /// Executes movement to the targeted position
        /// </summary>
        /// <param name="targetPosition">Where to move to</param>
        public void Move(WorldPositionType targetPosition);
    }
}
