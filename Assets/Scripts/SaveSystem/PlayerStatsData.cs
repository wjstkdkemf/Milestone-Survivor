[System.Serializable]
public class PlayerStatsData
{
    public long goldAmount;
    public int stageCleared;
    public int characterID;
    public int level;
    public int currentXP;
    public float requiredXP;

    public PlayerStatsData()
    {
        goldAmount = 0;
        stageCleared = 0;
        characterID = 0;
        level = 1;
        currentXP = 0;
        requiredXP = 50;
    }

    // Constructor to make it easy to create from PlayerStats
    public PlayerStatsData(PlayerStats stats)
    {
        goldAmount = stats.GoldAmount;
        stageCleared = stats.StageCleared;
        characterID = stats.CharacterID;
        level = stats.level;
        currentXP = stats.currentXP;
        requiredXP = stats.requiredXP;
    }
}
