using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [Header("Configuración de Escudo")]
    public float maxShield = 100f;
    public float currentShield;

    [Header("Mitigación de Daño Base")]
    [Range(0f, 1f)]
    public float shieldMitigation = 0.70f;

    [Header("Referencias")]
    public PlayerHealth playerHealth;

    void Start()
    {
        currentShield = maxShield;

        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        if (playerHealth != null && playerHealth.isDead) return;

        float incomingDamage = amount;

        if (ShopManager.Instance != null)
        {
            var passive = ShopManager.Instance.currentPassive;

            if (passive == PassiveType.Frenzy && ShopManager.Instance.frenzyActive)
            {
                incomingDamage *= ShopManager.Instance.frenzyIncomingDamageMultiplier;
            }
            else if (passive == PassiveType.Greed)
            {
                incomingDamage *= ShopManager.Instance.greedIncomingDamageMultiplier;
            }
        }

        if (ShopManager.Instance != null && ShopManager.Instance.currentPassive == PassiveType.Tank)
        {
            if (currentShield > 0f)
            {
                float damageToShield = incomingDamage * 0.20f;
                currentShield -= damageToShield;

                if (currentShield <= 0f)
                {
                    currentShield = 0f;
                    Debug.Log("¡El escudo del Tanque se ha roto!");
                }
                return;
            }
            else
            {
                float damageToHealth = incomingDamage * 0.30f;
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageToHealth);
                }
                return;
            }
        }

        if (currentShield > 0f)
        {
            float damageToShield = incomingDamage * shieldMitigation;
            float damageToHealth = incomingDamage * (1f - shieldMitigation);

            currentShield -= damageToShield;

            if (currentShield <= 0f)
            {
                float excessDamage = -currentShield;
                currentShield = 0f;

                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageToHealth + excessDamage);
                }
            }
            else
            {
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageToHealth);
                }
            }
        }
        else
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(incomingDamage);
            }
        }
    }

    public void RechargeShield(float amount)
    {
        currentShield += amount;
        currentShield = Mathf.Clamp(currentShield, 0f, maxShield);
    }
}