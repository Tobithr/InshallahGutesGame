using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Dual grapple hook system: RMB = primary, Q = secondary
// Farther grapple = more speed, higher grapple = more upward force (Redmatch 2 mechanic)
public class GrappleHook : MonoBehaviour
{
    [Header("Grapple Settings")]
    public float maxDistance = 50f;
    public float pullSpeed = 18f;
    public float releaseImpulseMultiplier = 1.2f;
    public float reelSpeed = 0.5f;       // how fast rope length shrinks
    public LayerMask grappleMask;

    [Header("Visual")]
    public Transform primaryFirePoint;   // muzzle for primary hook
    public Transform secondaryFirePoint; // muzzle for secondary hook
    public Material ropeMaterial;

    private PlayerController _player;
    private Camera _cam;

    // Primary grapple (RMB)
    private bool _primaryActive;
    private Vector3 _primaryPoint;
    private float _primaryLength;
    private LineRenderer _primaryLine;

    // Secondary grapple (Q)
    private bool _secondaryActive;
    private Vector3 _secondaryPoint;
    private float _secondaryLength;
    private LineRenderer _secondaryLine;

    void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
        _cam = GetComponentInParent<Camera>();
        if (_cam == null) _cam = Camera.main;

        _primaryLine = CreateLineRenderer("PrimaryRope");
        _secondaryLine = CreateLineRenderer("SecondaryRope");
    }

    LineRenderer CreateLineRenderer(string objName)
    {
        var go = new GameObject(objName);
        go.transform.SetParent(transform);
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;
        lr.enabled = false;
        if (ropeMaterial != null) lr.material = ropeMaterial;
        else
        {
            lr.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            lr.material.color = new Color(0.9f, 0.2f, 0.2f);
        }
        return lr;
    }

    void Update()
    {
        HandleInput();
        PullPlayer();
        UpdateLineRenderers();
    }

    void HandleInput()
    {
        // Primary grapple: Right Mouse Button
        if (Mouse.current.rightButton.wasPressedThisFrame)
            TryFireGrapple(ref _primaryActive, ref _primaryPoint, ref _primaryLength);
        if (Mouse.current.rightButton.wasReleasedThisFrame)
            ReleaseGrapple(ref _primaryActive, _primaryLine);

        // Secondary grapple: Q
        if (Keyboard.current.qKey.wasPressedThisFrame)
            TryFireGrapple(ref _secondaryActive, ref _secondaryPoint, ref _secondaryLength);
        if (Keyboard.current.qKey.wasReleasedThisFrame)
            ReleaseGrapple(ref _secondaryActive, _secondaryLine);
    }

    void TryFireGrapple(ref bool active, ref Vector3 point, ref float length)
    {
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, grappleMask, QueryTriggerInteraction.Ignore))
        {
            active = true;
            point = hit.point;
            length = Vector3.Distance(transform.position, point);
        }
    }

    void ReleaseGrapple(ref bool active, LineRenderer line)
    {
        if (!active) return;
        active = false;
        line.enabled = false;

        // On release: convert grapple velocity direction into an impulse
        // This lets players slingshot off grapple points
        Vector3 vel = _player.CurrentVelocity;
        Vector3 releaseBoost = vel * releaseImpulseMultiplier;
        _player.AddImpulse(releaseBoost * Time.deltaTime * 5f);
    }

    void PullPlayer()
    {
        if (_primaryActive)
            ApplyGrapplePull(_primaryPoint, ref _primaryLength);

        if (_secondaryActive)
            ApplyGrapplePull(_secondaryPoint, ref _secondaryLength);

        // Slingshot: two hooks active simultaneously → gain extra upward momentum
        if (_primaryActive && _secondaryActive)
        {
            Vector3 midPoint = (_primaryPoint + _secondaryPoint) * 0.5f;
            Vector3 toMid = (midPoint - transform.position).normalized;
            _player.AddImpulse(toMid * pullSpeed * 0.5f * Time.deltaTime);
        }
    }

    void ApplyGrapplePull(Vector3 grapplePoint, ref float ropeLength)
    {
        Vector3 dir = grapplePoint - transform.position;
        float dist = dir.magnitude;
        dir.Normalize();

        // Reel in: shorten rope length over time
        ropeLength = Mathf.Max(1f, ropeLength - reelSpeed * Time.deltaTime);

        // Only pull if further than rope length
        if (dist > ropeLength)
        {
            // Speed scales with distance (farther = faster)
            float speedScale = Mathf.Clamp(dist / maxDistance, 0.3f, 1f);
            float currentPull = pullSpeed * speedScale;

            // More upward speed if grapple is above player
            float upBonus = Mathf.Clamp(dir.y * 1.5f, 0f, 1f);
            Vector3 pullDir = dir + Vector3.up * upBonus;
            pullDir.Normalize();

            _player.MoveRaw(pullDir * currentPull * Time.deltaTime);
        }
    }

    void UpdateLineRenderers()
    {
        UpdateLine(_primaryLine, _primaryActive, primaryFirePoint, _primaryPoint);
        UpdateLine(_secondaryLine, _secondaryActive, secondaryFirePoint, _secondaryPoint);
    }

    void UpdateLine(LineRenderer line, bool active, Transform firePoint, Vector3 endpoint)
    {
        line.enabled = active;
        if (!active) return;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        line.SetPosition(0, origin);
        line.SetPosition(1, endpoint);
    }

    public bool IsGrappling => _primaryActive || _secondaryActive;
}
