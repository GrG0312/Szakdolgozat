using Model.Deck;
using Model.Units;
using Model.Weapons;
using System.Collections.Generic;

namespace Model
{
    public static partial class Defines
    {
        public static readonly IReadOnlyDictionary<WeaponIdentifier, Weapon> Weapons = new Dictionary<WeaponIdentifier, Weapon>()
        {
            {
                WeaponIdentifier.PlasmaGun,
                new Weapon(3,5, 25, 4, ArmoryType.PowerArmor)
            },
            {
                WeaponIdentifier.Powerfists,
                new Weapon(5, 10, 5, 2, ArmoryType.PowerArmor)
            }
        };

        public static readonly IReadOnlyDictionary<UnitIdentifier, UnitConstants> UnitValues = new Dictionary<UnitIdentifier, UnitConstants>()
        {
            {
                UnitIdentifier.ImperialMarine,
                new UnitConstants(
                    Side.Blue,
                    UnitType.HeavyInfantry,
                    10,
                    5,
                    10,
                    25,
                    ArmoryType.PowerArmor,
                    new HashSet<Weapon> 
                    { 
                        Weapons[WeaponIdentifier.PlasmaGun], 
                        Weapons[WeaponIdentifier.Powerfists] 
                    }
                    )
            },
            {
                UnitIdentifier.ChaosMarine,
                new UnitConstants(
                    Side.Red,
                    UnitType.HeavyInfantry,
                    10,
                    5,
                    5,
                    35,
                    ArmoryType.PowerArmor,
                    new HashSet<Weapon> 
                    { 
                        Weapons[WeaponIdentifier.PlasmaGun] 
                    }
                    )
            }
        };

        public static readonly IReadOnlyDictionary<UnitIdentifier, UnitVisualData> UnitVisuals;
    }
}
