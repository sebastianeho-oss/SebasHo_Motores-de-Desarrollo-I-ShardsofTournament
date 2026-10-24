using UnityEngine;

public class UpperBodyStateBehaviour : StateMachineBehaviour
{
    [Header("Configuración del Estado")]
    [Tooltip("El tipo de arma requerida para este estado")]
    public int targetWeaponType;

    [Tooltip("Suavizado de la capa superior al entrar (opcional)")]
    public bool enforceLayerWeight = true;
    public int layerIndex = 1; // Índice de la capa UpperBody (generalmente la 1)

    // OnStateEnter se ejecuta cuando el Animator entra en este estado
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enforceLayerWeight)
        {
            // Asegura que el peso de la capa de torso esté activo al entrar
            animator.SetLayerWeight(layerIndex, 1f);
        }
    }

    // OnStateUpdate se ejecuta en cada frame que el Animator permanece en el estado
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Lógica opcional en cada frame mientras el estado está activo.
    }

    // OnStateExit se ejecuta cuando se sale de este estado de animación
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Útil si necesitas realizar una limpieza o disparar eventos tras salir del estado.
    }
}