using Model.Commands;
using Model.Units;
using Model.Units.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Model
{
    public interface IPlayer
    {
        public int PlayerID { get; }
        public Side Side { get; }
        public int PointCurrency { get; }

        public History<Command> CommandHistory { get; }

        public ReadOnlyCollection<DeckUnitCard> Deck { get; }
        public ReadOnlyCollection<IUnit> UnitsInPlay { get; }
        public ReadOnlyCollection<ISelectable> Selection { get; }

        public void SelectNew(IEnumerable<ISelectable> units);
        public void SelectAdditive(IEnumerable<ISelectable> units);
    }
}
