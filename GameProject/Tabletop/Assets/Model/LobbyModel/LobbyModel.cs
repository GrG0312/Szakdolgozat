using System.Collections.Generic;
using System.Linq;
using Model.Deck;
using System;
using Unity.Properties;

namespace Model.Lobby
{
    public class StartException : Exception 
    { 
        public StartException(string message) : base(message) { }
    }
    public class LobbyModel<PlayerIdType> where PlayerIdType : IEquatable<PlayerIdType>
    {
        public const int LOBBY_SIZE = 4;
        public Dictionary<PlayerIdType, LobbyPlayerData> ConnectedClients { get; private set; }
        public List<LobbySlot> LobbySlots { get; private set; }

        public LobbyModel()
        {
            ConnectedClients = new ();
            LobbySlots = new ();
        }

        public bool CanStart()
        {
            if (!ConnectedClients.All(kvp => kvp.Value.IsReady))
            {
                throw new StartException("Not all players are ready");
            }
            if (!ConnectedClients.All(kvp => !kvp.Value.Deck.IsEmpty()))
            {
                throw new StartException("Not everyone have units selected in their deck");
            }
            if (!AreTeamsEqual())
            {
                throw new StartException("The teams are not equal");
            }
            return true;
        }

        #region Adding / removing Players

        public void AddNewPlayer(PlayerIdType id, string name, int slotId = 0)
        {
            if (ConnectedClients.ContainsKey(id))
            {
                throw new ArgumentException("A player already exists with this ID!");
            }
            LobbyPlayerData data = new LobbyPlayerData(name);
            ConnectedClients.Add(id, data);
            LobbySlots[slotId].PlayerData = data;
        }

        public void ReassignPlayerToSlot(PlayerIdType pid, int slotid)
        {
            LobbyPlayerData data = ConnectedClients[pid];
            LobbySlot oldSlot = LobbySlots.Single(slot => slot.PlayerData == data);
            oldSlot.PlayerData = null;
            LobbySlots[slotid].PlayerData = data;
        }

        public bool ReserveEmptySlot()
        {
            foreach (LobbySlot slot in LobbySlots)
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

        public LobbyPlayerData RemovePlayer(PlayerIdType id)
        {
            ConnectedClients.Remove(id, out LobbyPlayerData data);
            LobbySlot slotOfPlayer = LobbySlots.Single(slot => slot.PlayerData == data);
            slotOfPlayer.PlayerData = null;
            return data;
        }

        #endregion

        public void ResetDeckOfPlayer(PlayerIdType id)
        {
            ConnectedClients[id].Deck.Clear();
        }

        public LobbySlot? GetSlotOfPlayer(PlayerIdType id)
        {
            try
            {
                return LobbySlots.Single(slot => slot.PlayerData == ConnectedClients[id]);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool AreTeamsEqual()
        {
            int shouldBe = -1;
            foreach (Side side in Enum.GetValues(typeof(Side)))
            {
                int sum = 0;
                foreach (KeyValuePair<PlayerIdType, LobbyPlayerData> kvp in ConnectedClients)
                {
                    if (kvp.Value.Side == side)
                    {
                        sum++;
                    }
                }
                if (shouldBe == -1)
                {
                    shouldBe = sum;
                } else if (sum != shouldBe)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
