using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Lobby
{
    public class LobbyPlayerData<TypeId>
    {
        private LobbySlot slot;
        public TypeId ID { get; }
        public string Name { get; }
        public bool IsReady
        {
            get { return slot.IsReady; }
        }
        public Side Side
        {
            get { return slot.Side; }
        }

        public LobbyPlayerData(TypeId id, string name, LobbySlot slot)
        {
            ID = id;
            Name = name;
            this.slot = slot;
            slot.SetDisplayName(Name);
        }
    }
}
