using System;
using System.Security.Cryptography;

namespace Model.Lobby
{
    public class LobbySlot<PlayerIdType>
    {
        public event EventHandler? OccupantStatusChanged;
        public event EventHandler<bool>? PlayerChanged;

        #region Constant values
        public Side Side { get; }
        #endregion

        #region Status values
        private SlotOccupantStatus occupantStatus;
        private LobbyPlayerData<PlayerIdType>? playerData;
        public SlotOccupantStatus OccupantStatus
        {
            get { return occupantStatus; }
            set
            {
                occupantStatus = value;
                OccupantStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public LobbyPlayerData<PlayerIdType>? PlayerData
        {
            get { return playerData; }
            set
            {
                playerData = value;
                if (playerData != null)
                {
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
        public bool GetPlayerId(out PlayerIdType id)
        {
            id = PlayerData == null ? default : PlayerData.ID;
            return PlayerData != null;
        }
        public bool IsPlayerReady()
        {
            return playerData != null ? playerData.IsReady : false;
        }
        #endregion
    }
}

