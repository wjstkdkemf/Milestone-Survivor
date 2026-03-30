[System.Serializable]
public class PlayerStatsData
{
    public int goldAmount;
    public int stageCleared;
    public int characterID;
    public int level;
    public int currentXP;
    public float requiredXP;

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
