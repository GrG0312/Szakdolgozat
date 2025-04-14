using System.Threading.Tasks;

namespace Model.Interfaces
{
    /// <summary>
    /// Implementing objects can damage other, <see cref="IDamagable"/> objects
    /// </summary>
    public interface IDamageDealer
    {
        /// <summary>
        /// True if the object can deal damage. Used to track wether a weapon has been used in this turn or not
        /// </summary>
        public bool CanDamage { get; }
        /// <summary>
        /// Rolling the dices and based on the result damage the target with appropiate amount of points
        /// </summary>
        /// <returns>The caused damage</returns>
        public Task<int> Damage(IDamagable target, IDiceRoller roller);
    }
}
