using UnityEngine;

// Manages kill-point based stat upgrades (Redmatch 2 core progression)
public class PlayerStats : MonoBehaviour
{
    [Header("Kill Points")]
    public int killPoints = 0;

    public event System.Action<int> OnKillPointsChanged;
    public event System.Action OnStatsChanged;

    // Backing fields so they can be passed as ref
    private int _speedLevel;
    private int _healthLevel;
    private int _damageLevel;
    private int _jumpLevel;

    public int SpeedLevel  => _speedLevel;
    public int HealthLevel => _healthLevel;
    public int DamageLevel => _damageLevel;
    public int JumpLevel   => _jumpLevel;

    private static readonly int[] UpgradeCosts = { 2, 3, 5, 8, 12 };

    private static readonly float[] SpeedBonus  = { 0f, 0.12f, 0.25f, 0.40f, 0.55f, 0.75f };
    private static readonly float[] HealthBonus = { 0f, 25f,   50f,   80f,   120f,  170f  };
    private static readonly float[] DamageBonus = { 0f, 0.10f, 0.22f, 0.35f, 0.50f, 0.70f };
    private static readonly float[] JumpBonus   = { 0f, 0.10f, 0.22f, 0.35f, 0.50f, 0.70f };

    private PlayerController _ctrl;
    private PlayerHealth _health;
    private WeaponManager _weapons;

    void Awake()
    {
        _ctrl    = GetComponent<PlayerController>();
        _health  = GetComponent<PlayerHealth>();
        _weapons = GetComponent<WeaponManager>();
    }

    public void AddKillPoints(int amount)
    {
        killPoints += amount;
        OnKillPointsChanged?.Invoke(killPoints);
    }

    public int GetUpgradeCost(int currentLevel) =>
        currentLevel < UpgradeCosts.Length ? UpgradeCosts[currentLevel] : 999;

    public bool TryUpgradeSpeed()  => TryUpgrade(ref _speedLevel);
    public bool TryUpgradeHealth() => TryUpgrade(ref _healthLevel);
    public bool TryUpgradeDamage() => TryUpgrade(ref _damageLevel);
    public bool TryUpgradeJump()   => TryUpgrade(ref _jumpLevel);

    bool TryUpgrade(ref int level)
    {
        if (level >= 5) return false;
        int cost = GetUpgradeCost(level);
        if (killPoints < cost) return false;

        killPoints -= cost;
        level++;
        ApplyStats();
        OnKillPointsChanged?.Invoke(killPoints);
        OnStatsChanged?.Invoke();
        return true;
    }

    void ApplyStats()
    {
        if (_ctrl != null)
        {
            _ctrl.speedModifier = 1f + SpeedBonus[_speedLevel];
            _ctrl.jumpModifier  = 1f + JumpBonus[_jumpLevel];
        }

        if (_health != null)
            _health.SetMaxHealth(100f + HealthBonus[_healthLevel]);

        if (_weapons != null)
            _weapons.SetDamageMultiplier(1f + DamageBonus[_damageLevel]);
    }

    public float GetCurrentSpeedMultiplier()  => 1f + SpeedBonus[_speedLevel];
    public float GetCurrentHealthBonus()      => HealthBonus[_healthLevel];
    public float GetCurrentDamageMultiplier() => 1f + DamageBonus[_damageLevel];
    public float GetCurrentJumpMultiplier()   => 1f + JumpBonus[_jumpLevel];
}
