using Model.Deck;

namespace Model
{
    public class LobbyPlayerData
    {
        public bool IsReady { get; set; }
        public string Name { get; }
        public DeckObject Deck { get; }
        public Side Side { get; set; }

        public LobbyPlayerData(string name)
        {
            Name = name;
            IsReady = false;
            Deck = new DeckObject();
        }
    }
}
