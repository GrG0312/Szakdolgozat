using System;

namespace Persistence
{
    [Serializable]
    public class Profile
    {
        [NonSerialized]
        public static Profile Default = new Profile("Player", 0, 0);
        public string DisplayName { get; private set; }
        public int GamesPlayed { get; private set; }
        public int GamesWon { get; private set; }

        public Profile(string name, int gp, int gw)
        {
            DisplayName = name;
            GamesPlayed = gp;
            GamesWon = gw;
        }

        public void ChangeName(string input)
        {
            DisplayName = input;
        }
        public void AddPlayedGame()
        {
            GamesPlayed++;
        }
        public void AddWonGame()
        {
            GamesWon++;
        }
    }
}
