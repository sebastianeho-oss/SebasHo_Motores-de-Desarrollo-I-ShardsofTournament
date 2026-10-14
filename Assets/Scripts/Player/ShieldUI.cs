using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShieldUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image shieldFillImage;        // Imagen con Image Type: Filled
    public TextMeshProUGUI shieldText;   // El texto de la cifra del escudo (TMP)

    [Header("Referencia del Jugador")]
    public PlayerShield playerShield;    // Script PlayerShield del personaje[cite: 1]

    [Header("Efecto Visual y Color")]
    public float fillSpeed = 5f;         // Velocidad de suavizado de la barra[cite: 1]

    [Tooltip("Gradiente de color: el extremo izquierdo (0) es 0% de escudo y el derecho (1) es 100% de escudo.")]
    public Gradient shieldGradient;      // Gradiente configurable desde el Inspector

    void Start()
    {
        if (playerShield != null && shieldFillImage != null)
        {
            float fillPercentage = playerShield.currentShield / playerShield.maxShield;
            shieldFillImage.fillAmount = fillPercentage;

            // Aplicar el color inicial según el gradiente
            if (shieldGradient != null)
            {
                shieldFillImage.color = shieldGradient.Evaluate(fillPercentage);
            }
        }
    }

    void Update()
    {
        if (playerShield == null) return;

        // Calcular el porcentaje objetivo (actual / máximo)
        float targetFill = playerShield.currentShield / playerShield.maxShield;

        // Transición suave del valor de la imagen de escudo
        if (shieldFillImage != null)
        {
            shieldFillImage.fillAmount = Mathf.Lerp(shieldFillImage.fillAmount, targetFill, Time.deltaTime * fillSpeed);

            // Cambiar el color de la barra dinámicamente usando el gradiente basado en el fillAmount actual
            if (shieldGradient != null)
            {
                shieldFillImage.color = shieldGradient.Evaluate(shieldFillImage.fillAmount);
            }
        }

        // Actualizar cifra de escudo en pantalla[cite: 1]
        if (shieldText != null)
        {
            int currentS = Mathf.CeilToInt(playerShield.currentShield);
            int maxS = Mathf.CeilToInt(playerShield.maxShield);

            shieldText.text = $"{currentS} / {maxS}";
        }
    }
}