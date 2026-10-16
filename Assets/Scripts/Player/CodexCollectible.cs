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
            if (CodexManager.Instance != null)
            {
                // 1. Abre el códice con la página correspondiente[cite: 3]
                CodexManager.Instance.OpenCodex(pageIndexToUnlock);

                // 2. Notifica al CodexManager que se ha recogido una página para actualizar el HUD
                CodexManager.Instance.PageCollected();
            }

            // Destruye la página física del suelo para que desaparezca al tocarla[cite: 3]
            Destroy(gameObject);
        }
    }
}