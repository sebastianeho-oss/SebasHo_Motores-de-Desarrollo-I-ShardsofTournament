using System.Collections;
using UnityEngine;

public class ShootingSystem : MonoBehaviour
{
    public enum FireMode { SemiAutomatic, FullAutomatic, Burst }

    [Header("Permisos Locales")]
    public bool canShoot = true;

    [Header("Referencias de Origen")]
    public Transform firePoint;
    public Camera mainCamera;
    public LineRenderer lineRenderer;
    public PlayerHealth playerHealth;

    [Header("Modo de Disparo")]
    public FireMode fireMode = FireMode.FullAutomatic;

    [Header("Ajustes de Ráfaga")]
    public int burstCount = 3;
    public float burstDelay = 0.08f;

    [Header("Ajustes de Disparo General")]
    public float damage = 25f;
    public float maxShootDistance = 100f;
    public float fireRate = 0.15f;
    public float tracerDuration = 0.04f;
    public LayerMask shootableMask;

    private float nextFireTime = 0f;
    private bool isShootingBurst = false;

    public WeaponRecoil weaponRecoil;
    private WeaponAmmo weaponAmmo;
    private WeaponSoundController weaponSoundController; // <-- Referencia al sonido del arma
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Start()
    {
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (mainCamera == null) mainCamera = Camera.main;

        weaponAmmo = GetComponent<WeaponAmmo>();
        if (playerHealth == null) playerHealth = GetComponentInParent<PlayerHealth>();

        // Auto-asignar el controlador de sonido del arma
        weaponSoundController = GetComponent<WeaponSoundController>();
        if (weaponSoundController == null)
        {
            weaponSoundController = GetComponentInChildren<WeaponSoundController>();
        }
    }

    void Update()
    {
        if ((ShopManager.Instance != null && ShopManager.Instance.IsShopOpen) ||
            !canShoot || isShootingBurst || (playerHealth != null && playerHealth.isDead) || GameManager.IsPaused)
        {
            return;
        }

        bool isTriggerPressed = (fireMode == FireMode.FullAutomatic)
            ? inputActions.Player.Fire.IsPressed()
            : inputActions.Player.Fire.WasPressedThisFrame();

        if (isTriggerPressed && Time.time >= nextFireTime)
        {
            if (weaponAmmo != null && !weaponAmmo.CanShoot())
            {
                if (weaponAmmo.currentAmmo == 0) weaponAmmo.TryReload();
                return;
            }

            if (fireMode == FireMode.Burst)
            {
                StartCoroutine(FireBurstRoutine());
            }
            else
            {
                ExecuteSingleShot();
            }
        }
    }

    private void ExecuteSingleShot()
    {
        nextFireTime = Time.time + fireRate;
        Shoot();

        if (weaponAmmo != null) weaponAmmo.ConsumeBullet();
    }

    private IEnumerator FireBurstRoutine()
    {
        isShootingBurst = true;

        for (int i = 0; i < burstCount; i++)
        {
            if (weaponAmmo != null && !weaponAmmo.CanShoot())
            {
                if (weaponAmmo.currentAmmo == 0) weaponAmmo.TryReload();
                break;
            }

            Shoot();

            if (weaponAmmo != null) weaponAmmo.ConsumeBullet();

            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(burstDelay);
            }
        }

        nextFireTime = Time.time + fireRate;
        isShootingBurst = false;
    }

    void Shoot()
    {
        if (mainCamera == null || firePoint == null) return;

        // --- REPRODUCIR SONIDO DE DISPARO ---
        if (weaponSoundController != null)
        {
            weaponSoundController.PlayShootSound();
        }

        Ray cameraRay = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;
        bool hasCameraHit = Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxShootDistance, shootableMask);

        if (hasCameraHit) targetPoint = cameraHit.point;
        else targetPoint = cameraRay.GetPoint(maxShootDistance);

        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;
        Vector3 finalImpactPoint = targetPoint;

        float distanceToTarget = Vector3.Distance(firePoint.position, targetPoint);
        bool hasFireHit = Physics.Raycast(firePoint.position, shootDirection, out RaycastHit fireHit, distanceToTarget, shootableMask);

        Collider hitCollider = null;

        if (hasFireHit)
        {
            finalImpactPoint = fireHit.point;
            hitCollider = fireHit.collider;
        }
        else if (hasCameraHit)
        {
            hitCollider = cameraHit.collider;
        }

        if (hitCollider != null)
        {
            EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
            if (enemyHealth == null) enemyHealth = hitCollider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                float finalDamage = damage;
                if (ShopManager.Instance != null)
                {
                    finalDamage *= ShopManager.Instance.GetPlayerDamageMultiplier();
                }

                enemyHealth.TakeDamage(finalDamage);
            }
        }

        if (lineRenderer != null) StartCoroutine(RenderTracerLine(firePoint.position, finalImpactPoint));
        if (weaponRecoil != null) weaponRecoil.TriggerRecoil();
    }

    private IEnumerator RenderTracerLine(Vector3 startPoint, Vector3 endPoint)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        yield return new WaitForSeconds(tracerDuration);

        lineRenderer.enabled = false;
    }
}