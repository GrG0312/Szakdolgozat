using UnityEngine;
using System;

namespace Controllers.Objects.Lobby
{
    public class DeckParent : MonoBehaviour
    {
        public event EventHandler? OnActivated;
        private void OnEnable()
        {
            OnActivated?.Invoke(this, EventArgs.Empty);
        }
    }
}