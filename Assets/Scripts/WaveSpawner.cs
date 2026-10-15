using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private GameManager gameManager; // Referencia para controlar el GameOver

    [Header("Configuración de Olas y Spawns")]
    [SerializeField] private List<WaveData> waves;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Configuración de la Tienda (Estilo Killing Floor)")]
    [SerializeField] private GameObject shopPodPrefab;
    [SerializeField] private Transform[] shopSpawnPoints;
    private GameObject activeShopPod;

    [Header("Configuración de Objetos Recogibles / Consumibles")]
    [SerializeField] private List<GameObject> consumablePrefabs; // Lista de objetos que pueden aparecer
    [SerializeField] private Transform[] itemSpawnPoints;        // Puntos específicos para los objetos
    [SerializeField] private int itemsToSpawnPerWave = 3;        // Cantidad de objetos que aparecerán por ronda
    private List<GameObject> activeItems = new List<GameObject>(); // Instancias activas en el mapa

    private int currentWaveIndex = 0;
    private int activeEnemiesCount = 0;
    private int totalEnemiesInCurrentWave = 0;
    private int enemiesKilledInCurrentWave = 0;
    private bool isSpawning = false;
    private float waveTimer = 0f;
    private bool isCountingDown = false;

    private float activeWaveRemainingTime = 0f;
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
        if (waves.Count > 0)
        {
            StartCoroutine(StartNextWave());
        }
        else
        {
            Debug.LogWarning("No se asignaron WaveData al WaveSpawner.");
        }
    }

    private void Update()
    {
        if (isWaveActive)
        {
            activeWaveRemainingTime -= Time.deltaTime;

            // Al llegar a 0 se detiene la ronda y activa el GameOver
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
        // Omitir la espera antes de iniciar la oleada
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
                gameManager.Win();
            }
            yield break;
        }

        WaveData currentWave = waves[currentWaveIndex];

        // --- FASE DE DESCANSO / TIENDA ---
        waveTimer = currentWave.timeBeforeWave;
        isCountingDown = true;
        isWaveActive = false;

        SpawnShopPod();
        SpawnConsumableItems(); // Aparecen los objetos al empezar la fase de descanso/preparación

        while (waveTimer > 0)
        {
            waveTimer -= Time.deltaTime;
            yield return null;
        }
        isCountingDown = false;

        CloseAndDespawnShop();
        // Opcional: si quieres que los objetos no recogidos desaparezcan al iniciar la ronda, descomenta la siguiente línea:
        // ClearActiveItems(); 

        // --- FASE DE COMBATE ---
        isWaveActive = true;
        activeWaveRemainingTime = currentWave.waveDuration;

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

    // --- MÉTODOS PARA OBJETOS CONSUMIBLES ---

    private void SpawnConsumableItems()
    {
        // Comprobación de seguridad: si no hay prefabs configurados o la matriz de puntos está vacía o es nula, sale sin hacer nada.
        if (consumablePrefabs == null || consumablePrefabs.Count == 0 || itemSpawnPoints == null || itemSpawnPoints.Length == 0)
            return;

        // Limpiamos la lista por si acaso quedó algo de la ronda anterior
        ClearActiveItems();

        List<Transform> availablePoints = new List<Transform>(itemSpawnPoints);
        int spawnCount = Mathf.Min(itemsToSpawnPerWave, availablePoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int randomIndexPoint = Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[randomIndexPoint];
            availablePoints.RemoveAt(randomIndexPoint); // Evita duplicar objetos en el mismo punto

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

        // Mezclar aleatoriamente el orden de los enemigos
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
            isWaveActive = false; // Desactiva la cuenta regresiva antes de que llegue a 0

            Debug.Log("¡Ola " + (currentWaveIndex + 1) + " completada!");

            currentWaveIndex++;
            StartCoroutine(StartNextWave());
        }
    }
}