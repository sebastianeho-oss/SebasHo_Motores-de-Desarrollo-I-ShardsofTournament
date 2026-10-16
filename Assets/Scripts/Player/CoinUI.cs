using System.Collections;
using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private ShopManager shopManager;          // Referencia al ShopManager
    [SerializeField] private TextMeshProUGUI totalCoinsText;   // El texto principal que muestra el total de monedas en pantalla
    [SerializeField] private GameObject floatingTextPrefab;    // Prefab de texto flotante (un TextMeshProUGUI)
    [SerializeField] private Transform floatingTextSpawnPoint; // El lugar exacto donde nace el texto flotante

    [Header("Ajustes de Animación")]
    [SerializeField] private float floatSpeed = 30f;     // Velocidad con la que sube el texto flotante
    [SerializeField] private float displayDuration = 1f;   // Tiempo antes de desvanecerse (1 segundo)

    private void Start()
    {
        UpdateTotalCoinsDisplay();
    }

    private void Update()
    {
        // Mantenemos sincronizado el texto principal continuamente por si se gasta oro en la tienda
        UpdateTotalCoinsDisplay();
    }

    // Este método se llama exclusivamente cuando el jugador GANA monedas (al matar enemigos)
    public void PlayCoinGainedEffect(int amountGained)
    {
        // Actualizamos el total de inmediato
        UpdateTotalCoinsDisplay();

        // Instanciamos el texto flotante con el signo "+"
        if (floatingTextPrefab != null && floatingTextSpawnPoint != null)
        {
            GameObject floatObj = Instantiate(floatingTextPrefab, floatingTextSpawnPoint.position, Quaternion.identity, floatingTextSpawnPoint);

            // Reseteamos su escala local para evitar distorsiones del Canvas
            floatObj.transform.localScale = Vector3.one;

            TextMeshProUGUI floatText = floatObj.GetComponent<TextMeshProUGUI>();
            if (floatText != null)
            {
                floatText.text = "+" + amountGained;
                StartCoroutine(AnimateAndFadeFloatingText(floatObj, floatText));
            }
        }
    }

    private void UpdateTotalCoinsDisplay()
    {
        if (totalCoinsText != null && shopManager != null)
        {
            totalCoinsText.text = "$" + shopManager.Coins;
        }
    }

    private IEnumerator AnimateAndFadeFloatingText(GameObject textObj, TextMeshProUGUI textComp)
    {
        float elapsedTime = 0f;
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        Color startColor = textComp.color;
        Vector3 startPosition = rectTransform.anchoredPosition;

        while (elapsedTime < displayDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Usa unscaled por seguridad si hay pausas
            float progress = elapsedTime / displayDuration;

            // Mover hacia arriba ligeramente
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = startPosition + new Vector3(0f, floatSpeed * progress, 0f);
            }

            // Desvanecer el canal Alfa de 1 a 0 gradualmente
            float currentAlpha = Mathf.Lerp(1f, 0f, progress);
            textComp.color = new Color(startColor.r, startColor.g, startColor.b, currentAlpha);

            yield return null;
        }

        // Destruir el texto flotante al cumplirse el segundo
        Destroy(textObj);
    }
}