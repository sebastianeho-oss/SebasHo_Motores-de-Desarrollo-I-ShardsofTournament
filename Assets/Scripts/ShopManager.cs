using UnityEngine;
using TMPro;
using System.Collections;

// Enumerador para definir la pasiva equipada (Solo una a la vez)
public enum PassiveType { None, Frenzy, Tank, Greed }

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Economía")]
    [SerializeField] private int coins = 0;

    [Header("Tienda UI (Menús y Paneles)")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject mainShopButtons;
    [SerializeField] private GameObject attributesSectionPanel;
    [SerializeField] private GameObject passiveSectionPanel;
    [SerializeField] private GameObject resupplySectionPanel; // Nuevo panel de reabastecimiento

    [Header("Textos UI General")]
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("UI Velocidad")]
    [SerializeField] private TextMeshProUGUI speedLevelText;
    [SerializeField] private TextMeshProUGUI speedPriceText;

    [Header("UI Vida (+10 pts máx)")]
    [SerializeField] private TextMeshProUGUI healthLevelText;
    [SerializeField] private TextMeshProUGUI healthPriceText;

    [Header("UI Escudo (+10 pts máx)")]
    [SerializeField] private TextMeshProUGUI shieldLevelText;
    [SerializeField] private TextMeshProUGUI shieldPriceText;

    [Header("Efectos de UI en Juego")]
    [SerializeField] private CoinUI coinUI;

    [Header("Referencias de Jugador y Mejoras")]
    [SerializeField] private MovementPlayerController playerMovement;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerShield playerShield;
    [SerializeField] private PlayerAmmoInventory ammoInventory; // Inventario de munición
    [SerializeField] private WeaponSwitcher weaponSwitcher; // Selector de armas activas

    [Header("Límites y Niveles (Máximo 3)")]
    [SerializeField] private int maxSpeedLevel = 3;
    private int speedLevel = 0;

    [SerializeField] private int maxHealthLevels = 3;
    private int healthLevel = 0;

    [SerializeField] private int maxShieldLevels = 3;
    private int shieldLevel = 0;

    [Header("--- SISTEMA DE PASIVAS ---")]
    public PassiveType currentPassive = PassiveType.None;

    [Header("Precios de Pasivas (Monedas)")]
    [SerializeField] private int frenzyPrice = 300;
    [SerializeField] private int tankPrice = 400;
    [SerializeField] private int greedPrice = 500;

    [Header("Pasiva 1: Frenesí")]
    public float frenzyDuration = 5f;
    public bool frenzyActive = false;
    public float frenzyIncomingDamageMultiplier = 1.5f;
    public float frenzyRecoilMultiplier = 1.6f;
    private Coroutine frenzyCoroutine;

    [Header("Pasiva 2: Tanque")]
    public int tankKillStacks = 0;
    public int maxTankStacks = 10;
    public float tankStackDuration = 20f; // Tiempo de duración de las acumulaciones (20 segundos)
    private Coroutine tankTimerCoroutine;

    [Header("Pasiva 3: Codicia")]
    public float greedIncomingDamageMultiplier = 1.8f;

    private bool isShopOpen = false;

    public int Coins => coins;
    public bool IsShopOpen => isShopOpen;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentPassive = PassiveType.None;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (attributesSectionPanel != null) attributesSectionPanel.SetActive(false);
        if (passiveSectionPanel != null) passiveSectionPanel.SetActive(false);
        if (resupplySectionPanel != null) resupplySectionPanel.SetActive(false);
        if (mainShopButtons != null) mainShopButtons.SetActive(true);

        // Auto-asignación de referencias si no se arrastraron en el Inspector
        if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerShield == null) playerShield = FindAnyObjectByType<PlayerShield>();
        if (ammoInventory == null) ammoInventory = FindAnyObjectByType<PlayerAmmoInventory>();
        if (weaponSwitcher == null) weaponSwitcher = FindAnyObjectByType<WeaponSwitcher>();

        UpdateShopUI();
    }

    public void AddCoins(int amount)
    {
        // PASIVA 3: Duplica las monedas obtenidas (+100%)
        if (currentPassive == PassiveType.Greed)
        {
            amount *= 2;
        }

        coins += amount;
        UpdateShopUI();

        if (coinUI != null)
        {
            coinUI.PlayCoinGainedEffect(amount);
        }

        Debug.Log("Monedas obtenidas: +" + amount + ". Total: " + coins);
    }

    #region Lógica de Pasivas

    public void EquipPassive1_Frenzy() => BuyAndEquipPassive(PassiveType.Frenzy, frenzyPrice);
    public void EquipPassive2_Tank() => BuyAndEquipPassive(PassiveType.Tank, tankPrice);
    public void EquipPassive3_Greed() => BuyAndEquipPassive(PassiveType.Greed, greedPrice);

    public void BuyAndEquipPassive(PassiveType newPassive, int price)
    {
        if (currentPassive == newPassive)
        {
            Debug.Log("Ya tienes esta pasiva equipada.");
            return;
        }

        if (coins < price)
        {
            Debug.Log("No tienes suficientes monedas para equipar: " + newPassive + ". Costo: " + price);
            return;
        }

        coins -= price;

        // Limpiar temporadores y acumulaciones anteriores
        ResetPassiveStates();

        currentPassive = newPassive;

        Debug.Log($"¡Pasiva {currentPassive} equipada por {price} monedas! Total restante: {coins}");
        UpdateShopUI();
    }

    private void ResetPassiveStates()
    {
        frenzyActive = false;
        if (frenzyCoroutine != null) StopCoroutine(frenzyCoroutine);

        tankKillStacks = 0;
        if (tankTimerCoroutine != null) StopCoroutine(tankTimerCoroutine);
    }

    // Se invoca cuando muere cualquier enemigo
    public void OnEnemyKilled()
    {
        if (currentPassive == PassiveType.Frenzy)
        {
            if (frenzyCoroutine != null) StopCoroutine(frenzyCoroutine);
            frenzyCoroutine = StartCoroutine(FrenzyRoutine());
        }
        else if (currentPassive == PassiveType.Tank)
        {
            if (tankKillStacks < maxTankStacks)
            {
                tankKillStacks++;
            }

            // Reiniciar el temporizador de 20 segundos cada vez que mata un enemigo
            if (tankTimerCoroutine != null) StopCoroutine(tankTimerCoroutine);
            tankTimerCoroutine = StartCoroutine(TankStackTimerRoutine());

            Debug.Log($"Pasiva Tanque: Acumulación {tankKillStacks}/{maxTankStacks} (+{tankKillStacks * 10}% de daño). Temporizador de 20s reiniciado.");
        }
    }

    private IEnumerator FrenzyRoutine()
    {
        frenzyActive = true;
        Debug.Log("¡Pasiva Frenesí ACTIVADA!");
        yield return new WaitForSeconds(frenzyDuration);
        frenzyActive = false;
        Debug.Log("Pasiva Frenesí DESACTIVADA.");
    }

    private IEnumerator TankStackTimerRoutine()
    {
        yield return new WaitForSeconds(tankStackDuration);
        tankKillStacks = 0;
        Debug.Log("Pasiva Tanque: Expiraron los 20 segundos sin matar enemigos. Las acumulaciones de daño volvieron a 0.");
    }

    public float GetPlayerDamageMultiplier()
    {
        if (currentPassive == PassiveType.Tank)
        {
            // Cada acumulación otorga +10% de daño extra (0.10f)
            return 1f + (tankKillStacks * 0.10f);
        }
        return 1f;
    }

    public float GetReloadTimeMultiplier()
    {
        if (currentPassive == PassiveType.Frenzy && frenzyActive)
        {
            return 0.70f;
        }
        return 1f;
    }

    #endregion

    public void OpenShop()
    {
        if (shopPanel == null) return;

        isShopOpen = true;
        shopPanel.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowMainShopMenu();
        UpdateShopUI();
    }

    public void CloseShop()
    {
        if (shopPanel == null) return;

        isShopOpen = false;
        shopPanel.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ForceCloseShop()
    {
        if (shopPanel != null && shopPanel.activeSelf)
        {
            isShopOpen = false;
            shopPanel.SetActive(false);
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("El tiempo de la tienda expiró. Cerrando menú automáticamente.");
        }
    }

    #region Navegación de Paneles

    public void ShowMainShopMenu()
    {
        if (mainShopButtons != null) mainShopButtons.SetActive(true);
        if (attributesSectionPanel != null) attributesSectionPanel.SetActive(false);
        if (passiveSectionPanel != null) passiveSectionPanel.SetActive(false);
        if (resupplySectionPanel != null) resupplySectionPanel.SetActive(false);
    }

    public void OpenAttributesSection()
    {
        if (mainShopButtons != null) mainShopButtons.SetActive(false);
        if (attributesSectionPanel != null) attributesSectionPanel.SetActive(true);
        if (passiveSectionPanel != null) passiveSectionPanel.SetActive(false);
        if (resupplySectionPanel != null) resupplySectionPanel.SetActive(false);
    }

    public void OpenPassiveSection()
    {
        if (mainShopButtons != null) mainShopButtons.SetActive(false);
        if (attributesSectionPanel != null) attributesSectionPanel.SetActive(false);
        if (passiveSectionPanel != null) passiveSectionPanel.SetActive(true);
        if (resupplySectionPanel != null) resupplySectionPanel.SetActive(false);
    }

    public void OpenResupplySection()
    {
        if (mainShopButtons != null) mainShopButtons.SetActive(false);
        if (attributesSectionPanel != null) attributesSectionPanel.SetActive(false);
        if (passiveSectionPanel != null) passiveSectionPanel.SetActive(false);
        if (resupplySectionPanel != null) resupplySectionPanel.SetActive(true);
    }

    #endregion

    #region Lógica de Mejoras (Atributos)

    public void BuySpeedUpgrade()
    {
        if (speedLevel >= maxSpeedLevel) return;

        int cost = 100 * (speedLevel + 1);
        if (coins < cost) return;

        coins -= cost;
        speedLevel++;

        if (playerMovement != null)
        {
            playerMovement.walkSpeed *= 1.30f;
            playerMovement.sprintSpeed *= 1.30f;
        }

        UpdateShopUI();
    }

    public void BuyHealthUpgrade()
    {
        if (healthLevel >= maxHealthLevels) return;

        int cost = 75 * (healthLevel + 1);
        if (coins < cost) return;

        coins -= cost;
        healthLevel++;

        if (playerHealth != null)
        {
            playerHealth.maxHealth += 10f;
            playerHealth.Heal(10f);
        }

        UpdateShopUI();
    }

    public void BuyShieldUpgrade()
    {
        if (shieldLevel >= maxShieldLevels) return;

        int cost = 75 * (shieldLevel + 1);
        if (coins < cost) return;

        coins -= cost;
        shieldLevel++;

        if (playerShield != null)
        {
            playerShield.maxShield += 10f;
            playerShield.RechargeShield(10f);
        }

        UpdateShopUI();
    }

    #endregion

    #region Lógica de Reabastecimiento Flexible (Parcial o Total)

    // --- BOTÓN 1: Reabastecer Salud Flexible ---
    public void BuyResupplyHealth()
    {
        if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth == null) return;

        if (playerHealth.currentHealth >= playerHealth.maxHealth)
        {
            Debug.Log("La salud ya está llena.");
            return;
        }

        if (coins <= 0)
        {
            Debug.Log("No tienes monedas.");
            return;
        }

        float missingHealth = playerHealth.maxHealth - playerHealth.currentHealth;

        // Costo ideal para rellenar todo lo que falta (1 moneda por cada 1% faltante)
        int idealCost = Mathf.CeilToInt(100f * (missingHealth / playerHealth.maxHealth));

        int coinsToSpend;
        float healthRestored;

        if (coins >= idealCost)
        {
            // Tiene suficientes monedas para rellenar al 100% lo que falta
            coinsToSpend = idealCost;
            healthRestored = missingHealth;
        }
        else
        {
            // No le alcanza para el 100%, gasta todas las monedas disponibles proporcionalmente
            coinsToSpend = coins;
            // Cada moneda equivale a un 1% del total máximo de salud que se puede recuperar
            float healthPerCoin = playerHealth.maxHealth / 100f;
            healthRestored = coinsToSpend * healthPerCoin;

            // Asegurar que no cure más de lo que realmente falta
            if (healthRestored > missingHealth) healthRestored = missingHealth;
        }

        coins -= coinsToSpend;
        playerHealth.Heal(healthRestored);

        UpdateShopUI();
        Debug.Log($"¡Salud reabastecida parcialmente! Se gastaron {coinsToSpend} monedas y se recuperaron {healthRestored} puntos de salud.");
    }

    // --- BOTÓN 2: Reabastecer Escudo Flexible ---
    public void BuyResupplyShield()
    {
        if (playerShield == null) playerShield = FindAnyObjectByType<PlayerShield>();
        if (playerShield == null) return;

        if (playerShield.currentShield >= playerShield.maxShield)
        {
            Debug.Log("El escudo ya está lleno.");
            return;
        }

        if (coins <= 0)
        {
            Debug.Log("No tienes monedas.");
            return;
        }

        float missingShield = playerShield.maxShield - playerShield.currentShield;
        int idealCost = Mathf.CeilToInt(100f * (missingShield / playerShield.maxShield));

        int coinsToSpend;
        float shieldRestored;

        if (coins >= idealCost)
        {
            coinsToSpend = idealCost;
            shieldRestored = missingShield;
        }
        else
        {
            coinsToSpend = coins;
            float shieldPerCoin = playerShield.maxShield / 100f;
            shieldRestored = coinsToSpend * shieldPerCoin;

            if (shieldRestored > missingShield) shieldRestored = missingShield;
        }

        coins -= coinsToSpend;
        playerShield.RechargeShield(shieldRestored);

        UpdateShopUI();
        Debug.Log($"¡Escudo reabastecido parcialmente! Se gastaron {coinsToSpend} monedas y se recuperaron {shieldRestored} puntos de escudo.");
    }

    // --- BOTÓN 3: Reabastecer Munición Flexible ---
    public void BuyResupplyActiveWeaponAmmo()
    {
        if (weaponSwitcher == null) weaponSwitcher = FindAnyObjectByType<WeaponSwitcher>();
        if (ammoInventory == null) ammoInventory = FindAnyObjectByType<PlayerAmmoInventory>();

        if (weaponSwitcher == null || weaponSwitcher.selectedWeapon == -1)
        {
            Debug.Log("No hay ninguna arma activa seleccionada.");
            return;
        }

        Transform activeWeaponTransform = weaponSwitcher.transform.GetChild(weaponSwitcher.selectedWeapon);
        if (activeWeaponTransform == null || !activeWeaponTransform.gameObject.activeSelf)
        {
            Debug.Log("No se encontró el objeto del arma activa.");
            return;
        }

        WeaponAmmo weaponAmmo = activeWeaponTransform.GetComponent<WeaponAmmo>();
        if (weaponAmmo == null)
        {
            Debug.Log("El arma activa no tiene el componente WeaponAmmo.");
            return;
        }

        AmmoType activeAmmoType = weaponAmmo.ammoType;

        int currentMagazine = weaponAmmo.currentAmmo;
        int maxMagazine = weaponAmmo.magazineSize;

        int currentReserve = (ammoInventory != null) ? ammoInventory.GetReserveAmmo(activeAmmoType) : 0;
        int maxReserve = (ammoInventory != null) ? ammoInventory.GetMaxReserveAmmo(activeAmmoType) : 0;

        int totalCurrentAmmo = currentMagazine + currentReserve;
        int totalMaxAmmo = maxMagazine + maxReserve;

        if (totalCurrentAmmo >= totalMaxAmmo)
        {
            Debug.Log("La munición del arma activa ya está completamente al máximo.");
            return;
        }

        if (coins <= 0)
        {
            Debug.Log("No tienes monedas.");
            return;
        }

        int ammoNeeded = totalMaxAmmo - totalCurrentAmmo;
        int idealCost = Mathf.CeilToInt(100f * ((float)ammoNeeded / totalMaxAmmo));

        int coinsToSpend;
        int ammoToRestore;

        if (coins >= idealCost)
        {
            coinsToSpend = idealCost;
            ammoToRestore = ammoNeeded;
        }
        else
        {
            coinsToSpend = coins;
            // Cada moneda otorga un 1% del total máximo de munición combinada del arma
            float ammoPerCoin = (float)totalMaxAmmo / 100f;
            ammoToRestore = Mathf.FloorToInt(coinsToSpend * ammoPerCoin);

            if (ammoToRestore < 1 && coinsToSpend > 0) ammoToRestore = 1; // Asegurar al menos 1 bala si gasta monedas
            if (ammoToRestore > ammoNeeded) ammoToRestore = ammoNeeded;
        }

        coins -= coinsToSpend;

        // Distribuir primero las balas al cargador actual hasta llenarlo, y el sobrante a la reserva
        int spaceInMagazine = maxMagazine - currentMagazine;
        if (ammoToRestore <= spaceInMagazine)
        {
            weaponAmmo.currentAmmo += ammoToRestore;
        }
        else
        {
            weaponAmmo.currentAmmo = maxMagazine;
            int remainingAmmo = ammoToRestore - spaceInMagazine;
            if (ammoInventory != null)
            {
                ammoInventory.AddAmmo(activeAmmoType, remainingAmmo);
            }
        }

        UpdateShopUI();
        Debug.Log($"¡Munición reabastecida parcialmente! Se gastaron {coinsToSpend} monedas y se añadieron {ammoToRestore} balas.");
    }

    #endregion

    private void UpdateShopUI()
    {
        if (coinsText != null)
            coinsText.text = "Monedas: " + coins;

        if (speedLevelText != null)
            speedLevelText.text = "Nivel: " + speedLevel + " / " + maxSpeedLevel;

        if (speedPriceText != null)
            speedPriceText.text = (speedLevel >= maxSpeedLevel) ? "MÁXIMO" : "Costo: " + (100 * (speedLevel + 1));

        if (healthLevelText != null)
            healthLevelText.text = "Nivel: " + healthLevel + " / " + maxHealthLevels;

        if (healthPriceText != null)
            healthPriceText.text = (healthLevel >= maxHealthLevels) ? "MÁXIMO" : "Costo: " + (75 * (healthLevel + 1));

        if (shieldLevelText != null)
            shieldLevelText.text = "Nivel: " + shieldLevel + " / " + maxShieldLevels;

        if (shieldPriceText != null)
            shieldPriceText.text = (shieldLevel >= maxShieldLevels) ? "MÁXIMO" : "Costo: " + (75 * (shieldLevel + 1));
    }
}