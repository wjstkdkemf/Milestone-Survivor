using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinArcherScript : Enemy
{
    public GameObject arrowPrefab; // 화살 프리팹
    public Transform firePoint; // 화살 발사 위치
    public float arrowSpeed = 10f; // 화살 속도

    private Animator animator;

    void Start()
    {
        attackRange = 5.0f;
        animator = GetComponent<Animator>();
    }

    public override void Attack()
    {
        if (arrowPrefab == null || firePoint == null || player == null)
        {
            Debug.LogWarning("Arrow Prefab, Fire Point, or Player not set for Goblin Archer.");
            return;
        }

        animator.SetTrigger("Attack");

        // 플레이어를 향하는 방향 계산
        Vector2 direction = (player.position - firePoint.position).normalized;

        // 화살 생성
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        // 화살에 데미지 값 전달
        GoblinArrow arrowScript = arrow.GetComponent<GoblinArrow>();
        if (arrowScript != null)
        {
            arrowScript.damage = damage; // this.damage는 Enemy.cs에 정의된 변수
        }

        // 화살에 속도 적용
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * arrowSpeed;
        }
        else
        {
            Debug.LogError("Arrow prefab is missing a Rigidbody2D component.");
        }

        Debug.Log(gameObject.name + " fires an arrow with " + this.damage + " damage.");
    }
}