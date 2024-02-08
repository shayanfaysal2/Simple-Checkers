using UnityEngine;

[CreateAssetMenu]
public class Achievement : ScriptableObject
{
    public string id;
    public string title;
    public string description;
    public Reward[] rewards;
}

public enum RewardType
{
    coins,
    gems
}

[System.Serializable]
public class Reward
{
    public RewardType type;
    public int amount;
}