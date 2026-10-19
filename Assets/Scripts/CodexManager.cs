using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CodexManager : MonoBehaviour
{
    public static CodexManager Instance;

    [Header("UI References (Arrastra aquí tus elementos de UI)")]
    public GameObject codexPanel;
    public TMP_Text textComponent;
    public Button closeButton;

    [Header("HUD Counter Reference")]
    [Tooltip("Arrastra aquí el componente de texto del HUD que mostrará el contador (ej. 'Páginas: 0 / 6')")]
    public TMP_Text counterHUDText;

    [Header("Pages Text List")]
    [TextArea(5, 15)]
    public List<string> pageTexts = new List<string>();

    private int collectedPagesCount = 0;

    public int CollectedPagesCount => collectedPagesCount;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (codexPanel != null)
            codexPanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseCodex);
        }

        UpdateHUDCounter();
    }

    /// <summary>
    /// Incrementa el contador de páginas recogidas y actualiza el HUD.
    /// </summary>
    public void PageCollected()
    {
        collectedPagesCount++;
        UpdateHUDCounter();
    }

    private void UpdateHUDCounter()
    {
        if (counterHUDText != null)
        {
            int totalPages = pageTexts != null ? pageTexts.Count : 0;
            counterHUDText.text = $"Paginas: {collectedPagesCount} / {totalPages}";
        }
    }

    public bool HasCollectedAllPages(int totalRequired)
    {
        return collectedPagesCount >= totalRequired;
    }

    public void OpenCodex(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= pageTexts.Count) return;

        if (textComponent != null)
        {
            textComponent.text = pageTexts[pageIndex];
        }

        if (codexPanel != null)
        {
            codexPanel.SetActive(true);
        }

        if (FindAnyObjectByType<GameManager>() != null)
        {
            FindAnyObjectByType<GameManager>().OpenCodexGame();
        }
    }

    public void CloseCodex()
    {
        if (codexPanel != null)
        {
            codexPanel.SetActive(false);
        }

        if (FindAnyObjectByType<GameManager>() != null)
        {
            FindAnyObjectByType<GameManager>().CloseCodexGame();
        }
    }
}