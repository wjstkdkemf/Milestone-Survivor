using System;

public static class GlobalEventManager
{
    public static Action<string> OnEnemyKilled; 
    public static Action OnEncounterCleared;    
}