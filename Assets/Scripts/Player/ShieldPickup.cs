using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    [Header("Configuración de Escudo")]
    public float shieldAmount = 25f;

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
        PlayerShield playerShield = other.GetComponent<PlayerShield>();
        if (playerShield == null)
        {
            playerShield = other.GetComponentInParent<PlayerShield>();
        }
        if (playerShield == null)
        {
            playerShield = other.GetComponentInChildren<PlayerShield>();
        }

        if (playerShield != null)
        {
            if (playerShield.currentShield < playerShield.maxShield)
            {
                playerShield.RechargeShield(shieldAmount);
                Debug.Log($"¡Escudo recogido! Recargado: {shieldAmount} de escudo.");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("El escudo del jugador ya está al máximo. Objeto no recogido.");
            }
        }
    }
}