using System.Collections.ObjectModel;
using System;

namespace Model.Lobby
{
    public enum SlotOccupantStatus
    {
        /// <summary>
        /// A slot is open, when there is no player assigned to it and player can join or switch into it.
        /// </summary>
        Open,
        /// <summary>
        /// A reserved slot is haflway between a Closed and an Open slot,
        /// as there is no player assigned to it yet, but it is reserved for a currently joining player.
        /// For players other than the joining, the slot will be seen as Closed.
        /// </summary>
        Reserved,
        /// <summary>
        /// A Closed slot doesnt contains a player, but players are also unable to join or switch into it. This state can be used to limit the size of the lobby.
        /// </summary>
        Closed,
        /// <summary>
        /// A slot is occupied when a player is assigned to it.
        /// </summary>
        Occupied,
    }
}
