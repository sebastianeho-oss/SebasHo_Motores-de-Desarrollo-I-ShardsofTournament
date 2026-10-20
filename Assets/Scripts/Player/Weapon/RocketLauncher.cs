using UnityEngine;
using UnityEngine.InputSystem;

public class RocketLauncher : MonoBehaviour
{
    [Header("Permisos Locales")]
    public bool canShoot = true;

    [Header("Referencias")]
    public Transform firePoint;
    public Camera mainCamera;
    public GameObject rocketPrefab;
    public PlayerHealth playerHealth;

    [Header("Ajustes de Disparo")]
    public float fireRate = 0.8f;
    public float maxAimDistance = 100f;
    public LayerMask shootableMask;

    private float nextFireTime = 0f;
    private WeaponAmmo weaponAmmo;
    private WeaponSoundController weaponSoundController; // <-- Referencia al sonido del lanzamisiles
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Start()
    {
        weaponAmmo = GetComponent<WeaponAmmo>();

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
        }

        // Auto-asignar el controlador de sonido del lanzamisiles
        weaponSoundController = GetComponent<WeaponSoundController>();
        if (weaponSoundController == null)
        {
            weaponSoundController = GetComponentInChildren<WeaponSoundController>();
        }
    }

    void Update()
    {
        if ((ShopManager.Instance != null && ShopManager.Instance.IsShopOpen) ||
            !canShoot || (playerHealth != null && playerHealth.isDead) || GameManager.IsPaused)
        {
            return;
        }

        bool isTriggerPressed = inputActions.Player.Fire.IsPressed();

        if (isTriggerPressed && Time.time >= nextFireTime)
        {
            if (weaponAmmo != null && !weaponAmmo.CanShoot())
            {
                if (weaponAmmo.currentAmmo == 0) weaponAmmo.TryReload();
                return;
            }

            nextFireTime = Time.time + fireRate;
            ShootRocket();

            if (weaponAmmo != null) weaponAmmo.ConsumeBullet();
        }
    }

    void ShootRocket()
    {
        if (mainCamera == null || firePoint == null) return;

        // --- REPRODUCIR SONIDO DE LANZAMIENTO / EXPLOSIÓN ---
        if (weaponSoundController != null)
        {
            weaponSoundController.PlayShootSound();
        }

        Ray cameraRay = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxAimDistance, shootableMask))
        {
            targetPoint = cameraHit.point;
        }
        else
        {
            targetPoint = cameraRay.GetPoint(maxAimDistance);
        }

        Vector3 launchDirection = (targetPoint - firePoint.position).normalized;
        Quaternion launchRotation = Quaternion.LookRotation(launchDirection);
        Instantiate(rocketPrefab, firePoint.position, launchRotation);
    }
}