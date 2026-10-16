using UnityEngine;
using Unity.Cinemachine;

public class CinemachineStateController : MonoBehaviour
{
    public enum AimCameraType { Standard, Sniper }

    [Header("Referencias de Cámaras Cinemachine")]
    public CinemachineCamera followCamera;
    public CinemachineCamera aimCamera;
    public CinemachineCamera sniperAimCamera;

    [Header("Referencia al WeaponHolder")]
    public Transform weaponHolder;

    [Header("Ajustes de Prioridad")]
    public int activePriority = 20;
    public int inactivePriority = 0;

    [Header("Rotación y Sensibilidad")]
    public Transform playerTarget;
    public float mouseSensitivity = 2.0f;
    public float aimSensitivityMultiplier = 0.5f;
    public float minYAngle = -35.0f;
    public float maxYAngle = 60.0f;

    [Header("Efecto de Sacudida (Impulse)")]
    public CinemachineImpulseSource impulseSource;

    public PlayerHealth playerHealth;

    public bool IsAiming { get; private set; }

    private float currentX;
    private float currentY;

    private PlayerInputActions inputActions;
    private Vector2 lookInput;
    private bool aimHeld;
    private bool sprintHeld;

    public AimCameraType ActiveWeaponAimType
    {
        get
        {
            if (weaponHolder == null) return AimCameraType.Standard;

            foreach (Transform child in weaponHolder)
            {
                if (child.gameObject.activeSelf)
                {
                    WeaponInfo info = child.GetComponent<WeaponInfo>();
                    if (info != null) return info.cameraType;
                }
            }
            return AimCameraType.Standard;
        }
    }

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.Aim.performed += ctx => aimHeld = true;
        inputActions.Player.Aim.canceled += ctx => aimHeld = false;

        inputActions.Player.Sprint.performed += ctx => sprintHeld = true;
        inputActions.Player.Sprint.canceled += ctx => sprintHeld = false;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerHealth == null && playerTarget != null)
        {
            playerHealth = playerTarget.GetComponent<PlayerHealth>();
        }

        // Obtiene o autogenera el componente de impulso en este mismo objeto
        if (impulseSource == null)
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
            if (impulseSource == null)
            {
                impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            }
        }

        ResetAllAimCameras();
        if (followCamera != null) followCamera.Priority = activePriority;
    }

    void LateUpdate()
    {
        if (playerTarget == null) return;

        if (GameManager.IsPaused || (playerHealth != null && playerHealth.isDead))
        {
            IsAiming = false;
            ResetAllAimCameras();
            return;
        }

        IsAiming = aimHeld && !sprintHeld;

        UpdateCameraPriorities();

        float currentSensitivity = IsAiming ? (mouseSensitivity * aimSensitivityMultiplier) : mouseSensitivity;

        currentX += lookInput.x * currentSensitivity * Time.deltaTime * 60f;
        currentY -= lookInput.y * currentSensitivity * Time.deltaTime * 60f;
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        transform.position = playerTarget.position;
        transform.rotation = Quaternion.Euler(currentY, currentX, 0);
    }

    private void UpdateCameraPriorities()
    {
        if (!IsAiming)
        {
            ResetAllAimCameras();
            return;
        }

        AimCameraType currentType = ActiveWeaponAimType;

        if (currentType == AimCameraType.Sniper && sniperAimCamera != null)
        {
            if (aimCamera != null) aimCamera.Priority = inactivePriority;
            sniperAimCamera.Priority = activePriority + 5;
        }
        else if (aimCamera != null)
        {
            if (sniperAimCamera != null) sniperAimCamera.Priority = inactivePriority;
            aimCamera.Priority = activePriority + 5;
        }
    }

    private void ResetAllAimCameras()
    {
        if (aimCamera != null) aimCamera.Priority = inactivePriority;
        if (sniperAimCamera != null) sniperAimCamera.Priority = inactivePriority;
    }

    public void AddRecoil(float recoilX, float recoilY)
    {
        currentX += recoilX;
        currentY -= recoilY;
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
    }

    // --- MÉTODO DE INSTANCIA VÁLIDO PARA CINEMACHINE 3.x ---
    public void TriggerCameraShake()
    {
        if (impulseSource != null)
        {
            // Llama al método de instancia correctamente
            impulseSource.GenerateImpulse(Vector3.one * 1.5f);
        }
    }
}