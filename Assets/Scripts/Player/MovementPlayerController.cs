using UnityEngine;
using UnityEngine.InputSystem;

public class MovementPlayerController : MonoBehaviour
{
    [Header("Movimiento Base")]
    public float walkSpeed = 5.0f;
    public float sprintSpeed = 8.5f;
    public float crouchSpeed = 2.5f;
    public float aimSpeedMultiplier = 0.5f;
    public float gravity = -15.0f;
    public float jumpHeight = 1.2f;

    [Header("Agacharse (Crouch)")]
    public float standingHeight = 2.0f;
    public float crouchingHeight = 1.0f;
    public float crouchTransitionSpeed = 10.0f;

    [Header("Permisos Locales")]
    public bool canMove = true;
    public bool canJump = true;

    [Header("Referencias")]
    public Transform cameraTransform;
    public CinemachineStateController cameraController;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    public PlayerHealth playerHealth;
    public PlayerSoundController playerSoundController;

    private CharacterController controller;
    private Animator animator;
    private Vector3 velocity;
    private bool isGrounded;

    private float horizontalAirSpeed;

    public bool IsCrouching { get; private set; }

    public bool IsMoving
    {
        get
        {
            bool isAlive = (playerHealth == null || !playerHealth.isDead);
            bool allowedToMove = canMove && isAlive && !GameManager.IsPaused;
            return allowedToMove && moveInput.sqrMagnitude > 0.01f;
        }
    }

    private Vector3 standingCenter;
    private Vector3 crouchingCenter;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool sprintHeld;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Sprint.performed += ctx => sprintHeld = true;
        inputActions.Player.Sprint.canceled += ctx => sprintHeld = false;

        inputActions.Player.Crouch.performed += _ => ToggleCrouch();
        inputActions.Player.Jump.performed += _ => OnJump();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();

        if (cameraController == null && cameraTransform != null)
        {
            cameraController = cameraTransform.GetComponent<CinemachineStateController>();
        }

        if (controller != null)
        {
            standingHeight = controller.height;
            standingCenter = controller.center;
            crouchingCenter = new Vector3(standingCenter.x, standingCenter.y - (standingHeight - crouchingHeight) / 2f, standingCenter.z);
        }
    }

    void Update()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            isGrounded = controller != null && controller.isGrounded;
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        bool isAlive = (playerHealth == null || !playerHealth.isDead);
        bool isNotPaused = !GameManager.IsPaused;
        bool allowedToMove = canMove && isAlive && isNotPaused;

        if (controller != null)
        {
            float targetHeight = IsCrouching ? crouchingHeight : standingHeight;
            Vector3 targetCenter = IsCrouching ? crouchingCenter : standingCenter;
            controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            controller.center = Vector3.Lerp(controller.center, targetCenter, Time.deltaTime * crouchTransitionSpeed);

            if (cameraTransform != null && allowedToMove)
            {
                transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);
            }

            if (allowedToMove)
            {
                bool isAiming = (cameraController != null && cameraController.IsAiming);

                if (sprintHeld && !isAiming)
                {
                    IsCrouching = false;
                }

                float currentSpeed = walkSpeed;

                if (isGrounded)
                {
                    bool wantsToSprint = sprintHeld && !isAiming && !IsCrouching;

                    if (wantsToSprint) currentSpeed = sprintSpeed;
                    else if (IsCrouching) currentSpeed = crouchSpeed;

                    if (isAiming) currentSpeed *= aimSpeedMultiplier;

                    // --- PASIVAS DE VELOCIDAD ---
                    if (ShopManager.Instance != null)
                    {
                        if (ShopManager.Instance.currentPassive == PassiveType.Frenzy && ShopManager.Instance.frenzyActive)
                        {
                            currentSpeed *= 1.60f; // +60% de velocidad al matar
                        }
                        else if (ShopManager.Instance.currentPassive == PassiveType.Tank)
                        {
                            currentSpeed *= 0.70f; // 30% más lento
                        }
                    }

                    horizontalAirSpeed = currentSpeed;
                }
                else
                {
                    currentSpeed = Mathf.Max(horizontalAirSpeed, walkSpeed);
                    if (isAiming) currentSpeed *= aimSpeedMultiplier;

                    if (ShopManager.Instance != null && ShopManager.Instance.currentPassive == PassiveType.Tank)
                    {
                        currentSpeed *= 0.70f;
                    }
                }

                Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
                if (move.magnitude > 1f) move.Normalize();

                controller.Move(move * currentSpeed * Time.deltaTime);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        // Control del sonido de pasos en bucle
        if (IsMoving && isGrounded)
        {
            if (playerSoundController != null)
            {
                playerSoundController.playMove();
            }
        }
        else
        {
            if (playerSoundController != null)
            {
                playerSoundController.stopMove();
            }
        }

        if (animator != null)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveZ", moveInput.y);
            animator.SetBool("IsCrouching", IsCrouching);
        }
    }

    private void ToggleCrouch()
    {
        bool isAlive = (playerHealth == null || !playerHealth.isDead);
        if (canMove && isAlive && !GameManager.IsPaused && isGrounded)
        {
            IsCrouching = !IsCrouching;
        }
    }

    private void OnJump()
    {
        bool isAlive = (playerHealth == null || !playerHealth.isDead);
        if (canJump && isAlive && !GameManager.IsPaused && isGrounded)
        {
            bool isAiming = (cameraController != null && cameraController.IsAiming);
            bool wantsToSprint = sprintHeld && !isAiming && !IsCrouching;

            if (wantsToSprint) horizontalAirSpeed = sprintSpeed;
            else if (IsCrouching) horizontalAirSpeed = crouchSpeed;
            else horizontalAirSpeed = walkSpeed;

            if (isAiming) horizontalAirSpeed *= aimSpeedMultiplier;

            IsCrouching = false;

            // Reproducir salto y cortar sonido de caminar al instante
            if (playerSoundController != null)
            {
                playerSoundController.stopMove();
                playerSoundController.playJump();
            }

            float effectiveJumpHeight = jumpHeight;

            if (ShopManager.Instance != null && ShopManager.Instance.currentPassive == PassiveType.Tank)
            {
                effectiveJumpHeight *= 0.30f;
            }

            velocity.y = Mathf.Sqrt(effectiveJumpHeight * -2f * gravity);
        }
    }
}