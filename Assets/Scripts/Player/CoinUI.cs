using System.Collections;
using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private TextMeshProUGUI totalCoinsText;
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private Transform floatingTextSpawnPoint;
    [Header("Ajustes de Animación")]
    [SerializeField] private float floatSpeed = 30f;
    [SerializeField] private float displayDuration = 1f;

    private void Start()
    {
        UpdateTotalCoinsDisplay();
    }

    private void Update()
    {
        UpdateTotalCoinsDisplay();
    }

    public void PlayCoinGainedEffect(int amountGained)
    {
        UpdateTotalCoinsDisplay();

        if (floatingTextPrefab != null && floatingTextSpawnPoint != null)
        {
            GameObject floatObj = Instantiate(floatingTextPrefab, floatingTextSpawnPoint.position, Quaternion.identity, floatingTextSpawnPoint);

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
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / displayDuration;

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = startPosition + new Vector3(0f, floatSpeed * progress, 0f);
            }

            float currentAlpha = Mathf.Lerp(1f, 0f, progress);
            textComp.color = new Color(startColor.r, startColor.g, startColor.b, currentAlpha);

            yield return null;
        }

        Destroy(textObj);
    }
}