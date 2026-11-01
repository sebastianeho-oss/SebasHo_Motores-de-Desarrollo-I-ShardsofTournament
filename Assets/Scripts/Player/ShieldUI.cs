using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShieldUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image shieldFillImage;
    public TextMeshProUGUI shieldText;

    [Header("Referencia del Jugador")]
    public PlayerShield playerShield;

    [Header("Efecto Visual de la Barra")]
    public float fillSpeed = 5f;

    [Tooltip("Gradiente original para la barra de escudo.")]
    public Gradient shieldGradient;

    [Header("Configuración de Color del Texto (Current)")]
    [SerializeField] private float alertThreshold = 25f;
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color halfTextColor = Color.yellow;
    [SerializeField] private Color alertTextColor = Color.red;

    void Start()
    {
        if (playerShield != null && shieldFillImage != null)
        {
            float fillPercentage = playerShield.currentShield / playerShield.maxShield;
            shieldFillImage.fillAmount = fillPercentage;

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

        if (shieldFillImage != null)
        {
            shieldFillImage.fillAmount = Mathf.Lerp(shieldFillImage.fillAmount, targetFill, Time.deltaTime * fillSpeed);

            if (shieldGradient != null)
            {
                shieldFillImage.color = shieldGradient.Evaluate(shieldFillImage.fillAmount);
            }
        }

        if (shieldText != null)
        {
            int currentS = Mathf.CeilToInt(playerShield.currentShield);
            int maxS = Mathf.CeilToInt(playerShield.maxShield);

            shieldText.text = $"{currentS} / {maxS}";

            if (playerShield.currentShield <= alertThreshold)
            {
                shieldText.color = alertTextColor;
            }
            else if (playerShield.currentShield <= playerShield.maxShield * 0.5f)
            {
                shieldText.color = halfTextColor;
            }
            else
            {
                shieldText.color = normalTextColor;
            }
        }
    }
}