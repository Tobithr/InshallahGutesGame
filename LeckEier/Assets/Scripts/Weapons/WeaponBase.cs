using System.Collections;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Base Stats")]
    public string weaponName = "Weapon";
    public float baseDamage = 25f;
    public float fireRate = 0.1f;       // seconds between shots
    public int maxAmmo = 30;
    public int reserveAmmo = 90;
    public float reloadTime = 1.5f;
    public bool isAutomatic = true;
    public float range = 200f;

    [Header("References")]
    public Transform firePoint;         // where the ray originates
    public ParticleSystem muzzleFlash;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;

    public int CurrentAmmo { get; protected set; }
    public int ReserveAmmo { get; protected set; }
    public bool IsReloading { get; protected set; }

    protected float _nextFireTime;
    protected float _damageMultiplier = 1f;
    protected AudioSource _audioSource;
    protected Camera _cam;

    private Vector3 _restLocalPos;
    private Quaternion _restLocalRot;
    private Coroutine _reloadAnim;

    public event System.Action<int, int> OnAmmoChanged;  // current, reserve

    protected virtual void Awake()
    {
        CurrentAmmo = maxAmmo;
        ReserveAmmo = reserveAmmo;
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        _cam = Camera.main;

        _restLocalPos = transform.localPosition;
        _restLocalRot = transform.localRotation;
    }

    protected virtual void OnDisable()
    {
        if (_reloadAnim != null) StopCoroutine(_reloadAnim);
        IsReloading = false;
        transform.localPosition = _restLocalPos;
        transform.localRotation = _restLocalRot;
    }

    public void TryFire()
    {
        if (IsReloading) return;
        if (Time.time < _nextFireTime) return;

        if (CurrentAmmo <= 0)
        {
            PlaySound(emptySound);
            TryReload();
            return;
        }

        _nextFireTime = Time.time + fireRate;
        Fire();
    }

    protected abstract void Fire();

    public void TryReload()
    {
        if (IsReloading) return;
        if (CurrentAmmo == maxAmmo) return;
        if (ReserveAmmo <= 0) return;

        _reloadAnim = StartCoroutine(ReloadRoutine());
    }

    IEnumerator ReloadRoutine()
    {
        IsReloading = true;
        PlaySound(reloadSound);

        // Reload pose: lower the weapon and tilt it for a "drop mag" look
        Vector3 lowPos = _restLocalPos + new Vector3(0f, -0.18f, 0.08f);
        Quaternion lowRot = _restLocalRot * Quaternion.Euler(30f, -20f, 25f);

        float t = 0f;
        float lowerDur  = reloadTime * 0.25f;   // 25% of reload: lower
        float holdEnd   = reloadTime * 0.75f;   // hold until 75%
        float returnDur = reloadTime * 0.25f;   // last 25%: raise back

        // Phase 1 – lower weapon
        while (t < lowerDur)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / lowerDur);
            transform.localPosition = Vector3.Lerp(_restLocalPos, lowPos, a);
            transform.localRotation = Quaternion.Lerp(_restLocalRot, lowRot, a);
            yield return null;
        }

        // Phase 2 – hold in reload pose (magazine swap)
        while (t < holdEnd)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Phase 3 – raise back to rest
        float phaseStart = t;
        while (t < reloadTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01((t - phaseStart) / returnDur);
            transform.localPosition = Vector3.Lerp(lowPos, _restLocalPos, a);
            transform.localRotation = Quaternion.Lerp(lowRot, _restLocalRot, a);
            yield return null;
        }

        transform.localPosition = _restLocalPos;
        transform.localRotation = _restLocalRot;

        int needed = maxAmmo - CurrentAmmo;
        int take = Mathf.Min(needed, ReserveAmmo);
        CurrentAmmo += take;
        ReserveAmmo -= take;
        IsReloading = false;

        OnAmmoChanged?.Invoke(CurrentAmmo, ReserveAmmo);
    }

    // Hitscan ray from camera center
    protected bool DoHitscan(out RaycastHit hit, float spread = 0f)
    {
        Vector3 dir = _cam.transform.forward;

        if (spread > 0f)
        {
            dir += new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0f
            );
            dir.Normalize();
        }

        Ray ray = new Ray(_cam.transform.position, dir);
        return Physics.Raycast(ray, out hit, range, ~LayerMask.GetMask("Ignore Raycast"), QueryTriggerInteraction.Ignore);
    }

    protected void DealDamage(RaycastHit hit)
    {
        var dmgReceiver = hit.collider.GetComponentInParent<DamageReceiver>();
        if (dmgReceiver != null)
        {
            float finalDamage = baseDamage * _damageMultiplier;
            dmgReceiver.TakeDamage(finalDamage, gameObject.name);
            HUDManager.Instance?.ShowHitMarker();
        }
    }

    protected void PlaySound(AudioClip clip)
    {
        if (clip == null || _audioSource == null) return;
        _audioSource.PlayOneShot(clip);
    }

    protected void TriggerMuzzleFlash()
    {
        if (muzzleFlash != null) muzzleFlash.Play();
    }

    protected void ConsumeAmmo(int count = 1)
    {
        CurrentAmmo = Mathf.Max(0, CurrentAmmo - count);
        OnAmmoChanged?.Invoke(CurrentAmmo, ReserveAmmo);
    }

    public void SetDamageMultiplier(float mult) => _damageMultiplier = mult;

    public void AddReserveAmmo(int amount)
    {
        ReserveAmmo += amount;
        OnAmmoChanged?.Invoke(CurrentAmmo, ReserveAmmo);
    }
}
