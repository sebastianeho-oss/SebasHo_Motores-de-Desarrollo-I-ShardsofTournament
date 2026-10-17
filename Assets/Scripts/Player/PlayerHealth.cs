using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Salud")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Mitigación por Vida Baja")]
    [SerializeField] private float lowHealthThreshold = 25f;
    [SerializeField] private float damageReductionMultiplier = 0.10f;

    [Header("Estados y Permisos")]
    public bool isInvincible = false;
    public bool isDead = false;

    [Header("Referencias")]
    public GameManager gameManager;

    [Header("Referencias de UI")]
    public HealthUI healthUI;

    [Header("Efecto de Cámara")]
    [Tooltip("Arrastra aquí tu controlador de cámaras de Cinemachine (se auto-asigna si está en la escena).")]
    public CinemachineStateController cinemachineController;

    void Start()
    {
        currentHealth = maxHealth;

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        if (cinemachineController == null)
        {
            cinemachineController = FindAnyObjectByType<CinemachineStateController>();
        }

        if (healthUI == null)
        {
            healthUI = FindAnyObjectByType<HealthUI>();
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || isInvincible) return;

        float finalDamage = amount;
        if (currentHealth <= lowHealthThreshold)
        {
            finalDamage = amount * damageReductionMultiplier;
            Debug.Log($"¡Activada la reducción de daño por vida baja! Daño original: {amount} -> Daño reducido (90% menos): {finalDamage}");
        }

        currentHealth -= finalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"Jugador recibió {finalDamage} de daño. Salud actual: {currentHealth}");

        // --- DISPARAR LA SACUDIDA DE CÁMARA ---
        if (cinemachineController != null)
        {
            cinemachineController.TriggerCameraShake();
        }

        // --- DISPARAR EL EFECTO VISUAL DE DAÑO EN UI (PANEL ROJO) ---
        if (healthUI != null)
        {
            healthUI.TriggerDamageFlash();
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("El jugador ha muerto.");

        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }
}