using Model.Commands;
using Model.Deck;
using Model.Units.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Model.UnityDependant
{
    public class PlayerObject : MonoBehaviour//, IPlayer<ulong>
    {
        #region General properties
        /// <summary>
        /// The name that shows in-game
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// A unique player ID, same as the Client ID
        /// </summary>
        public ulong PlayerID { get; }
        /// <summary>
        /// Which side the Player is on
        /// </summary>
        public Side Side { get; }
        /// <summary>
        /// How many spendable points this player has
        /// </summary>
        public int PointCurrency { get; private set; }
        #endregion

        #region Unit collections
        /// <summary>
        /// List of still purchasable units in the deck
        /// </summary>
        private List<DeckEntry> deck { get; } = new List<DeckEntry>();
        /// <summary>
        /// <inheritdoc cref="deck"/>
        /// </summary>
        public IReadOnlyCollection<DeckEntry> Deck { get; private set; }

        /// <summary>
        /// List of units currently in the field, ready to control
        /// </summary>
        private List<IUnit> unitsInPlay = new List<IUnit>();
        /// <summary>
        /// <inheritdoc cref="unitsInPlay"/>
        /// </summary>
        public ReadOnlyCollection<IUnit> UnitsInPlay { get; private set; }

        /// <summary>
        /// List of currently selected units.
        /// </summary>
        private List<ISelectable<ulong>> selection = new List<ISelectable<ulong>>();
        /// <summary>
        /// <inheritdoc cref="selection"/>
        /// </summary>
        public ReadOnlyCollection<ISelectable<ulong>> Selection { get; private set; }
        #endregion

        #region Command collections
        public History<Command> CommandHistory { get; } = new History<Command>();
        public List<Command> UndoneCommands { get; } = new List<Command>();
        #endregion

        private void Awake()
        {
            Deck = new ReadOnlyCollection<DeckEntry>(deck);
            UnitsInPlay = new ReadOnlyCollection<IUnit>(unitsInPlay);
            Selection = new ReadOnlyCollection<ISelectable<ulong>>(selection);
            // TODO how should I pass the selected Deck to the player object?
        }

        #region Selection methods
        public void SelectAdditive(IEnumerable<ISelectable> units)
        {
            foreach (ISelectable unit in units)
            {
                selection.Add(unit as ISelectable<ulong>);
                unit.Selected();
            }
        }
        public void SelectNew(IEnumerable<ISelectable> units)
        {
            selection.Clear();
            SelectAdditive(units);
        }
        #endregion

        #region Command Methods
        public void IssueMoveCommand(Vector3 target)
        {
            foreach (ISelectable<ulong> selectable in Selection)
            {
                // I should check if the unit is mine, otherwise I can't issue commands to it
                if (selectable.Owner != PlayerID)
                {
                    continue;
                }
                if (selectable is IMovable<Vector3> movable)
                {
                    MoveCommand<Vector3> command = new MoveCommand<Vector3>(movable, target);
                    command.Execute();
                    CommandHistory.Push(command);
                }
            }
        }
        public void IssueAttackCommand(IDamagable<ulong> target)
        {
            foreach (ISelectable<ulong> selectable in Selection)
            {
                if (selectable.Owner != PlayerID || selectable.Owner == target.Owner)
                {
                    return;
                }
                if (selectable is IWeaponUser weaponHolder)
                {
                    AttackCommand command = new AttackCommand(weaponHolder, target);
                    command.Execute();
                    CommandHistory.Push(command);
                }
            }
        }
        #endregion
    }
}
