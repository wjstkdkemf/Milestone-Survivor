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

    [Header("Settings")]
    public PowerUpScriptableObject powerUp;
    public Color hoverColor = Color.yellow;
    private Color originalColor;
    
    // Scale Settings
    private Vector3 normalScale;
    public float hoverScaleFactor = 1.1f;
    public float animationDuration = 0.2f;

    private PowerUpManager powerUpManager;
    public bool IsSelected;

    public void Initialize(PowerUpManager manager)
    {
        this.powerUpManager = manager;
        normalScale = transform.localScale;

        // 그래픽 컴포넌트 가져오기 (Image 혹은 TMP 등)
        cardGraphic = GetComponent<Graphic>();
        if (cardGraphic != null)
        {
            originalColor = cardGraphic.color;
        }

        if (powerUp != null)
        {
            Icon.sprite = powerUp.IconSprite;
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (powerUp == null) return;

        powerUpNameText.text = powerUp.powerUpName;
        powerUpPointText.text = powerUp.CurrentLevel.ToString();
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

    public void Selected()
    {
        if (powerUpManager == null) return;

        // 다른 버튼 선택 해제 요청
        powerUpManager.DeselectOtherButtons();
        
        // 매니저에게 나(현재 버튼)의 정보를 전달
        powerUpManager.SetInfo(this);
        
        IsSelected = true;

        // 선택 효과 (확대, 소리, 색상)
        PlaySelectEffect();
    }

    public void DeSelected()
    {
        IsSelected = false;

        // 원래 상태로 복귀
        LeanTween.scale(gameObject, normalScale, animationDuration).setEaseInBack();
        
        if (cardGraphic != null)
            cardGraphic.color = originalColor;

        if (customCursor != null)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    // --- Visual Effects ---

    private void PlaySelectEffect()
    {
        if (hoverSound != null) hoverSound.Play();

        // 선택 시 약간 확대
        LeanTween.scale(gameObject, normalScale * hoverScaleFactor, animationDuration).setEaseOutBack();

        if (cardGraphic != null)
            cardGraphic.color = hoverColor;

        if (customCursor != null)
            Cursor.SetCursor(customCursor, Vector2.zero, CursorMode.Auto);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 이미 선택된 상태라면 마우스 오버 효과 무시 (또는 다르게 처리)
        if (IsSelected) return;

        if (hoverSound != null) hoverSound.Play();
        LeanTween.scale(gameObject, normalScale * hoverScaleFactor, animationDuration).setEaseOutBack();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 선택된 상태라면 크기를 줄이지 않음
        if (IsSelected) return;

        LeanTween.scale(gameObject, normalScale, animationDuration).setEaseInBack();
        
        if (customCursor != null)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}