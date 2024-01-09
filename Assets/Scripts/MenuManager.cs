using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject nameInputPanel;
    [SerializeField] private InputField settingsNameInputField;
    [SerializeField] private Slider volumeSlider;

    private const string usernameKey = "username";
    private const string volumeKey = "volume";

    private void Start()
    {
        //open name input panel if first time
        if (!PlayerPrefs.HasKey(usernameKey))
            nameInputPanel.SetActive(true);

        RefreshSettings();
    }

    //update name and slider values
    private void RefreshSettings()
    {
        settingsNameInputField.text = PlayerPrefs.GetString(usernameKey, "");
        volumeSlider.value = PlayerPrefs.GetFloat(volumeKey, 1);
    }

    //called from ok button in name input panel or settings panel
    public void SetUsername(InputField inputField)
    {
        string newUsername = inputField.text;
        if (!string.IsNullOrEmpty(newUsername))
        {
            PlayerPrefs.SetString(usernameKey, newUsername);
            RefreshSettings();
        } 
    }

    //dynamic float function called from volume slider
    public void SetVolume(float vol)
    {
        AudioListener.volume = vol;
        PlayerPrefs.SetFloat(volumeKey, vol);
        RefreshSettings();
    }

    //called from multiplayer button
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
}