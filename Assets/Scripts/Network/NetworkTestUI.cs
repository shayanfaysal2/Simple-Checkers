using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkTestUI : MonoBehaviour
{
    public void StartHost()
    {
        print("Starting as Host");
        NetworkGameManager.instance.StartHost();
        Hide();
    }

    public void StartClient()
    {
        print("Starting as Client");
        NetworkGameManager.instance.StartClient();
        Hide();
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }
}
