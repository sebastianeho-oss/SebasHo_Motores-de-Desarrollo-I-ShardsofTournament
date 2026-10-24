using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    [SerializeField] private float interactionDistance = 5f; // Distancia máxima de interacción
    [SerializeField] private float verticalOffset = 1.5f;     // Altura del raycast desde la posición del jugador (pecho/cabeza)

    [Header("Referencias")]
    [SerializeField] private Animator animator; // Referencia al Animator

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
        // Buscar el Animator automáticamente si no está asignado en el Inspector
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        isLookingAtInteractable = false;
        currentInteractableTarget = null;

        // 1. El Raycast nace del cuerpo del jugador con un desplazamiento vertical
        Vector3 rayOrigin = transform.position + (Vector3.up * verticalOffset);

        // 2. La dirección es hacia donde está rotando/mirando el cuerpo del jugador
        Vector3 rayDirection = transform.forward;

        Ray ray = new Ray(rayOrigin, rayDirection);
        RaycastHit hit;

        // 3. Lanzamos el raycast respetando la distancia máxima
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // 4. Verificamos si el objeto impactado implementa la interfaz IInteractable
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                isLookingAtInteractable = true;
                currentInteractableTarget = interactable;
                // (Opcional) Mostrar texto en UI tipo: "Presiona [Interact] para interactuar"
            }
        }
    }

    private void TryInteract()
    {
        // Si está mirando un objeto interactuable y el juego no está pausado
        if (isLookingAtInteractable && currentInteractableTarget != null && !GameManager.IsPaused)
        {
            // Disparar la animación de interacción
            if (animator != null)
            {
                animator.SetTrigger("Interact");
            }

            // Ejecutar la acción del objeto
            currentInteractableTarget.Interact();
        }
    }
}