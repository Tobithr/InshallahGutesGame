using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(DamageReceiver))]
public class BotController : MonoBehaviour
{
    [Header("Bot Settings")]
    public string botName       = "Bot";
    public float maxHealth      = 100f;
    public float detectionRange = 25f;
    public float attackRange    = 15f;
    public float attackDamage   = 15f;
    public float attackRate     = 0.8f;
    public float respawnDelay   = 4f;
    public float moveSpeed      = 12f;
    public float gravity        = -75f;

    public bool IsDead { get; private set; }

    private CharacterController _cc;
    private DamageReceiver _health;
    private Transform _target;
    private float _nextAttackTime;
    private float _verticalVelocity;

    private Vector3 _moveDir;
    private float   _dirTimer;
    private float   _strafeSign  = 1f;
    private float   _strafeTimer;

    enum BotState { Patrol, Chase, Attack }
    BotState _state = BotState.Patrol;

    void Awake()
    {
        _cc     = GetComponent<CharacterController>();
        _health = GetComponent<DamageReceiver>();
        _health.maxHealth = maxHealth;
        gameObject.name   = botName;
        _health.OnDied   += _ => OnKilled();
    }

    void Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        if (player != null) _target = player.transform;
        PickPatrolDir();
    }

    void Update()
    {
        if (IsDead || _target == null) return;

        float dist = Vector3.Distance(transform.position, _target.position);
        UpdateState(dist);

        Vector3 horizontal = _state switch
        {
            BotState.Chase  => ChaseMove(),
            BotState.Attack => AttackMove(dist),
            _               => PatrolMove(),
        };

        if (_cc.isGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
        _verticalVelocity += gravity * Time.deltaTime;
        _verticalVelocity  = Mathf.Max(_verticalVelocity, -50f);

        _cc.Move((horizontal + Vector3.up * _verticalVelocity) * Time.deltaTime);
    }

    void UpdateState(float dist)
    {
        if      (dist <= attackRange)    _state = BotState.Attack;
        else if (dist <= detectionRange) _state = BotState.Chase;
        else                             _state = BotState.Patrol;
    }

    // Random direction; bounces off walls; changes on timer
    Vector3 PatrolMove()
    {
        _dirTimer -= Time.deltaTime;

        if (Physics.Raycast(transform.position + Vector3.up, _moveDir, 1.5f))
            _moveDir = Quaternion.Euler(0f, Random.Range(90f, 270f), 0f) * _moveDir;

        if (_dirTimer <= 0f) PickPatrolDir();

        FaceTo(_moveDir);
        return _moveDir * moveSpeed * 0.45f;
    }

    // Zigzag toward player via sinusoidal side-step
    Vector3 ChaseMove()
    {
        Vector3 toTarget = Flat(_target.position - transform.position);
        if (toTarget.sqrMagnitude < 0.01f) return Vector3.zero;

        Vector3 fwd   = toTarget.normalized;
        Vector3 right = Vector3.Cross(Vector3.up, fwd);
        float   weave = Mathf.Sin(Time.time * 2.5f) * 0.55f;

        FaceTo(fwd);
        return (fwd + right * weave).normalized * moveSpeed;
    }

    // Circle-strafe; backs off when too close; shoots
    Vector3 AttackMove(float dist)
    {
        Vector3 toTarget = Flat(_target.position - transform.position);
        if (toTarget.sqrMagnitude < 0.01f) return Vector3.zero;

        Vector3 fwd   = toTarget.normalized;
        Vector3 right = Vector3.Cross(Vector3.up, fwd);

        _strafeTimer -= Time.deltaTime;
        if (_strafeTimer <= 0f)
        {
            _strafeSign  = Random.value > 0.5f ? 1f : -1f;
            _strafeTimer = Random.Range(1.2f, 2.8f);
        }

        float   backOff = dist < attackRange * 0.35f ? -0.5f : 0f;
        Vector3 move    = (right * _strafeSign + fwd * backOff).normalized;

        FaceTo(fwd);

        if (Time.time >= _nextAttackTime)
        {
            _nextAttackTime = Time.time + attackRate;
            ShootAtTarget();
        }

        return move * moveSpeed * 0.6f;
    }

    void PickPatrolDir()
    {
        float a = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        _moveDir  = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
        _dirTimer = Random.Range(1.5f, 4f);
    }

    void FaceTo(Vector3 worldDir)
    {
        Vector3 flat = Flat(worldDir);
        if (flat.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(flat),
            8f * Time.deltaTime);
    }

    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    void ShootAtTarget()
    {
        Vector3 dir = (_target.position + Vector3.up * 0.5f - transform.position).normalized;
        dir += Random.insideUnitSphere * 0.1f;
        if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, attackRange * 1.5f))
        {
            var dmg = hit.collider.GetComponentInParent<DamageReceiver>();
            if (dmg != null && dmg.gameObject != gameObject)
                dmg.TakeDamage(attackDamage, botName);
        }
    }

    public void OnKilled()
    {
        if (IsDead) return;
        IsDead      = true;
        _cc.enabled = false;
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        IsDead = false;
        _health.Revive();
        SpawnManager.Instance?.RespawnBot(this);
        _cc.enabled = true;
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = true;
        PickPatrolDir();
    }
}
