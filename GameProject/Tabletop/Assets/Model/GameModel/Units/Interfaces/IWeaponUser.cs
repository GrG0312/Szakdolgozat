using Model.Weapons;

namespace Model.Units.Interfaces
{
    /// <summary>
    /// Implementing objects have an active / selected weapon, and can damage other, <see cref="IDamagable"/> objects
    /// </summary>
    public interface IWeaponUser
    {
        /// <summary>
        /// Currently selected weapon which will be used if attacking an enemy
        /// </summary>
        public Weapon ActiveWeapon { get; }
        /// <summary>
        /// Rolling the dices and based on the result damage the target with appropiate amount of points
        /// </summary>
        /// <returns>The caused damage</returns>
        public int Damage(IDamagable target);
    }
}
