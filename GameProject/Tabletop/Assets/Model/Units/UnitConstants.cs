using Model.Weapons;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Model.Units
{
    /// <summary>
    /// This class holds the universal values that the units will have.
    /// </summary>
    public class UnitConstants
    {

        /// <summary>
        /// Which side the unit is available for.
        /// </summary>
        public Side Side { get; }

        #region Deck stats

        /// <summary>
        /// What type is the Unit. (Light Infantry / Tank / Special etc)
        /// </summary>
        public UnitType Type { get; }

        /// <summary>
        /// How many of this unit can be maximum in a deck
        /// </summary>
        public int LimitInDeck { get; }

        /// <summary>
        /// How much does it cost to purchase this unit
        /// </summary>
        public int Price { get; }

        #endregion

        #region Game Stats

        /// <summary>
        /// How far the Unit can travel at most in a phase.
        /// </summary>
        public int Movement { get; }

        /// <summary>
        /// How effective the Units armor is. 3 means any throw thats 3 or lower will not deal damage.
        /// </summary>
        public int ArmorSave { get; }

        /// <summary>
        /// This units hitpoints.
        /// </summary>
        public int Wound { get; }

        /// <summary>
        /// Ownership of capture points is calculated using this value.
        /// </summary>
        public int ObjectiveControl { get; }
        
        /// <summary>
        /// Collection of weapons that this unit has
        /// </summary>
        public IReadOnlyList<UnitWeapon> Weapons { get; }

        #endregion

        public UnitConstants(
            Side side,
            UnitType type,
            int limit,
            int price,
            int mov,
            int armorSave,
            int hp,
            int objcont,
            IList<UnitWeapon> weapons)
        {
            Side = side;

            Type = type;
            LimitInDeck = limit;
            Price = price;

            Movement = mov;
            ArmorSave = armorSave;
            Wound = hp;
            ObjectiveControl = objcont;

            Weapons = new List<UnitWeapon>(weapons);
        }
    }
}
