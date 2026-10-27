using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAmmo : MonoBehaviour
{
    [Header("Permisos Locales")]
    public bool canReload = true;

    [Header("Configuración del Arma")]
    public AmmoType ammoType;
    public int magazineSize = 30;
    public float reloadTime = 2.0f;

    [Header("Estado Actual")]
    public int currentAmmo;
    private bool isReloading = false;

    [Header("Referencias")]
    public PlayerHealth playerHealth;
    private PlayerAmmoInventory playerInventory;
    private WeaponSoundController weaponSoundController; // <-- Referencia al sonido del arma
    private PlayerInputActions inputActions;

    public bool IsReloading => isReloading;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable() => inputActions.Enable();

    private void OnDisable()
    {
        inputActions.Disable();
        CancelReload(); // Cancela la recarga al ocultar o cambiar el arma
    }

    void Start()
    {
        playerInventory = GetComponentInParent<PlayerAmmoInventory>();
        currentAmmo = magazineSize;

        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }

        // Auto-asignar el WeaponSoundController del arma
        weaponSoundController = GetComponent<WeaponSoundController>();
        if (weaponSoundController == null)
        {
            weaponSoundController = GetComponentInChildren<WeaponSoundController>();
        }
    }

    void Update()
    {
        if (isReloading) return;

        bool isAlive = (playerHealth == null || !playerHealth.isDead);
        bool allowedToReload = canReload && isAlive && !GameManager.IsPaused;

        if (inputActions.Player.Reload.WasPressedThisFrame() && currentAmmo < magazineSize && allowedToReload)
        {
            TryReload();
        }
    }

    public bool CanShoot()
    {
        bool isAlive = (playerHealth == null || !playerHealth.isDead);
                
        if (!isReloading && currentAmmo <= 0 && isAlive && !GameManager.IsPaused)
        {
            if (weaponSoundController != null)
            {
                weaponSoundController.PlayEmptyClickSound();
            }
        }

        return !isReloading && currentAmmo > 0 && isAlive && !GameManager.IsPaused;
    }

    public void ConsumeBullet()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
        }
    }

    public void TryReload()
    {
        if (playerInventory == null) return;

        bool isAlive = (playerHealth == null || !playerHealth.isDead);
        if (!canReload || !isAlive || GameManager.IsPaused) return;

        int reserve = playerInventory.GetReserveAmmo(ammoType);
        if (reserve > 0 && currentAmmo < magazineSize)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    public void CancelReload()
    {
        if (isReloading)
        {
            StopAllCoroutines();
            isReloading = false;
            Debug.Log($"Recarga interrumpida para {ammoType}");
        }
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log($"Iniciando recarga. Munición actual: {currentAmmo}/{magazineSize}");

        Animator animator = GetComponentInParent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.SetBool("IsReloading", true);
            Debug.Log("¡Trigger 'Reload' enviado al Animator con éxito!"); // <-- Mira si sale esto en consola
        }
        else
        {
            Debug.LogError("¡ERROR: No se encontró ningún Animator!");
        }

        // --- REPRODUCIR SONIDO DE RECARGA ---
        if (weaponSoundController != null)
        {
            weaponSoundController.PlayReloadSound();
        }

        yield return new WaitForSeconds(reloadTime);

        if (playerHealth != null && playerHealth.isDead)
        {
            isReloading = false;
            yield break;
        }

        int ammoNeeded = magazineSize - currentAmmo;
        int ammoExtracted = playerInventory.ExtractAmmo(ammoType, ammoNeeded);

        currentAmmo += ammoExtracted;
        isReloading = false;

        Debug.Log($"Recarga completada. {ammoType}: {currentAmmo}/{magazineSize}");
    }
}