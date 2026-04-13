using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillProjectileBase : MonoBehaviour
{
    public float hitRadius = 0.5f; // 알바생(Job)이 충돌 검사할 반경
    public int maxHits = 1;        // 관통 횟수 (터렛은 1)
    public float damage;           // 무기에서 전달받을 데미지

    protected virtual void OnEnable()
    {
        if (SkillCollisionManager.Instance != null)
            SkillCollisionManager.Instance.activeSkills.Add(this);
    }

    protected virtual void OnDisable()
    {
        //if (SkillCollisionManager.Instance != null)
            //SkillCollisionManager.Instance.activeSkills.Remove(this);
    }

    public abstract void OnHit(Enemy hitEnemy);
}
