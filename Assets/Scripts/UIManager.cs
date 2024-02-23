using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using _Project.Scripts.Authentication.Firebase;
using TRex.adplugin;
using System;
using _Project.Scripts.Authentication;

public class UIManager : MonoBehaviour
{
    public Sprite[] defaultAvatars;

    //main menu
    public TextMeshProUGUI welcomeText;
    public TextMeshProUGUI menuLevelText;
    public Slider menuXpSlider;

    //name input
    public GameObject nameInputPanel;

    //profile
    public GameObject profilePanel;
    public Image profilePhoto;
    public TextMeshProUGUI profileNameText;
    public TMP_InputField profileNameInputfield;
    public TextMeshProUGUI levelText;
    public Slider xpSlider;
    public TextMeshProUGUI winsText;
    public TextMeshProUGUI lossesText;

    //avatar selection
    public GameObject avatarSelectionPanel;

    //achievements
    public GameObject achievementRowPrefab;
    public Transform achievementRowParent;

    //leaderboard
    public Transform playerRow;
    public GameObject leaderboardRowPrefab;
    public Transform leaderboardRowParent;

    //settings
    public Slider volumeSlider;
    public TextMeshProUGUI userIdText;
    public TextMeshProUGUI linkButtonText;

    //extra
    private GameObject signUpPanel;
    private GameObject signInPanel;
    private InputField signUpEmailInputField;
    private InputField signUpPasswordInputField;
    private InputField signInEmailInputField;
    private InputField signInPasswordInputField;

    //strings
    private readonly string _googleProvider = "google.com";
    private readonly string _facebookProvider = "facebook.com";
    private readonly string _customProvider = "password";

    private void OnEnable()
    {
        EventManager.OnLoginSuccessful += InitializeMenu;
        EventManager.OnFetchUserDataSuccessful += InitializeProfile;
        EventManager.OnFetchLeaderboardSuccessful += DisplayLeaderboard;
        EventManager.OnDisplayNameSet += DisplayUsername;
        Authenticator.OnAuthenticationSuccessful += ChangeButtonText;
        Authenticator.OnAccountLinked += ChangeButtonText;
        Authenticator.OnAccountUnlinked += ChangeButtonText;
    }

    private void OnDisable()
    {
        EventManager.OnLoginSuccessful -= InitializeMenu;
        EventManager.OnFetchUserDataSuccessful -= InitializeProfile;
        EventManager.OnFetchLeaderboardSuccessful -= DisplayLeaderboard;
        EventManager.OnDisplayNameSet -= DisplayUsername;
        Authenticator.OnAuthenticationSuccessful -= ChangeButtonText;
        Authenticator.OnAccountLinked -= ChangeButtonText;
        Authenticator.OnAccountUnlinked -= ChangeButtonText;
    }

    private void Awake()
    {

    }

    private IEnumerator Start()
    {
        //60 fps
        Application.targetFrameRate = 60;

        AdsManager.Instance.Initialize(null);

        yield return null;

        volumeSlider.value = PlayerPrefs.GetFloat("volume", 0.5f);

        //show rewarded ad
        //AdsManager.Instance.ShowRewarded(true, Callback);

        InitializeProfile();
    }

    /*private void Callback(ADStatus status)
    {
        if (status == ADStatus.SKIPPED)
        {
            print("rewarded closed");
        }
    }*/

    void InitializeMenu()
    {
        //open name input panel if display name not set
        if (string.IsNullOrEmpty(FirebaseAuthenticatorHandler.Instance.DisplayName))
        {
            nameInputPanel.gameObject.SetActive(true);
        }
        //else show that name
        else
        {
            DisplayUsername();
        }

        userIdText.text = "User ID:\n" + FirebaseAuthenticatorHandler.Instance.UserId;
    }

    public void InitializeProfile()
    {
        if (FirebaseDBManager.instance.playerData != null)
        {
            //set name text
            profileNameText.text = FirebaseAuthenticatorHandler.Instance.DisplayName;

            //set xp sliders
            int lev = FirebaseDBManager.instance.playerData.level;
            int xp = FirebaseDBManager.instance.playerData.xp;
            int minXp;
            if (lev > 0)
                minXp = XPManager.instance.GetXPBracket(lev - 1);
            else
                minXp = 0;
            int maxXp = XPManager.instance.GetXPBracket(lev);
            xpSlider.minValue = minXp;
            menuXpSlider.minValue = minXp;
            xpSlider.maxValue = maxXp;
            menuXpSlider.maxValue = maxXp;
            xpSlider.value = xp;
            menuXpSlider.value = xp;

            //set level text
            menuLevelText.text = FirebaseDBManager.instance.playerData.level.ToString();
            levelText.text = FirebaseDBManager.instance.playerData.level.ToString();

            //set wins text
            winsText.text = FirebaseDBManager.instance.playerData.wins.ToString();

            //set losses text
            lossesText.text = FirebaseDBManager.instance.playerData.losses.ToString();

            //set avatar
            profilePhoto.sprite = defaultAvatars[PlayerPrefs.GetInt("avatar", 0)];
        }
    }

