using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrailOrb : Orb
{
private GameObject trailPrefab;
    private float spawnInterval;
    private float trailDuration;
    private float trailDamage;

    private float spawnTimer;

    // 무기(Weapon)에서 호출하여 장판 관련 정보 주입
    public void SetTrailInfo(GameObject prefab, float interval, float duration, float damage)
    {
        this.trailPrefab = prefab;
        this.spawnInterval = interval;
        this.trailDuration = duration;
        this.trailDamage = damage;

        spawnTimer = 0f; // 시작하자마자 하나 깔도록 0으로 초기화
    }

    // ZoneDamageArea의 Update를 오버라이드하여 장판 생성 로직 추가
    protected override void Update()
    {
        // 부모(ZoneDamageArea)의 지속 데미지 로직 실행
        base.Update();

        // 장판 생성 타이머 체크
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnTrail();
            spawnTimer = spawnInterval; // 타이머 리셋
        }
    }

    private void SpawnTrail()
    {
        if (trailPrefab == null) return;

        // 오브젝트 풀링으로 장판 생성
        GameObject trail = ObjectPoolingManager.instance.spawnGameObject(trailPrefab, transform.position, Quaternion.identity);

        // 장판의 DoDamage 스크립트 설정
        if (trail.TryGetComponent<DoDamage>(out var doDamage))
        {
            // 계산된 장판 데미지 주입
            doDamage.damage = this.trailDamage; 
            // 지속 시간 설정 (DoDamage가 이 값을 사용해 스스로 파괴됨)
            doDamage.lifeTime = this.trailDuration;

            // *중요 체크*: 님의 DoDamage 스크립트는 Start()에서 lifeTime을 사용해 자폭 코루틴을 시작합니다.
            // 풀링으로 재사용될 때는 Start()가 다시 호출되지 않을 수 있습니다.
            // 만약 풀링 사용 시 장판이 안 사라진다면, DoDamage의 OnEnable()에서 자폭 코루틴을 시작하도록 수정이 필요할 수 있습니다.
            // (일단 기존 코드 기반으로 작성했습니다)
        }
    }
}
