using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            print("Created relay with join code: " + joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            //NetworkManager.Singleton.StartHost();
            NetworkGameManager.instance.StartHost();

            LobbyUIManager.instance.StartGame();

            return joinCode;

        }
        catch (RelayServiceException e)
        {
            print(e);
            return null;
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            print("Joining relay with join code: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            //NetworkManager.Singleton.StartClient();
            NetworkGameManager.instance.StartClient();

            LobbyUIManager.instance.StartGame();
        }
        catch (RelayServiceException e)
        {
            print(e);
        }
    }
}
