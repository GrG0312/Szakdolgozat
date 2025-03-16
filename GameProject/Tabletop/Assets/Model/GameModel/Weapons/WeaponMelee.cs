namespace Model.Weapons
{
    public class WeaponMelee : Weapon
    {
        protected WeaponMelee(int hit, int pwr, int rng = 2, int fr = 1) : base(hit, pwr, rng, fr, ArmoryType.None) { }

        #region Preset
        public static WeaponMelee Shovel = new WeaponMelee(hit: 3, pwr: 4);
        public static WeaponMelee Bayonet = new WeaponMelee(hit: 4, pwr: 5);
        public static WeaponMelee Katana = new WeaponMelee(hit: 2, pwr: 8);
        #endregion
    }
}