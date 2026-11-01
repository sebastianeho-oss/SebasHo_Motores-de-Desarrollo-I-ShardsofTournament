using UnityEngine;

public class BossHealth : EnemyHealth
{
    [Header("Ajustes de Jefe")]
    [Tooltip("Porcentaje de daño que recibe el jefe (0.5 = 50%)")]
    [Range(0f, 1f)]
    public float damageReductionFactor = 0.5f;

    void Awake()
    {
        maxHealth = 1000f;
        currentHealth = maxHealth;
    }

    public override void TakeDamage(float amount)
    {
        float finalDamage = amount * damageReductionFactor;
        base.TakeDamage(finalDamage);
    }
}