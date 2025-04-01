using System;

namespace Model.Units.Interfaces
{
    public interface IDamagable
    {
        /// <summary>
        /// Current HP of this unit
        /// </summary>
        public int CurrentHP { get; }
        /// <summary>
        /// Event that should fire when <see cref="CurrentHP"/> reaches / falls below zero.
        /// </summary>
        public event EventHandler UnitDestroyed;
        /// <summary>
        /// This method should be called when this unit takes damage / hit.
        /// </summary>
        /// <param name="amount">How much - true - damage should be done</param>
        public void TakeDamage(int amount);
    }
    public interface IDamagable<IdType> : IOwned<IdType>, IDamagable { }
}
