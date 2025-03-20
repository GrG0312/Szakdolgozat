using Persistence;
using System;
using UnityEngine;

namespace Controllers
{
    public class ProfileController : BaseController<ProfileController>
    {

        private IDataManager<Profile> profileDataManager;
        private Profile UserProfile { get; set; }

        public event EventHandler ProfileUpdated;

        public string DisplayName
        {
            get { return UserProfile.DisplayName; }
            set { UserProfile.ChangeName(value); ProfileUpdated?.Invoke(this, EventArgs.Empty); }
        }
        public int GamesPlayed
        {
            get { return UserProfile.GamesPlayed; }
        }
        public int GamesWon
        {
            get { return UserProfile.GamesWon; }
        }

        public void GameFinished(bool didWin)
        {
            UserProfile.AddPlayedGame();
            if (didWin)
            {
                UserProfile.AddWonGame();
            }
            ProfileUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            } else
            {
                Debug.LogWarning($"Multiple {nameof(ProfileController)} instances found. Deleting duplicate...");
                Destroy(this.gameObject);
            }

            profileDataManager = new ProfileDataManager(Application.persistentDataPath);
            UserProfile = profileDataManager.Load();
            DontDestroyOnLoad(this);
        }
        public void OnApplicationQuit()
        {
            profileDataManager.Save(UserProfile);
        }
    }
}
