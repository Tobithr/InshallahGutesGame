using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Health")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public Image damageVignette;

    [Header("Ammo")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI weaponNameText;

    [Header("Kill Points")]
    public TextMeshProUGUI killPointsText;

    [Header("Timer & Score")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;

    [Header("Crosshair")]
    public Image crosshairDot;
    public Image[] crosshairLines;
    public float crosshairSpread = 0f;

    [Header("Hit Markers")]
    public Image hitMarkerImage;
    public float hitMarkerDuration = 0.1f;
    public Color hitMarkerColor = Color.white;
    public Color killMarkerColor = Color.red;

    [Header("Death Screen")]
    public GameObject deathPanel;
    public TextMeshProUGUI deathText;

    [Header("Speedometer")]
    public TextMeshProUGUI speedText;

    private PlayerController _playerCtrl;
    private Coroutine _hitMarkerCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _playerCtrl = FindAnyObjectByType<PlayerController>();
        SubscribeToEvents();

        if (damageVignette != null)
            damageVignette.color = new Color(1f, 0f, 0f, 0f);
        if (deathPanel != null)
            deathPanel.SetActive(false);
        if (hitMarkerImage != null)
            hitMarkerImage.color = new Color(1f, 1f, 1f, 0f);
    }

    void SubscribeToEvents()
    {
        var health = FindAnyObjectByType<PlayerHealth>();
        if (health != null)
        {
            health.OnHealthChanged += UpdateHealth;
            health.OnDied += ShowDeathScreen;
            health.OnRespawned += HideDeathScreen;
        }

        var stats = FindAnyObjectByType<PlayerStats>();
        if (stats != null)
            stats.OnKillPointsChanged += UpdateKillPoints;

        var weapons = FindAnyObjectByType<WeaponManager>();
        if (weapons != null)
            weapons.OnWeaponChanged += OnWeaponChanged;

        var gm = FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.OnTimerUpdated += UpdateTimer;
            gm.OnKillRegistered += (k, v) => UpdateScore();
        }
    }

    void Update()
    {
        UpdateSpeedometer();
        UpdateAmmoDisplay();
        FadeDamageVignette();
    }

    void UpdateHealth(float current, float max)
    {
        if (healthSlider != null) healthSlider.value = current / max;
        if (healthText != null) healthText.text = Mathf.CeilToInt(current).ToString();

        if (damageVignette != null)
            damageVignette.color = new Color(1f, 0f, 0f, Mathf.Clamp01(1f - current / max) * 0.4f + 0.1f);
    }

    void FadeDamageVignette()
    {
        if (damageVignette == null) return;
        var c = damageVignette.color;
        if (c.a > 0f)
            damageVignette.color = new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 0f, 3f * Time.deltaTime));
    }

    void UpdateKillPoints(int points)
    {
        if (killPointsText != null) killPointsText.text = $"KP: {points}";
    }

    void UpdateTimer(float seconds)
    {
        if (timerText == null) return;
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        timerText.text = $"{m:00}:{s:00}";
    }

    void UpdateScore()
    {
        if (scoreText == null) return;
        int playerScore = GameManager.Instance?.GetScore("Player") ?? 0;
        scoreText.text = $"Score: {playerScore}";
    }

    void OnWeaponChanged(WeaponBase weapon)
    {
        if (weapon == null) return;
        if (weaponNameText != null) weaponNameText.text = weapon.weaponName;
        weapon.OnAmmoChanged += UpdateAmmo;
        UpdateAmmo(weapon.CurrentAmmo, weapon.ReserveAmmo);
    }

    void UpdateAmmo(int current, int reserve)
    {
        if (ammoText != null) ammoText.text = $"{current} / {reserve}";
    }

    void UpdateAmmoDisplay()
    {
        var wm = FindAnyObjectByType<WeaponManager>();
        if (wm?.CurrentWeapon == null) return;
        if (ammoText != null)
            ammoText.text = wm.CurrentWeapon.CurrentAmmo + " / " + wm.CurrentWeapon.ReserveAmmo;
    }

    void UpdateSpeedometer()
    {
        if (speedText == null || _playerCtrl == null) return;
        speedText.text = $"{_playerCtrl.GetTotalHorizontalSpeed():F0} u/s";
    }

    public void ShowHitMarker(bool isKill = false)
    {
        if (hitMarkerImage == null) return;
        if (_hitMarkerCoroutine != null) StopCoroutine(_hitMarkerCoroutine);
        _hitMarkerCoroutine = StartCoroutine(HitMarkerRoutine(isKill ? killMarkerColor : hitMarkerColor));
    }

    IEnumerator HitMarkerRoutine(Color color)
    {
        hitMarkerImage.color = color;
        yield return new WaitForSeconds(hitMarkerDuration);
        hitMarkerImage.color = new Color(color.r, color.g, color.b, 0f);
    }

    void ShowDeathScreen(string killerName)
    {
        if (deathPanel != null) deathPanel.SetActive(true);
        if (deathText != null) deathText.text = $"Killed by {killerName}";
    }

    void HideDeathScreen()
    {
        if (deathPanel != null) deathPanel.SetActive(false);
    }

    public void SetCrosshairSpread(float spread)
    {
        crosshairSpread = spread;
        if (crosshairLines == null) return;
        for (int i = 0; i < Mathf.Min(crosshairLines.Length, 4); i++)
        {
            if (crosshairLines[i] == null) continue;
            var rt = crosshairLines[i].rectTransform;
            bool isVertical = i < 2;
            rt.anchoredPosition = isVertical
                ? new Vector2(0f, (i == 0 ? 1f : -1f) * (6f + spread))
                : new Vector2((i == 2 ? -1f : 1f) * (6f + spread), 0f);
        }
    }
}
