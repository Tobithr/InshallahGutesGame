using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Tab to open/close. Shows 4 upgrade buttons with current level, cost, and effect.
public class StatUpgradeUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject upgradePanel;

    [Header("Kill Points Display")]
    public TextMeshProUGUI killPointsDisplay;

    [Header("Speed Upgrade")]
    public Button speedButton;
    public TextMeshProUGUI speedLevelText;
    public TextMeshProUGUI speedCostText;
    public TextMeshProUGUI speedEffectText;

    [Header("Health Upgrade")]
    public Button healthButton;
    public TextMeshProUGUI healthLevelText;
    public TextMeshProUGUI healthCostText;
    public TextMeshProUGUI healthEffectText;

    [Header("Damage Upgrade")]
    public Button damageButton;
    public TextMeshProUGUI damageLevelText;
    public TextMeshProUGUI damageCostText;
    public TextMeshProUGUI damageEffectText;

    [Header("Jump Upgrade")]
    public Button jumpButton;
    public TextMeshProUGUI jumpLevelText;
    public TextMeshProUGUI jumpCostText;
    public TextMeshProUGUI jumpEffectText;

    private PlayerStats _stats;
    private bool _isOpen;

    void Start()
    {
        _stats = FindAnyObjectByType<PlayerStats>();

        speedButton?.onClick.AddListener(UpgradeSpeed);
        healthButton?.onClick.AddListener(UpgradeHealth);
        damageButton?.onClick.AddListener(UpgradeDamage);
        jumpButton?.onClick.AddListener(UpgradeJump);

        if (upgradePanel != null) upgradePanel.SetActive(false);
        RefreshUI();
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame)
            TogglePanel();
    }

    void TogglePanel()
    {
        _isOpen = !_isOpen;
        if (upgradePanel != null) upgradePanel.SetActive(_isOpen);

        // Unlock/lock cursor while upgrade menu is open
        Cursor.lockState = _isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _isOpen;

        if (_isOpen) RefreshUI();
    }

    void UpgradeSpeed()  { _stats?.TryUpgradeSpeed();  RefreshUI(); }
    void UpgradeHealth() { _stats?.TryUpgradeHealth(); RefreshUI(); }
    void UpgradeDamage() { _stats?.TryUpgradeDamage(); RefreshUI(); }
    void UpgradeJump()   { _stats?.TryUpgradeJump();   RefreshUI(); }

    void RefreshUI()
    {
        if (_stats == null) return;

        if (killPointsDisplay != null)
            killPointsDisplay.text = $"Kill Points: {_stats.killPoints}";

        UpdateRow(speedLevelText,  speedCostText,  speedEffectText,  speedButton,
            _stats.SpeedLevel, $"+{_stats.GetCurrentSpeedMultiplier() * 100f - 100f:F0}% Speed");

        UpdateRow(healthLevelText, healthCostText, healthEffectText, healthButton,
            _stats.HealthLevel, $"+{_stats.GetCurrentHealthBonus():F0} HP");

        UpdateRow(damageLevelText, damageCostText, damageEffectText, damageButton,
            _stats.DamageLevel, $"+{_stats.GetCurrentDamageMultiplier() * 100f - 100f:F0}% Damage");

        UpdateRow(jumpLevelText,   jumpCostText,   jumpEffectText,   jumpButton,
            _stats.JumpLevel, $"+{_stats.GetCurrentJumpMultiplier() * 100f - 100f:F0}% Jump");
    }

    void UpdateRow(TextMeshProUGUI levelTxt, TextMeshProUGUI costTxt,
                   TextMeshProUGUI effectTxt, Button btn, int level, string effectStr)
    {
        bool maxed = level >= 5;
        int cost = _stats.GetUpgradeCost(level);

        if (levelTxt  != null) levelTxt.text  = $"Lv {level}/5";
        if (costTxt   != null) costTxt.text   = maxed ? "MAX" : $"Cost: {cost} KP";
        if (effectTxt != null) effectTxt.text = effectStr;
        if (btn       != null) btn.interactable = !maxed && _stats.killPoints >= cost;
    }
}
