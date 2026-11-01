using UnityEngine;

public class CodexCollectible : MonoBehaviour
{
    [Header("Codex Configuration")]
    [Tooltip("Número de la página en la lista del CodexManager. 0 = Primer texto, 1 = Segundo texto, etc.")]
    public int pageIndexToUnlock;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (CodexManager.Instance != null)
            {
                CodexManager.Instance.OpenCodex(pageIndexToUnlock);
                CodexManager.Instance.PageCollected();
            }

            Destroy(gameObject);
        }
    }
}