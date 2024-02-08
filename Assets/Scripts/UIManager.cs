using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using _Project.Scripts.Authentication.Firebase;

public class UIManager : MonoBehaviour
{
    public Sprite[] defaultAvatars;

    //main menu
    public Text welcomeText;

    //name input
    public GameObject nameInputPanel;

    //profile
    public GameObject profilePanel;
    public Image profilePhoto;
    public InputField profileNameInputfield;
    public Text levelText;
    public Text xpText;
    public Text winsText;
    public Text lossesText;

    //avatar selection
    public GameObject avatarSelectionPanel;

    //leaderboard
    public GameObject rowPrefab;
    public Transform rowParent;

    //settings
    public Slider volumeSlider;

    //extra
    private GameObject signUpPanel;
    private GameObject signInPanel;
    private InputField signUpEmailInputField;
    private InputField signUpPasswordInputField;
    private InputField signInEmailInputField;
    private InputField signInPasswordInputField;

    private void OnEnable()
    {
        EventManager.OnLoginSuccessful += InitializeMenu;
        EventManager.OnFetchUserDataSuccessful += InitializeProfile;
        EventManager.OnDisplayNameSet += DisplayUsername;
    }

    private void OnDisable()
    {
        EventManager.OnLoginSuccessful -= InitializeMenu;
        EventManager.OnFetchUserDataSuccessful -= InitializeProfile;
        EventManager.OnDisplayNameSet -= DisplayUsername;
    }

    private void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("volume", 0.5f);
        volumeSlider.onValueChanged.AddListener(delegate { SetVolume(); });

        DisplayUsername();

        if (FirebaseDBManager.instance.playerData != null)
            InitializeProfile();
    }

    void InitializeMenu()
    {
        //hide loading screen
        LoadingManager.instance.ShowLoadingScreen(false);

        //open name input panel if display name not set
        if (string.IsNullOrEmpty(FirebaseAuthenticator.Instance.DisplayName))
        {
            nameInputPanel.gameObject.SetActive(true);
        }
        //else show that name
        else
        {
            DisplayUsername();
        }
    }

    void InitializeProfile()
    {
        //set level text
        levelText.text = "Level: " + FirebaseDBManager.instance.playerData.level.ToString();

        //set xp text
        xpText.text = "XP: " + FirebaseDBManager.instance.playerData.xp.ToString();

        //set wins text
        winsText.text = "Wins: " + FirebaseDBManager.instance.playerData.wins.ToString();

        //set losses text
        lossesText.text = "Losses: " + FirebaseDBManager.instance.playerData.losses.ToString();

        //set avatar
        profilePhoto.sprite = defaultAvatars[PlayerPrefs.GetInt("avatar", 0)];
    }

    public void SetVolume()
    {
        AudioListener.volume = volumeSlider.value;
        PlayerPrefs.SetFloat("volume", AudioListener.volume);
    }

    public void SetUsername(InputField inputField)
    {
        string oldUsername = FirebaseAuthenticator.Instance.DisplayName;
        string newUsername = inputField.text;

        if (oldUsername != newUsername)
            FirebaseAuthenticator.Instance.UpdateDisplayName(newUsername);
    }

    private void DisplayUsername()
    {
        welcomeText.text = "Welcome, " + FirebaseAuthenticator.Instance.DisplayName + "!";
        profileNameInputfield.text = FirebaseAuthenticator.Instance.DisplayName;
    }

    public void SelectAvatar(int index)
    {
        profilePhoto.sprite = defaultAvatars[index];
        PlayerPrefs.SetInt("avatar", index);

        avatarSelectionPanel.SetActive(false);
        profilePanel.SetActive(true);

        //unlock achievement
        EventManager.UnlockAchievement("change_avatar");
    }

    public void UpdateUserPhoto(Texture2D tex)
    {
        profilePhoto.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
    }

    public void DisplayLeaderboard()
    {
        //clearing
        foreach(Transform child in rowParent)
            Destroy(child.gameObject);

        PlayerData[] leaderboard = FirebaseDBManager.instance.leaderboard;

        //displaying
        for (int i = 0; i < leaderboard.Length; i++)
        {
            GameObject newRow = Instantiate(rowPrefab, rowParent);
            newRow.transform.GetChild(0).GetComponent<Text>().text = (i + 1).ToString();
            newRow.transform.GetChild(1).GetComponent<Text>().text = leaderboard[i].displayName.ToString();
            newRow.transform.GetChild(2).GetComponent<Text>().text = leaderboard[i].wins.ToString();
            newRow.transform.GetChild(3).GetComponent<Text>().text = leaderboard[i].losses.ToString();
        }
    }

    public void StartGame(int gameMode)
    {
        //0 = singleplayer, 1 = multiplayer, 2 = ai vs ai
        PlayerPrefs.SetInt("gameMode", gameMode);
        SceneManager.LoadScene(1);
    }

    public void StartMultiplayer()
    {
        SceneManager.LoadScene(2);
    }

    public void SignUp()
    {
        FirebaseAuthManager.instance.SignUpUser(signUpEmailInputField.text, signUpPasswordInputField.text);
    }

    public void SignIn()
    {
        FirebaseAuthManager.instance.SignInUser(signInEmailInputField.text, signInPasswordInputField.text);
    }
}