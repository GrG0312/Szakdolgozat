using Model.Units;

namespace Model.Weapons
{
    public class Weapon
    {
        #region Statistics
        /// <summary>
        /// The minimum roll needed on a dice for the usage to be effective.
        /// </summary>
        public int HitMinimum { get; }

        /// <summary>
        /// Determines the result of a successful use.
        /// </summary>
        public int Damage { get; }

        /// <summary>
        /// How far can the weapon reach.
        /// </summary>
        public int Range { get; }

        /// <summary>
        /// How many times you should roll when using the weapon
        /// </summary>
        public int RollNumber { get; }

        /// <summary>
        /// Armor penetration value. Can pen units whose armor is at max the specified amount.
        /// </summary>
        public ArmoryType Penetration { get; }
        #endregion

        public Weapon(int hitMinimum, int damage, int range, int rollNumber, ArmoryType pen)
        {
            HitMinimum = hitMinimum;
            Damage = damage;
            Range = range;
            RollNumber = rollNumber;
            Penetration = pen;
        }
    }
}