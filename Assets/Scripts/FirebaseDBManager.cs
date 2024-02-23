using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using _Project.Scripts.Authentication.Firebase;

public class FirebaseDBManager : MonoBehaviour
{
    public static FirebaseDBManager instance;

    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

    DatabaseReference databaseReference;

    public PlayerData[] leaderboard;
    public PlayerData playerData;

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

    IEnumerator Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

        yield return new WaitForSeconds(1);

        LoadPlayerData();
        LoadLeaderboard();
    }

    // Initialize the Firebase database:
    protected virtual void InitializeFirebase()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    [ContextMenu("IncreasePlayerWins")]
    public void IncreasePlayerWins()
    {
        if (playerData != null)
        {
            playerData.wins++;
            SavePlayerData();
        }
        else
        {
            print("Player data not loaded!");
        }
    }

    [ContextMenu("IncreasePlayerLosses")]
    public void IncreasePlayerLosses()
    {
        if (playerData != null)
        {
            playerData.losses++;
            SavePlayerData();
        }
        else
        {
            print("Player data not loaded!");
        }
    }

    public void IncreasePlayerXP(int xpGain)
    {
        if (playerData != null)
        {
            playerData.xp += xpGain;
            SavePlayerData();
        }
        else
        {
            print("Player data not loaded");
        }
    }

    [ContextMenu("IncreasePlayerLevel")]
    public void IncreasePlayerLevel()
    {
        if (playerData != null)
        {
            playerData.level++;
            SavePlayerData();
        }
        else
        {
            print("Player data not loaded");
        }
    }

    public void UnlockAchievement(string achievement)
    {
        if (playerData != null)
        {
            if (playerData.achievements != null)
            {
                playerData.achievements.Add(achievement);
            }
            else
            {
                playerData.achievements = new List<string>();
                playerData.achievements.Add(achievement);
            }
            SavePlayerData();
        }
        else
        {
            print("Player data not loaded!");
        }
    }

    public void SavePlayerData()
    {
        //default
        if (playerData == null)
        {
            playerData = new PlayerData();
        }

        playerData.displayName = FirebaseAuthenticatorHandler.Instance.DisplayName;

        string json = JsonConvert.SerializeObject(playerData);
        databaseReference.Child("Users").Child(FirebaseAuthenticatorHandler.Instance.UserId).SetRawJsonValueAsync(json);
        print("User data saved successfully!");

        LoadPlayerData();
        LoadLeaderboard();
    }

    public void LoadPlayerData()
    {
        StartCoroutine(LoadDataEnum());
    }

    IEnumerator LoadDataEnum()
    {
        var serverData = databaseReference.Child("Users").Child(FirebaseAuthenticatorHandler.Instance.UserId).GetValueAsync();
        yield return new WaitUntil(predicate: () => serverData.IsCompleted);

        DataSnapshot snapshot = serverData.Result;
        string jsonData = snapshot.GetRawJsonValue();

        if (jsonData != null)
        {
            playerData = JsonConvert.DeserializeObject<PlayerData>(jsonData);
            EventManager.FetchUserDataSuccessful();
            print("User data fetched succesfully!");
        }
        else
        {
            print("User data not found!");
            SavePlayerData();
        }
    }

    public void LoadLeaderboard()
    {
        StartCoroutine(LoadLeaderboardEnum());
    }

    IEnumerator LoadLeaderboardEnum()
    {
        var serverData = databaseReference.Child("Users").GetValueAsync();
        yield return new WaitUntil(predicate: () => serverData.IsCompleted);

        DataSnapshot snapshot = serverData.Result;
        if (snapshot.Exists)
        {
            leaderboard = new PlayerData[snapshot.ChildrenCount];

            int i = 0;
            foreach (var childSnapshot in snapshot.Children)
            {
                string id = childSnapshot.Key;
                leaderboard[i] = new PlayerData();
                leaderboard[i].displayName = childSnapshot.Child("displayName").Value.ToString();
                leaderboard[i].wins = Convert.ToInt32(childSnapshot.Child("wins").Value);
                leaderboard[i].losses = Convert.ToInt32(childSnapshot.Child("losses").Value);
                i++;
            }

            //sorting in decsending order
            Array.Sort(leaderboard, (x, y) => y.wins.CompareTo(x.wins));

            EventManager.FetchLeaderboardSuccessful();
            print("Leaderboard loaded successfully!");
        }
        else
        {
            print("Error fetching leaderboard!");
        }
    }

    public bool IsAchievementUnlocked(string achievement)
    {
        if (playerData == null)
        {
            print("Player data not loaded!");
            return false;
        }
        else if (playerData.achievements == null)
        {
            print("No achievements found!");
            return false;
        }
        else if (playerData.achievements.Contains(achievement))
            return true;

        return false;
    }
}

[Serializable]
public class PlayerData
{
    public string displayName;
    public int wins;
    public int losses;
    public int xp;
    public int level;
    public List<string> achievements;

    public PlayerData()
    {
        displayName = "";
        wins = 0;
        losses = 0;
        xp = 0;
        level = 1;
        achievements = new List<string>();
    }
}