using System;
using System.Collections.Generic;
using UnityEngine;
using View;
using TMPro;

namespace Controller
{
    public class UIController : MonoBehaviour
    {
        // Reference to the prefab of NetworkManager used in initialization
        [SerializeField] private GameObject menuParent;
        [SerializeField] private TMP_InputField ipInputField;

        private readonly Stack<ScreenType> screenStack = new Stack<ScreenType>();

        #region Menu Navigation

        #region Buttons
        public void HostGame()
        {
            if (SessionController.Instance.CreateSession())
            {
                // TODO display lobby
                // in the lobby the players have to select decks, set ready status, then the host can start the game
                ChangeScreen(ScreenType.Lobby);

            } else
            {
                throw new UnityException("Could not start a host!");
            }
        }

        public void JoinGame()
        {
            ChangeScreen(ScreenType.Joining);
            // TODO join lobby by IP + Port, select deck, ready up, await start
        }

        public void ConnectToGame()
        {
            Debug.Log($"Connecting to {ipInputField.text}...");
            if (SessionController.Instance.JoinSession(ipInputField.text))
            {
                ChangeScreen(ScreenType.Lobby);
                Debug.Log($"Connecting to {ipInputField.text}...");
            }

        }

        public void ViewProfile()
        {

        }

        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }

        public void Back()
        {
            if(screenStack.Pop() == ScreenType.Lobby)
            {
                SessionController.Instance.DestroySession();
            }

            try
            {
                ScreenType previousScreen = screenStack.Peek();
                ChangeScreen(previousScreen);
            } catch (InvalidOperationException)
            {
                Debug.LogError("The stack was empty. Navigated to main menu!");
                ChangeScreen(ScreenType.Main);
            }

        }
        #endregion

        private void ChangeScreen(ScreenType type)
        {


            screenStack.Push(type);

            foreach (Transform child in menuParent.transform)
            {
                if (child.gameObject.TryGetComponent<MenuElement>(out MenuElement element))
                {
                    child.gameObject.SetActive(element.ScreenType == type);
                } 
            }
        }
        #endregion

        private void Start()
        {
            screenStack.Push(ScreenType.Main);
        }
    }
}
