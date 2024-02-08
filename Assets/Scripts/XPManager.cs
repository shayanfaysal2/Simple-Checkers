using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPManager : MonoBehaviour
{
    public static XPManager instance;

    private int points = 0;

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

    public void ResetPoints()
    {
        points = 0;
    }

    public void NormalMove()
    {
        //Every move gain = 1 point
        points += 1;
        print("Points: " + points);
    }

    public void CaptureMove()
    {
        //Capture move gain = 2 points
        points += 2;
        print("Points: " + points);
    }

    public void CalculateXP(bool win)
    {
        //Total move points gained at the end of each game = (normal + capturing) ^ 0.3
        int totalPoints = (int)Mathf.Pow(points, 0.3f);
        print("Total Points: " + totalPoints);

        int level = FirebaseDBManager.instance.playerData.level;
        int xpGain = 0;

        if (win)
        {
            //Win = 10 points
            points += 10;

            //If the player wins then =((total points + 10) ^ 0.9) + no. levels
            xpGain = ((int)Mathf.Pow(totalPoints + 10, 0.9f) + level);
        }
        else
        {
            //Lose = 5 points
            points += 5;

            //If the player loses then =((total points + 5) ^ 0.9) / no. levels
            xpGain = ((int)Mathf.Pow(totalPoints + 5, 0.9f) / level);
        }

        //save xp
        FirebaseDBManager.instance.IncreasePlayerXP(xpGain);
        print("XP Gain: " + xpGain);

        //increase level
        int xp = FirebaseDBManager.instance.playerData.xp;
        print("XP: " + xp);

        int xpBracket = GetXPBracket(level);
        print("XP bracket: " + xpBracket);

        if (xp > xpBracket)
        {
            FirebaseDBManager.instance.IncreasePlayerLevel();
            print("New level: " + FirebaseDBManager.instance.playerData.level);
        }
    }

    int GetXPBracket(int level)
    {
        return (int)Mathf.Pow(level / 0.2f, 2);
    }
}
