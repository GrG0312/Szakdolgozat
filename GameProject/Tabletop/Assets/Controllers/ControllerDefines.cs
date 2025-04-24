using Model.Units;
using Controllers.Data;
using System.Collections.Generic;

namespace Controllers
{
    public static class ControllerDefines
    {
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
                new UnitVisualData("Units/kriegsman_p", "Units/kriegsman")
            },
            {
                UnitIdentifier.TacticalMarine,
                new UnitVisualData("Units/tacticalmarine_p", "Units/tacticalmarine")
            },
            {
                UnitIdentifier.MarineCaptain,
                new UnitVisualData("Units/marinecaptain_p", "Units/marinecaptain")
            },
            {
                UnitIdentifier.Baneblade,
                new UnitVisualData("Units/baneblade_p", "Units/baneblade")
            },
            {
                UnitIdentifier.Ballistus,
                new UnitVisualData("Units/ballistus_p", "Units/ballistus")
            },
            // Red
            {
                UnitIdentifier.ChaosCultist,
                new UnitVisualData("Units/chaoscultist_p", "Units/chaoscultist")
            },
            {
                UnitIdentifier.ChaosLegionnaire,
                new UnitVisualData("Units/chaoslegionnaire_p", "Units/chaoslegionnaire")
            },
            {
                UnitIdentifier.ChaosLord,
                new UnitVisualData("Units/chaoslord_p", "Units/chaoslord")
            },
            {
                UnitIdentifier.Predator,
                new UnitVisualData("Units/predator_p", "Units/predator")
            },
            {
                UnitIdentifier.Forgefiend,
                new UnitVisualData("Units/forgefiend_p", "Units/forgefiend")
            }
        };
    }
}
