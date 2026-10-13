using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossEnemy : MonoBehaviour
{
    [Header("Ajustes de Vuelo y Persecución")]
    public float flySpeed = 5f;
    public float hoverHeight = 15f;
    public float attackRange = 25f;
    public float floatFrequency = 2f;
    public float floatAmplitude = 1.5f;

    [Header("Ajustes de Ataque y Ráfaga")]
    public float attackRate = 3f;
    public float burstDelay = 0.15f;
    public GameObject energyBallPrefab;

    [Header("Puntos de Disparo")]
    public Transform[] firePoints;
    public bool shootAllSimultaneously = true;
    private int currentFirePointIndex = 0;

    [Header("Orientación (Si el jefe mira hacia un lado)")]
    public float rotationOffsetY = 0f;      // Modifica esto en el Inspector (ej. 90, 180 o -90) si el jefe no apunta bien al jugador

    [Header("Referencias")]
    public string playerTag = "Player";

    private Transform playerTransform;
    private Rigidbody rb;
    private float nextAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.freezeRotation = true;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        // 1. MOVIMIENTO Y PERSECUCIÓN
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0;

        float distanceToPlayer = directionToPlayer.magnitude;
        float targetY = playerTransform.position.y + hoverHeight;

        Vector3 targetPosition;
        if (distanceToPlayer > attackRange)
        {
            targetPosition = playerTransform.position - directionToPlayer.normalized * attackRange;
            targetPosition.y = targetY;
        }
        else
        {
            targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);
        }

        Vector3 moveDirection = (targetPosition - transform.position);
        float hoverOffsetY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        Vector3 finalVelocity = moveDirection.normalized * flySpeed;
        finalVelocity.y += hoverOffsetY;

        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z)) < 0.5f)
        {
            finalVelocity.x = 0;
            finalVelocity.z = 0;
        }

        rb.linearVelocity = finalVelocity;

        // 2. ROTACIÓN FORZADA HACIA EL JUGADOR (Con corrección de eje y offset)
        Vector3 lookDir = playerTransform.position - transform.position;
        lookDir.y = 0f; // Mantiene nivelado el giro horizontalmente

        if (lookDir != Vector3.zero)
        {
            Quaternion baseRotation = Quaternion.LookRotation(lookDir);
            // Aplicamos el offset por si el modelo 3D está desalineado de fábrica
            Quaternion correctedRotation = baseRotation * Quaternion.Euler(0, rotationOffsetY, 0);

            rb.MoveRotation(Quaternion.Slerp(rb.rotation, correctedRotation, Time.fixedDeltaTime * 10f));
        }

        // 3. ATAQUE EN RÁFAGA
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackRate;
            StartCoroutine(ShootBurstRoutine());
        }
    }

    IEnumerator ShootBurstRoutine()
    {
        for (int i = 0; i < 3; i++)
        {
            LaunchEnergyBalls();

            if (i < 2)
            {
                yield return new WaitForSeconds(burstDelay);
            }
        }
    }

    void LaunchEnergyBalls()
    {
        if (energyBallPrefab == null || firePoints == null || firePoints.Length == 0) return;

        Vector3 targetPosition = playerTransform.position + Vector3.up * 0.8f;

        if (shootAllSimultaneously)
        {
            foreach (Transform fp in firePoints)
            {
                if (fp != null)
                {
                    Vector3 direction = (targetPosition - fp.position).normalized;
                    Quaternion launchRotation = Quaternion.LookRotation(direction);
                    Instantiate(energyBallPrefab, fp.position, launchRotation);
                }
            }
        }
        else
        {
            Transform fp = firePoints[currentFirePointIndex];
            if (fp != null)
            {
                Vector3 direction = (targetPosition - fp.position).normalized;
                Quaternion launchRotation = Quaternion.LookRotation(direction);
                Instantiate(energyBallPrefab, fp.position, launchRotation);
            }

            currentFirePointIndex = (currentFirePointIndex + 1) % firePoints.Length;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}