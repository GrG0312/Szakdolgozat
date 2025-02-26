using Model;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Controller
{
    /// <summary>
    /// The class is singleton. In order to access it, use <see cref="Instance"/>.
    /// </summary>
    public sealed partial class SessionController : MonoBehaviour
    {
        [SerializeField] private GameObject networkManagerPrefab;

        #region Singleton
        public static SessionController Instance { get; private set; }
        private SessionController() { }
        #endregion

        public SessionModel CurrentSession { get; private set; }

        public bool CreateSession()
        {
            // The port 35420 is, by default, unassigned
            // By setting the listening port to 0.0.0.0, it will listen to every IP
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 35420, "0.0.0.0");
            return NetworkManager.Singleton.StartHost();
        }
        public bool JoinSession(string ipAddress)
        {
            if (!IPv4Check(ipAddress))
            {
                return false;
            }
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, 35420);
            return NetworkManager.Singleton.StartClient();
        }
        public void DestroySession()
        {
            NetworkManager.Singleton.Shutdown();
        }

        private bool IPv4Check(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }

            string[] splitted = input.Split('.');
            if (splitted.Length != 4)
            {
                return false;
            }

            return splitted.All(r => byte.TryParse(r, out byte temp));
        }

        public void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Found an already existing SessionController. Deleting duplicate...");
                Destroy(this);
            }
            Instance = this;
            DontDestroyOnLoad(this);

            if (NetworkManager.Singleton is null)
            {
                Debug.Log("Creating network manager...");
                Instantiate(networkManagerPrefab);
            }
        }

    }
}
