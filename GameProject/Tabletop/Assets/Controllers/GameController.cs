using Model;
using Persistence;
using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Collections.ObjectModel;
using UnityEngine.SceneManagement;
using System.Text;

namespace Controllers
{
    public sealed class GameController : BaseController<GameController>
    {

        public SessionModel CurrentSession { get; private set; }

        public void Awake()
        {
            if (Instance is null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Debug.LogWarning($"Multiple {nameof(GameController)} instances found. Deleting duplicate...");
                Destroy(this.gameObject);
            }

        }
    }
}
