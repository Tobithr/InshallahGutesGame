using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Match Settings")]
    public int killLimit = 20;
    public float matchDuration = 600f;   // 10 minutes
    public bool infiniteTime = false;

    public enum GameState { WaitingToStart, Playing, MatchOver }
    public GameState State { get; private set; } = GameState.Playing;

    // Kill scores: playerName → killCount
    private Dictionary<string, int> _scores = new Dictionary<string, int>();
    private float _matchTimer;

    public event System.Action<string, string> OnKillRegistered;    // killer, victim
    public event System.Action<string> OnMatchOver;                 // winner name
    public event System.Action<float> OnTimerUpdated;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _matchTimer = matchDuration;
        State = GameState.Playing;
    }

    void Update()
    {
        if (State != GameState.Playing) return;
        if (infiniteTime) return;

        _matchTimer -= Time.deltaTime;
        OnTimerUpdated?.Invoke(_matchTimer);

        if (_matchTimer <= 0f)
            EndMatch(GetLeader());
    }

    public void RegisterKill(string killerName, string victimName)
    {
        if (State != GameState.Playing) return;
        if (string.IsNullOrEmpty(killerName)) return;

        if (!_scores.ContainsKey(killerName)) _scores[killerName] = 0;
        _scores[killerName]++;

        OnKillRegistered?.Invoke(killerName, victimName);

        // Award kill points to the local player
        if (killerName == "Player")
        {
            var player = FindAnyObjectByType<PlayerStats>();
            player?.AddKillPoints(1);
        }

        KillFeedUI.Instance?.AddEntry(killerName, victimName);

        if (_scores[killerName] >= killLimit)
            EndMatch(killerName);
    }

    public int GetScore(string playerName) =>
        _scores.TryGetValue(playerName, out int s) ? s : 0;

    string GetLeader()
    {
        string leader = "";
        int max = -1;
        foreach (var kvp in _scores)
            if (kvp.Value > max) { max = kvp.Value; leader = kvp.Key; }
        return leader;
    }

    void EndMatch(string winner)
    {
        State = GameState.MatchOver;
        OnMatchOver?.Invoke(winner);
    }

    public Dictionary<string, int> GetScoreboard() => new Dictionary<string, int>(_scores);

    public float GetMatchTimeRemaining() => _matchTimer;
}
