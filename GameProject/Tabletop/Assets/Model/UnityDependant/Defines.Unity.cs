using Model.Units;
using Model.UnityDependant;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Model
{
    public static partial class Defines
    {
        public const int RANGE_ADJUSTMENT = 4;

        public static readonly IReadOnlyList<string> BlueColors = new List<string>
        {
            "#0038FF",
            "#1DC0FF"
        };

        public static readonly IReadOnlyList<string> RedColors = new List<string>
        {
            "#FF0029",
            "#FF7E00"
        };

        public static readonly IReadOnlyDictionary<UnitIdentifier, UnitVisualData> UnitVisuals = new Dictionary<UnitIdentifier, UnitVisualData>()
        {
            // Blue
            {
                UnitIdentifier.Kriegsman,
                new UnitVisualData("Kriegsman", "Units/kriegsman_p", "Units/kriegsman")
            },
            {
                UnitIdentifier.TacticalMarine,
                new UnitVisualData("Imperial Tactical Marine", "Units/tacticalmarine_p", "Units/tacticalmarine")
            },
            {
                UnitIdentifier.MarineCaptain,
                new UnitVisualData("Imperial Marine Captain", "Units/marinecaptain_p", "Units/marinecaptain")
            },
            {
                UnitIdentifier.Baneblade,
                new UnitVisualData("Baneblade Battle Tank", "Units/baneblade_p", "Units/baneblade")
            },
            {
                UnitIdentifier.Ballistus,
                new UnitVisualData("Ballistus Dreadnaught", "Units/ballistus_p", "Units/ballistus")
            },
            // Red
            {
                UnitIdentifier.ChaosCultist,
                new UnitVisualData("Chaos Cultist Mob", "Units/chaoscultist_p", "Units/chaoscultist")
            },
            {
                UnitIdentifier.ChaosLegionnaire,
                new UnitVisualData("Chaos Legionnaire", "Units/chaoslegionnaire_p", "Units/chaoslegionnaire")
            },
            {
                UnitIdentifier.ChaosLord,
                new UnitVisualData("Chaos Lord", "Units/chaoslord_p", "Units/chaoslord")
            },
            {
                UnitIdentifier.Predator,
                new UnitVisualData("Predator (Chaos)", "Units/predator_p", "Units/predator")
            },
            {
                UnitIdentifier.Forgefiend,
                new UnitVisualData("Forgefiend", "Units/forgefiend_p", "Units/forgefiend")
            }
        };
    }
}