    public void SetVolume(float vol)
    {
        AudioListener.volume = vol;
        PlayerPrefs.SetFloat("volume", AudioListener.volume);
    }

    public void SetUsername(TMP_InputField inputField)
    {
        string oldUsername = FirebaseAuthenticatorHandler.Instance.DisplayName;
        string newUsername = inputField.text;

        if (oldUsername != newUsername)
        {
            FirebaseAuthenticatorHandler.Instance.UpdateDisplayName(newUsername);

            welcomeText.text = newUsername;
            profileNameText.text = newUsername;

            //unlock achievement
            EventManager.UnlockAchievement("change_username");
        }
    }

    private void DisplayUsername()
    {
        welcomeText.text = FirebaseAuthenticatorHandler.Instance.DisplayName;
        profileNameText.text = FirebaseAuthenticatorHandler.Instance.DisplayName;
        profileNameInputfield.text = FirebaseAuthenticatorHandler.Instance.DisplayName;
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

    public void DisplayAchievements()
    {
        //clearing
        foreach (Transform child in achievementRowParent)
            Destroy(child.gameObject);

        if (FirebaseDBManager.instance.playerData == null)
            return;

        List<string> unlockedAchievements = FirebaseDBManager.instance.playerData.achievements;

        foreach(Achievement achievement in AchievementManager.instance.achievements)
        {
            GameObject newRow = Instantiate(achievementRowPrefab, achievementRowParent);
            newRow.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = achievement.title;
            if (unlockedAchievements.Contains(achievement.id))
            {
                newRow.transform.GetChild(2).gameObject.SetActive(true);
                //newRow.transform.GetChild(1).GetComponent<Text>().text = "Unlocked";
            }
            else
            {
                newRow.transform.GetChild(3).gameObject.SetActive(true);
                //newRow.transform.GetChild(1).GetComponent<Text>().text = "Locked";
            }
        }

        /*string txt = "";
        foreach(string achievement in achievements)
        {
            Achievement a = AchievementManager.GetAchievement(achievement);
            txt += a.title + "\n";
        }

        achievementsText.text = txt;*/
    }


    public void DisplayLeaderboard()
    {
        //clearing
        foreach(Transform child in leaderboardRowParent)
            Destroy(child.gameObject);

        if (FirebaseDBManager.instance.leaderboard == null)
            return;

        PlayerData[] leaderboard = FirebaseDBManager.instance.leaderboard;

        //displaying
        for (int i = 0; i < leaderboard.Length; i++)
        {
            Transform row;
            if (leaderboard[i].displayName == FirebaseAuthenticatorHandler.Instance.DisplayName)
                row = playerRow;
            else
                row = Instantiate(leaderboardRowPrefab, leaderboardRowParent).transform;

            row.GetChild(6).GetComponent<TextMeshProUGUI>().text = leaderboard[i].displayName;
            row.GetChild(7).GetComponent<TextMeshProUGUI>().text = leaderboard[i].wins.ToString();

            //rank
            if (i >= 0 && i < 3)
            {
                row.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                var t = row.GetChild(3);
                t.gameObject.SetActive(true);
                t.GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
            }
        }
    }

    public void LoadProfile()
    {
        FirebaseDBManager.instance.LoadPlayerData();
    }

    public void LoadLeaderboard()
    {
        FirebaseDBManager.instance.LoadLeaderboard();
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
        //FirebaseAuthManager.instance.SignUpUser(signUpEmailInputField.text, signUpPasswordInputField.text);
    }

    public void SignIn()
    {
        //FirebaseAuthManager.instance.SignInUser(signInEmailInputField.text, signInPasswordInputField.text);
    }

    public void LinkOrUnlinkWithGoogle()
    {
        AuthenticationData authenticatedData = Authenticator.Instance.AuthenticatedData;
        if (authenticatedData.Providers.Count != 0 && authenticatedData.Providers.Contains(_googleProvider))
        {
            Authenticator.Instance.UnlinkFromGoogle();
        }
        else
        {
            Authenticator.Instance.LinkToGoogle();
        }
    }

    private void ChangeButtonText(AuthenticationData authenticationData)
    {
        linkButtonText.SetText($"{(authenticationData.Providers.Count != 0 && authenticationData.Providers.Contains(_googleProvider) ? "Unlink Account" : "Link Account")}");
    }
}