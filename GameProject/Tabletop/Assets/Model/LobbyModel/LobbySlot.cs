using System;

namespace Model.Lobby
{
    public class LobbySlot
    {
        public event EventHandler? SlotDataChanged;

        private bool isready;

        public string DisplayName { get; private set; }
        public bool IsReady { get; private set; }
        public SlotOccupantStatus OccupantStatus { get; private set; }
        public Side Side { get; private set; }

        public LobbySlot(Side side)
        {
            DisplayName = string.Empty;
            IsReady = false;
            OccupantStatus = SlotOccupantStatus.OpenModel;
            Side = side;
        }

        public void ChangeData(string newName, SlotOccupantStatus newStatus, bool isReady = false)
        {
            DisplayName = newName;
            OccupantStatus = newStatus;
            IsReady = isready;
            SlotDataChanged?.Invoke(this, EventArgs.Empty);
        }
        public void SetReadiness(bool value)
        {
            IsReady = value;
        }
    }
}

