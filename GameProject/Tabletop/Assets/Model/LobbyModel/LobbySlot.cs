using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Lobby
{
    public class LobbySlot
    {
        public string DisplayName { get; private set; }
        public bool IsReady { get; set; }
        public bool IsOpen { get; private set; }
        public Side Side { get; private set; }

        public LobbySlot(Side side)
        {
            DisplayName = string.Empty;
            IsReady = false;
            IsOpen = true;
            Side = side;
        }

        public void SetDisplayName(string name)
        {
            DisplayName = name;
        }
    }
}

