
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightningSparkAbility : MonoBehaviour
{
    public float fireRate = 1f;
    public int damage = 10;
    public int bounces = 0;
    public float range = 10f;
    public int segments = 6;
    public float maxRandomness = 0.5f;
    public GameObject lightningSparkPrefab; // Assign a prefab with a LineRenderer

    private float timer;

    void Update()
    {
        if (bounces <= 0)
        {
            return; // 스킬 레벨이 0 이하면 아무것도 하지 않음
        }

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = fireRate;
            StartCoroutine(FireLightning());
        }
    }

    IEnumerator FireLightning()
    {
        List<Enemy> enemies = FindObjectsOfType<Enemy>().ToList();
        enemies = enemies.Where(e => e.IsActived && Vector2.Distance(transform.position, e.transform.position) < range).ToList();

        if (enemies.Count == 0)
            yield break;

        enemies = enemies.OrderBy(e => Vector2.Distance(transform.position, e.transform.position)).ToList();
        Enemy currentEnemy = enemies[0];

        Vector2 lastPosition = transform.position;

        for (int i = 0; i < bounces + 1; i++)
        {
            if (currentEnemy == null)
                break;

            GameObject spark = Instantiate(lightningSparkPrefab, Vector3.zero, Quaternion.identity);
            LineRenderer lineRenderer = spark.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, lastPosition);
            lineRenderer.SetPosition(1, currentEnemy.transform.position);

            GenerateLightning(lineRenderer , lastPosition , currentEnemy.transform.position);

            currentEnemy.GetComponent<IDamageable>().TakeDamage(damage);

            lastPosition = currentEnemy.transform.position;

            enemies.Remove(currentEnemy);

            if (enemies.Count == 0)
                break;

            enemies = enemies.OrderBy(e => Vector2.Distance(lastPosition, e.transform.position)).ToList();
            currentEnemy = enemies[0];

            yield return new WaitForSeconds(0.1f);
        }
    }
    
    void GenerateLightning(LineRenderer lineRenderer , Vector2 startpoint , Vector2 endpoint)
    {
        if (startpoint == null || endpoint == null)
        {
            Debug.LogError("시작점 또는 끝점이 설정되지 않았습니다.");
            return;
        }

        // 1. LineRenderer의 점 개수 설정
        lineRenderer.positionCount = segments;

        Vector2 startPos = startpoint;
        Vector2 endPos = endpoint;

        // 2. 각 점의 위치 계산
        for (int i = 0; i < segments; i++)
        {
            // 현재 점이 전체 선분에서 차지하는 비율 (0.0 ~ 1.0)
            float t = (float)i / (segments - 1);

            // 시작점과 끝점 사이의 직선 상의 위치
            Vector3 positionOnLine = Vector3.Lerp(startPos, endPos, t);

            // 첫 번째와 마지막 점은 고정 (움직이지 않음)
            if (i > 0 && i < segments - 1)
            {
                // 랜덤한 변위(offset) 추가
                // Random.insideUnitSphere는 (0,0,0)을 중심으로 반지름 1인 구 안의 랜덤한 점을 반환합니다.
                Vector3 offset = Random.insideUnitSphere * maxRandomness;
                positionOnLine += offset;
            }

            // 3. 계산된 위치를 LineRenderer에 설정
            lineRenderer.SetPosition(i, positionOnLine);
        }
    }
}
