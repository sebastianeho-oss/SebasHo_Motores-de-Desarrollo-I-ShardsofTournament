using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private GameManager gameManager;

    [Header("Configuración de Olas y Spawns")]
    [SerializeField] private List<WaveData> waves;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Configuración de la Tienda (Estilo Killing Floor)")]
    [SerializeField] private GameObject shopPodPrefab;
    [SerializeField] private Transform[] shopSpawnPoints;
    private GameObject activeShopPod;

    [Header("Configuración de Objetos Recogibles / Consumibles")]
    [SerializeField] private List<GameObject> consumablePrefabs;
    [SerializeField] private Transform[] itemSpawnPoints;
    [SerializeField] private int itemsToSpawnPerWave = 3;
    private List<GameObject> activeItems = new List<GameObject>();

    [Header("Configuración de Páginas de Códice")]
    [SerializeField] private List<GameObject> codexPagesPrefabs;
    [SerializeField] private Transform[] codexSpawnPoints;
    private List<GameObject> remainingCodexPages = new List<GameObject>();
    private GameObject activeCodexInstance;

    private int currentWaveIndex = 0;
    private int activeEnemiesCount = 0;
    private int totalEnemiesInCurrentWave = 0;
    private int enemiesKilledInCurrentWave = 0;
    private bool isSpawning = false;
    private float waveTimer = 0f;
    private bool isCountingDown = false;

    private float activeWaveRemainingTime = 0f;
    private float currentWaveTotalDuration = 0f;
    private bool isWaveActive = false;

    private PlayerInputActions inputActions;

    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int TotalWaves => waves.Count;
    public int EnemiesRemaining => activeEnemiesCount;
    public int EnemiesKilled => enemiesKilledInCurrentWave;
    public int TotalEnemiesInWave => totalEnemiesInCurrentWave;
    public float WaveTimer => waveTimer;
    public bool IsCountingDown => isCountingDown;
    public float ActiveWaveRemainingTime => activeWaveRemainingTime;
    public float CurrentWaveTotalDuration => currentWaveTotalDuration;
    public bool IsWaveActive => isWaveActive;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.PassTime.performed += _ => SkipCountdown();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        ResetCodexPages();

        if (waves.Count > 0)
        {
            StartCoroutine(StartNextWave());
        }
        else
        {
            Debug.LogWarning("No se asignaron WaveData al WaveSpawner.");
        }
    }

    private void ResetCodexPages()
    {
        if (codexPagesPrefabs != null)
        {
            remainingCodexPages = new List<GameObject>(codexPagesPrefabs);
            remainingCodexPages.RemoveAll(page => page == null);
        }
    }

    private void Update()
    {
        if (isWaveActive)
        {
            activeWaveRemainingTime -= Time.deltaTime;

            if (activeWaveRemainingTime <= 0f)
            {
                activeWaveRemainingTime = 0f;
                isWaveActive = false;

                if (gameManager != null)
                {
                    gameManager.GameOver();
                }
                else
                {
                    Debug.LogError("¡No se ha asignado el GameManager en el Inspector del WaveSpawner!");
                }
            }
        }
    }

    public void SkipCountdown()
    {
        if (isCountingDown)
        {
            waveTimer = 0f;
        }
    }

    private IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("¡Felicidades! Has sobrevivido a todas las olas.");
            if (gameManager != null)
            {
                // Verificamos si se recolectaron todas las páginas del códice
                bool collectedAllCodex = false;

                if (CodexManager.Instance != null && codexPagesPrefabs != null && codexPagesPrefabs.Count > 0)
                {
                    int totalCodexCount = codexPagesPrefabs.Count;
                    if (CodexManager.Instance.HasCollectedAllPages(totalCodexCount))
                    {
                        collectedAllCodex = true;
                    }
                }

                // Llamada a la pantalla de victoria correspondiente
                if (collectedAllCodex)
                {
                    gameManager.WinCodex();
                }
                else
                {
                    gameManager.Win();
                }
            }
            yield break;
        }

        WaveData currentWave = waves[currentWaveIndex];

        // --- FASE DE DESCANSO / TIENDA ---
        waveTimer = currentWave.timeBeforeWave;
        isCountingDown = true;
        isWaveActive = false;

        SpawnShopPod();
        SpawnConsumableItems();
        TrySpawnCodexPage();

        while (waveTimer > 0)
        {
            waveTimer -= Time.deltaTime;
            yield return null;
        }
        isCountingDown = false;

        CloseAndDespawnShop();

        // --- FASE DE COMBATE ---
        isWaveActive = true;
        activeWaveRemainingTime = currentWave.waveDuration;
        currentWaveTotalDuration = currentWave.waveDuration;

        List<GameObject> enemiesToSpawn = BuildEnemyList(currentWave);
        totalEnemiesInCurrentWave = enemiesToSpawn.Count;
        activeEnemiesCount = totalEnemiesInCurrentWave;
        enemiesKilledInCurrentWave = 0;
        isSpawning = true;

        foreach (GameObject prefab in enemiesToSpawn)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemyInstance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            if (enemyInstance.TryGetComponent<EnemyHealth>(out EnemyHealth health))
            {
                health.OnEnemyDiedWithReward += (coinReward) => OnEnemyKilled(coinReward);
            }
            else
            {
                Debug.LogError("El prefab " + prefab.name + " no tiene el componente EnemyHealth.");
            }

            yield return new WaitForSeconds(currentWave.spawnInterval);
        }

        isSpawning = false;
    }

    private void SpawnShopPod()
    {
        if (shopPodPrefab != null && shopSpawnPoints.Length > 0)
        {
            Transform randomPoint = shopSpawnPoints[Random.Range(0, shopSpawnPoints.Length)];
            activeShopPod = Instantiate(shopPodPrefab, randomPoint.position, randomPoint.rotation);
        }
    }

    private void CloseAndDespawnShop()
    {
        if (shopManager != null)
        {
            shopManager.ForceCloseShop();
        }

        if (activeShopPod != null)
        {
            Destroy(activeShopPod);
        }
    }

    private void SpawnConsumableItems()
    {
        if (consumablePrefabs != null)
        {
            consumablePrefabs.RemoveAll(prefab => prefab == null);
        }

        List<Transform> availablePoints = new List<Transform>();
        if (itemSpawnPoints != null)
        {
            foreach (Transform point in itemSpawnPoints)
            {
                if (point != null) availablePoints.Add(point);
            }
        }

        if (consumablePrefabs == null || consumablePrefabs.Count == 0 || availablePoints.Count == 0)
            return;

        ClearActiveItems();

        int spawnCount = Mathf.Min(itemsToSpawnPerWave, availablePoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int randomIndexPoint = Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[randomIndexPoint];
            availablePoints.RemoveAt(randomIndexPoint);

            GameObject randomPrefab = consumablePrefabs[Random.Range(0, consumablePrefabs.Count)];
            GameObject itemInstance = Instantiate(randomPrefab, spawnPoint.position, spawnPoint.rotation);

            activeItems.Add(itemInstance);
        }
    }

    private void ClearActiveItems()
    {
        foreach (GameObject item in activeItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        activeItems.Clear();
    }

    private void TrySpawnCodexPage()
    {
        List<Transform> availableCodexPoints = new List<Transform>();
        if (codexSpawnPoints != null)
        {
            foreach (Transform point in codexSpawnPoints)
            {
                if (point != null) availableCodexPoints.Add(point);
            }
        }

        if (remainingCodexPages == null || remainingCodexPages.Count == 0 || availableCodexPoints.Count == 0)
            return;

        if (activeCodexInstance != null)
        {
            Destroy(activeCodexInstance);
        }

        int wavesRemaining = waves.Count - currentWaveIndex;
        int pagesRemaining = remainingCodexPages.Count;

        bool shouldSpawn = false;

        if (wavesRemaining <= pagesRemaining)
        {
            shouldSpawn = true;
        }
        else
        {
            float spawnChance = (float)pagesRemaining / wavesRemaining;
            shouldSpawn = Random.value <= spawnChance;
        }

        if (shouldSpawn)
        {
            Transform randomSpawnPoint = availableCodexPoints[Random.Range(0, availableCodexPoints.Count)];

            int randomIndex = Random.Range(0, remainingCodexPages.Count);
            GameObject pageToSpawnPrefab = remainingCodexPages[randomIndex];

            activeCodexInstance = Instantiate(pageToSpawnPrefab, randomSpawnPoint.position, randomSpawnPoint.rotation);
            remainingCodexPages.RemoveAt(randomIndex);
        }
    }

    private List<GameObject> BuildEnemyList(WaveData wave)
    {
        List<GameObject> list = new List<GameObject>();

        foreach (var group in wave.enemyGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                list.Add(group.enemyPrefab);
            }
        }

        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }

        return list;
    }

    private void OnEnemyKilled(int coinReward)
    {
        activeEnemiesCount--;
        enemiesKilledInCurrentWave++;

        if (shopManager != null)
        {
            shopManager.AddCoins(coinReward);
        }

        if (activeEnemiesCount <= 0 && !isSpawning)
        {
            isWaveActive = false;

            Debug.Log("¡Ola " + (currentWaveIndex + 1) + " completada!");

            currentWaveIndex++;
            StartCoroutine(StartNextWave());
        }
    }
}