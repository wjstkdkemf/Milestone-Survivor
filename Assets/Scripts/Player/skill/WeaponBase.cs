using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponDataSO myData;
    public string WeaponID;
    public int CurrentLevel;
    protected long currentDamage;
    protected float currentCooldown;
    protected float currentHitRadius;
    protected int currentMaxHits;
    protected float currentProjectileSpeed;
    public virtual void Initialize(WeaponDataSO data)
    {   
        currentHitRadius = data.hitRadius;
        currentMaxHits = data.maxHits;
        currentProjectileSpeed = data.projectileSpeed;
    }

    // 플레이어의 Update에서 매 프레임 호출해줄 함수
    public abstract void OnUpdate();
    public virtual void LevelUp()
    {
        // 자식들이 오버라이드해서 구현
        // 예: OrbWeapon은 orbCount++, AxeWeapon은 데미지++
        Debug.Log("무기 레벨업!");
    }
    public virtual UpgradePreviewData GetUpgradePreview(UpgradeScriptableObject upgrade)
    {
        UpgradePreviewData preview = new UpgradePreviewData();

        preview.ShortDescription = upgrade != null ? upgrade.GetCurrentShortDescription() : "";
        preview.Description = upgrade != null ? upgrade.GetCurrentDescription() : "강화 후 효과를 확인합니다.";

        preview.Lines.Add(new UpgradePreviewLine(
            "상태",
            "장착됨",
            "강화"
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "레벨",
            $"Lv.{CurrentLevel}",
            $"Lv.{CurrentLevel + 1}"
        ));

        return preview;
    }
}
