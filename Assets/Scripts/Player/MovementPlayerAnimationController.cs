using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MovementPlayerAnimationController : MonoBehaviour
{
    [Header("Referencias Opcionales")]
    public MovementPlayerController movementController;
    public CinemachineStateController cameraController;
    public PlayerHealth playerHealth;

    private Animator animator;
    private CharacterController characterController;
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool wasGroundedLastFrame = true;

    private void Awake()
    {
        // Inicializamos los inputs locales para leer MoveX y MoveZ de forma independiente
        inputActions = new PlayerInputActions();
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        if (movementController == null)
            movementController = GetComponent<MovementPlayerController>();

        if (cameraController == null && cameraTransformReference())
            cameraController = GetComponentInChildren<CinemachineStateController>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
    }

    private bool cameraTransformReference() => true;

    void Update()
    {
        if (animator == null) return;

        UpdateMovementParameters();
        UpdateStateParameters();
    }

    private void UpdateMovementParameters()
    {
        // Enviamos los valores de los ejes directamente al Animator (igual que lo hacía antes tu script viejo)
        animator.SetFloat("MoveX", moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveZ", moveInput.y, 0.1f, Time.deltaTime);
    }

    private void UpdateStateParameters()
    {
        // 1. isGrounded
        bool isGrounded = characterController != null && characterController.isGrounded;
        animator.SetBool("isGrounded", isGrounded);

        // 2. Detección de Aterrizaje (Land)
        if (!wasGroundedLastFrame && isGrounded)
        {
            animator.SetTrigger("Land");
        }
        wasGroundedLastFrame = isGrounded;

        // 3. IsWalking (Verifica si se está moviendo y en tierra)
        bool isWalking = movementController != null && movementController.IsMoving && isGrounded;
        animator.SetBool("IsWalking", isWalking);

        // 4. isAiming
        bool isAiming = cameraController != null && cameraController.IsAiming;
        animator.SetBool("isAiming", isAiming);

        // 5. IsCrouching
        if (movementController != null)
        {
            animator.SetBool("IsCrouching", movementController.IsCrouching);
        }
    }

    public void TriggerJump()
    {
        animator.SetTrigger("Jump");
    }
}