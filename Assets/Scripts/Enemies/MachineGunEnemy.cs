using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))] // Asegura que tenga Animator
public class MachineGunEnemy : MonoBehaviour
{
    [Header("Ajustes de Disparo (Ametralladora)")]
    public float shootingRange = 15f;
    public float fireRate = 0.1f;
    public float damage = 5f;
    public float bulletSpread = 0.05f;
    public float tracerDuration = 0.04f;

    [Header("Ajustes de Retirada y Melé")]
    public float retreatRange = 5f;
    public float meleeRange = 1.2f;
    public float meleeDamage = 15f;
    public float meleePushForce = 5f;
    public float meleeCooldown = 1.5f;

    [Header("Capas de Colisión")]
    public LayerMask collisionLayers;

    [Header("Referencias Visuales y Componentes")]
    public Transform firePoint;
    public LineRenderer lineRenderer;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public string playerTag = "Player";

    private NavMeshAgent agent;
    private Animator animator; // Referencia al Animator
    private Transform playerTransform;
    private float nextFireTime = 0f;
    private float nextMeleeTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Inicializar Animator

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        agent.stoppingDistance = retreatRange;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Actualizar parámetro de velocidad en el Animator basado en la velocidad actual del NavMeshAgent
        float currentSpeed = agent.velocity.magnitude;
        animator.SetFloat("Speed", currentSpeed);

        // 1. Rango Cuerpo a Cuerpo
        if (distanceToPlayer <= meleeRange)
        {
            RotateTowardsPlayer();
            agent.SetDestination(transform.position);

            if (Time.time >= nextMeleeTime)
            {
                nextMeleeTime = Time.time + meleeCooldown;
                MeleeAttack();
            }
        }
        // 2. Rango de Retirada
        else if (distanceToPlayer < retreatRange)
        {
            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
            Vector3 retreatPosition = transform.position - dirToPlayer * (retreatRange + 2f);

            agent.SetDestination(retreatPosition);
        }
        // 3. Rango de Disparo
        else if (distanceToPlayer <= shootingRange)
        {
            RotateTowardsPlayer();
            agent.SetDestination(playerTransform.position);
            agent.stoppingDistance = retreatRange;

            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                ShootHitscan();
            }
        }
        // 4. Fuera de rango (Perseguir al jugador)
        else
        {
            agent.SetDestination(playerTransform.position);
            agent.stoppingDistance = retreatRange;
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 8f);
        }
    }

    void ShootHitscan()
    {
        if (firePoint == null) return;

        // Activar la animación de disparo mediante el Trigger
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        Vector3 targetCenter = playerTransform.position + Vector3.up * 1f;
        Vector3 baseDirection = (targetCenter - firePoint.position).normalized;

        Vector3 spreadDirection = baseDirection + new Vector3(
            Random.Range(-bulletSpread, bulletSpread),
            Random.Range(-bulletSpread, bulletSpread),
            Random.Range(-bulletSpread, bulletSpread)
        );

        Vector3 endPoint;

        if (Physics.Raycast(firePoint.position, spreadDirection, out RaycastHit hit, shootingRange, collisionLayers))
        {
            endPoint = hit.point;

            PlayerShield playerShield = hit.collider.GetComponent<PlayerShield>();
            if (playerShield == null) playerShield = hit.collider.GetComponentInParent<PlayerShield>();
            if (playerShield == null) playerShield = hit.collider.GetComponentInChildren<PlayerShield>();

            if (playerShield != null)
            {
                playerShield.TakeDamage(damage);
            }
            else
            {
                PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth == null) playerHealth = hit.collider.GetComponentInParent<PlayerHealth>();
                if (playerHealth == null) playerHealth = hit.collider.GetComponentInChildren<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }

            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        else
        {
            endPoint = firePoint.position + spreadDirection * shootingRange;
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (lineRenderer != null)
        {
            StartCoroutine(ShowTracer(firePoint.position, endPoint));
        }
    }

    void MeleeAttack()
    {
        PlayerShield playerShield = playerTransform.GetComponent<PlayerShield>();
        if (playerShield != null) playerShield.TakeDamage(meleeDamage);

        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null) playerHealth.TakeDamage(meleeDamage);

        CharacterController playerController = playerTransform.GetComponent<CharacterController>();
        if (playerController != null)
        {
            StartCoroutine(PushPlayerRoutine(playerController));
        }
    }

    IEnumerator PushPlayerRoutine(CharacterController controller)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 pushDir = (playerTransform.position - transform.position).normalized;
        pushDir.y = 0;

        while (elapsed < duration)
        {
            controller.Move(pushDir * meleePushForce * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ShowTracer(Vector3 startPos, Vector3 endPos)
    {
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
        lineRenderer.enabled = true;

        yield return new WaitForSeconds(tracerDuration);

        lineRenderer.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, retreatRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}