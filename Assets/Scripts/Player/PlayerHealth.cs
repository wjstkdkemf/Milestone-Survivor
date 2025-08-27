using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    private Coroutine flashRoutine;

    void Start()
    {   slider = GameObject.FindGameObjectWithTag("HealthSlider").GetComponent<Slider>();
        CurrentHealth = MaxHealth;
        setmaxhealth(MaxHealth);
        spriteRenderer =transform.GetChild(0).GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
    }

    // Update is called once per frame
    void Update()
    {
       // slider.transform.position = Camera.main.WorldToScreenPoint(transform.position + SliderOffset);
        Dashing = GetComponent<Player_Controller>().Dashing;
        if (CurrentHealth <= 0)
        {
            IsDead = true;
            gameObject.SetActive(false);
        }
        else
            IsDead = false;

        if (CurrentHealth < MaxHealth && PlayerStats.Instance.HealthRegeneration != 0)
            CurrentHealth += PlayerStats.Instance.HealthRegeneration;

    }



   
    public void TakeDamage(float damage, float knockBackDuration = 0f)
    {

        if (!Dashing)
        {
          //  AudioManager.instance.PlaySound("Hurt");
            CurrentHealth -= damage;
            sethealth(CurrentHealth);
            Flash();
            if(CurrentHealth<=0)
                GameOver.Instance.GameEnded(false);
        }



    }
    public void Heal(float Healing)
    {
        if (CurrentHealth + Healing >= MaxHealth)
        {
            CurrentHealth = MaxHealth;
            slider.value = MaxHealth;
        }
        else
        {
            CurrentHealth += Healing;
            slider.value = CurrentHealth;
        }

    }
    public void setmaxhealth(float mhealth)
    {

        slider.maxValue = mhealth;

        slider.value = mhealth;



    }

    public void sethealth(float health)
    {


        slider.value = health;

        //  slider.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(Color.red, Color.green, slider.normalizedValue);
    }

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
        if (flashMaterial != null)
            spriteRenderer.material = flashMaterial;
        yield return new WaitForSeconds(duration);
        spriteRenderer.material = originalMaterial;
        flashRoutine = null;
    }


}

