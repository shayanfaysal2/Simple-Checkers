using UnityEngine.Events;

public static class EventManager
{
    //actions
    public static event UnityAction OnLoginSuccessful;
    public static event UnityAction OnFetchUserDataSuccessful;
    public static event UnityAction OnFetchLeaderboardSuccessful;
    public static event UnityAction OnDisplayNameSet;
    public static event UnityAction<string> OnAchievementUnlock;

    //functions
    public static void LoginSuccessful() => OnLoginSuccessful?.Invoke();
    public static void FetchUserDataSuccessful() => OnFetchUserDataSuccessful?.Invoke();
    public static void FetchLeaderboardSuccessful() => OnFetchLeaderboardSuccessful?.Invoke();
    public static void DisplayNameSet() => OnDisplayNameSet?.Invoke();
    public static void UnlockAchievement(string achievementId) => OnAchievementUnlock?.Invoke(achievementId);
}