using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMelee : NavEnemy
{
    [Header("Animation Frames")]
    public Sprite[] runFrames; // 걷기 이미지들 (Inspector에서 드래그 앤 드롭)
    public Sprite[] attackFrames; // 걷기 이미지들 (Inspector에서 드래그 앤 드롭)

    // 필요하다면 attackFrames 등 추가 가능

    [Header("Settings")]
    public float frameRate = 0.15f; // 이미지가 바뀌는 속도 (0.15초마다 다음 장)

    private float timer;
    private int currentFrameIndex;

    public override void ManualUpdate()
    {
        base.ManualUpdate();
        if (currentState == AnimState.Run && runFrames.Length > 0)
        {
            UpdateRunAnimation();
        }
    }
    private void UpdateRunAnimation()
    {
        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer -= frameRate;
            currentFrameIndex = (currentFrameIndex + 1) % runFrames.Length;
            spriteRenderer.sprite = runFrames[currentFrameIndex];
        }
    }

    public void PlayAttackAnimation()
    {
        if (currentState == AnimState.Attack) return;
        if (attackFrames.Length == 0) return;

        StopAllCoroutines();
        StartCoroutine(AttackAnimationCoroutine());
    }

    private IEnumerator AttackAnimationCoroutine()
    {
        currentState = AnimState.Attack;
        currentFrameIndex = 0;

        while (currentFrameIndex < attackFrames.Length)
        {
            spriteRenderer.sprite = attackFrames[currentFrameIndex];
            currentFrameIndex++;

            yield return new WaitForSeconds(frameRate);
        }

        currentState = AnimState.Run;
        currentFrameIndex = 0;
    }
    public override void Attack()
    {
        // Example melee attack logic
        PlayAttackAnimation();
        playerHealth.TakeDamage(damage);
        
        Debug.Log(gameObject.name + " performs a melee attack, dealing " + damage + " damage.");
    }
}
