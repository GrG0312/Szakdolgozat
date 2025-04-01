using Model.Deck;
using System;

namespace Model.Lobby
{
    public class LobbyPlayerData<TypeId>
    {
        public bool IsReady { get; set; }
        public TypeId ID { get; }
        public string Name { get; }
        public DeckObject Deck { get; }

        public LobbyPlayerData(TypeId id, string name)
        {
            ID = id;
            Name = name;
            IsReady = false;
            Deck = new DeckObject();
        }
    }
}
