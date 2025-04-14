namespace Model.Weapons
{
    public class UnitWeapon
    {
        public WeaponIdentifier Identity { get; }
        public WeaponConstants Constants { get; }
        public int Count { get; }

        public UnitWeapon(WeaponIdentifier id, int count)
        {
            Identity = id;
            Constants = Defines.Weapons[Identity];
            Count = count;
        }
    }
}
