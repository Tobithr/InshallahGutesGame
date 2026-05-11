using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float respawnDelay = 3f;

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event System.Action<float, float> OnHealthChanged;   // current, max
    public event System.Action<string> OnDied;                  // killer name
    public event System.Action OnRespawned;

    private string _playerName = "Player";

    void Awake()
    {
        CurrentHealth = maxHealth;
        _playerName = gameObject.name;
    }

    public void TakeDamage(float amount, string attackerName)
    {
        if (IsDead) return;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth <= 0f)
            Die(attackerName);
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void SetMaxHealth(float newMax)
    {
        float ratio = CurrentHealth / maxHealth;
        maxHealth = newMax;
        CurrentHealth = ratio * maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void Die(string killerName)
    {
        IsDead = true;
        OnDied?.Invoke(killerName);
        GameManager.Instance?.RegisterKill(killerName, _playerName);

        // Disable player visuals/input while dead
        var ctrl = GetComponent<PlayerController>();
        if (ctrl != null) ctrl.enabled = false;

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    public void Respawn()
    {
        IsDead = false;
        CurrentHealth = maxHealth;

        var ctrl = GetComponent<PlayerController>();
        if (ctrl != null) ctrl.enabled = true;

        // Ask spawn manager for a position
        SpawnManager.Instance?.RespawnPlayer(this);

        OnRespawned?.Invoke();
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
