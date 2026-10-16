using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerUpButton : MonoBehaviour,  IPointerClickHandler// IPointerEnterHandler, IPointerExitHandler,
{
    [Header("UI Components")]
    public TMP_Text powerUpNameText;
    public TMP_Text powerUpPointText;
    public Image Icon;
    public GameObject[] LevelUpPlusObjects;

    [Header("Settings")]
    public PowerUpScriptableObject powerUp;
    private PowerUpManager powerUpManager;
    public bool IsSelected;
    [SerializeField]
    private CardUIEffects cardUIEffects;
    private void Awake()
    {
        if(cardUIEffects == null)
            cardUIEffects = GetComponent<CardUIEffects>();
    }

    public void Initialize(PowerUpManager manager)
    {
        cardUIEffects.EnsureVisualStateInitialized();

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

        CacheComponents();

        if (cardUIEffects != null)
            cardUIEffects.SetSelected(true);
    }

    public void DeSelected(bool Imm = false)
    {
        IsSelected = false;

        CacheComponents();
        
        if (cardUIEffects != null)
            cardUIEffects.SetSelected(false, Imm);
    }
    private void CacheComponents()
    {
        if (cardUIEffects == null)
            cardUIEffects = GetComponent<CardUIEffects>();
    }
}
