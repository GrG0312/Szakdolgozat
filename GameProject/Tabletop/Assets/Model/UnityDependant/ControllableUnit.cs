using System;
using UnityEngine;
using Model.Units;
using Model.Units.Interfaces;
using Model.Weapons;

namespace Model.UnityDependant
{
    public class ControllableUnit : MonoBehaviour, 
        IUnit, ISelectable, IDamagable, IMovable<Vector3>, IWeaponUser
    {
        public int Owner { get; protected set; }
        public UnitConstants Base { get; protected set; }
        public int CurrentHP { get; protected set; }
        public Weapon ActiveWeapon { get; protected set; }

        public event EventHandler UnitDestroyed;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            CurrentHP = Base.MaxHP;
        }
        public void Move(Vector3 moveTarget)
        {
            
        }
        public int Damage(IDamagable damagable)
        {
            int caused = 0;
            // TODO roll dices, count results, deliver damage
            return caused;
        }
        public void Selected()
        {
            // TODO change color? display something around the unit?
        }
        public void TakeDamage(int amount)
        {
            // TODO play animation when taking damage?
            CurrentHP -= amount;
            if (CurrentHP <= 0)
            {
                UnitDestroyed.Invoke(this, EventArgs.Empty);
                // TODO die
                // Don't destroy the corpse right away, the object is still needed in case of reversing the order
            }
        }
    }
}