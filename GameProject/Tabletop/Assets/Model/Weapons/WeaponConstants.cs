using Model.Units;

namespace Model.Weapons
{
    public class WeaponConstants
    {
        public string Name { get; }

        #region Statistics
        
        /// <summary>
        /// How far can the weapon reach.
        /// </summary>
        public int Range { get; }

        /// <summary>
        /// How many times you should roll when using the weapon
        /// </summary>
        public int Attacks { get; }
        
        /// <summary>
        /// The minimum roll needed on a dice for the usage to be effective.
        /// </summary>
        public int BallisticSkill { get; }

        /// <summary>
        /// This is a modifier applied to the target's Armor Save value. 1 means -1 is needed to score a successful hit.
        /// </summary>
        public int ArmorPiercing { get; }

        /// <summary>
        /// How much damage does 1 hit deal
        /// </summary>
        public int Damage { get; }
        #endregion

        public WeaponConstants(string name, int range, int attacks, int ballistics, int armorPiercing, int damage)
        {
            Name = name;
            Range = range;
            Attacks = attacks;
            BallisticSkill = ballistics;
            ArmorPiercing = armorPiercing;
            Damage = damage;
        }
    }
}