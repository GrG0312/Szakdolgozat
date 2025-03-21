using System;
using System.Security.Cryptography;

namespace Model.Lobby
{
    public class LobbySlot<PlayerIdType>
    {
        public event EventHandler? SlotDataChanged;

        #region Constant values
        public int SlotId { get; }
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
                SlotDataChanged?.Invoke(this, EventArgs.Empty);
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
                    playerData.ReadyChanged += (o, ea) => SlotDataChanged?.Invoke(this, EventArgs.Empty);
                }
                SlotDataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion

        public LobbySlot(int id, Side side)
        {
            SlotId = id;
            OccupantStatus = SlotOccupantStatus.Open;
            Side = side;
            playerData = null;
        }

        #region Getting Player stuff
        public string GetName()
        {
            return playerData == null ? string.Empty : playerData.Name;
        }
        public bool GetPlayerId(out PlayerIdType id)
        {
            id = PlayerData == null ? PlayerData.ID : default;
            return PlayerData == null;
        }
        public bool IsPlayerReady()
        {
            return playerData != null ? playerData.IsReady : false;
        }
        #endregion
    }
}

