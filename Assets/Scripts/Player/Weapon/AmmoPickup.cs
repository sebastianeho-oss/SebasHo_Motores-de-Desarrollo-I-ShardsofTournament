using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Configuración de Munición")]
    public AmmoType ammoType;
    public int ammoAmount = 30;

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
    PlayerAmmoInventory playerInventory = other.GetComponent<PlayerAmmoInventory>();

    if (playerInventory == null)
    {
        playerInventory = other.GetComponentInParent<PlayerAmmoInventory>();
    }

    if (playerInventory != null)
        {
        int amountAdded = playerInventory.AddAmmo(ammoType, ammoAmount);

        if (amountAdded > 0)
            {
            Debug.Log($"¡Recogiste {amountAdded} unidades de munición para {ammoType}!");         
            Destroy(gameObject);
            }
        else
            {
            Debug.Log($"Reserva de {ammoType} está al máximo ({playerInventory.GetMaxReserveAmmo(ammoType)}). Caja no recogida.");
            }
        }
    }
}