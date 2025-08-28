using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeteorStrikeAbility : MonoBehaviour
{
    [Header("Skill Stats")]
    public int MeteorCount = 1; // The number of meteors to drop, controlled by UpgradeManager

    [Header("Skill Timing")]
    public float cooldown = 10f;
    public float warningDuration = 1f;
    public float volleyDelay = 0.3f; // Delay between each meteor in a volley

    [Header("Targeting")]
    public float searchRadius = 15f;
    public float densityCheckRadius = 4f;
    public LayerMask enemyLayer;

    [Header("Prefabs")]
    public GameObject magicCirclePrefab;
    public GameObject meteorPrefab;

    private float cooldownTimer;

    void Start()
    {
        cooldownTimer = cooldown;
    }

    void Update()
    {
        cooldownTimer += Time.deltaTime;

        if (cooldownTimer >= cooldown)
        {
            ActivateMeteorStrike();
        }
    }

    void ActivateMeteorStrike()
    {
        // Find all enemies in the search radius first
        Collider2D[] enemiesInSearchRadius = Physics2D.OverlapCircleAll(transform.position, searchRadius, enemyLayer);

        
        if (enemiesInSearchRadius.Length == 0)
        {
            return; // No enemies, don't activate the skill
        }

        cooldownTimer = 0f;
        
        // Determine all target points for the volley
        List<Vector3> targetPoints = FindTargetPoints(enemiesInSearchRadius);

        // Launch the volley
        StartCoroutine(LaunchMeteorVolley(targetPoints));
    }

    private List<Vector3> FindTargetPoints(Collider2D[] enemies)
    {
        List<Vector3> targets = new List<Vector3>();
        List<Collider2D> potentialRandomTargets = new List<Collider2D>(enemies);

        // 1. Find the densest point for the first meteor
        Vector3? densestPoint = FindDensestPoint(potentialRandomTargets.ToArray());
        if (densestPoint.HasValue)
        {
            targets.Add(densestPoint.Value);
        }
        else
        {
            // Fallback if no point is found, which is unlikely if enemies exist
            targets.Add(potentialRandomTargets[0].transform.position);
        }

        // 2. Find additional random targets for the rest of the meteors
        for (int i = 1; i < this.MeteorCount; i++)
        {
            if (potentialRandomTargets.Count > 0)
            {
                int randomIndex = Random.Range(0, potentialRandomTargets.Count);
                targets.Add(potentialRandomTargets[randomIndex].transform.position);
                potentialRandomTargets.RemoveAt(randomIndex); // Avoid targeting the same enemy twice
            }
            else
            {
                // If we run out of unique enemies, just target the main densest point again
                targets.Add(densestPoint.Value);
            }
        }

        return targets;
    }

    private Vector3? FindDensestPoint(Collider2D[] enemies)
    {
        if (enemies.Length == 0) return null;

        int maxDensity = 0;
        Vector3 densestPoint = Vector3.zero;

        foreach (Collider2D enemyCollider in enemies)
        {
            Collider2D[] enemiesInDensityRadius = Physics2D.OverlapCircleAll(enemyCollider.transform.position, densityCheckRadius, enemyLayer);
            if (enemiesInDensityRadius.Length > maxDensity)
            {
                maxDensity = enemiesInDensityRadius.Length;
                densestPoint = enemyCollider.transform.position;
            }
        }

        return maxDensity > 0 ? densestPoint : (Vector3?)enemies[0].transform.position;
    }

    private IEnumerator LaunchMeteorVolley(List<Vector3> targets)
    {
        foreach (Vector3 target in targets)
        {
            StartCoroutine(MeteorImpactSequence(target));
            yield return new WaitForSeconds(volleyDelay);
        }
    }

    private IEnumerator MeteorImpactSequence(Vector3 targetPoint)
    {
        GameObject circle = Instantiate(magicCirclePrefab, targetPoint, Quaternion.identity);
        yield return new WaitForSeconds(warningDuration);
        Destroy(circle);

        Vector3 meteorSpawnPoint = targetPoint + new Vector3(0, 0, 0);
        Instantiate(meteorPrefab, meteorSpawnPoint, Quaternion.identity);
    }
}
