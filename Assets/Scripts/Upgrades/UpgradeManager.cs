using System.Collections.Generic;
using UnityEngine;
using System.Linq; // 리스트 검색용 (FirstOrDefault 등)
using System.IO;   // 저장 기능용

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Core References")]
    public PlayerWeaponController playerWeaponController; // [핵심] 무기 관리자 연결
    public GameObject PlayerObject; // 패시브 스탯 적용을 위한 플레이어 참조

    [Header("UI References")]
    [SerializeField] private GameObject UpgradePanelObject; // 전체 UI 패널
    public GameObject[] UpgradeUiSlots; // UI 슬롯들 (3~4개)

    [Header("Data Deck")]
    public List<UpgradeScriptableObject> MasterDeck;
    public List<UpgradeScriptableObject> UpgradeDeck; // 뽑을 카드 목록 (기존 UpgadeToSpawn)
    private List<UpgradeScriptableObject> spawnedUpgrades = new List<UpgradeScriptableObject>();

    [Header("Job System")]
    public List<JobDataSO> allJobs; // 모든 직업 데이터 리스트 (인스펙터 할당)
    private JobDataSO currentJob = null;
    private bool isJobClassSet = false;

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
    // ========================================================================
    // [NEW] 리셋 기능 구현
    // ========================================================================
    public void ResetRunData()
    {
        Debug.Log("새 게임을 위해 데이터를 초기화합니다...");

        // 1. 덱(Deck) 초기화 (만렙 찍어서 사라진 카드들 복구)
        // MasterDeck에 있는 모든 카드를 복사해서 UpgradeDeck으로 가져옴
        UpgradeDeck = new List<UpgradeScriptableObject>(MasterDeck);

        // 2. 카드 상태 초기화 (레벨 0으로, 확률 원상복구)
        foreach (var card in UpgradeDeck)
        {
            card.Points = 0; // 레벨 0
            
            // [주의] 만약 직업 시스템이나 이벤트로 Chance를 건드렸다면,
            // 여기서 Chance도 초기값으로 돌려놔야 합니다.
            // card.Chance = card.InitialChance; // (InitialChance 변수를 따로 뒀다면)
        }

        // 3. 직업(Job) 상태 초기화
        currentJob = null;
        isJobClassSet = false;
        // 5. 저장된 진행 데이터(파일)가 있다면 삭제 로직 (선택사항)
        // if (File.Exists(saveUpgradeFilePath)) File.Delete(saveUpgradeFilePath);
    }

    // ========================================================================
    // 1. 업그레이드 UI 표시 로직 (가중치 랜덤 뽑기)
    // ========================================================================
    public void DisplayUpgrades()
    {
        // 1. 만렙 찍은 스킬은 덱에서 제거
        for (int i = UpgradeDeck.Count - 1; i >= 0; i--)
        {
            if (UpgradeDeck[i].Points >= UpgradeDeck[i].MaxPoints)
            {
                UpgradeDeck.RemoveAt(i);
            }
        }

        // 2. 게임 일시정지 및 UI 켜기
        if (MenuButtonController.Instance != null) MenuButtonController.Instance.InGameUpgrade = true;
        if (GameManager.Instance != null) GameManager.Instance.Pause = true;
        SetUpgradePanelState(true);

        // 3. 랜덤 뽑기 로직
        List<UpgradeScriptableObject> availableUpgrades = new List<UpgradeScriptableObject>(UpgradeDeck);
        int slotsCount = Mathf.Min(UpgradeUiSlots.Length, availableUpgrades.Count);

        for (int i = 0; i < UpgradeUiSlots.Length; i++)
        {
            if (i < slotsCount)
            {
                // 가중치(Chance) 합계 계산
                int totalChance = availableUpgrades.Sum(x => x.Chance);

                // if (totalChance == 0)
                // {
                //     UpgradeUiSlots[i].SetActive(false);
                //     continue;
                // }
                UpgradeScriptableObject chosenUpgrade = null;
                if (totalChance <= 0)
                {
                    // 비상 대책: 확률 무시하고 그냥 아무거나(0번) 뽑음
                    // (어차피 남은 게 다 확률 0인 카드들뿐이라는 뜻이니까요)
                    if (availableUpgrades.Count > 0)
                    {
                        int fallbackIndex = Random.Range(0, availableUpgrades.Count);
                        chosenUpgrade = availableUpgrades[fallbackIndex];
                    }
                }
                else
                {
                    int randomValue = Random.Range(0, totalChance);
                    // 룰렛 돌리기
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
                    // UI 슬롯 스크립트에 정보 전달 (UpgradeUi 스크립트가 있다고 가정)
                    UpgradeUiSlots[i].GetComponent<UpgradeUi>().SetInfo(chosenUpgrade);
                    
                    spawnedUpgrades.Add(chosenUpgrade);
                    availableUpgrades.Remove(chosenUpgrade); // 중복 등장 방지
                }
            }
            else
            {
                UpgradeUiSlots[i].SetActive(false); // 남는 슬롯 끄기
            }
        }
    }

    // ========================================================================
    // 2. 업그레이드 선택 시 실행 (UI 버튼에서 이 함수를 호출해야 함!)
    // ========================================================================
    public void OnUpgradeSelected(UpgradeScriptableObject chosenUpgrade)
    {
        // 1. 포인트(레벨) 증가
        chosenUpgrade.Points++;

        // 2. [핵심] 무기인가? 패시브 스탯인가?
        if (chosenUpgrade.linkedWeaponData != null)
        {
            // A. 무기라면 -> 플레이어 웨폰 컨트롤러에게 "이거 장착해/레벨업해"라고 던짐
            if (playerWeaponController != null)
            {
                playerWeaponController.AddWeapon(chosenUpgrade.linkedWeaponData);
                Debug.Log($"[Upgrade] 무기 적용: {chosenUpgrade.linkedWeaponData.weaponName}");
            }
        }
        else
        {
            // B. 스탯이라면 -> 패시브 적용 로직 실행
            ApplyStatBonus(chosenUpgrade);
            Debug.Log($"[Upgrade] 스탯 적용: {chosenUpgrade.upgradeType}");
        }

        // 3. 잡 클래스 조건 달성 확인 (포인트가 변했으므로)
        if (!isJobClassSet)
        {
            CheckAndSetJobClass();
        }

        // 4. 창 닫기
        Close();
    }

    // ========================================================================
    // 3. 패시브 스탯 적용 로직 (Switch문)
    // ========================================================================
    private void ApplyStatBonus(UpgradeScriptableObject upgrade)
    {
        // PlayerStats 싱글톤이 없다면 PlayerObject에서 가져옴
        var stats = PlayerStats.Instance; 
        // PlayerHealth 컴포넌트
        var health = PlayerObject.GetComponent<PlayerHealth>();

        float value = upgrade.statValue; // SO에 설정된 값 (예: 10, 0.5 ...)

        switch (upgrade.upgradeType)
        {
            case UpgradeScriptableObject.UpgradeType.Stat_MaxHealth:
                if (health != null) health.MaxHealth += value;
                break;

            case UpgradeScriptableObject.UpgradeType.Stat_Heal:
                if (health != null) health.Heal(value);
                break;

            case UpgradeScriptableObject.UpgradeType.Stat_Might: // 데미지
                if (stats != null) stats.DamageBonus += value;
                break;

            case UpgradeScriptableObject.UpgradeType.Stat_MoveSpeed:
                if (stats != null) stats.SpeedBonus += value;
                break;

            case UpgradeScriptableObject.UpgradeType.Stat_Cooldown:
                if (stats != null) stats.cooldownReduction += value; // 쿨타임 감소 로직에 따라 +인지 -인지 확인 필요
                break;
            
            case UpgradeScriptableObject.UpgradeType.Stat_Growth: // 경험치
                if (stats != null) stats.experienceBonus += value;
                break;

            // ... 필요한 스탯 케이스들 추가 ...
        }
    }

    // ========================================================================
    // 4. 잡 클래스 시스템 (데이터 기반)
    // ========================================================================
    private void CheckAndSetJobClass()
    {
        if (isJobClassSet || currentJob != null) return;

        foreach (JobDataSO job in allJobs)
        {
            if (CheckJobRequirements(job))
            {
                SetJob(job);
                break;
            }
        }
    }

    private bool CheckJobRequirements(JobDataSO job)
    {
        // 플레이어가 가진 활성화된 무기들
        var activeWeapons = playerWeaponController.activeWeapons;

        foreach (WeaponDataSO reqWeapon in job.requiredWeapons)
        {
            // 내 무기 중에 요구하는 무기 데이터랑 일치하는 게 있는지 확인
            bool hasIt = activeWeapons.Any(w => w.myData == reqWeapon);
            if (!hasIt) return false; // 하나라도 없으면 탈락
        }
        return true; // 모두 통과
    }

    private void SetJob(JobDataSO newJob)
    {
        currentJob = newJob;
        isJobClassSet = true;
        Debug.Log($"[Job Change] {newJob.jobName} 전직 완료!");

        // 전직 혜택 적용 (특정 스킬 확률 증가)
        foreach (var card in UpgradeDeck)
        {
            if (newJob.bonusUpgrades.Contains(card))
            {
                card.Chance += newJob.bonusChanceAmount;
            }
            else
            {
                // 필요 없는 스킬 확률 0 만들기 (선택사항)
                card.Chance = 0; 
            }
        }
        
        // 잡 클래스 상태 저장 필요 시 호출
        // SaveUpgradeData(); 
    }

    // ========================================================================
    // 5. 유틸리티 및 종료
    // ========================================================================
    public void Close()
    {
        if (GameManager.Instance != null) GameManager.Instance.Pause = false;
        if (MenuButtonController.Instance != null) MenuButtonController.Instance.InGameUpgrade = false;
        
        SetUpgradePanelState(false);
        spawnedUpgrades.Clear();
    }

    private void SetUpgradePanelState(bool isActive)
    {
        if (UpgradePanelObject != null) UpgradePanelObject.SetActive(isActive);
    }
    
    public void SetCombatState(bool isActive)
{
    // 예전 방식: 일일이 하나씩 끔 (이제 필요 없음)
    // if (TurretObject != null) TurretObject.SetActive(isActive);
    // ...

    // [새로운 방식] 무기 관리자에게 "전투 모드 전환해!" 라고 명령
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