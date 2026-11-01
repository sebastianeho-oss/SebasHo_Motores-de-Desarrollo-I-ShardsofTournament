using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HealthUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image healthFillImage;
    public TextMeshProUGUI healthText;

    [Header("Panel de Alerta de Vida Baja (Permanente)")]
    public GameObject lowHealthPanel;

    [Header("Efecto de Daño (Flash Rojo)")]
    public Image damageFlashImage;
    public float flashDuration = 0.25f;
    private Coroutine flashCoroutine;

    [Header("Referencia del Jugador")]
    public PlayerHealth playerHealth;

    [Header("Efecto Visual de la Barra")]
    public float fillSpeed = 5f;

    [Tooltip("Gradiente original para la barra de vida.")]
    public Gradient healthGradient;

    [Header("Configuración de Color del Texto (Current)")]
    [SerializeField] private float alertThreshold = 25f;
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color halfTextColor = Color.yellow;
    [SerializeField] private Color alertTextColor = Color.red;

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

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = Mathf.Lerp(healthFillImage.fillAmount, targetFill, Time.deltaTime * fillSpeed);

            if (healthGradient != null)
            {
                healthFillImage.color = healthGradient.Evaluate(healthFillImage.fillAmount);
            }
        }

        if (healthText != null)
        {
            int currentHP = Mathf.CeilToInt(playerHealth.currentHealth);
            int maxHP = Mathf.CeilToInt(playerHealth.maxHealth);

            healthText.text = $"{currentHP} / {maxHP}";

            if (playerHealth.currentHealth <= alertThreshold)
            {
                healthText.color = alertTextColor;

                if (lowHealthPanel != null && !lowHealthPanel.activeSelf)
                {
                    lowHealthPanel.SetActive(true);
                }
            }
            else
            {
                if (lowHealthPanel != null && lowHealthPanel.activeSelf)
                {
                    lowHealthPanel.SetActive(false);
                }

                if (playerHealth.currentHealth <= playerHealth.maxHealth * 0.5f)
                {
                    healthText.color = halfTextColor;
                }
                else
                {
                    healthText.color = normalTextColor;
                }
            }
        }
    }

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
        Color startColor = damageFlashImage.color;
        startColor.a = 0.6f;
        damageFlashImage.color = startColor;

        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.6f, 0f, elapsed / flashDuration);

            Color currentColor = damageFlashImage.color;
            currentColor.a = alpha;
            damageFlashImage.color = currentColor;

            yield return null;
        }

        Color endColor = damageFlashImage.color;
        endColor.a = 0f;
        damageFlashImage.color = endColor;
    }
}