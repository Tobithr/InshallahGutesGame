using UnityEngine;

// Attach to any entity that can receive damage (players, bots, destructibles)
public class DamageReceiver : MonoBehaviour
{
    public float maxHealth = 100f;
    public bool isPlayer = false;

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event System.Action<float, float> OnHealthChanged;
    public event System.Action<string> OnDied;

    void Awake()
    {
        CurrentHealth = maxHealth;
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
        CurrentHealth = maxHealth * ratio;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void Die(string killerName)
    {
        IsDead = true;
        OnDied?.Invoke(killerName);

        // Delegate to PlayerHealth for player-specific respawn logic
        var playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // PlayerHealth handles its own death flow
            return;
        }

        // For bots and other entities
        var bot = GetComponent<BotController>();
        if (bot != null)
        {
            GameManager.Instance?.RegisterKill(killerName, gameObject.name);
            bot.OnKilled();
        }
    }

    public void Revive()
    {
        IsDead = false;
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
