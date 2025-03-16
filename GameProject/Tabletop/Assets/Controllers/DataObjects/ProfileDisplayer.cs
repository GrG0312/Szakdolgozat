using System;
using TMPro;
using UnityEngine;

namespace Controllers.DataObjects
{
    public class ProfileDisplayer : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_Text gamePlayed;
        [SerializeField] private TMP_Text gameWon;
        private void Start()
        {
            ProfileController.Instance.ProfileUpdated += (object o, EventArgs e) => UpdateFields();
            nameInput.onEndEdit.AddListener((v) => ProfileController.Instance.DisplayName = nameInput.text);
            UpdateFields();
        }

        private void UpdateFields()
        {
            nameInput.text = ProfileController.Instance.DisplayName;
            gamePlayed.text = ProfileController.Instance.GamesPlayed.ToString();
            gameWon.text = ProfileController.Instance.GamesWon.ToString();
        }
    }
}
