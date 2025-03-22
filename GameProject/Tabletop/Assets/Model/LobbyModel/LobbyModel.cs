using System.Collections.Generic;
using System.Linq;

namespace Model.Lobby
{
    public class LobbyModel<PlayerIdType>
    {
        public List<LobbyPlayerData<PlayerIdType>> ConnectedClients { get; private set; }
        public List<LobbySlot<PlayerIdType>> LobbySlots { get; private set; }

        public LobbyModel()
        {
            ConnectedClients = new ();
            LobbySlots = new ();
        }

        public bool CanStart()
        {
            bool[] conditions = new bool[]
            {
                ConnectedClients.All(c => c.IsReady),
            };

            return conditions.All(c => c);
        }
        public bool ReserveEmptySlot()
        {
            foreach (LobbySlot<PlayerIdType> slot in LobbySlots)
            {
                if (slot.OccupantStatus == SlotOccupantStatus.Open)
                {
                    slot.OccupantStatus = SlotOccupantStatus.Reserved;
                    return true;
                }
            }
            return false;
        }
        public int FindReservedSlot()
        {
            for (int i = 0; i < LobbySlots.Count; i++)
            {
                if (LobbySlots[i].OccupantStatus == SlotOccupantStatus.Reserved)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
