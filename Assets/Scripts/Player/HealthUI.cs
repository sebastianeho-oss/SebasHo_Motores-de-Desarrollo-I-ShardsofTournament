using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image healthFillImage;        // Imagen con Image Type: Filled
    public TextMeshProUGUI healthText;   // Texto de la cifra (TMP)

    [Header("Referencia del Jugador")]
    public PlayerHealth playerHealth;    // Script PlayerHealth del personaje

    [Header("Efecto Visual y Color")]
    public float fillSpeed = 5f;         // Velocidad de suavizado de la barra

    [Tooltip("Gradiente de color: el extremo izquierdo (0) es 0% de vida y el derecho (1) es 100% de vida.")]
    public Gradient healthGradient;      // Gradiente configurable desde el Inspector

    void Start()
    {
        if (playerHealth != null && healthFillImage != null)
        {
            float fillPercentage = playerHealth.currentHealth / playerHealth.maxHealth;
            healthFillImage.fillAmount = fillPercentage;

            // Aplicar el color inicial según el gradiente
            if (healthGradient != null)
            {
                healthFillImage.color = healthGradient.Evaluate(fillPercentage);
            }
        }
    }

    void Update()
    {
        if (playerHealth == null) return;

        // Calcular el porcentaje objetivo (actual / máximo)
        float targetFill = playerHealth.currentHealth / playerHealth.maxHealth;

        // Transición suave del valor de la imagen de vida
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFill, Time.deltaTime * fillSpeed);

            // Cambiar el color de la barra dinámicamente usando el gradiente basado en el fillAmount actual
            if (healthGradient != null)
            {
                healthFillImage.color = healthGradient.Evaluate(healthFillImage.fillAmount);
            }
        }

        // Actualizar cifra en pantalla
        if (healthText != null)
        {
            int currentHP = Mathf.CeilToInt(playerHealth.currentHealth);
            int maxHP = Mathf.CeilToInt(playerHealth.maxHealth);

            healthText.text = $"{currentHP} / {maxHP}";
        }
    }
}