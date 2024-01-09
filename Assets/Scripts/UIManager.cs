using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Zubash
{
    public class UIManager : MonoBehaviour
    {
        public GameObject nameInputPanel;
        public InputField nameInputField;
        public TMP_InputField nameInputField2;
        public TMP_Text welcomeText;
        public GameObject settingsPanel;
        public Button SettingsBtn;

        //public AudioManager audioManager;
        public Slider volumeSlider;

        private void Start()
        {
            UpdateWelcomeText();

            if (volumeSlider != null)
            {
                volumeSlider.value = PlayerPrefs.GetFloat("volume", 0.5f);
                volumeSlider.onValueChanged.AddListener(delegate { SetVolume(); });
                //volumeSlider.value = audioManager.GetVolume(); // Set slider value to current volume
            }
        }

        public void SetVolume()
        {
            //if (audioManager != null && volumeSlider != null)
            {
                AudioListener.volume = volumeSlider.value;
                PlayerPrefs.SetFloat("volume", AudioListener.volume);
                //audioManager.SetVolume(volumeSlider.value);
            }
        }

        private void UpdateWelcomeText()
        {
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("UserName")))
            {
                nameInputPanel.gameObject.SetActive(true);
                settingsPanel.SetActive(false);
            }
            else
            {
                nameInputPanel.gameObject.SetActive(false);
                settingsPanel.SetActive(false); // Ensure settings panel is hidden on start
                string userName = PlayerPrefs.GetString("UserName");
                welcomeText.text = "Welcome, " + userName + "!";
                nameInputField2.text = userName;
            }
        }

        public void SaveUserName()
        {
            // Save the entered name in PlayerPrefs (persistent data)
            string userName = nameInputField.text;
            PlayerPrefs.SetString("UserName", userName);
            SetPlaceholder();
            welcomeText.text = "Welcome, " + userName + "!";
        }

        public void OpenSettingsPanel()
        {
            settingsPanel.SetActive(true);
        }

        public void CloseSettingsPanel()
        {
            settingsPanel.SetActive(false);
        }

        public void UpdateUserName()
        {
            string newUserName = nameInputField2.text;
            PlayerPrefs.SetString("UserName", newUserName);
            CloseSettingsPanel();
        }

        private void SetPlaceholder()
        {
            string existingName = PlayerPrefs.GetString("UserName");
            nameInputField2.text = existingName;
        }

        public void StartSingleplayer()
        {
            PlayerPrefs.SetInt("gameMode", 0);
            SceneManager.LoadScene(1);
        }

        public void StartMultiplayer()
        {
            PlayerPrefs.SetInt("gameMode", 1);
            SceneManager.LoadScene(1);
        }

        public void StartAIvsAI()
        {
            PlayerPrefs.SetInt("gameMode", 2);
            SceneManager.LoadScene(1);
        }
    }
}
