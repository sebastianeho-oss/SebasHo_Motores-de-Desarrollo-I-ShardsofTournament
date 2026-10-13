using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Ajustes de Salud y Recompensa")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    [Header("Economía de la Tienda")]
    [SerializeField] private int coinReward = 15;

    public event Action<int> OnEnemyDiedWithReward;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // AQUÍ AGREGAMOS 'virtual' para permitir que los jefes u otros enemigos lo modifiquen
    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Notificar monedas
        OnEnemyDiedWithReward?.Invoke(coinReward);

        // Notificar activación de pasivas al ShopManager
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnEnemyKilled();
        }

        Destroy(gameObject);
    }
}