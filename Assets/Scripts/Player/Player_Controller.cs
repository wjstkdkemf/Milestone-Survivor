using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{

    public float movmentSpeed = 5f;
    private SpriteRenderer spriteRenderer;

    private enum State
    {
        Normal,
        Dashing,
    }
    private Rigidbody2D rb;
    private Vector3 moveDir;
    private Vector3 dashDir;
    private Vector3 lastMoveDir;
   [SerializeField] private float dashSpeed, dashForce = 35, cooltime = 2;
    private State state;
    private float dashCoolDown;
   [HideInInspector] public bool Dashing;
    private TrailRenderer Trail;
    [SerializeField]
    private bool haveAnimation;
    private Animator animator;
    private void Awake()
    {    
        state = State.Normal;
        
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Trail = GetComponent<TrailRenderer>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {

        Trail.emitting = Dashing;
        switch (state)
        {
            case State.Normal:
                float moveX = 0f;
                float moveY = 0f;

                if (Input.GetKey(KeyCode.W))
                {
                    moveY = +1f;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    moveY = -1f;
                }
                if (Input.GetKey(KeyCode.A))
                {
                //    spriteRenderer.flipX = true;
                       transform.localScale = new Vector3(-1, 1, 1); // or activate look right some other way
                    moveX = -1f;
                }
                if (Input.GetKey(KeyCode.D))
                {
                  //  spriteRenderer.flipX = true;
                       transform.localScale = new Vector3(1, 1, 1); // activate looking left
                    moveX = +1f;
                }

                moveDir = new Vector3(moveX, moveY).normalized;
                if (moveX != 0 || moveY != 0)
                {
                    if(haveAnimation)
                    animator.SetBool("Moving", true);
                    // Not idle
                    lastMoveDir = moveDir;
                }
                else 
                {
                    if(haveAnimation)
                    animator.SetBool("Moving",false); 
                }

                if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift)) && dashCoolDown <= 0)
                {
                   // AudioManager.instance.PlaySound("Dash");
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
      
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Normal:
                rb.velocity = moveDir * (movmentSpeed*(1+PlayerStats.Instance.SpeedBonus/100));
               
                break;
            case State.Dashing:
                rb.velocity = dashDir * dashSpeed;
                break;
        }
    }
}
