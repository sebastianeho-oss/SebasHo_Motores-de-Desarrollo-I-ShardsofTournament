using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Configuración de Curación")]
    public float healAmount = 25f;

    [Header("Efecto Visual (Opcional)")]
    public float rotationSpeed = 50f;
    public float floatSpeed = 2f;
    public float floatAmount = 0.25f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {        
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            if (!playerHealth.isDead && playerHealth.currentHealth < playerHealth.maxHealth)
            {
                playerHealth.Heal(healAmount);
                Debug.Log($"¡Botiquín recogido! Curado: {healAmount} de salud.");
                Destroy(gameObject);
            }
            else if (playerHealth.currentHealth >= playerHealth.maxHealth)
            {
                Debug.Log("La vida del jugador ya está al máximo. Botiquín no recogido.");
            }
        }
    }
}