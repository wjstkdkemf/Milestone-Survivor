using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerUpButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Components")]
    public TMP_Text powerUpNameText;
    public TMP_Text powerUpPointText;
    public Image Icon;
    
    // 이펙트용 컴포넌트 (없으면 null)
    private Graphic cardGraphic; 
    public AudioSource hoverSound;
    public Texture2D customCursor;
    public GameObject[] LevelUpPlusObjects;

    [Header("Settings")]
    public PowerUpScriptableObject powerUp;
    public Color hoverColor = Color.yellow;
    private Color originalColor;
    private bool visualStateInitialized;
    
    // Scale Settings
    private Vector3 normalScale;
    public float hoverScaleFactor = 1.1f;
    public float animationDuration = 0.2f;
    private int scaleTweenId = -1;
    private Vector3 scaleTweenTarget;

    private PowerUpManager powerUpManager;
    public bool IsSelected;
    private void Awake()
    {
        EnsureVisualStateInitialized();
    }

    private void EnsureVisualStateInitialized()
    {
        if (visualStateInitialized) return;

        // Power-up selection owns the visuals; the generic card effect also animates this Transform.
        if (TryGetComponent<CardUIEffects>(out var cardEffects))
            cardEffects.enabled = false;

        normalScale = transform.localScale;
        cardGraphic = GetComponent<Graphic>();

        if (cardGraphic != null)
            originalColor = cardGraphic.color;

        visualStateInitialized = true;
    }

    public void Initialize(PowerUpManager manager)
    {
        EnsureVisualStateInitialized();

        this.powerUpManager = manager;
        DeSelected(true);

        if (powerUp != null)
        {
            Icon.sprite = powerUp.IconSprite;
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (powerUp == null) return;

        powerUpNameText.text = powerUp.GetLocalizedName();
        int currentLevel = Mathf.Clamp(powerUp.CurrentLevel, 0, LevelUpPlusObjects.Length);
        SetAllPlusInactive();

        for (int i = 0; i < LevelUpPlusObjects.Length; i++)
        {
            if (LevelUpPlusObjects[i] == null)
                continue;

            LevelUpPlusObjects[i].SetActive(i < currentLevel);
        }
    }
    private void SetAllPlusInactive()
    {
        for (int i = 0; i < LevelUpPlusObjects.Length; i++)
        {
            if (LevelUpPlusObjects[i] != null)
                LevelUpPlusObjects[i].SetActive(false);
        }
    }

    public void ResetUI()
    {
        // 필요한 경우 초기화 로직 (예: 이펙트 끄기 등)
        UpdateUI();
    }

    // --- Interaction Logic ---

    public void OnPointerClick(PointerEventData eventData)
    {
        Selected();
    }
    private void OnDisable()
    {
        DeSelected(true);
    }

    public void Selected()
    {
        if (powerUpManager == null || IsSelected) return;

        // 다른 버튼 선택 해제 요청
        powerUpManager.DeselectOtherButtons();
        
        // 매니저에게 나(현재 버튼)의 정보를 전달
        powerUpManager.SetInfo(this);
        
        IsSelected = true;

        // 선택 효과 (확대, 소리, 색상)
        PlaySelectEffect();
    }

    public void DeSelected(bool Imm = false)
    {
        EnsureVisualStateInitialized();

        IsSelected = false;

        // 원래 상태로 복귀
        ChangeScale(normalScale, Imm);
        
        if (cardGraphic != null)
            cardGraphic.color = originalColor;

        if (customCursor != null)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    // --- Visual Effects ---

    private void PlaySelectEffect()
    {
        EnsureVisualStateInitialized();
        if (hoverSound != null) hoverSound.Play();

        // 선택 시 약간 확대
        ChangeScale(normalScale * hoverScaleFactor);


        if (cardGraphic != null)
            cardGraphic.color = hoverColor;

        if (customCursor != null)
            Cursor.SetCursor(customCursor, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isActiveAndEnabled) return;
        EnsureVisualStateInitialized();
        // 이미 선택된 상태라면 마우스 오버 효과 무시 (또는 다르게 처리)
        if (IsSelected) return;

        if (hoverSound != null) hoverSound.Play();
        ChangeScale(normalScale * hoverScaleFactor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isActiveAndEnabled) return;
        EnsureVisualStateInitialized();
        // 선택된 상태라면 크기를 줄이지 않음
        if (IsSelected) return;

        ChangeScale(normalScale);
        
        if (customCursor != null)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void CancelScaleTween()
    {
        if (scaleTweenId < 0) return;

        LeanTween.cancel(scaleTweenId);
        scaleTweenId = -1;
    }
    private void ChangeScale(Vector3 target, bool immediate = false)
    {
        // Hover followed by click must not restart a tween already moving to the same size.
        if (!immediate && isActiveAndEnabled && animationDuration > 0f
            && scaleTweenId >= 0 && LeanTween.isTweening(scaleTweenId)
            && scaleTweenTarget == target)
            return;

        CancelScaleTween();

        if(immediate || !isActiveAndEnabled || animationDuration <= 0f)
        {
            transform.localScale = target;
            return;
        }

        if(transform.localScale == target)
        {
            transform.localScale = target;
            return;
        }

        scaleTweenTarget = target;
        scaleTweenId = LeanTween.scale(gameObject, target, animationDuration)
                                .setIgnoreTimeScale(true)
                                .setEase(LeanTweenType.easeOutQuad)
                                .setOnComplete(() =>
                                {
                                    transform.localScale = target;
                                    scaleTweenId = -1;
                                })
                                .id;
    }
}
