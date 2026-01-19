using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour ,IDamageable
{
    private float _maxHealth;
    public float MaxHealth
    {
        get { return _maxHealth; }
        set
        {
            _maxHealth = value;
            CurrentHealth = _maxHealth; // MaxHealth가 설정될 때 항상 현재 체력을 최대치로 설정
        }
    }
    private Animator animator;
    public float CurrentHealth;
    public bool Dashing = true;
    private Slider slider;
    public Vector3 SliderOffset;
    public bool IsDead;
    private bool isInvincible = false; // 무적 상태 플래그


    // ******************** Flash Stuff*********************
    public Material flashMaterial;
    private BarrierWeapon myBarrierSkill;
    public float duration = .1f;
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    protected Coroutine flashRoutine;
    //private bool FirstSetting = false;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When a new scene loads, we need to re-initialize.
        InitializeForNewScene();
    }

    void Start()
    {
        // Initialize when the object first starts.
        InitializeForNewScene();
    }

    private void InitializeForNewScene()
    {
        // Core stat initialization - this should always run.
        IsDead = false;
        // UI and component initialization - this can fail if objects aren't ready.
        // We wrap it in a null check for safety.
        if (GameObject.FindGameObjectWithTag("GameScene"))
        {
            slider = GameObject.FindGameObjectWithTag("HealthSlider")?.GetComponent<Slider>();
            spriteRenderer = transform.GetChild(0)?.GetComponent<SpriteRenderer>();
            if(spriteRenderer != null) originalMaterial = spriteRenderer.material;
        }
        myBarrierSkill = GetComponentInChildren<BarrierWeapon>();

        UpdateHealthUI();
    }

    // Update is called once per frame
    void Update()
    {
        Dashing = GetComponent<Player_Controller>().Dashing;
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
        myBarrierSkill = gameObject.GetComponent<BarrierWeapon>();
    }

    public void TakeDamage(float damage, float knockBackDuration = 0f)
    {
        if (isInvincible) return;

        if (myBarrierSkill != null && myBarrierSkill.isBarrierActive)
        {
            // 배리어를 터뜨림 -> 이때 내부적으로 SetInvincible(0.2f)가 호출됨
            myBarrierSkill.BreakBarrier();
            return; // 데미지 무효화
        }

        if (!Dashing)
        {
            CurrentHealth -= damage;
            UpdateHealthUI();
            Flash();
            if(CurrentHealth <= 0)
            {
                GameOver.Instance.GameEnded(false);
            }
        }
    }

    public void Heal(float Healing)
    {
        if (CurrentHealth + Healing >= MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        else
        {
            CurrentHealth += Healing;
        }
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
            spriteRenderer.material = flashMaterial;
            yield return new WaitForSeconds(duration);
            spriteRenderer.material = originalMaterial;
        }
        flashRoutine = null;
    }
}