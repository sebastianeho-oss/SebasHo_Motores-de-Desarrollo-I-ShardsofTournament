using UnityEngine;

public class CodexCollectible : MonoBehaviour
{
    [Header("Codex Configuration")]
    [Tooltip("Número de la página en la lista del CodexManager. 0 = Primer texto, 1 = Segundo texto, etc.")]
    public int pageIndexToUnlock;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que ha tocado la página es el jugador
        if (other.CompareTag("Player"))
        {
            // Abre el códice con la página correspondiente
            if (CodexManager.Instance != null)
            {
                CodexManager.Instance.OpenCodex(pageIndexToUnlock);
            }

            // Destruye la página física del suelo para que desaparezca al tocarla
            Destroy(gameObject);
        }
    }
}