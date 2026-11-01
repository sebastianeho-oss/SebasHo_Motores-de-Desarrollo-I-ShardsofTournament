using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Ajustes del Proyectil")]
    public float speed = 20f;
    public float damage = 15f;
    public float lifeTime = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            return;
        }

        PlayerShield playerShield = other.GetComponent<PlayerShield>();
        if (playerShield == null) playerShield = other.GetComponentInParent<PlayerShield>();
        if (playerShield == null) playerShield = other.GetComponentInChildren<PlayerShield>();

        if (playerShield != null)
        {
            playerShield.TakeDamage(damage);
            Debug.Log($"¡Proyectil enemigo impactó el escudo del jugador! -{damage}");
            Destroy(gameObject);
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null) playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) playerHealth = other.GetComponentInChildren<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log($"¡Proyectil enemigo dañó la vida del jugador! -{damage}");
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}