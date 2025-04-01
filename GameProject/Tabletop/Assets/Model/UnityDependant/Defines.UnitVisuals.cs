using Model.Units;
using System.Collections.Generic;

namespace Model
{
    public static partial class Defines
    {
        static Defines()
        {
            UnitVisuals = new Dictionary<UnitIdentifier, UnitVisualData>()
            {
                {
                    UnitIdentifier.ImperialMarine,
                    new UnitVisualData("Imperial Space Marine", "Units/ImpMarine")
                },
                {
                    UnitIdentifier.ChaosMarine,
                    new UnitVisualData("Chaos Marine", "Units/ChaosMarine")
                }
            };
        }
    }
}
