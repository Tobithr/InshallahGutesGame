using UnityEngine;

public class AssaultRifle : WeaponBase
{
    [Header("AR Settings")]
    public float spread = 0.02f;

    protected override void Awake()
    {
        weaponName    = "Assault Rifle";
        baseDamage    = 22f;
        fireRate      = 0.09f;
        maxAmmo       = 30;
        reserveAmmo   = 120;
        reloadTime    = 1.8f;
        isAutomatic   = true;
        range         = 250f;
        base.Awake();
    }

    protected override void Fire()
    {
        TriggerMuzzleFlash();
        PlaySound(fireSound);
        ConsumeAmmo();

        if (DoHitscan(out RaycastHit hit, spread))
            DealDamage(hit);
    }
}
