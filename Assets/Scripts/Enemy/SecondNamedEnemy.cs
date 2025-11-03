using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SecondNamedEnemy : Enemy
{
    [Header("Slam Attack Settings")]
    public float windUpTime = 0.8f; // Time before the slam occurs
    public float attackOffset = 2f; // Distance in front of the enemy for the attack
    public Vector2 slamBoxSize = new Vector2(5f, 3f);
    public GameObject slamEffectPrefab; // Visual effect for the slam
    public LayerMask playerLayer; // To detect the player

    [Header("Slam Warning Settings")]
    public GameObject slamWarningPrefab; // '차오르는' 경고 이펙트 프리팹
    public Color warningStartColor = new Color(0.5f, 0.5f, 0.5f, 0.7f); // 반투명 회색
    public Color warningEndColor = new Color(0, 0, 0, 0.9f);         // 진한 검은색

    private bool isAttacking = false;

    public override void Attack()
    {
        if (isAttacking) return;
        StartCoroutine(SlamAttackSequence());
    }

    private IEnumerator SlamAttackSequence()
    {
        isAttacking = true;
        stopMoving = true;

        // 1. 플레이어 방향으로 공격 방향, 위치, 회전각 계산
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 attackCenter = (Vector2)transform.position + directionToPlayer * attackOffset;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // 2. 경고 이펙트 생성 및 표시
        GameObject indicatorInstance = null;
        if (slamWarningPrefab != null)
        {
            indicatorInstance = Instantiate(slamWarningPrefab, attackCenter, rotation);
        }

        Image warningImage = indicatorInstance?.GetComponentInChildren<Image>();
        if (warningImage != null) warningImage.color = warningStartColor;

        float timer = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = new Vector3(slamBoxSize.x, slamBoxSize.y, 1f);

        while (timer < windUpTime)
        {
            float progress = timer / windUpTime;
            if (indicatorInstance != null)
            {
                indicatorInstance.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            }
            if (warningImage != null)
            {
                warningImage.color = Color.Lerp(warningStartColor, warningEndColor, progress);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (indicatorInstance != null)
        {
            indicatorInstance.transform.localScale = endScale;
            if (warningImage != null) warningImage.color = warningEndColor;
        }

        // 3. 공격 실행
        if (slamEffectPrefab != null)
        {
            Instantiate(slamEffectPrefab, attackCenter, rotation);
        }

        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(attackCenter, slamBoxSize, angle, playerLayer);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }
        }

        // 4. 마무리
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }

        stopMoving = false;
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return; // 플레이어가 없으면 기즈모를 그리지 않음

        Gizmos.color = Color.red;

        // 플레이어 방향으로 기즈모를 회전시키기 위해 Matrix 사용
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 attackCenter = (Vector2)transform.position + directionToPlayer * attackOffset;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // TRS(Translate, Rotate, Scale) 행렬을 생성하여 기즈모에 적용
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(attackCenter, rotation, Vector3.one);

        // 회전된 좌표계의 원점에서 박스를 그림
        Gizmos.DrawWireCube(Vector3.zero, slamBoxSize);

        // 기즈모 매트릭스를 원래대로 복원
        Gizmos.matrix = oldMatrix;
    }
}
