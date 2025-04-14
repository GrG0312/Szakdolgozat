using System;
using System.Security.Cryptography;

namespace Model.Lobby
{
    public class LobbySlot
    {
        public event EventHandler? OccupantStatusChanged;
        public event EventHandler<bool>? PlayerChanged;

        public Side Side { get; }

        #region Status values
        private SlotOccupantStatus occupantStatus;
        private LobbyPlayerData? playerData;
        public SlotOccupantStatus OccupantStatus
        {
            get { return occupantStatus; }
            set
            {
                occupantStatus = value;
                OccupantStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public LobbyPlayerData? PlayerData
        {
            get { return playerData; }
            set
            {
                playerData = value;
                if (playerData != null)
                {
                    value.Side = this.Side;
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
        public string GetPlayerName()
        {
            return playerData == null ? string.Empty : playerData.Name;
        }
        public bool IsPlayerReady()
        {
            return playerData != null ? playerData.IsReady : false;
        }
        #endregion
    }
}

