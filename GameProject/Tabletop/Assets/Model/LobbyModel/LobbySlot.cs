using System;

namespace Model.Lobby
{
    public class LobbySlot
    {
        public event EventHandler? SlotDataChanged;
        public event EventHandler? SlotReadinessChanged;

        private bool isready;

        public string DisplayName { get; private set; }
        public bool IsReady { get; set; }
        public SlotOccupantStatus OccupantStatus { get; private set; }
        public Side Side { get; private set; }

        public LobbySlot(Side side)
        {
            DisplayName = string.Empty;
            IsReady = false;
            OccupantStatus = SlotOccupantStatus.OpenModel;
            Side = side;
        }

        public void ChangeDisplayedData(string newName, SlotOccupantStatus newStatus)
        {
            DisplayName = newName;
            OccupantStatus = newStatus;
            SlotDataChanged?.Invoke(this, EventArgs.Empty);
        }
        public void SetReadiness(bool value)
        {
            IsReady = value;
            SlotReadinessChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

