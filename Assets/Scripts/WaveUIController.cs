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

    void Update()
    {
        if (waveSpawner == null) return;

        // Ocultar o mostrar el texto de la oleada según si está en tiempo de espera
        if (waveText != null)
        {
            if (waveSpawner.IsCountingDown)
            {
                // Si está esperando a la siguiente oleada, se oculta
                waveText.gameObject.SetActive(false);
            }
            else
            {
                // Si no está contando para la siguiente oleada, se muestra y actualiza
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

        // Muestra la cuenta atrás de la horda activa
        if (activeWaveTimerText != null)
        {
            if (waveSpawner.IsWaveActive)
            {
                activeWaveTimerText.gameObject.SetActive(true);

                float time = waveSpawner.ActiveWaveRemainingTime;
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                activeWaveTimerText.text = $"{minutes:00}:{seconds:00}";
            }
            else
            {
                activeWaveTimerText.gameObject.SetActive(false);
            }
        }
    }
}