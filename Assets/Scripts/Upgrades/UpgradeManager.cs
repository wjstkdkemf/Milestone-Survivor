using System.Collections.Generic;
using UnityEngine;
using System.Linq; // 리스트 검색용 (FirstOrDefault 등)
using System.IO;
using TMPro;
using UnityEngine.Localization.Settings;
using Unity.VisualScripting;   // 저장 기능용

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Core References")]
    public PlayerWeaponController playerWeaponController; // 무기 관리자 연결
    public GameObject PlayerObject; // 패시브 스탯 적용을 위한 플레이어 참조

    [Header("UI References")]
    [SerializeField] private GameObject UpgradePanelObject; // 전체 UI 패널
    public List<GameObject> UpgradeUiSlots; // UI 슬롯들
    public PowerUpScriptableObject PowerUpgrade;

    [Header("Data Deck")]
    public List<UpgradeScriptableObject> MasterDeck;
    public List<UpgradeScriptableObject> StartingDeck;
    public List<UpgradeScriptableObject> UpgradeDeck; // 뽑을 카드 목록
    private List<UpgradeScriptableObject> spawnedUpgrades = new List<UpgradeScriptableObject>();
    private int UpgradeCount = 3;
    private bool InGameUpgrade = false;
    public Transform UpgradeUIContainer;
    public GameObject UpgradeUIPrefab;
    [SerializeField] private UpgradeDescriptionPanel descriptionPanel;

    [Header("Job System")]
    public List<JobDataSO> allJobs; // 모든 직업 데이터 리스트
    private JobDataSO currentJob = null;

    [Header("Level Up Queue")]
    // 처리되지 않고 대기 중인 업그레이드 횟수
    public int pendingUpgrades = 0;

    // 저장 경로
    private string saveUpgradeFilePath;

    private void Awake()
    {
        Instance = this;
        saveUpgradeFilePath = Path.Combine(Application.persistentDataPath, "PlayerUpgradeData.json");
    }

    private void Start()
    {
        // 런타임 데이터 초기화 (저장된 포인트/확률 불러오기 등)
        // 만약 PersistentDataManager를 쓴다면 여기서 Sync 로직 호출
        // SyncChancesFromPersistentData(); 
        
        // UI 초기화
        SetUpgradePanelState(false);
    }
    public void SetUpgradeUICount()
    {
        foreach (Transform child in UpgradeUIContainer) Destroy(child.gameObject);

        for(int i = 0 ; i < UpgradeCount ; i++)
        {
            GameObject UIObj = Instantiate(UpgradeUIPrefab, UpgradeUIContainer);
            UpgradeUiSlots.Add(UIObj);
        }
    }
    public void SetUpgradeCount()
    {
         if (PowerUpgrade.CurrentLevel > 0 && PowerUpgrade.CurrentLevel <= PowerUpgrade.upgradeValues.Length)
            UpgradeCount = (int)PowerUpgrade.upgradeValues[PowerUpgrade.CurrentLevel] + 3;
        Debug.Log(UpgradeCount);
    }
    public void ResetRunData(List<UpgradeScriptableObject> startingDeck)
    {
        Debug.Log("새 게임을 위해 데이터를 초기화합니다...");

        // 덱 초기화
        // MasterDeck에 있는 모든 카드를 복사해서 UpgradeDeck으로 가져옴
        SetUpgradeCount();
        SetUpgradeUICount();

        // 카드 상태 초기화
        foreach (var card in MasterDeck)
        {
            card.Points = 0;
            card.Chance = card.InitialChance;
        }
        if(startingDeck.Count != 0)
            UpgradeDeck = new List<UpgradeScriptableObject>(startingDeck);
        else
            UpgradeDeck = new List<UpgradeScriptableObject>(StartingDeck);

        // 직업 상태 초기화
        currentJob = null;
    }

    public void DisplayUpgrades()
    {
        for (int i = UpgradeDeck.Count - 1; i >= 0; i--)
        {
            if (UpgradeDeck[i].Points >= UpgradeDeck[i].MaxPoints)
            {
                UpgradeDeck.RemoveAt(i);
            }
        }

        // 게임 일시정지 및 UI 켜기
        if (MenuButtonController.Instance != null) MenuButtonController.Instance.CloseAllMenus();
        if (GameManager.Instance != null) GameManager.Instance.Pause = true;

        InGameUpgrade = true;
        SetUpgradePanelState(true);

        foreach (var slot in UpgradeUiSlots)
        {
            if (slot.activeSelf) // 켜져 있는 것만 끔
            {
                slot.SetActive(false);
            }
        }

        // 랜덤 뽑기 로직
        List<UpgradeScriptableObject> availableUpgrades = new List<UpgradeScriptableObject>(UpgradeDeck);
        int slotsCount = Mathf.Min(UpgradeCount, availableUpgrades.Count);

        for (int i = 0; i < UpgradeCount; i++)
        {
            if (i < slotsCount)
            {
                // 가중치합계 계산
                int totalChance = availableUpgrades.Sum(x => x.Chance);

                UpgradeScriptableObject chosenUpgrade = null;
                if (totalChance <= 0)
                {
                    // 비상 대책: 확률 무시하고 그냥 아무거나 뽑음
                    if (availableUpgrades.Count > 0)
                    {
                        int fallbackIndex = Random.Range(0, availableUpgrades.Count);
                        chosenUpgrade = availableUpgrades[fallbackIndex];
                    }
                }
                else
                {
                    int randomValue = Random.Range(0, totalChance);
                    foreach (var upgrade in availableUpgrades)
                    {
                        if (randomValue < upgrade.Chance)
                        {
                            chosenUpgrade = upgrade;
                            break;
                        }
                        randomValue -= upgrade.Chance;
                    }
                }

                // 슬롯에 데이터 세팅
                if (chosenUpgrade != null)
                {
                    UpgradeUiSlots[i].SetActive(true);
                    // UI 슬롯 스크립트에 정보 전달
                    UpgradeUiSlots[i].GetComponent<UpgradeUi>().SetInfo(
                        chosenUpgrade,
                        descriptionPanel,
                        playerWeaponController
                    );
                    
                    spawnedUpgrades.Add(chosenUpgrade);
                    availableUpgrades.Remove(chosenUpgrade); // 중복 등장 방지
                }
            }
            else
            {
                UpgradeUiSlots[i].SetActive(false); // 남는 슬롯 끄기
            }
            descriptionPanel.Hide();
        }
    }

    public void OnUpgradeSelected(UpgradeScriptableObject chosenUpgrade)
    {
        chosenUpgrade.Points++;

        // 무기인가? 패시브 스탯인가?
        if (chosenUpgrade.linkedWeaponData != null)
        {
            // 무기라면
            if (playerWeaponController != null)
            {
                playerWeaponController.AddWeapon(chosenUpgrade.linkedWeaponData);
                //Debug.Log($"[Upgrade] 무기 적용: {chosenUpgrade.linkedWeaponData.weaponName}");
            }
        }
        else
        {
            // 스탯이라면 
            ApplyStatBonus(chosenUpgrade);
            Debug.Log($"[Upgrade] 스탯 적용: {chosenUpgrade.upgradeType}");
        }

        // 잡 클래스 조건 달성 확인
        while(CheckAndSetJobClass())
        {
        }

        // 창 닫기
        ProcessNextUpgrade();
    }

    private void ApplyStatBonus(UpgradeScriptableObject upgrade)
    {
        var stats = PlayerStats.Instance; 
        var health = PlayerObject.GetComponent<PlayerHealth>();

        float value = upgrade.statValue; // SO에 설정된 값

        switch (upgrade.upgradeType)
        {
            case UpgradeScriptableObject.UpgradeType.Stat_MaxHealth:
                if (health != null) health.MaxHealth += value;
                break;

            case UpgradeScriptableObject.UpgradeType.Stat_Heal:
                if (health != null) health.Heal(value);
                break;

            case UpgradeScriptableObject.UpgradeType.Stat_Might:
                if (stats != null) stats.DamageBonus += (long)value;
                break;

            case UpgradeScriptableObject.UpgradeType.Stat_MoveSpeed:
                if (stats != null) stats.SpeedBonus += value;
                break;

            case UpgradeScriptableObject.UpgradeType.Stat_Cooldown:
                if (stats != null) stats.cooldownReduction += value; 
                break;
            
            case UpgradeScriptableObject.UpgradeType.Stat_Growth:
                if (stats != null) stats.experienceBonus += value;
                break;

        }
    }

    private bool CheckAndSetJobClass()
    {
        if (currentJob == null)
        {
            foreach (JobDataSO job in allJobs)
            {
                if (CheckJobRequirements(job))
                {
                    SetJob(job);
                    return true;
                }
            }
        }
        else
        {
            if(currentJob.nextAbleJobs == null || currentJob.nextAbleJobs.Count == 0) return false;

            foreach (JobDataSO nextjob in currentJob.nextAbleJobs)
            {
                if (CheckJobRequirements(nextjob))
                {
                    SetJob(nextjob);
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckJobRequirements(JobDataSO job)
    {
        foreach (JobDataSO.JobRequirement req in job.requirements)
        {
            // 플레이어가 해당 업그레이드 카드를 가지고 있는지 확인
            UpgradeScriptableObject myUpgrade = MasterDeck.Find(u => u.name == req.requiredUpgrade.name);

            // 카드가 아예 없다면 탈락
            if (myUpgrade == null)
            {
                //Debug.Log($"전직 실패: {req.requiredUpgrade.Title} 없음");
                return false; 
            }

            // 카드는 있지만 레벨이 부족하면 탈락
            if (myUpgrade.Points < req.requiredLevel)
            {
                //Debug.Log($"전직 실패: {myUpgrade.Title} 레벨 부족 (현재:{myUpgrade.Points} / 필요:{req.requiredLevel})");
                return false;
            }
        }
        return true;
    }

    private void SetJob(JobDataSO newJob)
    {
        currentJob = newJob;
        Debug.Log($"[Job Change] {newJob.jobName} 전직 완료!");

        // 직업 보너스 스킬 추가
        if (newJob.bonusUpgrades != null)
        {
            foreach (var bonusCard in newJob.bonusUpgrades)
            {
                if (!UpgradeDeck.Contains(bonusCard))
                {
                    //bonusCard.Chance += newJob.bonusChanceAmount; 
                    UpgradeDeck.Add(bonusCard);
                }
                else
                {
                    bonusCard.Chance += newJob.bonusChanceAmount;
                }
            }
        }

        // 금지된 스킬 제거
        if (newJob.bannedUpgrades != null)
        {
            foreach (var bannedCard in newJob.bannedUpgrades)
            {
                if (UpgradeDeck.Contains(bannedCard))
                {
                    UpgradeDeck.Remove(bannedCard);
                    
                    // 혹시 모르니 확률도 초기화해둘 수 있음
                    // bannedCard.Chance = 0; 
                    
                    Debug.Log($"[Deck] 직업 제한으로 스킬 제거됨: {bannedCard.Title}");
                }
            }
        }
            
        // SaveUpgradeData(); 
    }

    public void Close()
    {
        if (GameManager.Instance != null) GameManager.Instance.Pause = false;
        InGameUpgrade = false;
        
        SetUpgradePanelState(false);
        spawnedUpgrades.Clear();
    }
    public void AddPendingUpgrade()
    {
        pendingUpgrades++;

        // 만약 지금 UI가 꺼져있다면, 바로 보여주기 시작
        if (!InGameUpgrade) 
        {
            ProcessNextUpgrade();
        }
    }
    private void ProcessNextUpgrade()
    {
        if (pendingUpgrades > 0)
        {
            // 대기열 하나 소모
            pendingUpgrades--; 
            
            DisplayUpgrades(); 
        }
        else
        {
            // 더 이상 남은 게 없으면 진짜 종료
            Close();
        }
    }

    private void SetUpgradePanelState(bool isActive)
    {
        if (UpgradePanelObject != null) UpgradePanelObject.SetActive(isActive);
    }
    
    public void SetCombatState(bool isActive)
    {
        if (playerWeaponController != null)
        {
            playerWeaponController.ToggleCombatMode(isActive);
        }
        else
        {
            Debug.LogWarning("PlayerWeaponController가 연결되지 않았습니다!");
        }
    }
}
