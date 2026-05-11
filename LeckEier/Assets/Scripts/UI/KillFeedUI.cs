using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KillFeedUI : MonoBehaviour
{
    public static KillFeedUI Instance { get; private set; }

    public GameObject killEntryPrefab;   // Prefab with a TextMeshProUGUI component
    public Transform entryContainer;
    public int maxEntries = 5;
    public float entryLifetime = 5f;
    public Color playerKillColor = new Color(1f, 0.3f, 0.3f);
    public Color botKillColor = Color.white;

    private Queue<GameObject> _entries = new Queue<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddEntry(string killer, string victim)
    {
        if (killEntryPrefab == null || entryContainer == null) return;

        // Remove oldest if over limit
        while (_entries.Count >= maxEntries)
        {
            var old = _entries.Dequeue();
            if (old != null) Destroy(old);
        }

        var entry = Instantiate(killEntryPrefab, entryContainer);
        var tmp = entry.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = $"{killer}  ›  {victim}";
            tmp.color = (killer == "Player") ? playerKillColor : botKillColor;
        }

        _entries.Enqueue(entry);
        StartCoroutine(FadeAndRemove(entry, entryLifetime));
    }

    IEnumerator FadeAndRemove(GameObject entry, float lifetime)
    {
        yield return new WaitForSeconds(lifetime * 0.7f);

        // Fade out in last 30% of lifetime
        float fadeTime = lifetime * 0.3f;
        float elapsed = 0f;
        var tmp = entry?.GetComponent<TextMeshProUGUI>();

        while (elapsed < fadeTime && entry != null)
        {
            elapsed += Time.deltaTime;
            if (tmp != null)
            {
                Color c = tmp.color;
                tmp.color = new Color(c.r, c.g, c.b, 1f - (elapsed / fadeTime));
            }
            yield return null;
        }

        if (entry != null) Destroy(entry);
    }
}
