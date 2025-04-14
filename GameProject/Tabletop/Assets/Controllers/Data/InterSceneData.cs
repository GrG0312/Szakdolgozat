using Model;
using Model.GameModel;
using Model.Lobby;
using System.Collections.Generic;

namespace Controllers.Data
{
    internal static class InterSceneData
    {
        public static bool ShouldHost = false;
        public static string ConnectionAddress = "127.0.0.1";

        public static Dictionary<ulong, GamePlayerData> Players { get; private set; }

        public static void Reset()
        {
            ShouldHost = false;
            ConnectionAddress = "127.0.0.1";
            Players = null;
        }

        public static void ConvertLobbyData(Dictionary<ulong, LobbyPlayerData> lobbyData)
        {
            Players = new Dictionary<ulong, GamePlayerData>();
            foreach (KeyValuePair<ulong, LobbyPlayerData> item in lobbyData)
            {
                GamePlayerData data = new GamePlayerData(item.Value.Name, item.Value.Deck, item.Value.Side);
                Players.Add(item.Key, data);
            }
        }
    }
}
