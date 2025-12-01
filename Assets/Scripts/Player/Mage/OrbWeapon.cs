using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbWeapon : WeaponBase // 부모 클래스 변경
{
    // [런타임 상태 변수] (게임 중에 변할 수 있는 값들)
    private int currentOrbCount;
    private float currentRadius;
    private float currentRotationSpeed;
    private float currentDamage;

    // [내부 변수]
    private GameObject orbPrefab; // 작은 구체 프리팹
    private List<GameObject> spawnedOrbs = new List<GameObject>();
    private float angleStep;
    
    // 초기화: 플레이어가 무기를 획득했을 때 딱 1번 실행됨
    public override void Initialize(WeaponDataSO data)
    {
        // 1. 데이터 가져오기 (다운캐스팅 사용)
        // "너 일반 무기 데이터 아니고, Orb 전용 데이터 맞지?"
        if (data is OrbWeaponDataSO orbData)
        {
            currentOrbCount = orbData.orbCount;
            currentRadius = orbData.radius;
            currentRotationSpeed = orbData.rotationSpeed;
            currentDamage = orbData.baseDamage; // 부모 SO에 있는 데미지
            orbPrefab = orbData.orbProjectilePrefab;
        }
        else
        {
            Debug.LogError("잘못된 데이터가 들어왔습니다! OrbWeaponDataSO가 필요합니다.");
            return;
        }

        // 2. 구체 생성 시작
        SpawnOrbs();
    }

    // 플레이어 컨트롤러가 매 프레임 호출해줌
    public override void OnUpdate()
    {
        // 기존의 OrbitOrbs 로직을 여기서 실행
        OrbitOrbs();
    }

    // --- 아래는 기존 로직을 거의 그대로 사용 ---

    void SpawnOrbs()
    {
        ClearOrbs();

        if (currentOrbCount == 0) return;

        angleStep = 360f / currentOrbCount;

        for (int i = 0; i < currentOrbCount; i++)
        {
            float angle = i * angleStep;
            Vector3 orbPosition = GetOrbPosition(angle);
            
            // transform은 이제 Player 밑에 붙은 "OrbWeapon" 오브젝트가 됨
            GameObject orb = Instantiate(orbPrefab, orbPosition, Quaternion.identity, transform);
            
            // 데미지 정보 전달
            // (IceOrb 스크립트가 있다면 그대로 사용)
            var iceOrbScript = orb.GetComponent<IceOrb>();
            if(iceOrbScript != null)
                iceOrbScript.SetInfo(currentDamage, 0); // 0은 크리티컬 확률 등
            
            spawnedOrbs.Add(orb);
        }
    }

    Vector3 GetOrbPosition(float angle)
    {
        float angleInRadians = angle * Mathf.Deg2Rad;
        float x = Mathf.Cos(angleInRadians) * currentRadius;
        float y = Mathf.Sin(angleInRadians) * currentRadius;
        
        // transform.position은 이 무기(플레이어의 자식)의 위치 = 플레이어 위치
        return new Vector3(x, y, 0) + transform.position;
    }

    void OrbitOrbs()
    {
        // 굳이 FixedUpdate 아니어도 Time.deltaTime 쓰면 Update에서도 부드러움
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

    // --- 업그레이드 시스템을 위한 함수들 ---

    public void UpgradeOrbCount(int amount)
    {
        currentOrbCount += amount;
        SpawnOrbs(); // 개수 바뀌면 다시 소환
    }

    public void UpgradeRadius(float amount)
    {
        currentRadius += amount;
        // 반경은 실시간 반영되므로 재소환 안 해도 됨 (OrbitOrbs에서 계산함)
    }

    public void UpgradeSpeed(float amount)
    {
        currentRotationSpeed += amount;
    }
    
    public void UpgradeDamage(float amount)
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
}