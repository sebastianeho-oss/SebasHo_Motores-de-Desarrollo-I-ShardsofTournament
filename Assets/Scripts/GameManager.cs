using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static bool IsPaused { get; private set; } = false;

    [Header("UI Panels")]
    public GameObject HUD;
    public GameObject gameOverUI;
    public GameObject winUI;
    public GameObject winCodexUI; // <--- Segunda pantalla de victoria para el Códice completo
    public GameObject pauseUI;
    public GameObject shopUI;

    [Header("Referencias del Jugador")]
    public GameObject player;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Pause.performed += _ => OnPauseInput();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Start()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        LockCursor();

        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (winUI != null) winUI.SetActive(false);
        if (winCodexUI != null) winCodexUI.SetActive(false);
        if (pauseUI != null) pauseUI.SetActive(false);
        if (HUD != null) HUD.SetActive(true);
    }

    void Update()
    {
        bool isEndGameUIActive = (winUI != null && winUI.activeInHierarchy) ||
                                 (winCodexUI != null && winCodexUI.activeInHierarchy) ||
                                 (gameOverUI != null && gameOverUI.activeInHierarchy);

        bool isShopUIActive = shopUI != null && shopUI.activeInHierarchy;

        bool isAnyUIActive = isEndGameUIActive || IsPaused || isShopUIActive;

        if (isAnyUIActive)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    private void OnPauseInput()
    {
        bool isEndGameUIActive = (winUI != null && winUI.activeInHierarchy) ||
                                 (winCodexUI != null && winCodexUI.activeInHierarchy) ||
                                 (gameOverUI != null && gameOverUI.activeInHierarchy);

        if (!isEndGameUIActive)
        {
            TogglePause();
        }
    }

    private void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UnlockCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        if (pauseUI != null) pauseUI.SetActive(true);
        if (HUD != null) HUD.SetActive(false);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pauseUI != null) pauseUI.SetActive(false);
        if (HUD != null) HUD.SetActive(true);
    }

    public void GameOver()
    {
        if (gameOverUI != null) gameOverUI.SetActive(true);
        if (HUD != null) HUD.SetActive(false);
        Time.timeScale = 0f;
    }

    public void Win()
    {
        if (winUI != null) winUI.SetActive(true);
        if (HUD != null) HUD.SetActive(false);
        Time.timeScale = 0f;
    }

    // --- MÉTODOS PARA PANTALLA DE VICTORIA ESPECIAL ---
    public void WinCodex()
    {
        if (winCodexUI != null)
        {
            winCodexUI.SetActive(true);
        }
        else if (winUI != null)
        {
            // Respaldar con la victoria normal si no asignaste el panel en el Inspector
            winUI.SetActive(true);
        }

        if (HUD != null) HUD.SetActive(false);
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void MainMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // --- MÉTODOS AÑADIDOS PARA EL CÓDICE ---
    public void OpenCodexGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        if (HUD != null) HUD.SetActive(false);
        UnlockCursor();
    }

    public void CloseCodexGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (HUD != null) HUD.SetActive(true);
        LockCursor();
    }
}