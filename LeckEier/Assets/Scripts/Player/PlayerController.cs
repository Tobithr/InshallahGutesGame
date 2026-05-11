using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 9f;
    public float sprintMultiplier = 1.5f;
    public float crouchMultiplier = 0.5f;
    public float airAcceleration = 40f;

    [Header("Jump")]
    public float jumpForce = 8f;
    public float gravity = -30f;
    public float coyoteTime = 0.12f;

    [Header("Crouch")]
    public float normalHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchCenter = 0.5f;
    public float normalCenter = 1f;

    [Header("Ground Check")]
    public float groundCheckRadius = 0.35f;
    public LayerMask groundMask;

    // Applied by PlayerStats
    [HideInInspector] public float speedModifier = 1f;
    [HideInInspector] public float jumpModifier = 1f;

    public bool IsGrounded { get; private set; }
    public bool IsCrouching { get; private set; }
    public Vector3 CurrentVelocity => _cc.velocity;

    public event System.Action OnLanded;

    private CharacterController _cc;
    private float _verticalVelocity;
    private Vector3 _bhopMomentum;   // preserved momentum (grapple + bhop)
    private float _coyoteTimer;
    private bool _wasGrounded;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        ApplyHeight(normalHeight, normalCenter);
    }

    void Update()
    {
        CheckGround();
        HandleCrouch();
        HandleHorizontalMove();
        HandleJump();
        ApplyVertical();
        ApplyBhopMomentum();
    }

    void CheckGround()
    {
        // Extra probe below CC bottom: skinWidth accounts for CC hover, +0.1 ensures reliable overlap
        Vector3 spherePos = transform.position + _cc.center
            + Vector3.down * (_cc.height * 0.5f - groundCheckRadius + _cc.skinWidth + 0.1f);
        bool grounded = Physics.CheckSphere(spherePos, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        if (grounded && !_wasGrounded) OnLanded?.Invoke();
        if (grounded) _coyoteTimer = coyoteTime;
        else _coyoteTimer -= Time.deltaTime;

        if (grounded && _verticalVelocity < 0f) _verticalVelocity = -2f;

        _wasGrounded = IsGrounded;
        IsGrounded = grounded;
    }

    void HandleHorizontalMove()
    {
        var kb = Keyboard.current;
        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float z = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        bool hasInput = x != 0f || z != 0f;

        bool sprinting = kb.leftShiftKey.isPressed && z > 0f && !IsCrouching;
        float speed = walkSpeed * speedModifier;
        if (sprinting) speed *= sprintMultiplier;
        if (IsCrouching) speed *= crouchMultiplier;

        Vector3 wishDir = (transform.right * x + transform.forward * z).normalized;

        if (IsGrounded)
        {
            if (hasInput)
            {
                // WASD on ground kills bhop momentum
                _bhopMomentum = Vector3.Lerp(_bhopMomentum, Vector3.zero, 20f * Time.deltaTime);
                _cc.Move(wishDir * speed * Time.deltaTime);
            }
            else
            {
                _bhopMomentum = Vector3.Lerp(_bhopMomentum, Vector3.zero, 10f * Time.deltaTime);
            }
        }
        else
        {
            if (hasInput)
            {
                Vector3 airMove = wishDir * speed * 0.75f;
                _cc.Move(airMove * Time.deltaTime);
                _bhopMomentum = Vector3.Lerp(_bhopMomentum, Vector3.zero, 3f * Time.deltaTime);
            }
        }
    }

    void HandleJump()
    {
        var kb = Keyboard.current;
        bool jumpPressed = kb.spaceKey.wasPressedThisFrame;
        bool jumpHeld = kb.spaceKey.isPressed;

        bool canJump = _coyoteTimer > 0f;

        if (jumpPressed && canJump)
        {
            PerformJump();
        }

        // Bhop: holding space when landing without WASD → auto-jump, keep momentum
        if (IsGrounded && !_wasGrounded == false && jumpHeld && _verticalVelocity <= 0f)
        {
            var kb2 = Keyboard.current;
            bool hasWasd = kb2.wKey.isPressed || kb2.aKey.isPressed || kb2.sKey.isPressed || kb2.dKey.isPressed;
            if (!hasWasd && jumpHeld && canJump)
            {
                PerformJump();
            }
        }

        _verticalVelocity += gravity * Time.deltaTime;
        _verticalVelocity = Mathf.Max(_verticalVelocity, -50f);
    }

    void PerformJump()
    {
        _verticalVelocity = Mathf.Sqrt(jumpForce * jumpModifier * -2f * gravity);
        _coyoteTimer = 0f;
    }

    void ApplyVertical()
    {
        _cc.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
    }

    void ApplyBhopMomentum()
    {
        if (_bhopMomentum.magnitude > 0.1f)
            _cc.Move(new Vector3(_bhopMomentum.x, 0f, _bhopMomentum.z) * Time.deltaTime);
    }

    void HandleCrouch()
    {
        bool crouchInput = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.cKey.isPressed;

        if (IsCrouching && !crouchInput)
        {
            // Only stand up if there's room
            if (!Physics.Raycast(transform.position, Vector3.up, normalHeight + 0.1f, groundMask))
                IsCrouching = false;
        }
        else if (!IsCrouching && crouchInput)
        {
            IsCrouching = true;
        }

        float targetH = IsCrouching ? crouchHeight : normalHeight;
        float targetC = IsCrouching ? crouchCenter : normalCenter;
        float newH = Mathf.Lerp(_cc.height, targetH, 12f * Time.deltaTime);
        _cc.height = newH;
        _cc.center = Vector3.up * Mathf.Lerp(_cc.center.y, targetC, 12f * Time.deltaTime);
    }

    void ApplyHeight(float height, float center)
    {
        _cc.height = height;
        _cc.center = Vector3.up * center;
    }

    // Called by GrappleHook to add momentum
    public void AddImpulse(Vector3 impulse)
    {
        _bhopMomentum.x += impulse.x;
        _bhopMomentum.z += impulse.z;
        if (impulse.y > 0f)
            _verticalVelocity = Mathf.Max(_verticalVelocity + impulse.y, impulse.y);
    }

    // Used by grapple pull (direct move, not momentum)
    public void MoveRaw(Vector3 delta)
    {
        _cc.Move(delta);
    }

    public float GetTotalHorizontalSpeed()
    {
        Vector3 h = new Vector3(_cc.velocity.x + _bhopMomentum.x, 0f, _cc.velocity.z + _bhopMomentum.z);
        return h.magnitude;
    }
}
