using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image healthFillImage;        // Imagen con Image Type: Filled[cite: 4]
    public TextMeshProUGUI healthText;   // Texto de la cifra (TMP)[cite: 4]

    [Header("Panel de Alerta de Vida Baja")]
    public GameObject lowHealthPanel;    // Arrastra aquí el Panel rojo transparente

    [Header("Referencia del Jugador")]
    public PlayerHealth playerHealth;    // Script PlayerHealth del personaje[cite: 4]

    [Header("Efecto Visual de la Barra")]
    public float fillSpeed = 5f;         // Velocidad de suavizado de la barra[cite: 4]

    [Tooltip("Gradiente original para la barra de vida.")]
    public Gradient healthGradient;      // Gradiente configurable desde el Inspector[cite: 4]

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

        // Asegurarse de que el panel comience apagado si la vida es mayor a 25
        if (lowHealthPanel != null)
        {
            lowHealthPanel.SetActive(false);
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

            // Lógica de colores exclusiva para el número actual y activación del Panel
            if (playerHealth.currentHealth <= alertThreshold)
            {
                healthText.color = alertTextColor; // Rojo cuando es <= 25

                if (lowHealthPanel != null && !lowHealthPanel.activeSelf)
                {
                    lowHealthPanel.SetActive(true); // Activa el panel rojo transparente
                }
            }
            else
            {
                // Si la salud vuelve a subir (por curación), apagamos el panel de alerta
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
}