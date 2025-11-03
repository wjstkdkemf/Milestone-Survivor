using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FirstNamedEnemy : Enemy
{
    [Header("Slam Attack Settings")]
    public float slamRadius = 3f; // The radius of the slam attack
    public float windUpTime = 0.8f; // Time before the slam occurs
    public GameObject slamEffectPrefab; // Visual effect for the slam
    public LayerMask playerLayer; // To detect the player
    [Header("Slam Warning Settings")]
    public GameObject slamWarningPrefab; // '차오르는' 경고 이펙트 프리팹
    public Color warningStartColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // 반투명 회색
    public Color warningEndColor = new Color(0, 0, 0, 0.9f);         // 진한 검은색

    private bool isAttacking = false;

    // Override the Attack method to implement the slam pattern
    public override void Attack()
    {
        // Prevent starting a new attack if one is already in progress
        if (isAttacking)
        {
            return;
        }

        // Use a coroutine to handle the attack sequence (wind-up, attack, cooldown)
        StartCoroutine(SlamAttackSequence());
    }

    private IEnumerator SlamAttackSequence()
    {
        isAttacking = true;
        stopMoving = true; // Stop the enemy from moving during the attack

        Debug.Log(gameObject.name + " is winding up!");

        GameObject indicatorInstance = null;
        Image warningImage = null;

        // 1. 경고 이펙트 프리팹 생성 (부모를 지정하지 않음)
        if (slamWarningPrefab != null)
        {
            indicatorInstance = Instantiate(slamWarningPrefab, transform.position, Quaternion.identity);

            warningImage = indicatorInstance.GetComponentInChildren<Image>();
            if (warningImage != null)
            {
                warningImage.color = warningStartColor; // 시작 색상 설정
            }
        }

        // 2. windUpTime 동안 크기와 색상을 변경합니다.
        float timer = 0f;
        Vector3 startScale = Vector3.zero;
        // 최종 크기는 slamRadius의 지름(diameter)을 기반으로 계산합니다.
        float diameter = slamRadius * 2f;
        Vector3 endScale = new Vector3(diameter, diameter, 1f);

        while (timer < windUpTime)
        {
            float progress = timer / windUpTime; // 0.0 to 1.0

            if (indicatorInstance != null)
            {
                indicatorInstance.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            }

            if (warningImage != null)
            {
                warningImage.color = Color.Lerp(warningStartColor, warningEndColor, progress);
            }

            timer += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 3. windUpTime이 끝나면 크기와 색상을 최종 값으로 확정
        if (indicatorInstance != null)
        {
            indicatorInstance.transform.localScale = endScale;
            if (warningImage != null)
            {
                warningImage.color = warningEndColor;
            }
        }

        // --- 공격 실행 단계 ---
        Debug.Log(gameObject.name + " SLAMS!");

        if (slamEffectPrefab != null)
        {
            Instantiate(slamEffectPrefab, transform.position, Quaternion.identity);
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, slamRadius, playerLayer);
        foreach (var hitCollider in hitColliders)
        {
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log("Slam attack hit " + hitCollider.name + " for " + damage + " damage.");
            }
        }

        // 4. 경고 이펙트 파괴
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }

        // --- 마무리 단계 ---
        stopMoving = false;
        isAttacking = false;
    }

    // Optional: Draw a gizmo in the editor to visualize the slam radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }
}
