using Model.GameModel.Commands;
using Model.Units;
using Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using static UnityEngine.UI.CanvasScaler;

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

        public event EventHandler? RequestMoreData;

        public event EventHandler? SelectedUnitChanged;

        public event EventHandler<Side>? GameOver;

        #endregion

        #region Turn Variables

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

        #region Player variables

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

        #region Selected Unit variables

        private ISelectable<PlayerIdType>? selectedUnit;
        public ISelectable<PlayerIdType>? SelectedUnit
        {
            get
            {
                return selectedUnit;
            }
            private set
            {
                Debug.Log($"<color=magenta>Selecting unit...</color>");
                Debug.Log($"<color=magenta>Is previous null? {selectedUnit == null}</color>");
                if (selectedUnit != null)
                {
                    Debug.Log($"<color=magenta>Deselecting old one...</color>");
                    selectedUnit.SetSelected(false);
                }
                selectedUnit = value;
                Debug.Log($"<color=magenta>New value set...</color>");
                Debug.Log($"<color=magenta>Is new value null? {selectedUnit == null}</color>");
                if (selectedUnit != null)
                {
                    Debug.Log($"<color=magenta>Applying selection to new...</color>");
                    selectedUnit.SetSelected(true);
                }
                Debug.Log($"<color=magenta>Invoking event...</color>");
                SelectedUnitChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        public IUnitCommand PendingCommand { get; private set; }

        private History<IUnitCommand> commandHistory;

        public GameModel(Dictionary<PlayerIdType, GamePlayerData> players, IUnitFactory<PlayerIdType> ufactory, ICommandFactory cfactory, IDiceRoller diceRoller)
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
        }

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

            Debug.Log($"<color=green>Current player: {ConnectedPlayers[ActivePlayerId].Name} {indexOfPlayer}</color>");

            bool found = CollectionHelper.LoopbackSearch(
                ConnectedPlayers.ToList(), p => !p.Value.IsDefeated, indexOfPlayer, out nextIndex);

            Debug.Log($"<color=green>Found the index of the next player: {ConnectedPlayers.ElementAt(nextIndex).Value.Name} {nextIndex}</color>");

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
            if (IsGameOver(out Side winner))
            {
                Debug.Log("<color=orange>The game is over!</color>");
                GameOver?.Invoke(this, winner);
            }

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

        #region Public methods

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

        public async void CreateCommand<T>(PlayerIdType id, params object[] args) where T : IUnitCommand
        {
            Debug.Log("<color=aqua>Creating command in the model...</color>");
            // AttackCommand : id, {targetUnit}
            // MoveCommand: id, {targetLocation}
            Debug.Log($"<color=aqua>Is selected unit null? {SelectedUnit == null}</color>");
            if (SelectedUnit == null || !IsCurrentPlayer(id) || SelectedUnit.Owner.CompareTo(id) != 0)
            {
                Debug.Log($"<color=aqua>Then call it off...</color>");
                return;
            }
            Debug.Log($"<color=aqua>Is previous command null? {PendingCommand == null}</color>");
            Debug.Log($"<color=aqua>Selected unit is not null, going into the factory...</color>");
            PendingCommand = commandFactory.Produce<T>(SelectedUnit, args, diceRoller);
            Debug.Log($"<color=aqua>Factory finished. Trying to execute the command.</color>");
            await ExecutePendingCommand();
        }

        public async Task ExecutePendingCommand()
        {
            // Only request data if the AttackCommand has its preconditions but cannot execute
            if (PendingCommand is AttackCommand<PlayerIdType> c && c.Preconditions(CurrentPhase) && !c.CanExecute(CurrentPhase))
            {
                Debug.Log($"<color=aqua>Pending is an AC, but needs more info.</color>");
                RequestMoreData?.Invoke(this, EventArgs.Empty);
            }
            else if (PendingCommand.CanExecute(CurrentPhase))
            {
                Debug.Log($"<color=aqua>Command can execute.</color>");
                await PendingCommand.Execute();
                Debug.Log($"<color=aqua>Command executed. Pushing into history and removing from pending.</color>");
                commandHistory.Push(PendingCommand);
                PendingCommand = null;
                Debug.Log($"<color=aqua>Is Pending null? {PendingCommand == null}</color>");
            }
        }
        #endregion

        private bool IsCurrentPlayer(PlayerIdType id)
        {
            return ActivePlayerId.CompareTo(id) == 0;
        }
    }
}