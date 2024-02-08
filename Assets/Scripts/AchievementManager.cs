using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager instance;

    public Achievement[] achievements;

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
            //print("Reward: " + achievement.rewards[0]);
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
}
