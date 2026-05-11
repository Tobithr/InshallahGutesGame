using System.Collections;
using UnityEngine;

public class SniperRifle : WeaponBase
{
    [Header("Sniper Settings")]
    public float adsZoom = 5f;
    public float adsTransitionSpeed = 8f;

    private bool _isADS;
    private Camera _mainCam;
    private float _defaultFov;
    private float _targetFov;

    protected override void Awake()
    {
        weaponName  = "Sniper Rifle";
        baseDamage  = 150f;   // one-shot base (health upgrades counter this)
        fireRate    = 1.4f;
        maxAmmo     = 5;
        reserveAmmo = 20;
        reloadTime  = 2.5f;
        isAutomatic = false;
        range       = 500f;
        base.Awake();

        _mainCam    = Camera.main;
        _defaultFov = _mainCam != null ? _mainCam.fieldOfView : 60f;
        _targetFov  = _defaultFov;
    }

    void OnEnable()
    {
        if (_mainCam != null)
        {
            _defaultFov = _mainCam.fieldOfView;
            _targetFov  = _defaultFov;
        }
    }

    void OnDisable()
    {
        // Reset zoom when switching away
        _isADS = false;
        if (_mainCam != null) _mainCam.fieldOfView = _defaultFov;
    }

    void Update()
    {
        HandleADS();
        SmoothFov();
    }

    void HandleADS()
    {
        if (UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)
        {
            _isADS = !_isADS;
            _targetFov = _isADS ? _defaultFov / adsZoom : _defaultFov;
        }
    }

    void SmoothFov()
    {
        if (_mainCam == null) return;
        _mainCam.fieldOfView = Mathf.Lerp(_mainCam.fieldOfView, _targetFov, adsTransitionSpeed * Time.deltaTime);
    }

    protected override void Fire()
    {
        TriggerMuzzleFlash();
        PlaySound(fireSound);
        ConsumeAmmo();

        if (DoHitscan(out RaycastHit hit, 0f))
            DealDamage(hit);

        // Bolt action: force reload after each shot
        if (CurrentAmmo > 0)
            StartCoroutine(BoltDelay());
    }

    IEnumerator BoltDelay()
    {
        yield return new WaitForSeconds(fireRate * 0.6f);
        // small bolt-cycle animation hook (no actual effect)
    }
}
