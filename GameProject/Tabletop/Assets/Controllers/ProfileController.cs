using Persistence;
using Model;
using System;
using UnityEngine;

namespace Controllers
{
    public class ProfileController : BaseController<ProfileController>
    {
        #region Profiling

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
            UserProfile.GamesPlayed++;
            if (didWin)
            {
                UserProfile.GamesWon++;
            }
            ProfileUpdated?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Unity Messages
        private void Awake()
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
            DontDestroyOnLoad(gameObject);
        }
        private void OnApplicationQuit()
        {
            profileDataManager.Save(UserProfile);
        }
        #endregion
    }
}
