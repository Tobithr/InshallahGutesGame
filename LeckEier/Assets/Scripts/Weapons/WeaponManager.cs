using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public WeaponBase[] weapons;      // Assign in Inspector (AR, Shotgun, Sniper, Melee)
    public int startingWeaponIndex = 0;

    public WeaponBase CurrentWeapon { get; private set; }
    public int CurrentIndex { get; private set; }

    private float _damageMultiplier = 1f;

    public event System.Action<WeaponBase> OnWeaponChanged;

    void Start()
    {
        // Disable all weapons, then activate starting one
        foreach (var w in weapons)
            if (w != null) w.gameObject.SetActive(false);

        EquipWeapon(startingWeaponIndex);
    }

    void Update()
    {
        HandleWeaponSwitch();
        HandleFire();
        HandleReload();
    }

    void HandleWeaponSwitch()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll > 0f) SwitchWeapon(-1);
        else if (scroll < 0f) SwitchWeapon(1);

        // Number keys 1-4
        var kb = Keyboard.current;
        if (kb.digit1Key.wasPressedThisFrame) EquipWeapon(0);
        if (kb.digit2Key.wasPressedThisFrame) EquipWeapon(1);
        if (kb.digit3Key.wasPressedThisFrame) EquipWeapon(2);
        if (kb.digit4Key.wasPressedThisFrame) EquipWeapon(3);
    }

    void HandleFire()
    {
        if (CurrentWeapon == null) return;

        if (CurrentWeapon.isAutomatic)
        {
            if (Mouse.current.leftButton.isPressed) CurrentWeapon.TryFire();
        }
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame) CurrentWeapon.TryFire();
        }
    }

    void HandleReload()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
            CurrentWeapon?.TryReload();
    }

    void SwitchWeapon(int direction)
    {
        int newIndex = (CurrentIndex + direction + weapons.Length) % weapons.Length;
        EquipWeapon(newIndex);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;
        if (weapons[index] == null) return;

        if (CurrentWeapon != null)
            CurrentWeapon.gameObject.SetActive(false);

        CurrentIndex = index;
        CurrentWeapon = weapons[index];
        CurrentWeapon.gameObject.SetActive(true);
        CurrentWeapon.SetDamageMultiplier(_damageMultiplier);

        OnWeaponChanged?.Invoke(CurrentWeapon);
    }

    public void SetDamageMultiplier(float multiplier)
    {
        _damageMultiplier = multiplier;
        if (CurrentWeapon != null)
            CurrentWeapon.SetDamageMultiplier(_damageMultiplier);
    }
}
