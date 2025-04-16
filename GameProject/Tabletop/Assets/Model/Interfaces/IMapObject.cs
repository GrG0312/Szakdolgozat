using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Interfaces
{

    /// <summary>
    /// An interface for objects which can be played on the map in-game.
    /// </summary>
    /// <typeparam name="WorldPositionType">The type used by the implementing platform to identify a position in the world</typeparam>
    public interface IMapObject<WorldPositionType>
    {

        /// <summary>
        /// The current position of the object
        /// </summary>
        public WorldPositionType Position { get; set; }

        /// <summary>
        /// How far is this object from the <paramref name="other"/> <see cref="IMapObject{WorldPositionType}"/> object
        /// </summary>
        /// <returns>The distance as <see cref="float"/></returns>
        public float DistanceTo(IMapObject<WorldPositionType> other);
    }
}
