using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 0.05f;
    public float verticalClamp = 85f;

    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public float bobFrequency = 10f;
    public float bobAmplitude = 0.03f;

    [Header("References")]
    public Transform playerBody;      // horizontal rotation target
    public Transform cameraHolder;    // vertical rotation target

    private float _xRotation;
    private float _bobTimer;
    private Vector3 _bobOffset;
    private Vector3 _camHolderRestPos;
    private PlayerController _playerCtrl;

    void Awake()
    {
        _playerCtrl = GetComponentInParent<PlayerController>();
        if (playerBody == null) playerBody = transform.parent;
        if (cameraHolder == null) cameraHolder = transform;

        _camHolderRestPos = cameraHolder.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        if (enableHeadBob) HandleHeadBob();
        HandleCursorToggle();
    }

    void HandleMouseLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime * 100f;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -verticalClamp, verticalClamp);

        cameraHolder.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    void HandleHeadBob()
    {
        if (_playerCtrl == null) return;

        float speed = _playerCtrl.GetTotalHorizontalSpeed();
        if (_playerCtrl.IsGrounded && speed > 0.5f)
        {
            _bobTimer += Time.deltaTime * bobFrequency;
            _bobOffset = new Vector3(
                Mathf.Sin(_bobTimer * 0.5f) * bobAmplitude,
                Mathf.Sin(_bobTimer) * bobAmplitude,
                0f
            );
        }
        else
        {
            _bobTimer = 0f;
            _bobOffset = Vector3.Lerp(_bobOffset, Vector3.zero, 6f * Time.deltaTime);
        }

        cameraHolder.localPosition = _camHolderRestPos + _bobOffset;
    }

    void HandleCursorToggle()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
        }
    }

    public void SetSensitivity(float value) => mouseSensitivity = value;
}
