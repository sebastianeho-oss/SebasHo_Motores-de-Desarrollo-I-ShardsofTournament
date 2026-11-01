using UnityEngine;

public class UpperBodyStateBehaviour : StateMachineBehaviour
{
    [Header("Configuración del Estado")]
    [Tooltip("El tipo de arma requerida para este estado")]
    public int targetWeaponType;

    [Tooltip("Suavizado de la capa superior al entrar (opcional)")]
    public bool enforceLayerWeight = true;
    public int layerIndex = 1;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enforceLayerWeight)
        {
            animator.SetLayerWeight(layerIndex, 1f);
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}