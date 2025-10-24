using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ItemDropEffect : MonoBehaviour
{
    [Header("Effects")]
    public GameObject epicBeamEffectPrefab; // 아이템 생성 시 나타날 이펙트 프리팹
    public GameObject groundGlowEffectPrefab; // 바닥에 닿은 후 지속될 이펙트 프리팹

    [Header("Physics")]
    public float popForce = 5f; // 아이템이 튀어 오르는 힘

    private Rigidbody2D rb;
    private bool hasLanded = false;
    private bool isFalling = false;
    private float initialYPosition;
    private GameObject epicBeamEffectInstance;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialYPosition = transform.position.y;

        // 1. "에픽 빔" 이펙트 생성
        if (epicBeamEffectPrefab != null)
        {
            epicBeamEffectInstance = Instantiate(epicBeamEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. 위쪽으로 튀어 오르는 힘 적용
        rb.AddForce(Vector2.up * popForce, ForceMode2D.Impulse);
    }

    void Update()
    {
        // 이미 착지했다면 더 이상 처리하지 않음
        if (hasLanded) return;

        // 3. 튀어 오른 후, 떨어지기 시작했는지 확인 (y축 속도가 음수)
        if (!isFalling && rb.velocity.y < 0)
        {
            isFalling = true;
        }

        // 4. 떨어지는 중이고, 처음 높이보다 낮거나 같아졌는지 확인
        if (isFalling && transform.position.y <= initialYPosition)
        {
            hasLanded = true;

            // 모든 물리적 움직임을 멈춤
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;

            // 정확히 처음 높이에 위치하도록 보정
            transform.position = new Vector3(transform.position.x, initialYPosition, transform.position.z);

            if (epicBeamEffectInstance != null)
            {
                Destroy(epicBeamEffectInstance);
            }
            // 5. "지속 발광" 이펙트 생성
            if (groundGlowEffectPrefab != null)
            {
                Instantiate(groundGlowEffectPrefab, transform.position, Quaternion.identity, transform);
            }
        }
    }
    public bool GetHasLand()
    {
        return hasLanded;
    }
}
