using System;

public static class GlobalEventManager
{
    public static Action OnEncounterCleared;    
    public static Action<string> OnEnemyKilled;     // 적 처치 시
    public static Action<int> OnExpGained;          // 경험치 획득 시
    public static Action<string> OnItemCollected;   // 아이템 획득 시
}