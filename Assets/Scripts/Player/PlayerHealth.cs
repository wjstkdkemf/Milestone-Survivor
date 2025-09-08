using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour ,IDamageable
{
    public float MaxHealth = 100;
    private Animator animator;
    public float CurrentHealth;
    public bool Dashing = true;
    private Slider slider;
    public Vector3 SliderOffset;
    public bool IsDead;


    // ******************** Flash Stuff*********************
    public Material flashMaterial;
    public float duration = .1f;
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    protected Coroutine flashRoutine;
    private bool FirstSetting = false;

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
        CurrentHealth = MaxHealth;
        IsDead = false;
        
        // UI and component initialization - this can fail if objects aren't ready.
        // We wrap it in a null check for safety.
        if (GameObject.FindGameObjectWithTag("MainScene") == null)
        {
            slider = GameObject.FindGameObjectWithTag("HealthSlider")?.GetComponent<Slider>();
            spriteRenderer = transform.GetChild(0)?.GetComponent<SpriteRenderer>();
            if(spriteRenderer != null) originalMaterial = spriteRenderer.material;
        }

        // After setting health, update the UI.
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

        if (CurrentHealth < MaxHealth && PlayerStats.Instance.HealthRegeneration != 0)
        {
            CurrentHealth += PlayerStats.Instance.HealthRegeneration;
            UpdateHealthUI();
        }
    }

    public void TakeDamage(float damage, float knockBackDuration = 0f)
    {
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

    private void UpdateHealthUI()
    {
        if (slider != null)
        {
            slider.maxValue = MaxHealth;
            slider.value = CurrentHealth;
        }
    }

    // Deprecated methods, replaced by UpdateHealthUI
    // public void setmaxhealth(float mhealth) {}
    // public void sethealth(float health) {}

    public void Flash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }
        flashRoutine = StartCoroutine(FlashRoutine());
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