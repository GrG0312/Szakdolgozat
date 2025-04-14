using UnityEngine;
using TMPro;
using Controllers.Objects;
using Controllers.Data;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace Controllers
{
    public class MainMenuController : MenuControllerBase<MainMenuController>
    {
        [SerializeField] private TMP_InputField ipInputField;
        [SerializeField] private GameObject networkManagerPrefab;

        #region Menu Navigation
        public void SwitchToJoin() => ChangeScreen(ScreenType.Joining);
        public void SwitchToProfile() => ChangeScreen(ScreenType.Profile);
        public void TryToConnect()
        {
            if (NetworkManager.Singleton == null)
            {
                Instantiate(networkManagerPrefab);
            }
            InterSceneData.ShouldHost = false;
            InterSceneData.ConnectionAddress = ipInputField.text;
            SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
        }
        public void HostGame()
        {
            if (NetworkManager.Singleton == null)
            {
                Instantiate(networkManagerPrefab);
            }
            InterSceneData.ShouldHost = true;
            SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
        }
        #endregion

        private void Start()
        {
            // Set the starting screen
            ChangeScreen(ScreenType.Main);
        }
    }
}
