using System.Collections.Generic;
using System.Linq;

namespace Model.Lobby
{
    public class LobbyModel<TypeId>
    {
        public List<LobbyPlayerData<TypeId>> ConnectedClients { get; private set; }

        public LobbyModel()
        {
            ConnectedClients = new ();
        }

        public bool CanStart()
        {
            bool[] conditions = new bool[]
            {
                ConnectedClients.All(c => c.IsReady),
            };

            return conditions.All(c => c);
        }
    }
}
