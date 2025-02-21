using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Controller
{
    public class MenuController : MonoBehaviour
    {
        #region Menu Navigation
        public void HostGame()
        {
            if (NetworkManager.Singleton.StartHost())
            {
                throw new UnityException("Could not start a host!");
            }
            // TODO create lobby
            // in the lobby the players have to select decks, set ready status, then the host can start the game
        }

        public void JoinGame()
        {
            
            NetworkManager.Singleton.StartClient();
            // TODO join lobby by IP + Port, select deck, ready up, await start
            throw new NotImplementedException(nameof(JoinGame));
        }

        public void ViewProfile()
        {

        }

        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();
        }
        #endregion
    }
}
