using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneDamageArea : MonoBehaviour
{
    [Header("Zone Stats")]
    private float damage;
    private float radius;
    public float tickRate = 0.5f; 
    
    private float tickTimer;

    // 💡 [핵심] 가비지 컬렉션(GC)을 막기 위해 넉넉한 사이즈의 바구니를 미리 만들어 둡니다.
    private List<Enemy> enemiesInside = new List<Enemy>(100); 

    // 무기 관리자(WeaponBase)가 장판을 깔 때 호출하여 스탯을 꽂아줍니다.
    public void Setup(float zoneDamage, float zoneRadius, float zoneTickRate)
    {
        damage = zoneDamage;
        radius = zoneRadius;
        tickRate = zoneTickRate;
    }

    private void OnEnable()
    {
        // 켜지자마자 0.1초 뒤 첫 타격이 들어가도록 세팅 (즉발을 원하면 0으로 세팅)
        tickTimer = 0.1f; 
    }

    private void Update()
    {
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            ApplyAreaDamage();
            tickTimer = tickRate; // 타이머 리셋
        }
    }

    private void ApplyAreaDamage()
    {
        //EnemySwarmSystem.Instance.GetEnemiesInRadius(transform.position, radius, enemiesInside);

        // 2. 바구니에 담긴 불쌍한 몬스터들에게 데미지를 뿌립니다.
        for (int i = 0; i < enemiesInside.Count; i++)
        {
            Enemy target = enemiesInside[i];
            
           // if (Time.time >= EnemySwarmSystem.Instance.nextHitTimes[target.currentIndex])
           // {
            //    target.TakeDamage(damage);

                // 안전한 메인 스레드이므로 바로 무적 시간 갱신! (예: 0.1초)
            //    EnemySwarmSystem.Instance.nextHitTimes[target.currentIndex] = Time.time + 0.1f;
           // }
        }
    }

    // 💡 시각적 디버깅을 위해 기즈모(Gizmos)로 장판 범위를 그려줍니다.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
