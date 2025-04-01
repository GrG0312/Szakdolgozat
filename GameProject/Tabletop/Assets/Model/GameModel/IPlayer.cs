using Model.Commands;
using Model.Deck;
using Model.Units.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Model
{
    public interface IPlayer<PlayerIdType>
    {
        public PlayerIdType PlayerID { get; }
        public Side Side { get; }
        public int PointCurrency { get; }

        public History<Command> CommandHistory { get; }

        public IReadOnlyList<DeckEntry> Deck { get; }
        public IReadOnlyList<IUnit> UnitsInPlay { get; }
        public IReadOnlyList<ISelectable> Selection { get; }

        public void SelectNew(IEnumerable<ISelectable> units);
        public void SelectAdditive(IEnumerable<ISelectable> units);
    }
}
