using UnityEngine;

public class BossHealth : EnemyHealth
{
    [Header("Ajustes de Jefe")]
    [Tooltip("Porcentaje de daño que recibe el jefe (0.5 = 50%)")]
    [Range(0f, 1f)]
    public float damageReductionFactor = 0.5f;

    void Awake()
    {
        // Establecer la salud máxima a 1000
        maxHealth = 1000f;
        currentHealth = maxHealth;
    }

    // Sobrescribir el método TakeDamage para reducir el daño recibido
    public override void TakeDamage(float amount)
    {
        // Modificador de daño: solo percibimos el 50%
        float finalDamage = amount * damageReductionFactor;
        base.TakeDamage(finalDamage);
    }
}