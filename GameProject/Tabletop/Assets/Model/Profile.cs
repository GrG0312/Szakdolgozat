using System;

namespace Model
{
    [Serializable]
    public class Profile
    {
        [NonSerialized]
        public static readonly Profile Default = new Profile("Player", 0, 0);
        public string DisplayName { get; private set; }
        public int GamesPlayed { get; set; }
        public int GamesWon { get; set; }

        public Profile(string name, int gp, int gw)
        {
            DisplayName = name;
            GamesPlayed = gp;
            GamesWon = gw;
        }

        public void ChangeName(string input)
        {
            if (input == string.Empty)
            {
                return;
            }
            DisplayName = input;
        }
    }
}
