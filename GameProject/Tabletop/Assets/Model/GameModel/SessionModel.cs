using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Model.Units;
using Model.Units.Interfaces;

namespace Model
{
    /// <summary>
    /// GameModel class responsible for handling turn changes and checking whether the ending conditions fulfill.
    /// </summary>

    public sealed class SessionModel
    {
        public const int MAX_UNDO_COUNT = 5;
        #region Turn Variables
        /// <summary>
        /// If we enabled a turn limit or not
        /// </summary>
        public bool IsTurnLimited { get; }
        /// <summary>
        /// The selected turn limit. -1 if not limited
        /// </summary>
        public int TurnLimit { get; }

        private int turnCounter;
        /// <summary>
        /// The current turn. At the start of the game this is 1.
        /// </summary>
        public int TurnCounter
        {
            get { return turnCounter; }
            private set
            {
                TurnChanging.Invoke(this, EventArgs.Empty);
                turnCounter = value;
                TurnChanged.Invoke(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// This event fires before <see cref="TurnCounter"/> is modified.
        /// </summary>
        public event EventHandler TurnChanging;
        /// <summary>
        /// This event fires after <see cref="TurnCounter"/> is modified.
        /// </summary>
        public event EventHandler TurnChanged;

        private Phase currentPhase;
        /// <summary>
        /// The current Phase of the turn. Phases are in the following order:
        ///     <list type="number">
        ///         <item><see cref="Phase.Command"/></item>
        ///         <item><see cref="Phase.Movement"/></item>
        ///         <item><see cref="Phase.Fighting"/></item>
        ///         <item><see cref="Phase.Melee"/></item>
        ///     </list>
        /// </summary>
        public Phase CurrentPhase
        {
            get { return currentPhase; }
            private set
            {
                PhaseChanging.Invoke(this, EventArgs.Empty);
                currentPhase = value;
                PhaseChanged.Invoke(this, EventArgs.Empty);
            }
        }
        /// <summary>
        /// This event fires before <see cref="CurrentPhase"/> is modified.
        /// </summary>
        public event EventHandler PhaseChanging;
        /// <summary>
        /// This event fires after <see cref="CurrentPhase"/> is modified.
        /// </summary>
        public event EventHandler PhaseChanged;
        #endregion

        #region Player variables
        /// <summary>
        /// List of players participating in the game.
        /// </summary>
        private List<IPlayer> players;

        /// <summary>
        /// A read-only list of players
        /// </summary>
        public ReadOnlyCollection<IPlayer> Players { get; }

        private IPlayer activePlayer;
        /// <summary>
        /// The currently active player. Setting this will also set the <see cref="ActiveSide"/>
        /// </summary>
        public IPlayer ActivePlayer 
        {
            get
            {
                return activePlayer;
            }
            private set
            {
                activePlayer = value;
                ActiveSide = activePlayer.Side;
            }
        }
        /// <summary>
        /// Which side is the currently active player on
        /// </summary>
        public Side ActiveSide { get; private set; }
        #endregion

        public SessionModel(SessionSettings initData)
        {
            TurnCounter = 1;
            IsTurnLimited = initData.IsLimited;
            TurnLimit = IsTurnLimited ? initData.TurnLimit : -1;

            if (IsTurnLimited)
            {
                // TODO Game over by event handler
                // TurnChanged += SessionModel_TurnChanged;
            }
            
            players = new List<IPlayer>();
            // TODO add players here
            Players = new ReadOnlyCollection<IPlayer>(players);
            ActivePlayer = players.First();
        }

        #region Turn manipulation
        /// <summary>
        /// This method will be call once when a Player finishes his phase
        /// </summary>
        public void PlayerDoneWithPhase()
        {
            if (IsLastPlayer())
            {
                NextPhase();
            }
            ActivePlayer = GetNextPlayer();
        }
        /// <summary>
        /// Returns the next Player in line from <see cref="Players"/>. Returns the first if the current is the last one.
        /// </summary>
        private IPlayer GetNextPlayer()
        {
            if (IsLastPlayer())
            {
                return Players.First();
            }
            
            int index = 0;
            while(Players.ElementAt(index) != ActivePlayer)
            {
                index++;
            }
            // I have to return index + 1 because index will be the current player
            index++;
            return Players.ElementAt(index);
        }
        /// <summary>
        /// Gets if the ActivePlayer is the last in the Players' list.
        /// </summary>
        private bool IsLastPlayer()
        {
            return ActivePlayer == Players.Last();
        }
        /// <summary>
        /// Sets the <see cref="CurrentPhase"/> and potentially the <see cref="TurnCounter"/> variables.
        /// </summary>
        private void NextPhase()
        {
            if (CurrentPhase == Phase.Fighting)
            {
                CurrentPhase = Phase.Command;
                TurnCounter++;
            } else
            {
                CurrentPhase++;
            }
        }
        #endregion
    }
}