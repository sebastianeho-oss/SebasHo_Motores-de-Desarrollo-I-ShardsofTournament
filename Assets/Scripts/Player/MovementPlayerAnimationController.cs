using UnityEngine;

public class MovementPlayerAnimationController : MonoBehaviour
{
    [Header("Referencias Opcionales")]
    public MovementPlayerController movementController;
    public CinemachineStateController cameraController;
    public PlayerHealth playerHealth;

    [Header("Referencias")]
    [SerializeField] private Animator animator;

    private CharacterController characterController;
    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool wasGroundedLastFrame = true;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInParent<Animator>();
        }

        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = GetComponentInParent<CharacterController>();
        }

        if (movementController == null)
            movementController = GetComponentInParent<MovementPlayerController>();

        if (cameraController == null)
            cameraController = GetComponentInChildren<CinemachineStateController>();

        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();
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
        animator.SetFloat("MoveX", moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveZ", moveInput.y, 0.1f, Time.deltaTime);
    }

    private void UpdateStateParameters()
    {
        bool isGrounded = characterController != null && characterController.isGrounded;
        animator.SetBool("isGrounded", isGrounded);

        if (!wasGroundedLastFrame && isGrounded)
        {
            animator.SetTrigger("Land");
        }
        wasGroundedLastFrame = isGrounded;

        bool isWalking = movementController != null && movementController.IsMoving && isGrounded;
        animator.SetBool("IsWalking", isWalking);

        bool isAiming = cameraController != null && cameraController.IsAiming;
        animator.SetBool("isAiming", isAiming);

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