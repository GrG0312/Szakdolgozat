using UnityEngine;
using System;

namespace Controllers.Objects
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