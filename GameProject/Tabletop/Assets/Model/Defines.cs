using Model.Deck;
using Model.Units;
using Model.Weapons;
using System.Collections.Generic;

namespace Model
{
    public static partial class Defines
    {
        public const int POINTS_ON_START = 1200;
        public const int POINTS_PER_TURN = 200;
        public const int POINTS_PER_CAP = 100;

        public static readonly IReadOnlyDictionary<WeaponIdentifier, WeaponConstants> Weapons = new Dictionary<WeaponIdentifier, WeaponConstants>()
        {
            {
                WeaponIdentifier.Lasgun,
                new WeaponConstants("Lasgun", 24, 1, 4, 0, 1)
            },
            {
                WeaponIdentifier.Boltgun,
                new WeaponConstants("Boltgun", 24, 1, 3, 0, 1)
            },
            {
                WeaponIdentifier.HeavyBoltPistol,
                new WeaponConstants("Heavy Bolt Pistol", 18, 1, 2, 1, 1)
            },
            {
                WeaponIdentifier.HeavyBolter,
                new WeaponConstants("Heavy Bolter", 36, 3, 4, 1, 2)
            },
            {
                WeaponIdentifier.Lascannon,
                new WeaponConstants("Lascannon", 48, 1, 4, 3, 5) // NOTE: dmg should be D6+1?
            },
            {
                WeaponIdentifier.HeavyFlamer,
                new WeaponConstants("Heavy Flamer", 12, 4, 0, 1, 1) // NOTE: atk should be D6?
            },
            {
                WeaponIdentifier.StormBolter,
                new WeaponConstants("Storm Bolter Autocannon",24, 2, 3, 0, 1)
            },
            {
                WeaponIdentifier.BallistusMissileLauncher_AP,
                new WeaponConstants("Ballistus Missile Launcher (Krak)",48, 2, 3, 2, 4) // NOTE: dmg should be D6?
            },
            {
                WeaponIdentifier.BallistusMissileLauncher_HE,
                new WeaponConstants("Ballistus Missile Launcher (Frag)", 48, 8, 3, 0, 1) // NOTE: atk should be 2D6?
            },
            {
                WeaponIdentifier.Autopistol,
                new WeaponConstants("Autopistol", 12, 1, 4, 0, 1)
            },
            {
                WeaponIdentifier.Plasmagun,
                new WeaponConstants("Plasmagun", 24, 1,3, 0, 1)
            },
            {
                WeaponIdentifier.PlasmaPistol,
                new WeaponConstants("Plasma Pistol", 12, 1, 2, 0, 1)
            },
            {
                WeaponIdentifier.HadesAutocannon,
                new WeaponConstants("Hades-class Autocannon", 36, 6, 3, 1, 2)
            },
            {
                WeaponIdentifier.EctoplasmaCannon,
                new WeaponConstants("Ectoplasma Cannon", 36, 2, 3, 3, 3) // NOTE: atk should be D3?
            }
        };

