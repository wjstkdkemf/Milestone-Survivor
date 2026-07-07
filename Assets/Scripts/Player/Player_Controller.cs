using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Controller : MonoBehaviour
{
    public float movmentSpeed = 5f;
    private SpriteRenderer spriteRenderer;

    private enum State { Normal, Dashing }
    private Rigidbody2D rb;
    private Vector3 moveDir;
    private Vector3 dashDir;
    private Vector3 lastMoveDir;
    [SerializeField] private float dashSpeed, dashForce = 35, cooltime = 2;
    private State state;
    private float dashCoolDown;
    private float knockbackTimer = 0f; 
    [HideInInspector] public bool Dashing;
    private TrailRenderer Trail;
    [SerializeField] private bool haveAnimation;
    private Slider slider;
    private Material originalMaterial;
    [SerializeField]private Animator PlayerAnimator;
    public float DashCoolTimeRation = 1;
    public bool StopMoving = false;

    [Header("Fake Collision Settings")]
    public LayerMask enemyLayer;
    public float playerRadius = 0.5f;
    public float bumpResistance = 5.0f;
    private Collider2D[] nearbyEnemies = new Collider2D[100];

    private int facingSign = 1;
    [SerializeField] private Transform bodyVisual;
    private WeaponVisualController waponVisualController;

    private void Awake()
    {    
        state = State.Normal;
        waponVisualController = GetComponentInChildren<WeaponVisualController>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Trail = GetComponentInChildren<TrailRenderer>();
        //PlayerAnimator = transform.GetChild(0)?.GetComponent<PlayerAnimator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        slider = GameObject.FindGameObjectWithTag("DashCoolTimeSlider")?.GetComponent<Slider>();
        
        if (transform.childCount > 0)
        {
            spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) originalMaterial = spriteRenderer.material;
        }

        UpdateDashCoolTimeUI();
    }

    public void UpdateDashCoolTimeUI()
    {
        if (slider != null)
        {
            slider.maxValue = cooltime;
            slider.value = cooltime - dashCoolDown;
        }
    }

    private void Update()
    {
        if(StopMoving) return;

        Trail.emitting = Dashing;
        
        switch (state)
        {
            case State.Normal:
                float moveX = 0f;
                float moveY = 0f;

                if (Input.GetKey(KeyCode.W)) moveY = +1f;
                if (Input.GetKey(KeyCode.S)) moveY = -1f;
                
                if (Input.GetKey(KeyCode.A))
                {
                    SetFacing(-1);
                    moveX = -1f;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    SetFacing(1);
                    moveX = +1f;
                }

                moveDir = new Vector3(moveX, moveY).normalized;
                
                if (moveX != 0 || moveY != 0)
                {
                    if(haveAnimation) PlayerAnimator.SetBool("Moving", true);
                    lastMoveDir = moveDir;

                    waponVisualController.SetAimDirection(moveDir);
                }
                else 
                {
                    if(haveAnimation) PlayerAnimator.SetBool("Moving",false); 
                }

                if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift)) && dashCoolDown <= 0)
                {
                    dashCoolDown = cooltime;
                    dashDir = lastMoveDir;
                    dashSpeed = dashForce;
                    state = State.Dashing;
                    Dashing = true;
                }
                else if (dashCoolDown >= 0)
                {
                    Dashing = false;
                    dashCoolDown -= Time.deltaTime;
                }
                break;

            case State.Dashing:
                float dashForceMultiplier  = 5f;
                dashSpeed -= dashSpeed * dashForceMultiplier * Time.deltaTime;

                float rollSpeedMinimum = dashForce / 2;
                if (dashSpeed < rollSpeedMinimum)
                {
                    state = State.Normal;
                }
                break;
        }
        UpdateDashCoolTimeUI();
    }

    public void SetDashCoolTime(float dashCool)
    {
        cooltime = (cooltime - dashCool) * DashCoolTimeRation;
        if(cooltime <= 1.0f) cooltime = 1.0f;
        UpdateDashCoolTimeUI();
    }

    public void ApplyKnockback(Vector2 forceDir, float forcePower, float duration)
    {
        knockbackTimer = duration;
        rb.velocity = Vector2.zero;
        rb.AddForce(forceDir * forcePower , ForceMode2D.Impulse);
    }

    private void FixedUpdate()
    {
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return; 
        }
        
        switch (state)
        {
            case State.Normal:
                Vector2 myVelocity = moveDir * (movmentSpeed * (1 + PlayerStats.Instance.SpeedBonus / 100f));
                
                int count = Physics2D.OverlapCircleNonAlloc(transform.position, playerRadius, nearbyEnemies, enemyLayer);
                Vector2 pushBackForce = Vector2.zero;

                for (int i = 0; i < count; i++)
                {
                    Vector2 diff = transform.position - nearbyEnemies[i].transform.position;
                    float dist = diff.magnitude;

                    if (dist > 0.001f && dist < playerRadius)
                    {
                        float pushStrength = 1.0f - (dist / playerRadius);
                        pushBackForce += diff.normalized * pushStrength * bumpResistance;
                    }
                }

                rb.velocity = myVelocity + pushBackForce;
                break;

            case State.Dashing:
                float currentMoveDist = dashSpeed * Time.fixedDeltaTime;
                RaycastHit2D hit = Physics2D.CircleCast(transform.position, playerRadius, dashDir, currentMoveDist, enemyLayer);

                if (hit.collider != null)
                {
                    state = State.Normal;
                    Dashing = false;
                    rb.velocity = Vector2.zero;
                }
                else
                {
                    rb.velocity = dashDir * dashSpeed;
                }
                break;
        }
    }
    private void SetFacing(int sign)
    {
        if (facingSign == sign) return;

        facingSign = sign;

        bodyVisual.localScale = new Vector3(sign, 1f, 1f);

        if (waponVisualController != null)
            waponVisualController.SetFacing(sign);
    }
}