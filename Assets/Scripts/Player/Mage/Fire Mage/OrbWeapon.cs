using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbWeapon : WeaponBase // 부모 클래스 변경
{
    private int currentOrbCount;
    private float currentRadius;
    private float currentRotationSpeed;
    private float currentScaling = 1.0f;

    
    private GameObject orbPrefab; // 작은 구체 프리팹
    protected List<GameObject> spawnedOrbs = new List<GameObject>();
    private float angleStep;

    private PlayerStats playerStats;   // 데미지 계산용
    
    // 초기화: 플레이어가 무기를 획득했을 때 딱 1번 실행됨
    public override void Initialize(WeaponDataSO data)
    {
        if (data is OrbWeaponDataSO orbData)
        {
            currentOrbCount = orbData.orbCount;
            currentRadius = orbData.radius;
            currentRotationSpeed = orbData.rotationSpeed;
            currentDamage = orbData.baseDamage; // 부모 SO에 있는 데미지

            currentHitRadius = orbData.hitRadius;

            orbPrefab = orbData.orbProjectilePrefab;
        }
        else
        {
            Debug.LogError("잘못된 데이터가 들어왔습니다! OrbWeaponDataSO가 필요합니다.");
            return;
        }

        if (PlayerStats.Instance != null)
        {
            playerStats = PlayerStats.Instance;
        }
        else
        {
            // 만약 싱글톤이 아니라면 부모에서 찾기
            playerStats = GetComponentInParent<PlayerStats>();
        }
        SpawnOrbs();
    }

    // 플레이어 컨트롤러가 매 프레임 호출해줌
    public override void OnUpdate()
    {
        OrbitOrbs();
    }

    void SpawnOrbs()
    {
        ClearOrbs();

        if (currentOrbCount == 0) return;

        angleStep = 360f / currentOrbCount;

        for (int i = 0; i < currentOrbCount; i++)
        {
            GameObject orb = Instantiate(orbPrefab, transform.position, Quaternion.identity, transform);
            
            float angle = i * angleStep;
            orb.transform.localPosition = GetOrbPosition(angle);
            
            SetupSpawnedOrb(orb);
            spawnedOrbs.Add(orb);
        }
    }
    Vector3 GetOrbPosition(float angle)
    {
        float angleInRadians = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(angleInRadians) * currentRadius;
        float y = Mathf.Sin(angleInRadians) * currentRadius;

        return new Vector3(x, y, 0) + transform.position;
    }
    protected virtual void SetupSpawnedOrb(GameObject orb)
    {
        if (orb.TryGetComponent<Orb>(out var orbScript))
        {
            orbScript.SetInfo(GetDamage(), currentHitRadius, 0);
        }
    }
    void OrbitOrbs()
    {
       for (int i = 0; i < spawnedOrbs.Count; i++)

        {

            if (spawnedOrbs[i] == null) continue;



            float angle = (i * angleStep) + Time.time * currentRotationSpeed;

            spawnedOrbs[i].transform.position = GetOrbPosition(angle);

        }
    }

    void ClearOrbs()
    {
        foreach (GameObject orb in spawnedOrbs)
        {
            if (orb != null) Destroy(orb);
        }
        spawnedOrbs.Clear();
    }


    public void UpgradeOrbCount(int amount)
    {
        currentOrbCount += amount;
        SpawnOrbs(); // 개수 바뀌면 다시 소환
    }

    public void UpgradeRadius(float amount)
    {
        currentRadius += amount;
    }

    public void UpgradeSpeed(float amount)
    {
        currentRotationSpeed += amount;
    }
    
    public virtual void UpgradeDamage(float amount)
    {
        currentDamage += amount;
        SpawnOrbs(); // 데미지 갱신을 위해 재소환 (혹은 기존 orb들에 접근해서 수치만 변경)
    }
    public override void LevelUp()
    {
        // 예시: 레벨업 시 총알 개수 증가 혹은 데미지 증가
        currentOrbCount++; 
        currentDamage++;
        SpawnOrbs();
    }
    public float GetDamage()
    {
        float bonus = (playerStats != null) ? playerStats.DamageBonus : 0;
        return currentDamage + (bonus * currentScaling);
    }
}