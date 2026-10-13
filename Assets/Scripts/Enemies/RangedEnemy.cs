using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RangedEnemy : MonoBehaviour
{
    [Header("Ajustes de Disparo")]
    public float shootingRange = 12f;   // Distancia máxima de ataque a distancia
    public float fireRate = 1.5f;       // Tiempo entre disparos
    public GameObject projectilePrefab; // Prefab de la bala/proyectil
    public Transform firePoint;        // Punto de salida del proyectil

    [Header("Ajustes de Retirada y Melé")]
    public float retreatRange = 4f;      // Si el jugador entra a esta distancia o menos, se retira
    public float meleeRange = 1.2f;      // Distancia para ejecutar ataque cuerpo a cuerpo
    public float meleeDamage = 15f;      // Daño del ataque cuerpo a cuerpo
    public float meleePushForce = 5f;    // Fuerza de empuje del golpe
    public float meleeCooldown = 1.5f;   // Cooldown del ataque cuerpo a cuerpo

    [Header("Referencias")]
    public string playerTag = "Player";

    private NavMeshAgent agent;
    private Transform playerTransform;
    private float nextFireTime = 0f;
    private float nextMeleeTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        agent.stoppingDistance = retreatRange;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 1. Rango Cuerpo a Cuerpo (Aprox. 1 metro o menos)
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
        // 2. Rango de Retirada (El jugador rompió la distancia mínima de seguridad)
        else if (distanceToPlayer < retreatRange)
        {
            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
            Vector3 retreatPosition = transform.position - dirToPlayer * (retreatRange + 2f);

            agent.SetDestination(retreatPosition);
            // No dispara proyectiles mientras retrocede para reposicionarse
        }
        // 3. Rango de Disparo Normal
        else if (distanceToPlayer <= shootingRange)
        {
            RotateTowardsPlayer();
            agent.SetDestination(playerTransform.position);
            agent.stoppingDistance = retreatRange;

            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
        }
        // 4. Fuera de rango (Acercarse al jugador)
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

    void Shoot()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            Vector3 targetPosition = playerTransform.position + Vector3.up * 1f;
            Vector3 shootDirection = (targetPosition - firePoint.position).normalized;

            Quaternion shootRotation = Quaternion.LookRotation(shootDirection);
            Instantiate(projectilePrefab, firePoint.position, shootRotation);
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