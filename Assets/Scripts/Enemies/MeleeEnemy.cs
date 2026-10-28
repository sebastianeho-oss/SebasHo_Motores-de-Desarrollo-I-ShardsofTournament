using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))] // Asegura que tenga Animator
public class MeleeEnemy : MonoBehaviour
{
    [Header("Ajustes de Combate")]
    public float attackRange = 2f;
    public float attackDamage = 15f;
    public float attackRate = 1.2f;

    [Header("Referencias")]
    public string playerTag = "Player";

    private NavMeshAgent agent;
    private Animator animator; // Referencia al Animator
    private Transform playerTransform;
    private PlayerShield playerShield;
    private PlayerHealth playerHealth;
    private float nextAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Inicializar Animator

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerShield = playerObj.GetComponent<PlayerShield>();
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        agent.stoppingDistance = attackRange - 0.2f;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Actualizar el parámetro Speed en el Animator basado en el movimiento del NavMeshAgent
        float currentSpeed = agent.velocity.magnitude;
        animator.SetFloat("Speed", currentSpeed);

        agent.SetDestination(playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            RotateTowardsPlayer();

            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackRate;
                Attack();
            }
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

    void Attack()
    {
        // Activar la animación de ataque mediante el Trigger
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Validar si el jugador no está muerto antes de atacar
        bool isDead = playerHealth != null && playerHealth.isDead;

        if (!isDead)
        {
            if (playerShield != null)
            {
                playerShield.TakeDamage(attackDamage);
                Debug.Log($"¡MeleeEnemy atacó el escudo del jugador con {attackDamage} de daño!");
            }
            else if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"¡MeleeEnemy atacó la vida directa del jugador con {attackDamage} de daño!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}