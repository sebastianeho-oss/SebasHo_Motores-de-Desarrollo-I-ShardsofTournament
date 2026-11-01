using UnityEngine;

public class AimCameraPlayerController : MonoBehaviour
{
    [Header("Permisos Locales")]
    public bool canAim = true;

    [Header("Objetivo y Altura de Pivote")]
    public Transform target;
    [Tooltip("Altura de la cámara al caminar/esprintar (normalmente a la cabeza ~1.5m).")]
    public float targetHeightOffset = 1.5f;
    [Tooltip("Altura por defecto del pivote al apuntar si no hay un script de arma asignado.")]
    public float defaultAimTargetHeightOffset = 0.6f;

    [Header("Control de Torso y Animaciones")]
    [Tooltip("Componente Animator de tu personaje (se auto-asigna desde target si está vacío).")]
    public Animator characterAnimator;

    [Tooltip("Asigna el hueso Spine, Spine1 o Chest de la jerarquía de tu personaje.")]
    public Transform spineBone;

    [Tooltip("Multiplicador de inclinación (0.5 hace que el torso absorba la mitad del ángulo vertical de la cámara).")]
    [Range(0.1f, 1f)]
    public float spinePitchMultiplier = 0.5f;

    [Tooltip("Eje de rotación del hueso para inclinar adelante/atrás. Cambia según la exportación de Mixamo.")]
    public Vector3 spineRotationAxis = Vector3.right;

    [Tooltip("Nombre exactamente igual del parámetro booleano en tu Animator Controller.")]
    public string aimingBoolParameter = "isAiming";

    [Header("Posición Normal (Caminar)")]
    public Vector3 normalOffset = new Vector3(0.8f, 0.2f, -2.2f);
    public float normalFOV = 60f;

    [Header("Posición Constreñida (Sprint con Shift)")]
    public Vector3 sprintOffset = new Vector3(0.5f, 0.1f, -1.4f); 
    public float sprintFOV = 50f; 

    [Header("Configuración Por Defecto de Apuntado (Fallback)")]
    [Tooltip("Posición de cámara al apuntar si no hay un script de arma asignado.")]
    public Vector3 defaultAimOffset = new Vector3(0.5f, 0.0f, -1.2f);
    [Tooltip("Campo de visión al apuntar si no hay un script de arma asignado.")]
    public float defaultAimFOV = 45f;
    [Tooltip("Multiplicador de sensibilidad al apuntar si no hay un script de arma asignado.")]
    [Range(0.1f, 1f)]
    public float defaultAimSensitivity = 0.6f;
    [Tooltip("Multiplicador de movimiento al apuntar por defecto si no hay un script de arma asignado.")]
    [Range(0.05f, 1f)]
    public float defaultAimMovementSpeedMultiplier = 0.5f;

    [Header("Referencia Dinámica del Arma")]
    [Tooltip("Arrastra aquí tu GameObject WeaponHolder. El script detectará el arma visible activada.")]
    public Transform weaponHolder; 
    public PlayerHealth playerHealth;

    [Header("Suavizado de Transición")]
    public float transitionSpeed = 8.0f;

    [Header("Sensibilidad y Límites")]
    public float mouseSensitivity = 3.0f;
    public float minYAngle = -35.0f;
    public float maxYAngle = 60.0f;

    public bool IsAiming { get; private set; }

    public WeaponAim ActiveWeaponAim
    {
        get
        {
            if (weaponHolder == null) return null;

            foreach (Transform child in weaponHolder)
            {
                if (child.gameObject.activeSelf)
                {
                    WeaponAim aimConfig = child.GetComponent<WeaponAim>();
                    if (aimConfig != null) return aimConfig;
                }
            }
            return null;
        }
    }

    private float currentX = 0.0f;
    private float currentY = 0.0f;
    
    private Vector3 currentOffset;
    private float currentHeightOffset;
    private Camera cam;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        cam = GetComponent<Camera>();
        currentOffset = normalOffset;
        currentHeightOffset = targetHeightOffset;
        cam.fieldOfView = normalFOV;

        if (target != null)
        {
            if (playerHealth == null) playerHealth = target.GetComponent<PlayerHealth>();
            if (characterAnimator == null) characterAnimator = target.GetComponentInChildren<Animator>();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (GameManager.IsPaused || (playerHealth != null && playerHealth.isDead))
        {
            SetAimingState(false);
            return;
        }

        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving;
        
        bool aimingInput = canAim && Input.GetButton("Fire2") && !isSprinting;
        SetAimingState(aimingInput);

        Vector3 targetOffset = normalOffset;
        float targetFOV = normalFOV;
        float currentSensitivity = mouseSensitivity;
        float desiredHeight = targetHeightOffset;

        if (IsAiming)
        {
            WeaponAim currentAim = ActiveWeaponAim;

            if (currentAim != null)
            {
                targetOffset = currentAim.aimOffset;
                targetFOV = currentAim.aimFOV;
                currentSensitivity *= currentAim.aimSensitivityMultiplier;
                desiredHeight = currentAim.aimTargetHeightOffset;
            }
            else
            {
                targetOffset = defaultAimOffset;
                targetFOV = defaultAimFOV;
                currentSensitivity *= defaultAimSensitivity;
                desiredHeight = defaultAimTargetHeightOffset;
            }
        }
        else if (isSprinting)
        {
            targetOffset = sprintOffset;
            targetFOV = sprintFOV;
        }

        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * transitionSpeed);
        currentHeightOffset = Mathf.Lerp(currentHeightOffset, desiredHeight, Time.deltaTime * transitionSpeed);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * transitionSpeed);

        currentX += Input.GetAxis("Mouse X") * currentSensitivity;
        currentY -= Input.GetAxis("Mouse Y") * currentSensitivity;
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        Vector3 targetPosition = target.position + Vector3.up * currentHeightOffset;
        Vector3 finalPosition = targetPosition + rotation * currentOffset;

        transform.position = finalPosition;
        transform.rotation = rotation;

        RotateSpineWithCamera();
    }

    
    private void SetAimingState(bool state)
    {
        IsAiming = state;

        if (characterAnimator != null && !string.IsNullOrEmpty(aimingBoolParameter))
        {
            characterAnimator.SetBool(aimingBoolParameter, IsAiming);
        }
    }
         
    private void RotateSpineWithCamera()
    {
        if (spineBone == null) return;
        spineBone.Rotate(spineRotationAxis, currentY * spinePitchMultiplier, Space.Self);
    }

    public void AddRecoil(float recoilX, float recoilY)
    {
        currentX += recoilX;
        currentY -= recoilY;
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
    }
}