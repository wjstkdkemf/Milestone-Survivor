using System.Collections.Generic;
using UnityEngine;

public static class StatEntryFactory
{
    public static List<StatEntry> FromCharacterBase(CharacterScriptableObject character, StatIconDatabase statIconDatabase)
    {
        List<StatEntry> entries = new List<StatEntry>();
        if (character == null)
            return entries;

        entries.Add(StatEntry.Header("character.section.basic_stats", CharacterLocalization.Get("character.section.basic_stats", "Basic Stats")));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.BaseHP), character.BaseHP, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.BaseHP))));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.Damage), character.Damage, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.Damage))));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.MovementSpeed), character.MovementSpeed, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.MovementSpeed))));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.Armor), character.Armor, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.Armor))));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.HealthRegeneration), character.HealthRegeneration, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.HealthRegeneration))));
        entries.Add(StatEntry.Percent(nameof(CharacterScriptableObject.LuckBoost), character.LuckBoost, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.LuckBoost))));
        entries.Add(StatEntry.Percent(nameof(CharacterScriptableObject.CooldownReduction), character.CooldownReduction, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.CooldownReduction))));
        entries.Add(StatEntry.Percent(nameof(CharacterScriptableObject.DobleDamageChance), character.DobleDamageChance, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.DobleDamageChance))));

        return entries;
    }

    public static List<StatEntry> FromCharacterLevelUpBonus(CharacterScriptableObject character , StatIconDatabase statIconDatabase)
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
            entries.Add(StatEntry.Signed(modifier.statName, modifier.value, GetIcon(statIconDatabase, modifier.statName)));

        return entries;
    }

    public static List<StatEntry> FromPlayerStats(PlayerStats stats, StatIconDatabase statIconDatabase)
    {
        List<StatEntry> entries = new List<StatEntry>();
        if (stats == null)
            return entries;

        PlayerHealth health = null;
        if (stats.Player != null)
            health = stats.Player.GetComponent<PlayerHealth>();

        entries.Add(StatEntry.Header("character.section.basic_stats", CharacterLocalization.Get("character.section.basic_stats", "Basic Stats")));

        if (health != null)
            entries.Add(StatEntry.CurrentMax(nameof(CharacterScriptableObject.BaseHP), health.CurrentHealth, health.MaxHealth, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.BaseHP))));

        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.Damage), stats.DamageBonus, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.Damage))));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.MovementSpeed), stats.SpeedBonus, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.MovementSpeed))));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.Armor), stats.ArmorBonus, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.Armor))));
        entries.Add(StatEntry.Number(nameof(CharacterScriptableObject.HealthRegeneration), stats.HealthRegeneration, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.HealthRegeneration))));
        entries.Add(StatEntry.Percent(nameof(CharacterScriptableObject.LuckBoost), stats.LuckBonus, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.LuckBoost))));
        entries.Add(StatEntry.Percent(nameof(CharacterScriptableObject.CooldownReduction), stats.cooldownReduction, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.CooldownReduction))));
        entries.Add(StatEntry.Percent(nameof(CharacterScriptableObject.DobleDamageChance), stats.DoubleDamageChance * 100f, GetIcon(statIconDatabase, nameof(CharacterScriptableObject.DobleDamageChance))));
        entries.Add(StatEntry.Percent("ProjectileSpeed", stats.projectileSpeedBonus, GetIcon(statIconDatabase, "ProjectileSpeed")));
        entries.Add(StatEntry.Number("KnockBack", stats.KnockBackBonus, GetIcon(statIconDatabase, "KnockBack")));
        entries.Add(StatEntry.Percent("XPBoost", stats.experienceBonus, GetIcon(statIconDatabase, "XPBoost")));

        return entries;
    }

    private static Sprite GetIcon(StatIconDatabase statIconDatabase, string statKey)
    {
        return statIconDatabase != null ? statIconDatabase.GetIcon(statKey) : null;
    }
}
