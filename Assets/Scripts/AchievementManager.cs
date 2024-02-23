using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;

    public Achievement[] achievements;

    [HideInInspector] public int gamesWonInRow = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        EventManager.OnAchievementUnlock += UnlockAchievement;
    }

    private void OnDisable()
    {
        EventManager.OnAchievementUnlock -= UnlockAchievement;
    }

    public static void WonGame()
    {
        instance.gamesWonInRow++;

        if (instance.gamesWonInRow == 2)
            UnlockAchievement("win_2_games_in_row");
        else if (instance.gamesWonInRow == 5)
            UnlockAchievement("win_5_games_in_row");
        else if (instance.gamesWonInRow == 10)
            UnlockAchievement("win_10_games_in_row");
    }

    public static void LostGame()
    {
        instance.gamesWonInRow = 0;
    }

    private static void UnlockAchievement(string achievementId)
    {
        //check if achievement is valid
        Achievement achievement = instance.achievements.FirstOrDefault(obj => obj.id == achievementId);
        if (achievement == null)
            return;

        //if achievement not unlocked
        if (!IsAchievementUnlocked(achievementId))
        {
            //unlock achievement in firebase
            FirebaseDBManager.instance.UnlockAchievement(achievementId);

            //show achievement unlocked
            print("Achivement unlocked: " + achievementId);

            //give reward
            if (achievement.rewards != null)
                foreach(Reward reward in achievement.rewards)
                    print("Reward: " + reward);
        }
        else
        {
            print("Achievement already unlocked: " + achievementId);
        }
    }

    public static bool IsAchievementUnlocked(string achievementId)
    {
        return (FirebaseDBManager.instance.IsAchievementUnlocked(achievementId));
    }

    public static Achievement GetAchievement(string achievementId)
    {
        return instance.achievements.FirstOrDefault(obj => obj.id == achievementId);
    }
}
