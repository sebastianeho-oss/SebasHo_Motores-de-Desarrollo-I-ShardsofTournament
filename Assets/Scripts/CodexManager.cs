using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CodexManager : MonoBehaviour
{
    public static CodexManager Instance;

    [Header("UI References (Arrastra aquí tus elementos de UI)")]
    public GameObject codexPanel;       // El Panel único general con la imagen y el texto
    public TMP_Text textComponent;      // El componente de texto (TextMeshPro)
    public Button closeButton;          // El botón único de "Salir" o "Cerrar"

    [Header("Pages Text List")]
    [Tooltip("Escribe aquí el texto de cada página. Element 0 = Página 1, Element 1 = Página 2, etc.")]
    [TextArea(5, 15)]
    public List<string> pageTexts = new List<string>();

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

        // Delega la pausa, la ocultación del HUD y la liberación del cursor al GameManager
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

        // Delega la reanudación del juego y el bloqueo del cursor al GameManager
        if (FindAnyObjectByType<GameManager>() != null)
        {
            FindAnyObjectByType<GameManager>().CloseCodexGame();
        }
    }
}