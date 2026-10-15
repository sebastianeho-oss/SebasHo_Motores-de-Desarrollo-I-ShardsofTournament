using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Configuración del Pick Up")]
    [Tooltip("Índice del arma en el WeaponHolder (0 a 5)")]
    public int weaponIndex = 0;

    [Header("Configuración de Munición por Duplicado")]
    [Tooltip("Tipo de munición que utiliza esta arma")]
    public AmmoType ammoType;
    [Tooltip("Cantidad de munición que se añade a la reserva si el jugador ya posee el arma")]
    public int ammoAmountOnDuplicate = 30;

    [Tooltip("Efecto de sonido opcional")]
    public AudioClip pickupSound;

    [Header("Efecto Visual")]
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
        // Rotación sobre su propio eje vertical
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

        // Movimiento de flotación vertical (senoidal)
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Buscar el WeaponSwitcher en el collider o en los padres
        WeaponSwitcher switcher = other.GetComponent<WeaponSwitcher>();
        if (switcher == null)
        {
            switcher = other.GetComponentInChildren<WeaponSwitcher>();
        }

        // Buscar el inventario de munición
        PlayerAmmoInventory playerAmmo = other.GetComponent<PlayerAmmoInventory>();
        if (playerAmmo == null)
        {
            playerAmmo = other.GetComponentInParent<PlayerAmmoInventory>();
        }

        if (switcher != null)
        {
            // Intentamos desbloquear el arma. (Nota: Si tu WeaponSwitcher devuelve un bool indicando si ya la tenía, puedes usarlo aquí).
            // Por defecto, llamamos a desbloquear y equipar.
            switcher.UnlockWeapon(weaponIndex, autoEquip: true);

            // Si el jugador ya tiene el arma (o quieres que siempre otorgue munición al pasar por encima), 
            // le inyectamos la munición a la reserva usando tu método AddAmmo.
            if (playerAmmo != null)
            {
                int addedAmmo = playerAmmo.AddAmmo(ammoType, ammoAmountOnDuplicate);
                if (addedAmmo > 0)
                {
                    Debug.Log($"¡Recogiste munición extra del arma! Se añadieron {addedAmmo} unidades de {ammoType}.");
                }
                else
                {
                    Debug.Log($"Reserva de {ammoType} al máximo. Arma recogida/equipada pero sin munición extra.");
                }
            }

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            Destroy(gameObject);
        }
    }
}