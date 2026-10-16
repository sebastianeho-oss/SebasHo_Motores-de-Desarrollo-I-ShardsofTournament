using UnityEngine;
using TMPro;

public class WaveUIController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WaveSpawner waveSpawner;

    [Header("Textos UI (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemiesRemainingText;
    [SerializeField] private TextMeshProUGUI enemiesKilledText;
    [SerializeField] private TextMeshProUGUI countdownTimerText;
    [SerializeField] private TextMeshProUGUI activeWaveTimerText;

    [Header("Configuración de Color del Temporizador de Oleada")]
    [SerializeField] private Color normalTimerColor = Color.white;  // Color normal al iniciar
    [SerializeField] private Color halfTimerColor = Color.yellow;   // Color al llegar a la mitad (50%)
    [SerializeField] private Color alertTimerColor = Color.red;     // Color rojo cuando faltan 10 segundos o menos

    void Update()
    {
        if (waveSpawner == null) return;

        // Ocultar o mostrar el texto de la oleada según si está en tiempo de espera
        if (waveText != null)
        {
            if (waveSpawner.IsCountingDown)
            {
                waveText.gameObject.SetActive(false);
            }
            else
            {
                waveText.gameObject.SetActive(true);
                waveText.text = $"Oleada: {waveSpawner.CurrentWaveNumber} / {waveSpawner.TotalWaves}";
            }
        }

        if (enemiesRemainingText != null)
        {
            enemiesRemainingText.text = $"Vivos: {waveSpawner.EnemiesRemaining}";
        }

        if (enemiesKilledText != null)
        {
            enemiesKilledText.text = $"Eliminados: {waveSpawner.EnemiesKilled} / {waveSpawner.TotalEnemiesInWave}";
        }

        if (countdownTimerText != null)
        {
            if (waveSpawner.IsCountingDown)
            {
                countdownTimerText.gameObject.SetActive(true);
                countdownTimerText.text = $"Siguiente horda en: {Mathf.Ceil(waveSpawner.WaveTimer)}s";
            }
            else
            {
                countdownTimerText.gameObject.SetActive(false);
            }
        }

        // Muestra la cuenta atrás de la horda activa y aplica los cambios de color
        if (activeWaveTimerText != null)
        {
            if (waveSpawner.IsWaveActive)
            {
                activeWaveTimerText.gameObject.SetActive(true);

                float time = waveSpawner.ActiveWaveRemainingTime;
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                activeWaveTimerText.text = $"{minutes:00}:{seconds:00}";

                // --- LÓGICA DE COLORES PARA EL TEMPORIZADOR ---
                if (time <= 10f)
                {
                    activeWaveTimerText.color = alertTimerColor; // Rojo a los 10 segundos o menos
                }
                else if (time <= (waveSpawner.CurrentWaveTotalDuration * 0.5f))
                {
                    activeWaveTimerText.color = halfTimerColor;  // Amarillo al llegar a la mitad del tiempo
                }
                else
                {
                    activeWaveTimerText.color = normalTimerColor; // Color normal en la primera mitad
                }
            }
            else
            {
                activeWaveTimerText.gameObject.SetActive(false);
            }
        }
    }
}