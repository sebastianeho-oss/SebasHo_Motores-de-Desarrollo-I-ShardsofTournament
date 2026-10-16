using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShieldUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image shieldFillImage;        // Imagen con Image Type: Filled
    public TextMeshProUGUI shieldText;   // El texto de la cifra del escudo (TMP)[cite: 5]

    [Header("Referencia del Jugador")]
    public PlayerShield playerShield;    // Script PlayerShield del personaje[cite: 5]

    [Header("Efecto Visual de la Barra")]
    public float fillSpeed = 5f;         // Velocidad de suavizado de la barra[cite: 5]

    [Tooltip("Gradiente original para la barra de escudo.")]
    public Gradient shieldGradient;      // Gradiente configurable desde el Inspector[cite: 5]

    [Header("Configuración de Color del Texto (Current)")]
    [SerializeField] private float alertThreshold = 25f;          // Umbral crítico para rojo (<= 25)
    [SerializeField] private Color normalTextColor = Color.white; // Color normal (> 50%)
    [SerializeField] private Color halfTextColor = Color.yellow;  // Color intermedio a la mitad (<= 50%)
    [SerializeField] private Color alertTextColor = Color.red;    // Color en rojo (<= 25)

    void Start()
    {
        if (playerShield != null && shieldFillImage != null)
        {
            float fillPercentage = playerShield.currentShield / playerShield.maxShield;
            shieldFillImage.fillAmount = fillPercentage;

            // Aplicar el color inicial de la barra según el gradiente original
            if (shieldGradient != null)
            {
                shieldFillImage.color = shieldGradient.Evaluate(fillPercentage);
            }
        }
    }

    void Update()
    {
        if (playerShield == null) return;

        float targetFill = playerShield.currentShield / playerShield.maxShield;

        // Transición suave de la barra usando tu gradiente original
        if (shieldFillImage != null)
        {
            shieldFillImage.fillAmount = Mathf.Lerp(shieldFillImage.fillAmount, targetFill, Time.deltaTime * fillSpeed);

            if (shieldGradient != null)
            {
                shieldFillImage.color = shieldGradient.Evaluate(shieldFillImage.fillAmount);
            }
        }

        // Actualizar cifra de escudo en pantalla y cambiar el color SOLO del texto
        if (shieldText != null)
        {
            int currentS = Mathf.CeilToInt(playerShield.currentShield);
            int maxS = Mathf.CeilToInt(playerShield.maxShield);

            shieldText.text = $"{currentS} / {maxS}";

            // Lógica de colores exclusiva para el número actual del escudo
            if (playerShield.currentShield <= alertThreshold)
            {
                shieldText.color = alertTextColor; // Rojo cuando es <= 25
            }
            else if (playerShield.currentShield <= playerShield.maxShield * 0.5f)
            {
                shieldText.color = halfTextColor;  // Color intermedio al llegar a la mitad (50%)
            }
            else
            {
                shieldText.color = normalTextColor; // Color normal por encima de la mitad
            }
        }
    }
}