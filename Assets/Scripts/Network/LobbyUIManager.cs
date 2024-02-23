using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _Project.Scripts.Authentication.Firebase;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager instance;

    //panels
    public GameObject lobbyPanel;
    public GameObject joinCodePanel;

    //create lobby
    public InputField lobbyNameInputField;
    public Toggle privateToggle;

    //join lobby
    public TMP_InputField joinCodeInputField;

    //join panel
    public Text joinCodeText;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public async void CreateLobby()
    {
        //LobbyManager.instance.CreateLobby(lobbyNameInputField.text, false);
        string result = await LobbyManager.instance.CreateLobby(FirebaseAuthenticatorHandler.Instance.DisplayName, false);
        if (!string.IsNullOrEmpty(result))
        {
            PopUpManager.instance.CreatePopUp(result);
        }
    }

    public async void JoinLobby()
    {
        //RelayManager.instance.JoinRelay(joinCodeInputField.text);

        if (string.IsNullOrEmpty(joinCodeInputField.text))
        {
            PopUpManager.instance.CreatePopUp("Enter join code!");
        }
        else
        {
            string result = await LobbyManager.instance.JoinLobbyByCode(joinCodeInputField.text);
            if (!string.IsNullOrEmpty(result))
            {
                PopUpManager.instance.CreatePopUp(result);
            }
        }
    }

    public async void QuickJoinLobby()
    {
        string result = await LobbyManager.instance.QuickJoinLobby();
        if (!string.IsNullOrEmpty(result))
        {
            PopUpManager.instance.CreatePopUp(result);
        }
    }

    public void SetJoinCodeText(string joinCode)
    {
        lobbyPanel.SetActive(false);
        joinCodePanel.SetActive(true);
        joinCodeText.text = "Join Code:\n" + joinCode;
    }

    public void StartGame()
    {
        lobbyPanel.SetActive(false);
        joinCodePanel.SetActive(false);
    }
}
