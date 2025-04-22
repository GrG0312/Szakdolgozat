using System;
using System.Threading.Tasks;

namespace Model.Interfaces
{
    /// <summary>
    /// An object that can be damaged.
    /// </summary>
    public interface IDamageable
    {

        public int CurrentHP { get; }

        public bool Alive { get; }

        /// <summary>
        /// Event that should fire when <see cref="CurrentHP"/> reaches / falls below zero.
        /// </summary>
        public event EventHandler UnitDestroyed;

        /// <summary>
        /// This method should be called when this unit takes damage / hit.
        /// </summary>
        /// <param name="amount">How much damage should be done</param>
        public void TakeDamage(int amount);

        /// <summary>
        /// A method which will be called once the <see cref="CurrentHP"/> reaches 0.
        /// </summary>
        public void Die();
    }

    /// <summary>
    /// An expanded version of the <see cref="IDamageable"/> interface. 
    /// This includes the <see cref="IMapObject{WorldPositionType}"/> interface, and with that access to the object's position.
    /// </summary>
    public interface IDamageable<WorldPositionType> : IDamageable, IMapObject<WorldPositionType> { }
}
