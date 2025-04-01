using Model.Weapons;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Model.Units
{
    /// <summary>
    /// This class holds the universal values that the units will have.
    /// </summary>
    public class UnitConstants
    {
        public UnitConstants(
            Side side,
            UnitType type,
            int limit,
            int price,
            int mov, 
            int hp,
            ArmoryType armor,
            ISet<Weapon> weapons) 
        {
            Side = side;
            Type = type;
            LimitInDeck = limit;
            Price = price;
            Movement = mov; 
            MaxHP = hp; 
            Armor = armor;
            Weapons = new HashSet<Weapon>(weapons);
        }

        #region Values
        public UnitType Type { get; }
        /// <summary>
        /// How many of this unit can be maximum in a deck
        /// </summary>
        public int LimitInDeck { get; }
        /// <summary>
        /// How much does it cost to purchase this unit
        /// </summary>
        public int Price { get; }
        /// <summary>
        /// How far the Unit can travel at most in a phase.
        /// </summary>
        public int Movement { get; }
        /// <summary>
        /// The maximum hitpoints this unit can have.
        /// </summary>
        public int MaxHP { get; }
        /// <summary>
        /// This unit can only be damaged by weapons that have at least this much penetration.
        /// </summary>
        public ArmoryType Armor { get; }
        /// <summary>
        /// Which side the unit is available for.
        /// </summary>
        public Side Side { get; }
        /// <summary>
        /// Collection of weapons that this unit has
        /// </summary>
        public ISet<Weapon> Weapons { get; }
        #endregion
    }
}
