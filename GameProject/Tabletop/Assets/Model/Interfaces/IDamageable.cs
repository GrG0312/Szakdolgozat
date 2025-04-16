using System;
using System.Threading.Tasks;

namespace Model.Interfaces
{
    public interface IDamageable : ISidedObject
    {
        /// <summary>
        /// Current HP of this unit
        /// </summary>
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

        public void Die();

        public void Delete();
    }

    public interface IDamageable<WorldPositionType> : IDamageable, IMapObject<WorldPositionType> { }
}
