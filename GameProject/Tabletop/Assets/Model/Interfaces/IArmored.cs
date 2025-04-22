using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Interfaces
{
    /// <summary>
    /// Represents an object which can roll dices to tank up incoming damage
    /// </summary>
    public interface IArmored
    {
        /// <summary>
        /// Using the <paramref name="roller"/> object, we will roll <paramref name="amount"/> dices and determine how many of the incoming hits we evaded
        /// </summary>
        /// <param name="amount">How many dices should be rolled</param>
        /// <param name="roller">The object which will be used for determining the rolls' results</param>
        /// <returns>The number of evaded attacks</returns>
        public Task<int> ArmorSave(int amount, int armorPiercing, IDiceRoller roller);
    }
}
