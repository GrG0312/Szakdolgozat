using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Lobby
{
#nullable enable

    /// <summary>
    /// The main model class for the lobby. Contains logic for adding or removing a player, player chaging slots etc.
    /// </summary>
    /// <typeparam name="PlayerIdType">The type which with the player's are represented</typeparam>
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
            if (ConnectedClients.Count == 0)
            {
                throw new TabletopException("There are no connected players");
            }
            if (!ConnectedClients.All(kvp => kvp.Value.IsReady))
            {
                throw new TabletopException("Not all players are ready");
            }
            if (!ConnectedClients.All(kvp => !kvp.Value.Deck.IsEmpty()))
            {
                throw new TabletopException("Not everyone have units selected in their deck");
            }
            if (!AreTeamsEqual())
            {
                throw new TabletopException("The teams are not equal");
            }
            return true;
        }

        #region Adding / removing Players

        /// <summary>
        /// Adds a new player to a reserved slot.
        /// </summary>
        /// <param name="id">ID of the new player</param>
        /// <param name="name">Name of the nem player</param>
        /// <param name="slotId">Which slot to add to</param>
        /// <exception cref="TabletopException"></exception>
        public void AddNewPlayer(PlayerIdType id, string name, int slotId = 0)
        {
            if (ConnectedClients.ContainsKey(id))
            {
                throw new TabletopException("A player already exists with this ID.");
            }
            if (LobbySlots[slotId].OccupantStatus != SlotOccupantStatus.Reserved)
            {
                throw new TabletopException("The slot must be reserved in order to assign a player to it.");
            }
            LobbyPlayerData data = new LobbyPlayerData(name);
            ConnectedClients.Add(id, data);
            LobbySlots[slotId].PlayerData = data;
        }

        /// <summary>
        /// Reassigns a played to an other, open slot
        /// </summary>
        /// <param name="pid">ID of the player to be reassigned</param>
        /// <param name="slotid">Which slot to assign to</param>
        /// <exception cref="TabletopException"></exception>
        public void ReassignPlayerToSlot(PlayerIdType pid, int slotid)
        {
            if (LobbySlots[slotid].OccupantStatus != SlotOccupantStatus.Open)
            {
                throw new TabletopException("Cannot switch to slot that is not open.");
            }
            if (!ConnectedClients.ContainsKey(pid))
            {
                throw new TabletopException("There is no player with the provided ID.");
            }
            LobbyPlayerData data = ConnectedClients[pid];
            LobbySlot oldSlot = LobbySlots.Single(slot => slot.PlayerData == data);
            oldSlot.PlayerData = null;
            LobbySlots[slotid].PlayerData = data;
            data.Deck.Clear();
        }

        /// <summary>
        /// Reserves a slot for a joining player.
        /// </summary>
        /// <returns>If the reservation was successful or not</returns>
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

        /// <summary>
        /// Finds and returns the index of a reserved slot. Returns -1 if there were no reserved slots.
        /// </summary>
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

        /// <summary>
        /// Removes and returns the Data of a player from a slot
        /// </summary>
        public LobbyPlayerData RemovePlayer(PlayerIdType id)
        {
            if (!ConnectedClients.ContainsKey(id))
            {
                throw new TabletopException("There is no player present with the provided ID");
            }
            LobbySlot? slotOfPlayer = GetSlotOfPlayer(id);
            if (slotOfPlayer == null)
            {
                throw new TabletopException("There is no slot that has this player assigned to it");
            }
            ConnectedClients.Remove(id, out LobbyPlayerData data);
            slotOfPlayer.PlayerData = null;
            return data;
        }

        #endregion

        /// <summary>
        /// Returns the slot whose <see cref="LobbySlot.PlayerData"/> equals the player's data object.
        /// Returns null if there is no such slot.
        /// </summary>
        public LobbySlot? GetSlotOfPlayer(PlayerIdType id)
        {
            if (!ConnectedClients.ContainsKey(id))
            {
                throw new TabletopException("There is no player with the provided ID");
            }
            try
            {
                return LobbySlots.Single(slot => slot.PlayerData == ConnectedClients[id]);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool AreTeamsEqual()
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
