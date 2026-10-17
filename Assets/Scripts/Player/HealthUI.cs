using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Necesario para las Corrutinas

public class HealthUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image healthFillImage;        // Imagen con Image Type: Filled
    public TextMeshProUGUI healthText;   // Texto de la cifra (TMP)

    [Header("Panel de Alerta de Vida Baja (Permanente)")]
    public GameObject lowHealthPanel;    // Panel rojo para cuando estás bajo de vida

    [Header("Efecto de Daño (Flash Rojo)")]
    public Image damageFlashImage;       // Imagen UI en pantalla completa que parpadea al recibir daño
    public float flashDuration = 0.25f;  // Cuánto dura el destello en pantalla
    private Coroutine flashCoroutine;

    [Header("Referencia del Jugador")]
    public PlayerHealth playerHealth;    // Script PlayerHealth del personaje

    [Header("Efecto Visual de la Barra")]
    public float fillSpeed = 5f;         // Velocidad de suavizado de la barra

    [Tooltip("Gradiente original para la barra de vida.")]
    public Gradient healthGradient;      // Gradiente configurable desde el Inspector

    [Header("Configuración de Color del Texto (Current)")]
    [SerializeField] private float alertThreshold = 25f;          // Umbral crítico (<= 25)
    [SerializeField] private Color normalTextColor = Color.white; // Color normal (> 50%)
    [SerializeField] private Color halfTextColor = Color.yellow;  // Color intermedio a la mitad (<= 50%)
    [SerializeField] private Color alertTextColor = Color.red;    // Color en rojo (<= 25)

    void Start()
    {
        if (playerHealth != null && healthFillImage != null)
        {
            float fillPercentage = playerHealth.currentHealth / playerHealth.maxHealth;
            healthFillImage.fillAmount = fillPercentage;

            if (healthGradient != null)
            {
                healthFillImage.color = healthGradient.Evaluate(fillPercentage);
            }
        }

        // Asegurarse de que los paneles comiencen apagados / transparentes
        if (lowHealthPanel != null)
        {
            lowHealthPanel.SetActive(false);
        }

        if (damageFlashImage != null)
        {
            Color col = damageFlashImage.color;
            col.a = 0f;
            damageFlashImage.color = col;
        }
    }

    void Update()
    {
        if (playerHealth == null) return;

        float targetFill = playerHealth.currentHealth / playerHealth.maxHealth;

        // Transición suave de la barra usando tu gradiente original
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFill, Time.deltaTime * fillSpeed);

            if (healthGradient != null)
            {
                healthFillImage.color = healthGradient.Evaluate(healthFillImage.fillAmount);
            }
        }

        // Actualizar cifra en pantalla y cambiar el color SOLO del texto
        if (healthText != null)
        {
            int currentHP = Mathf.CeilToInt(playerHealth.currentHealth);
            int maxHP = Mathf.CeilToInt(playerHealth.maxHealth);

            healthText.text = $"{currentHP} / {maxHP}";

            // Lógica de colores exclusiva para el número actual y activación del Panel de vida baja
            if (playerHealth.currentHealth <= alertThreshold)
            {
                healthText.color = alertTextColor; // Rojo cuando es <= 25

                if (lowHealthPanel != null && !lowHealthPanel.activeSelf)
                {
                    lowHealthPanel.SetActive(true); // Activa el panel rojo permanente de vida baja
                }
            }
            else
            {
                // Si la salud vuelve a subir (por curación), apagamos el panel de vida baja
                if (lowHealthPanel != null && lowHealthPanel.activeSelf)
                {
                    lowHealthPanel.SetActive(false);
                }

                if (playerHealth.currentHealth <= playerHealth.maxHealth * 0.5f)
                {
                    healthText.color = halfTextColor;  // Color intermedio al llegar a la mitad (50%)
                }
                else
                {
                    healthText.color = normalTextColor; // Color normal por encima de la mitad
                }
            }
        }
    }

    // Método público para disparar el destello rojo (llamado desde PlayerHealth)
    public void TriggerDamageFlash()
    {
        if (damageFlashImage != null)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(DamageFlashRoutine());
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        // Pone la imagen totalmente roja y opaca de golpe
        Color startColor = damageFlashImage.color;
        startColor.a = 0.6f; // Ajusta este valor (0 a 1) para cambiar qué tan intenso es el rojo
        damageFlashImage.color = startColor;

        float elapsed = 0f;

        // Hace un desvanecimiento (Fade Out) suave hasta que desaparezca
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.6f, 0f, elapsed / flashDuration);

            Color currentColor = damageFlashImage.color;
            currentColor.a = alpha;
            damageFlashImage.color = currentColor;

            yield return null;
        }

        // Asegurar que quede completamente transparente al terminar
        Color endColor = damageFlashImage.color;
        endColor.a = 0f;
        damageFlashImage.color = endColor;
    }
}