using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class PlayerHealth : MonoBehaviour ,IDamageable
{
    private float _maxHealth;
    public float MaxHealth
    {
        get { return _maxHealth; }
        set
        {
            if(_maxHealth < value)
            {
                CurrentHealth += value - _maxHealth; // MaxHealth가 커질 때 항상 이전 최대체력과 지금의 최대체력의 차이만큼 회복
                _maxHealth = value;
            }
            else
            {
                _maxHealth = value;
            }

            if(CurrentHealth > _maxHealth)//만약 maxHealth가 줄어도 알아서 줄어듬
                CurrentHealth = _maxHealth;
        }
    }
    private Animator animator;
    public float health_Cal = 0.05f;
    public float CurrentHealth;
    public bool Dashing = true;
    private Slider slider;
    public Vector3 SliderOffset;
    public bool IsDead;
    private bool isInvincible = false; // 무적 상태 플래그
    private Player_Controller player_Controller;

    // ******************** Flash Stuff*********************
    public Material flashMaterial;
    private BarrierWeapon myBarrierSkill;
    public float duration = .1f;
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    protected Coroutine flashRoutine;
    private PlayerStats playerStats;
    //private bool FirstSetting = false;

    void Start()
    {
        // Initialize when the object first starts.
        CurrentHealth = 1.0f;
    }

    public void InitializeForNewScene()
    {
        // Core stat initialization - this should always run.
        IsDead = false;
        // UI and component initialization - this can fail if objects aren't ready.
        // We wrap it in a null check for safety.
        playerStats = PlayerStats.Instance;


        if (GameObject.FindGameObjectWithTag("GameScene"))
        {
            slider = GameObject.FindGameObjectWithTag("HealthSlider")?.GetComponent<Slider>();
            spriteRenderer = transform.GetChild(0)?.GetComponent<SpriteRenderer>();
            if(spriteRenderer != null) originalMaterial = spriteRenderer.material;
            player_Controller = GetComponent<Player_Controller>();
        }
        myBarrierSkill = GetComponentInChildren<BarrierWeapon>();

        UpdateHealthUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(player_Controller != null)
            Dashing = player_Controller.Dashing;
            
        if (CurrentHealth <= 0)
        {
            if (!IsDead) // Prevent this from running multiple times
            {
                IsDead = true;
                gameObject.SetActive(false);
            }
        }
        else
        {
            IsDead = false;
        }
        if (GameManager.Instance.Heal == true)
        {
            HealthRegenerationByEncount();
            GameManager.Instance.Heal = false;
        }
        UpdateHealthUI();
    }
    public void HealthRegenerationByEncount()
    {
        if (CurrentHealth < MaxHealth && PlayerStats.Instance.HealthRegeneration != 0)//회복이 과함. 추후 스테이지 클리어당 회복으로 수정예정
        {
            CurrentHealth += PlayerStats.Instance.HealthRegeneration;
            if(CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
            UpdateHealthUI();
        }

    }
    public void SetBarrier(GameObject gameObject)
    {
        myBarrierSkill = gameObject != null ? gameObject.GetComponent<BarrierWeapon>() : null;
    }

    public void RegisterBarrier(BarrierWeapon barrier)
    {
        if (barrier != null && barrier.isActiveAndEnabled)
            myBarrierSkill = barrier;
    }

    public void UnregisterBarrier(BarrierWeapon barrier)
    {
        if (myBarrierSkill == barrier) myBarrierSkill = null;
    }

    public void TakeDamage(float damage, float knockBackDuration = 0f)
    {
        if (isInvincible) return;

        if (myBarrierSkill != null && myBarrierSkill.isActiveAndEnabled && myBarrierSkill.isBarrierActive)
        {
            // 배리어를 터뜨림 -> 이때 내부적으로 SetInvincible(0.2f)가 호출됨
            myBarrierSkill.BreakBarrier();
            return; // 데미지 무효화
        }

        if (!Dashing)
        {
            CurrentHealth -= DamageCal(damage);
            UpdateHealthUI();
            Flash();
            if(CurrentHealth <= 0)
            {
                GameOver.Instance.GameEnded(false);
            }
        }
    }
    private float DamageCal(float damage)//추후 새로운 데미지 계산식 추가가능.
    {
        float ReturnDamage = DefenseCoefficientProcessing(damage);
        return ReturnDamage;
    }
    private float DefenseCoefficientProcessing(float damage)
    {
        return damage * playerStats.NormalArmorRatio / (playerStats.ArmorBonus + playerStats.NormalArmorRatio);
    }

    public void Heal(float Healing)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + Healing, 0.0f, MaxHealth);
        UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        if (slider != null)
        {
            slider.maxValue = MaxHealth;
            slider.value = CurrentHealth;
        }
    }

    // Deprecated methods, replaced by UpdateHealthUI
    public void setmaxhealth(float mhealth)
    {
        MaxHealth += mhealth;
        Heal(mhealth/10);
    }
    // public void sethealth(float health) {}

    public void Flash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        SetInvincible(duration);
        flashRoutine = StartCoroutine(FlashRoutine());
    }
    public void SetInvincible(float duration)
    {
        StartCoroutine(InvincibilityRoutine(duration));
    }
    private IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    private IEnumerator FlashRoutine()
    {
        if (flashMaterial != null && spriteRenderer != null)
        {
            Material beforeMaterial = spriteRenderer.material;
            spriteRenderer.material = flashMaterial;
            yield return new WaitForSeconds(duration);
            spriteRenderer.material = beforeMaterial;
        }
        flashRoutine = null;
    }
}