        public static readonly IReadOnlyDictionary<UnitIdentifier, UnitConstants> UnitValues = new Dictionary<UnitIdentifier, UnitConstants>()
        {
            // Blue side
            {
                UnitIdentifier.Kriegsman,
                new UnitConstants(
                    Side.Imperium,
                    UnitType.LightInfantry,
                    16, // max: 36
                    10, // cost: 8
                    6,
                    5,
                    1,
                    2,
                    new List<UnitWeapon>()
                    {
                        new UnitWeapon(WeaponIdentifier.Lasgun, 1)
                    }
                    )
            },
            {
                UnitIdentifier.TacticalMarine,
                new UnitConstants(
                    Side.Imperium,
                    UnitType.HeavyInfantry,
                    8, // max: 12
                    50, // cost: 30
                    6,
                    3,
                    2,
                    2,
                    new List<UnitWeapon>()
                    {
                        new UnitWeapon(WeaponIdentifier.Boltgun, 1)
                    }
                    )
            },
            {
                UnitIdentifier.MarineCaptain,
                new UnitConstants(
                    Side.Imperium,
                    UnitType.SpecialCharacter,
                    1, // max: 3
                    60, // cost: 80
                    6,
                    3,
                    6, // NOTE: 5+1 with Relic Shield?
                    1,
                    new List<UnitWeapon>()
                    {
                        new UnitWeapon(WeaponIdentifier.HeavyBoltPistol, 1)
                    }
                    )
            },
            {
                UnitIdentifier.Baneblade,
                new UnitConstants(
                    Side.Imperium,
                    UnitType.Tank,
                    2, // max: 2 (3)
                    600, // cost: (480)
                    12,
                    2,
                    24,
                    8,
                    new List<UnitWeapon>()
                    {
                        new UnitWeapon(WeaponIdentifier.HeavyBolter, 2),
                        new UnitWeapon(WeaponIdentifier.Lascannon, 4),
                        new UnitWeapon(WeaponIdentifier.HeavyFlamer, 2)
                    }
                    )
            },
            {
                UnitIdentifier.Ballistus,
                new UnitConstants(
                    Side.Imperium,
                    UnitType.Warmachine,
                    2, // max: 3
                    400, // cost: 130
                    8,
                    2,
                    12,
                    4,
                    new List<UnitWeapon>()
                    {
                        new UnitWeapon(WeaponIdentifier.Lascannon, 1),
                        new UnitWeapon(WeaponIdentifier.BallistusMissileLauncher_AP, 1),
                        new UnitWeapon(WeaponIdentifier.BallistusMissileLauncher_HE, 1),
                        new UnitWeapon(WeaponIdentifier.StormBolter, 2)
                    }
                    )
            },
            // Red side
            {
                UnitIdentifier.ChaosCultist,
                new UnitConstants(
                    Side.Chaos,
                    UnitType.LightInfantry,
                    20, // max: 40 (6)
                    10, // cost: 5
                    6,
                    6,
                    1,
                    1,
                    new List<UnitWeapon>()
                    {
                        new UnitWeapon(WeaponIdentifier.Autopistol, 1)
                    }
                    )
            },
            {
                UnitIdentifier.ChaosLegionnaire,
                new UnitConstants(
                    Side.Chaos,
                    UnitType.HeavyInfantry,
                    8, // max: 12
                    50, // cost: 25
                    6,
                    3,
                    2,
                    2,
                    new List<UnitWeapon>()
                    {
                        new UnitWeapon(WeaponIdentifier.Plasmagun, 1)
                    }
                    )
            },
            {
                UnitIdentifier.ChaosLord,
                new UnitConstants(
                    Side.Chaos,
                    UnitType.SpecialCharacter,
                    1, // max: 3
                    60, // cost: 90
                    6,
                    3,
                    5,
                    1,
                    new List<UnitWeapon>()
                    {
                        new UnitWeapon(WeaponIdentifier.PlasmaPistol, 1)
                    }
                    )
            },
            {
                UnitIdentifier.Predator,
                new UnitConstants(
                    Side.Chaos,
                    UnitType.Tank,
                    4, // max: 6 (3)
                    400, // cost: 150
                    10,
                    3,
                    11,
                    4,
                    new List<UnitWeapon>()
                    {
                        new UnitWeapon(WeaponIdentifier.Lascannon, 2),
                        new UnitWeapon(WeaponIdentifier.HeavyBolter, 2)
                    }
                    )
            },
            {
                UnitIdentifier.Forgefiend,
                new UnitConstants(
                    Side.Chaos,
                    UnitType.Warmachine,
                    2, // max: 4 (3)
                    500, // cost: 190
                    8,
                    3,
                    12,
                    3,
                    new List<UnitWeapon>()
                    {
                        new UnitWeapon(WeaponIdentifier.EctoplasmaCannon, 3),
                        new UnitWeapon(WeaponIdentifier.HadesAutocannon, 2)
                    }
                    )
            }
        };
    }
}
