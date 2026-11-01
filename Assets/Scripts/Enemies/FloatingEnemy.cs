using UnityEngine;

public class FloatingEnemy : MonoBehaviour
{
    [Header("Ajustes de Vuelo y Distancia")]
    public float flySpeed = 4f;
    public float hoverHeight = 4f; 
    public float preferredDistance = 18f;
    public float floatFrequency = 2f;
    public float floatAmplitude = 0.5f;

    [Header("Ajustes de Ataque")]
    public float attackRate = 2.5f;
    public GameObject energyBallPrefab;
    public Transform firePoint;

    [Header("Referencias")]
    public string playerTag = "Player";

    private Transform playerTransform;
    private float nextAttackTime = 0f;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        Vector3 directionFromPlayer = (transform.position - playerTransform.position);
        directionFromPlayer.y = 0;
                
        Vector3 targetPosition = playerTransform.position + directionFromPlayer.normalized * preferredDistance;
        targetPosition.y = playerTransform.position.y + hoverHeight;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * flySpeed);

        float hoverOffsetY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position += Vector3.up * hoverOffsetY * Time.deltaTime;

        Vector3 lookDir = (playerTransform.position - transform.position).normalized;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
               
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackRate;
            LaunchEnergyBall();
        }
    }

    void LaunchEnergyBall()
    {
        if (energyBallPrefab != null && firePoint != null)
        {
            Vector3 targetPosition = playerTransform.position + Vector3.up * 0.8f;
            Vector3 direction = (targetPosition - firePoint.position).normalized;

            Quaternion launchRotation = Quaternion.LookRotation(direction);
            Instantiate(energyBallPrefab, firePoint.position, launchRotation);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, preferredDistance);
    }
}