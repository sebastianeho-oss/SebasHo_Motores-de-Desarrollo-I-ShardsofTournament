using System.Collections;
using UnityEngine;

public class ShotgunShootingSystem : MonoBehaviour
{
    public enum FireMode { SemiAutomatic, FullAutomatic }

    [Header("Permisos Locales")]
    public bool canShoot = true;

    [Header("Referencias de Origen")]
    public Transform firePoint;
    public Camera mainCamera;
    public LineRenderer lineRendererPrefab;
    public PlayerHealth playerHealth;

    [Header("Modo de Disparo")]
    public FireMode fireMode = FireMode.SemiAutomatic;

    [Header("Ajustes de Escopeta")]
    [Tooltip("Número de perdigones por cartucho")]
    public int pelletsCount = 8;

    [Tooltip("Ángulo máximo de dispersión cónica (en grados)")]
    public float spreadAngle = 5f;

    [Tooltip("Daño máximo base de cada perdigón (a corta distancia)")]
    public float damagePerPellet = 12f;

    [Header("Ajustes de Caída de Daño (Falloff)")]
    [Tooltip("Distancia a partir de la cual el daño empieza a reducirse")]
    public float minDamageDistance = 5f;

    [Tooltip("Distancia donde el daño alcanza su valor mínimo/pésimo")]
    public float maxDamageDistance = 30f;

    [Tooltip("Porcentaje de daño a distancia máxima (ej. 0.2f = 20% del daño base)")]
    [Range(0f, 1f)]
    public float minDamagePercent = 0.15f;

    [Header("Ajustes de Disparo General")]
    public float maxShootDistance = 50f;
    public float fireRate = 0.6f;
    public float tracerDuration = 0.04f;
    public LayerMask shootableMask;

    private float nextFireTime = 0f;

    public WeaponRecoil weaponRecoil;
    private WeaponAmmo weaponAmmo;
    private WeaponSoundController weaponSoundController; // <-- Referencia al sonido de la escopeta
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        weaponAmmo = GetComponent<WeaponAmmo>();
        if (playerHealth == null) playerHealth = GetComponentInParent<PlayerHealth>();

        // Auto-asignar el controlador de sonido de la escopeta
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

            ExecuteShotgunShot();
        }
    }

    private void ExecuteShotgunShot()
    {
        nextFireTime = Time.time + fireRate;

        // --- REPRODUCIR SONIDO DE DISPARO DE LA ESCOPETA ---
        if (weaponSoundController != null)
        {
            weaponSoundController.PlayShootSound();
        }

        ShootPellets();

        if (weaponAmmo != null) weaponAmmo.ConsumeBullet();
        if (weaponRecoil != null) weaponRecoil.TriggerRecoil();
    }

    void ShootPellets()
    {
        if (mainCamera == null || firePoint == null) return;

        Ray cameraRay = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, maxShootDistance, shootableMask))
        {
            targetPoint = cameraHit.point;
        }
        else
        {
            targetPoint = cameraRay.GetPoint(maxShootDistance);
        }

        Vector3 baseDirection = (targetPoint - firePoint.position).normalized;

        float damageMultiplier = (ShopManager.Instance != null) ? ShopManager.Instance.GetPlayerDamageMultiplier() : 1f;

        for (int i = 0; i < pelletsCount; i++)
        {
            Vector3 pelletDirection = GetRandomConeDirection(baseDirection, spreadAngle);
            Vector3 impactPoint = firePoint.position + pelletDirection * maxShootDistance;

            if (Physics.Raycast(firePoint.position, pelletDirection, out RaycastHit fireHit, maxShootDistance, shootableMask))
            {
                impactPoint = fireHit.point;

                Collider hitCollider = fireHit.collider;
                if (hitCollider != null)
                {
                    EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                    if (enemyHealth == null) enemyHealth = hitCollider.GetComponentInParent<EnemyHealth>();

                    if (enemyHealth != null)
                    {
                        float hitDistance = fireHit.distance;
                        float distanceFactor = CalculateDamageFalloff(hitDistance);
                        float finalPelletDamage = damagePerPellet * distanceFactor * damageMultiplier;

                        enemyHealth.TakeDamage(finalPelletDamage);
                    }
                }
            }

            if (lineRendererPrefab != null)
            {
                StartCoroutine(RenderTracerLine(firePoint.position, impactPoint));
            }
        }
    }

    private float CalculateDamageFalloff(float distance)
    {
        if (distance <= minDamageDistance)
        {
            return 1.0f;
        }
        else if (distance >= maxDamageDistance)
        {
            return minDamagePercent;
        }

        float t = (distance - minDamageDistance) / (maxDamageDistance - minDamageDistance);
        return Mathf.Lerp(1.0f, minDamagePercent, t);
    }

    private Vector3 GetRandomConeDirection(Vector3 direction, float coneAngle)
    {
        float halfAngle = coneAngle * 0.5f;
        Quaternion randomRotation = Quaternion.Euler(
            Random.Range(-halfAngle, halfAngle),
            Random.Range(-halfAngle, halfAngle),
            0f
        );

        return Quaternion.LookRotation(direction) * randomRotation * Vector3.forward;
    }

    private IEnumerator RenderTracerLine(Vector3 startPoint, Vector3 endPoint)
    {
        LineRenderer line = Instantiate(lineRendererPrefab, startPoint, Quaternion.identity);
        line.enabled = true;
        line.SetPosition(0, startPoint);
        line.SetPosition(1, endPoint);

        yield return new WaitForSeconds(tracerDuration);

        Destroy(line.gameObject);
    }
}