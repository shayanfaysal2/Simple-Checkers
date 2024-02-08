using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager instance;

    public GameObject loadingPanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        loadingPanel.SetActive(false);
    }

    public void ShowLoadingScreen(bool show)
    {
        loadingPanel.SetActive(show);
    }
}
