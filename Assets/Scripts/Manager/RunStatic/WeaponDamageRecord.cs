public class WeaponDamageRecord
{
    public string WeaponId { get; }
    public long TotalDamage { get; private set; }
    public long HighestSingleHit { get; private set; }
    public int HitCount { get; private set; }

    public WeaponDamageRecord(string weaponId)
    {
        WeaponId = weaponId;
    }

    public void AddDamage(long damage)
    {
        if (damage <= 0)
            return;

        TotalDamage = SaturatingAdd(TotalDamage, damage);

        if (damage > HighestSingleHit)
            HighestSingleHit = damage;

        HitCount++;
    }
    private long SaturatingAdd(long a, long b)
    {
        if (a > long.MaxValue - b)
            return long.MaxValue;

        return a + b;
    }
}