using UnityEngine;

public class Shotgun : WeaponBase
{
    [Header("Shotgun Settings")]
    public int pelletCount = 8;
    public float spread = 0.08f;

    protected override void Awake()
    {
        weaponName  = "Shotgun";
        baseDamage  = 14f;   // per pellet (8 × 14 = 112 max)
        fireRate    = 0.85f;
        maxAmmo     = 8;
        reserveAmmo = 32;
        reloadTime  = 2.2f;
        isAutomatic = false;
        range       = 60f;
        base.Awake();
    }

    protected override void Fire()
    {
        TriggerMuzzleFlash();
        PlaySound(fireSound);
        ConsumeAmmo();

        for (int i = 0; i < pelletCount; i++)
        {
            if (DoHitscan(out RaycastHit hit, spread))
                DealDamage(hit);
        }
    }
}
