using Model.Weapons;
using System.Collections.ObjectModel;

namespace Model.Units
{
    /// <summary>
    /// This class holds the universal values of each unit
    /// </summary>
    public class UnitStat
    {
        protected UnitStat(int distance, int hp, Weapon[] weapons) { MoveDistance = distance; MaxHP = hp; Weapons = new ReadOnlyCollection<Weapon>(weapons); }

        #region Statistics
        /// <summary>
        /// How far the Unit can travel at most in a phase.
        /// </summary>
        public int MoveDistance { get; }
        /// <summary>
        /// The maximum hitpoints this unit can have.
        /// </summary>
        public int MaxHP { get; }
        /// <summary>
        /// This unit can only be damaged by weapons that have at least this much penetration.
        /// </summary>
        public ArmoryType Armor { get; }
        /// <summary>
        /// Collection of weapons that this unit has
        /// </summary>
        public ReadOnlyCollection<Weapon> Weapons { get; }
        #endregion

        public static UnitStat Rifleman = new UnitStat(10, 100, new Weapon[] { Weapon.Rifle });
        public static UnitStat Sturmman = new UnitStat(8, 150, new Weapon[] { Weapon.Smg, Weapon.AT });
        public static UnitStat Tank = new UnitStat(35, 200, new Weapon[] { Weapon.TankCannon });
    }
}
