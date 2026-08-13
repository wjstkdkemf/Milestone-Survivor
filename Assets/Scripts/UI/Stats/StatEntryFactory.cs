using System.Collections.Generic;
using UnityEngine;

public static class StatEntryFactory
{
    public static List<StatEntry> FromCharacterBase(CharacterScriptableObject character)
    {
        List<StatEntry> entries = new List<StatEntry>();
        if (character == null)
            return entries;

        entries.Add(StatEntry.Header("character.section.basic_stats", CharacterLocalization.Get("character.section.basic_stats", "Basic Stats")));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.BaseHP), character.BaseHP));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.Damage), character.Damage));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.MovementSpeed), character.MovementSpeed));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.Armor), character.Armor));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.HealthRegeneration), character.HealthRegeneration));
        entries.Add(StatEntry.Percent(nameof(CharacterScriptableObject.LuckBoost), character.LuckBoost));
        entries.Add(StatEntry.Percent(nameof(CharacterScriptableObject.CooldownReduction), character.CooldownReduction));
        entries.Add(StatEntry.Percent(nameof(CharacterScriptableObject.DobleDamageChance), character.DobleDamageChance));

        return entries;
    }

    public static List<StatEntry> FromCharacterLevelUpBonus(CharacterScriptableObject character)
    {
        List<StatEntry> entries = new List<StatEntry>
        {
            StatEntry.Header("character.section.special_stats", CharacterLocalization.Get("character.section.special_stats", "Special Stats"))
        };

        if (character == null || character.statModifiers == null || character.statModifiers.Count == 0)
        {
            entries.Add(StatEntry.Empty("character.stat.none", CharacterLocalization.Get("character.stat.none", "None")));
            return entries;
        }

        foreach (StatModifier modifier in character.statModifiers)
            entries.Add(StatEntry.Signed(modifier.statName, modifier.value));

        return entries;
    }

    public static List<StatEntry> FromPlayerStats(PlayerStats stats)
    {
        List<StatEntry> entries = new List<StatEntry>();
        if (stats == null)
            return entries;

        PlayerHealth health = null;
        if (stats.Player != null)
            health = stats.Player.GetComponent<PlayerHealth>();

        entries.Add(StatEntry.Header("character.section.basic_stats", CharacterLocalization.Get("character.section.basic_stats", "Basic Stats")));

        if (health != null)
            entries.Add(StatEntry.CurrentMax(nameof(CharacterScriptableObject.BaseHP), health.CurrentHealth, health.MaxHealth));

        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.Damage), stats.DamageBonus));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.MovementSpeed), stats.SpeedBonus));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.Armor), stats.ArmorBonus));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.HealthRegeneration), stats.HealthRegeneration));
        entries.Add(StatEntry.Percent(nameof(CharacterScriptableObject.LuckBoost), stats.LuckBonus));
        entries.Add(StatEntry.Percent(nameof(CharacterScriptableObject.CooldownReduction), stats.cooldownReduction));
        entries.Add(StatEntry.Percent(nameof(CharacterScriptableObject.DobleDamageChance), stats.DoubleDamageChance * 100f));
        entries.Add(StatEntry.Percent("ProjectileSpeed", stats.projectileSpeedBonus));
        entries.Add(StatEntry.Number("KnockBack", stats.KnockBackBonus));
        entries.Add(StatEntry.Percent("XPBoost", stats.experienceBonus));

        return entries;
    }
}
