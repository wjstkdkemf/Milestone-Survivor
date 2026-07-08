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
    [SerializeField] protected WeaponMotionData attackMotion;
    protected WeaponAnimationController weaponAnimator;
    public virtual void Initialize(WeaponDataSO data)
    {   
        currentHitRadius = data.hitRadius;
        currentMaxHits = data.maxHits;
        currentProjectileSpeed = data.projectileSpeed;

        weaponAnimator =  GetComponentInParent<WeaponAnimationController>();
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
        preview.Description = upgrade != null ? upgrade.GetCurrentDescription() : string.Empty;

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.status",
            "upgrade.value.equipped",
            "upgrade.value.upgrade",
            true,
            true
        ));

        preview.Lines.Add(new UpgradePreviewLine(
            "upgrade.stat.level",
            $"Lv.{CurrentLevel}",
            $"Lv.{CurrentLevel + 1}"
        ));

        return preview;
    }
    protected void PlayAttackMotion()
    {
        if (weaponAnimator != null && attackMotion != null)
            weaponAnimator.PlayMotion(attackMotion);
    }
}
