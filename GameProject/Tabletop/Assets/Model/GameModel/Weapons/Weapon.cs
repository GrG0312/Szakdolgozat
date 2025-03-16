namespace Model.Weapons
{
    public class Weapon
    {
        protected Weapon(int hit, int pwr, int rng, int fr, ArmoryType pen)
        {
            Hit = hit;
            Power = pwr;
            Range = rng;
            Pen = pen;
        }

        #region Statistics
        /// <summary>
        /// The minimum roll needed on a D6 for the usage to be effective.
        /// </summary>
        public int Hit { get; }

        /// <summary>
        /// Determines the result of a successful use.
        /// </summary>
        public int Power { get; }

        /// <summary>
        /// How far can the weapon reach.
        /// </summary>
        public int Range { get; }

        /// <summary>
        /// How many times you should roll when using the weapon
        /// </summary>
        public int FireRate { get; }

        /// <summary>
        /// Armor penetration value. Can pen units whose armor is at max the specified amount.
        /// </summary>
        public ArmoryType Pen { get; }
        #endregion

        #region Presets
        public static Weapon Rifle = new Weapon(hit: 4, pwr: 5, 10, fr: 3, ArmoryType.None);
        public static Weapon Smg = new Weapon(hit: 3, pwr: 3, 6, fr: 6, ArmoryType.None);
        public static Weapon AT = new Weapon(hit: 5, pwr: 10, 6, fr: 1, ArmoryType.Heavy);

        public static Weapon TankCannon = new Weapon(12, 12, 20, 1, ArmoryType.Heavy);
        #endregion
    }
}