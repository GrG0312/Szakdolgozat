using System;
using System.Security.Cryptography;

namespace Model.Lobby
{
#nullable enable

    /// <summary>
    /// Represent a slot in the lobby, which a player can join into or switch to.
    /// </summary>
    public class LobbySlot
    {
        #region Events

        /// <summary>
        /// Invokes when <see cref="OccupantStatus"/>'s value has changed.
        /// </summary>
        public event EventHandler? OccupantStatusChanged;

        /// <summary>
        /// Invokes when <see cref="PlayerData"/>'s value has changed. It's argument represent whether <see cref="PlayerData"/>'s value is null (false) or not (true).
        /// </summary>
        public event EventHandler<bool>? PlayerChanged;

        #endregion

        /// <summary>
        /// Which side does the slot belong to. The player switching to this slot will inherit this side.
        /// </summary>
        public Side Side { get; }

        #region Properties

        private SlotOccupantStatus occupantStatus;

        private LobbyPlayerData? playerData;

        /// <summary>
        /// The slot's occupant. Invokes <see cref="OccupantStatusChanged"/> event on changing values.
        /// </summary>
        public SlotOccupantStatus OccupantStatus
        {
            get { return occupantStatus; }
            set
            {
                occupantStatus = value;
                OccupantStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// If the slot is open, this value is null. If it's occupied, this value contains the occupying player's data.
        /// Invokes <see cref="PlayerChanged"/> event when changing values. Modifying this will also set <see cref="OccupantStatus"/>'s value.
        /// </summary>
        public LobbyPlayerData? PlayerData
        {
            get { return playerData; }
            set
            {
                playerData = value;
                if (playerData != null)
                {
                    playerData.Side = this.Side;
                    OccupantStatus = SlotOccupantStatus.Occupied;
                } else
                {
                    OccupantStatus = SlotOccupantStatus.Open;
                }
                PlayerChanged?.Invoke(this, playerData != null);
            }
        }

        #endregion

        public LobbySlot(Side side)
        {
            OccupantStatus = SlotOccupantStatus.Open;
            Side = side;
            playerData = null;
        }

        #region Getting Player stuff

        /// <summary>
        /// Returns the occupying player's name, or an empty string if the slot isn't occupied.
        /// </summary>
        /// <returns></returns>
        public string GetPlayerName()
        {
            return playerData == null ? string.Empty : playerData.Name;
        }

        /// <summary>
        /// Returns true if there is an occupying player and if that player's <see cref="LobbyPlayerData.IsReady"/>'s value is true.
        /// Returns false otherwise.
        /// </summary>
        public bool IsPlayerReady()
        {
            return playerData != null && playerData.IsReady;
        }

        #endregion
    }
}

