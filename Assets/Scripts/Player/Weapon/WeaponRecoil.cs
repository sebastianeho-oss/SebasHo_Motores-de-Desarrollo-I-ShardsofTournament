using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Retroceso Base (Sin apuntar / De pie)")]
    public float recoilX = 0.5f;
    public float recoilY = 1.5f;

    [Header("Multiplicadores de Apuntado")]
    public float aimRecoilMultiplier = 0.5f;

    [Header("Multiplicadores al Agacharse")]
    public float crouchStillRecoilMultiplier = 0.6f;
    public float crouchMovingRecoilMultiplier = 1.4f;

    [Header("Referencias")]
    public CinemachineStateController cameraController;
    public MovementPlayerController movementController;

    private void Start()
    {
        if (cameraController == null)
            cameraController = GetComponentInParent<CinemachineStateController>();

        if (movementController == null)
            movementController = GetComponentInParent<MovementPlayerController>();
    }

    public void TriggerRecoil()
    {
        float currentRecoilX = recoilX;
        float currentRecoilY = recoilY;

        if (ShopManager.Instance != null && ShopManager.Instance.currentPassive == PassiveType.Frenzy && ShopManager.Instance.frenzyActive)
        {
            currentRecoilX *= ShopManager.Instance.frenzyRecoilMultiplier;
            currentRecoilY *= ShopManager.Instance.frenzyRecoilMultiplier;
        }

        if (cameraController != null && cameraController.IsAiming)
        {
            currentRecoilX *= aimRecoilMultiplier;
            currentRecoilY *= aimRecoilMultiplier;
        }

        if (movementController != null && movementController.IsCrouching)
        {
            float crouchMultiplier = movementController.IsMoving
                ? crouchMovingRecoilMultiplier
                : crouchStillRecoilMultiplier;

            currentRecoilX *= crouchMultiplier;
            currentRecoilY *= crouchMultiplier;
        }

        float finalX = Random.Range(-currentRecoilX, currentRecoilX);
        float finalY = currentRecoilY;

        if (cameraController != null)
        {
            cameraController.AddRecoil(finalX, finalY);
        }
    }
}