using System.Collections.Generic;
using System.Linq;
using Model.Deck;
using System;

namespace Model.Lobby
{
    public class LobbyModel<PlayerIdType> where PlayerIdType : IEquatable<PlayerIdType>
    {
        public List<LobbyPlayerData<PlayerIdType>> ConnectedClients { get; private set; }
        public List<LobbySlot<PlayerIdType>> LobbySlots { get; private set; }
        public Dictionary<PlayerIdType, DeckObject> PlayerDecks { get; private set; }

        public LobbyModel()
        {
            ConnectedClients = new ();
            LobbySlots = new ();
            PlayerDecks = new Dictionary<PlayerIdType, DeckObject>();
        }

        public bool CanStart()
        {
            return ConnectedClients.All(c => c.IsReady && PlayerDecks.GetValueOrDefault(c.ID) != null);
        }

        #region Adding / removing Players

        public void AddPlayer(PlayerIdType id, string name)
        {
            LobbyPlayerData<PlayerIdType> data = new LobbyPlayerData<PlayerIdType>(id, name);
            AddPlayer(data);
        }
        public void AddPlayer(LobbyPlayerData<PlayerIdType> data)
        {
            if (PlayerDecks.ContainsKey(data.ID))
            {
                throw new ArgumentException("A player already exists with this ID!");
            }
            ConnectedClients.Add(data);
            PlayerDecks.Add(data.ID, new DeckObject());
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

        public LobbyPlayerData<PlayerIdType> RemovePlayer(PlayerIdType id)
        {
            LobbyPlayerData<PlayerIdType> data = ConnectedClients.Single(d => d.ID.Equals(id));
            ConnectedClients.Remove(data);
            PlayerDecks.Remove(id);
            return data;
        }

        #endregion

        public void ResetDeckOfPlayer(PlayerIdType id)
        {
            PlayerDecks[id].Reset();
        }
    }
}
