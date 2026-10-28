using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class RangedEnemyWithAnimations : MonoBehaviour
{
    [Header("Ajustes de Disparo")]
    public float shootingRange = 12f;
    public float fireRate = 1.5f;
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Ajustes de Retirada y Movimiento")]
    public float runSpeed = 6f;          // Velocidad al correr
    public float retreatRange = 4f;
    public float meleeRange = 1.2f;
    public float meleeDamage = 15f;
    public float meleePushForce = 5f;
    public float meleeCooldown = 1.5f;

    [Header("Referencias")]
    public string playerTag = "Player";

    private NavMeshAgent agent;
    private Animator animator;
    private Transform playerTransform;
    private float nextFireTime = 0f;
    private float nextMeleeTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.speed = runSpeed;

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

        // Enviar la velocidad actual al Animator para que ejecute la animación de correr/caminar
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

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
        // 3. Rango de Disparo / Lanzamiento del proyectil
        else if (distanceToPlayer <= shootingRange)
        {
            RotateTowardsPlayer();
            agent.SetDestination(transform.position); // Se detiene para apuntar/disparar
            agent.stoppingDistance = retreatRange;

            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                StartCoroutine(ShootRoutine());
            }
        }
        // 4. Fuera de rango (Acercarse corriendo)
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

    IEnumerator ShootRoutine()
    {
        // Activar la animación de disparo / lanzamiento
        if (animator != null)
        {
            animator.SetTrigger("Throw");
        }

        // Opcional: Pequeña pausa (delay) para que la instanciación de la bala coincida 
        // exactamente con el fotograma donde el personaje realiza el gesto de arrojar/disparar.
        yield return new WaitForSeconds(0.3f);

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