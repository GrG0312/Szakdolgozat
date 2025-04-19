using Model.Interfaces;
using Model.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Model.GameModel
{
    /// <summary>
    /// The GameModel is class responsible for handling turn changes, active player changes...
    /// </summary>

    public sealed class GameModel<PlayerIdType> where PlayerIdType : IComparable<PlayerIdType>
    {
        private IUnitFactory<PlayerIdType> unitFactory;
        private ICommandFactory commandFactory;
        private IDiceRoller diceRoller;

        #region Events

        public event EventHandler? ActivePlayerChanged;
        /// <summary>
        /// This event fires after any player's points have been altered.
        /// </summary>
        public event EventHandler<PlayerPointsChangedEventArgs<PlayerIdType>>? PlayerPointsChanged;
        /// <summary>
        /// This event fires after <see cref="TurnCounter"/> is modified.
        /// </summary>
        public event EventHandler? TurnChanged;

        /// <summary>
        /// This event fires after <see cref="CurrentPhase"/> is modified.
        /// </summary>
        public event EventHandler? PhaseChanged;

        public event EventHandler? SelectedUnitChanged;

        public event EventHandler<Side>? GameOver;

        public event EventHandler? UnitCycled;

        #endregion

        #region Turn

        private int turnCounter;

        /// <summary>
        /// The current turn. At the start of the game this is 1.
        /// </summary>
        public int TurnCounter
        {
            get { return turnCounter; }
            private set
            {
                turnCounter = value;
                TurnChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private Phase currentPhase;

        /// <summary>
        /// The current Phase of the turn. Phases are in the following order:
        ///     <list type="number">
        ///         <item><see cref="Phase.Command"/></item>
        ///         <item><see cref="Phase.Movement"/></item>
        ///         <item><see cref="Phase.Fighting"/></item>
        ///     </list>
        /// </summary>
        public Phase CurrentPhase
        {
            get { return currentPhase; }
            private set
            {
                currentPhase = value;
                PhaseChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Player

        private int indexOfPlayer;

        /// <summary>
        /// A read-only list of players
        /// </summary>
        public IReadOnlyDictionary<PlayerIdType, GamePlayerData> ConnectedPlayers { get; private set; }
        
        private PlayerIdType activePlayerId;

        /// <summary>
        /// The currently active player's ID. Setting this will also set the <see cref="ActiveSide"/>
        /// </summary>
        public PlayerIdType ActivePlayerId 
        {
            get
            {
                return activePlayerId;
            }
            private set
            {
                activePlayerId = value;
                ActiveSide = ConnectedPlayers[activePlayerId].Side;
                ActivePlayerChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public GamePlayerData ActivePlayerData => ConnectedPlayers[activePlayerId];
        /// <summary>
        /// Which side is the currently active player on
        /// </summary>
        public Side ActiveSide { get; private set; }

        #endregion

        #region Selected Unit

        private ISelectable<PlayerIdType>? selectedUnit;
        public ISelectable<PlayerIdType>? SelectedUnit
        {
            get
            {
                return selectedUnit;
            }
            private set
            {
                if (selectedUnit != null)
                {
                    selectedUnit.SetSelected(false);
                }
                selectedUnit = value;
                if (selectedUnit != null)
                {
                    selectedUnit.SetSelected(true);
                }
                SelectedUnitChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Command

        private History<IUnitCommand> commandHistory;

        public IUnitCommand? PendingCommand { get; private set; }

        #endregion

        private List<ControlPointModel> controlPoints;

        #region Constructor

        public GameModel(
            Dictionary<PlayerIdType, GamePlayerData> players, 
            IUnitFactory<PlayerIdType> ufactory, 
            ICommandFactory cfactory, 
            IDiceRoller diceRoller, 
            IEnumerable<ControlPointModel> controlpoints)
        {
            commandHistory = new History<IUnitCommand>();
            ConnectedPlayers = new Dictionary<PlayerIdType, GamePlayerData>(players);
            foreach (GamePlayerData player in ConnectedPlayers.Values)
            {
                player.PointsChanged += Player_PointsChanged;
            }
            unitFactory = ufactory;
            commandFactory = cfactory;
            this.diceRoller = diceRoller;
            TurnCounter = 0;
            this.controlPoints = new List<ControlPointModel>();
            foreach (ControlPointModel cp in controlpoints)
            {
                cp.OwnerChanged += ControlPointOwnerChanged;
                controlPoints.Add(cp);
            }
        }

        #endregion

        #region Turn manipulation

        public void StartGame()
        {
            ActivePlayerId = ConnectedPlayers.First().Key;
            indexOfPlayer = 0;
            TurnCounter = 1;
            CurrentPhase = Phase.Command;
            foreach (GamePlayerData data in ConnectedPlayers.Values)
            {
                data.AddPoints(Defines.POINTS_ON_START);
            }
        }

        public void Forfeit(PlayerIdType id)
        {
            ConnectedPlayers[id].Forfeit();
            if (IsCurrentPlayer(id))
            {
                PlayerDone(id);
            }
        }

        /// <summary>
        /// This method will be called once when a Player finishes his phase
        /// </summary>
        public void PlayerDone(PlayerIdType id)
        {
            if (!IsCurrentPlayer(id))
            {
                return;
            }
            commandHistory.Flush();

            if (IsGameOver(out Side winner))
            {
                foreach (GamePlayerData data in ConnectedPlayers.Values)
                {
                    data.DeleteUnits(true);
                }
                GameOver?.Invoke(this, winner);
                return;
            }

            if (IsLastPlayer())
            {
                NextPhase();
            }
            if (GetNextPlayer(out int index))
            {
                indexOfPlayer = index;
                ActivePlayerId = ConnectedPlayers.ElementAt(index).Key;
            }
        }

        /// <summary>
        /// Returns the next Player in line from <see cref="ConnectedPlayers"/>.
        /// </summary>
        private bool GetNextPlayer(out int nextIndex)
        {
            nextIndex = default;

            bool found = CollectionHelper.LoopbackSearch(
                ConnectedPlayers.ToList(), p => !p.Value.IsDefeated && p.Value.IsConnected, indexOfPlayer, out nextIndex);

            return found;
        }

        private bool IsLastPlayer()
        {
            for (int i = ConnectedPlayers.Count - 1; i > indexOfPlayer; i--)
            {
                if (!ConnectedPlayers.ElementAt(i).Value.IsDefeated)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sets the <see cref="CurrentPhase"/> and potentially the <see cref="TurnCounter"/> variables.
        /// </summary>
        private void NextPhase()
        {
            if (CurrentPhase == Phase.Fighting)
            {
                NextTurn();
            } else
            {
                CurrentPhase++;
            }
        }

        private void NextTurn()
        {
            CurrentPhase = Phase.Command;
            TurnCounter++;

            foreach (GamePlayerData player in ConnectedPlayers.Values)
            {
                player.AddPoints();
                player.ResetUnits();
            }
        }

        private bool IsGameOver(out Side winner)
        {
            winner = default;
            int imp = 0;
            int chaos = 0;

            foreach (GamePlayerData data in ConnectedPlayers.Values)
            {
                if (!data.IsDefeated)
                {
                    if (data.Side == Side.Imperium)
                    {
                        imp++;
                    } else
                    {
                        chaos++;
                    }
                }
            }

            if (imp == 0)
            {
                winner = Side.Imperium;
            }
            else if(chaos == 0)
            {
                winner = Side.Chaos;
            }

            return imp == 0 || chaos == 0;
        }

        #endregion

        #region Event handlers

        private void Player_PointsChanged(object sender, EventArgs e)
        {
            GamePlayerData data = sender as GamePlayerData;
            PlayerIdType id = ConnectedPlayers.Single(kvp => kvp.Value == data).Key;
            PlayerPointsChanged?.Invoke(this, new PlayerPointsChangedEventArgs<PlayerIdType>(id, data.Currency, data.PointsGainedPerTurn));
        }

        #endregion

        #region Units

        public bool BuyUnit(PlayerIdType id, UnitIdentifier identity)
        {
            if (!IsCurrentPlayer(id) || CurrentPhase != Phase.Command)
            {
                return false;
            }
            if (ConnectedPlayers[id].BuyUnit(identity))
            {
                IUnit unit = unitFactory.Produce(id, identity, ConnectedPlayers[id].Side);
                ConnectedPlayers[id].GetUnit(unit);
                return true;
            }
            return false;
        }

        public void SelectUnit(PlayerIdType id, ISelectable<PlayerIdType> unit)
        {
            if (!IsCurrentPlayer(id))
            {
                return;
            }
            SelectedUnit = unit;
        }

        public void CycleUnits(PlayerIdType id)
        {
            if (!IsCurrentPlayer(id))
            {
                return;
            }

            IUnit? found;
            // If the selected is not null and it's owned by the player, then select a new unit after this one
            if (SelectedUnit != null && SelectedUnit.Owner.CompareTo(id) == 0)
            {
                found = ActivePlayerData.Cycle(CurrentPhase, SelectedUnit as IUnit);
            }
            // If not, then just select the first available
            else
            {
                found = ActivePlayerData.Cycle(CurrentPhase);
            }

            // If the selected is still useful and it's the player's
            if (SelectedUnit != null && SelectedUnit.Owner.CompareTo(id) == 0 && SelectedUnit is IUsable u && u.IsUsable(CurrentPhase))
            {
                // Only switch if we found one
                if (found != null)
                {
                    SelectedUnit = found as ISelectable<PlayerIdType>;
                }
                UnitCycled?.Invoke(this, EventArgs.Empty);
            }
            // Otherwise if the selected is null or it's not mine or it's not usable then switch. Whats the worst that could happen
            else
            {
                SelectedUnit = found as ISelectable<PlayerIdType>;
                UnitCycled?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Commands

        public void CreateCommand<T>(PlayerIdType id, params object[] args) where T : IUnitCommand
        {
            // AttackCommand : id, {targetUnit}
            // MoveCommand: id, {targetLocation}
            if (SelectedUnit == null || !IsCurrentPlayer(id) || SelectedUnit.Owner.CompareTo(id) != 0)
            {
                return;
            }
            PendingCommand = commandFactory.Produce<T>(SelectedUnit, args, diceRoller);
        }

        public async Task ExecuteCommand()
        {
            if (PendingCommand.CanExecute(CurrentPhase))
            {
                await PendingCommand.Execute();
                commandHistory.Push(PendingCommand);
                PendingCommand = null;
            }
        }

        public void AbortCommand()
        {
            PendingCommand = null;
        }

        public void UndoCommand(PlayerIdType id)
        {
            if (!IsCurrentPlayer(id))
            {
                return;
            }
            try
            {
                if (commandHistory.Peek() is IUndoableCommand undoable)
                {
                    undoable.Undo();
                    commandHistory.Pop();
                }
            } catch (IndexOutOfRangeException) { /* No need to do anything, since if the history is empty then there is nothing to undo */ }
        }

        #endregion

        private bool IsCurrentPlayer(PlayerIdType id)
        {
            return ActivePlayerId.CompareTo(id) == 0;
        }

        private void ControlPointOwnerChanged(object sender, int oldvalue)
        {
            ControlPointModel m = sender as ControlPointModel;
            if (oldvalue != -1)
            {
                Side oldowner = (Side)oldvalue;
                foreach (GamePlayerData data in ConnectedPlayers.Values)
                {
                    if (data.Side == oldowner)
                    {
                        data.CapturePoint(true);
                    }
                }
            }
            if (m.Owner != -1)
            {
                Side owner = (Side)m.Owner;
                foreach (GamePlayerData data in ConnectedPlayers.Values)
                {
                    if (data.Side == owner)
                    {
                        data.CapturePoint();
                    }
                }
            }
        }

    }
}