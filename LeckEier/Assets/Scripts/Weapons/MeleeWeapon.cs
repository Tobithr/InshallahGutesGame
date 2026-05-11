using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    [Header("Melee Settings")]
    public float meleeRange = 2.5f;

    protected override void Awake()
    {
        weaponName  = "Melee";
        baseDamage  = 55f;
        fireRate    = 0.55f;
        maxAmmo     = 999;
        reserveAmmo = 999;
        isAutomatic = false;
        range       = meleeRange;
        base.Awake();
    }

    protected override void Fire()
    {
        PlaySound(fireSound);

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, meleeRange, ~LayerMask.GetMask("Ignore Raycast"), QueryTriggerInteraction.Ignore))
        {
            var dmg = hit.collider.GetComponentInParent<DamageReceiver>();
            if (dmg != null)
            {
                dmg.TakeDamage(baseDamage * _damageMultiplier, gameObject.name);
                HUDManager.Instance?.ShowHitMarker();
            }
        }
    }

    // Melee has unlimited ammo, disable reload entirely
    public new void TryReload() { }
}
