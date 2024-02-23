using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private bool isLobbyHost = false;
    private bool gameStarted = false;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0)
            {
                float lobbyUpdateTimerMax = 2f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;

                print(joinedLobby.Name + " " + joinedLobby.Players.Count + "/" + joinedLobby.MaxPlayers);

                if (isLobbyHost && !gameStarted && joinedLobby.Players.Count == joinedLobby.MaxPlayers)
                {
                    StartGame();
                }

                if (joinedLobby.Data != null)
                {
                    if (!isLobbyHost && joinedLobby.Data["StartGame"].Value != "0")
                    {
                        RelayManager.instance.JoinRelay(joinedLobby.Data["StartGame"].Value);
                        LeaveLobby();
                        joinedLobby = null;
                    }
                }
            }
        }
    }

    public async Task<string> CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            int maxPlayers = 2;
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject>
                {
                    {"StartGame", new DataObject(DataObject.VisibilityOptions.Member, "0")}
                }
            };
            
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);
            hostLobby = lobby;
            joinedLobby = hostLobby;
            isLobbyHost = true;
            print("Created lobby! " + lobby.Name + " " + lobby.Players.Count + "/" + lobby.MaxPlayers);

            print("Join code: " + lobby.LobbyCode);
            LobbyUIManager.instance.SetJoinCodeText(lobby.LobbyCode);

            //string joinCode = await RelayManager.instance.CreateRelay();
            //print("Join code: " + joinCode);

            return string.Empty;
        }
        catch (LobbyServiceException e)
        {
            print(e);
            return e.Reason.ToString();
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            print("Found " + queryResponse.Results.Count + " lobbies");
            foreach (Lobby lobby in queryResponse.Results)
            {
                print(lobby.Name + " " + lobby.Players.Count + "/" + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            print(e);
        }
    }

    public async Task<string> JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
            joinedLobby = lobby;
            print("Joined lobby with code " + lobby.LobbyCode);
            return string.Empty;
        }
        catch (LobbyServiceException e)
        {
            print(e);
            return e.Reason.ToString();
        }
    }

    public async Task<string> QuickJoinLobby()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            joinedLobby = lobby;
            print("Quick joined lobby with code " + lobby.LobbyCode);
            return string.Empty;
        }
        catch (LobbyServiceException e)
        {
            print(e);
            return e.Reason.ToString();
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            print(e);
        }
    }

    public async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch(LobbyServiceException e)
        {
            print(e);
        }
    }

    public async void StartGame()
    {
        try
        {
            string relayCode = await RelayManager.instance.CreateRelay();
            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "StartGame", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            });
            gameStarted = true;
            print("Started game!");

            LeaveLobby();
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            print(e);
        }
    }
}
