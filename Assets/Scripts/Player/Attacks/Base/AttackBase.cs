using UnityEngine;

/// <summary>
/// Base class for all attack types.
/// Handles common logic like cooldowns and provides a structure for individual attack implementations.
/// </summary>
public abstract class AttackBase : MonoBehaviour
{
    [Header("Attack Stats")]
    [SerializeField] protected float baseDamage;
    [SerializeField] protected float cooldown;

    protected PlayerStats playerStats;
    protected float currentCooldown;

    protected virtual void Awake()
    {
        // Get the singleton instance of PlayerStats
        playerStats = PlayerStats.Instance;
    }

    protected virtual void Update()
    {
        // Handle cooldown timing
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Calculates the final damage for the attack.
    /// Can be overridden by child classes for special damage calculations.
    /// </summary>
    /// <returns>The final damage value.</returns>
    public virtual float GetDamage()
    {
        return baseDamage;
    }

    /// <summary>
    /// Calculates the final cooldown for the attack, applying player stats.
    /// </summary>
    /// <returns>The final cooldown duration.</returns>
    public virtual float GetCooldown()
    {
        if (playerStats == null) return cooldown;
        return cooldown * (1 - playerStats.cooldownReduction);
    }

    /// <summary>
    /// Checks if the attack is ready to be performed.
    /// </summary>
    /// <returns>True if the attack can be performed, false otherwise.</returns>
    public bool IsReady()
    {
        return currentCooldown <= 0;
    }

    /// <summary>
    /// Resets the attack's cooldown after it has been performed.
    /// </summary>
    protected void ResetCooldown()
    {
        currentCooldown = GetCooldown();
    }

    /// <summary>
    /// The specific attack logic to be implemented by each child class.
    /// </summary>
    public abstract void PerformAttack();
}
