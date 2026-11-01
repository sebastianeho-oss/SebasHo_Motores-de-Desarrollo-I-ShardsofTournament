using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private float verticalOffset = 1.5f;

    [Header("Referencias")]
    [SerializeField] private Animator animator;

    private PlayerInputActions inputActions;
    private bool isLookingAtInteractable = false;
    private IInteractable currentInteractableTarget;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Interact.performed += ctx => TryInteract();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        isLookingAtInteractable = false;
        currentInteractableTarget = null;

        Vector3 rayOrigin = transform.position + (Vector3.up * verticalOffset);
        Vector3 rayDirection = transform.forward;

        Ray ray = new Ray(rayOrigin, rayDirection);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                isLookingAtInteractable = true;
                currentInteractableTarget = interactable;
            }
        }
    }

    private void TryInteract()
    {
        if (isLookingAtInteractable && currentInteractableTarget != null && !GameManager.IsPaused)
        {
            if (animator != null)
            {
                animator.SetTrigger("Interact");
            }
            currentInteractableTarget.Interact();
        }
    }
}