using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyGroup
{
    public GameObject enemyPrefab;
    public int count;
}

[CreateAssetMenu(fileName = "NuevaOleada", menuName = "Horda/Wave Data")]
public class WaveData : ScriptableObject
{
    public List<EnemyGroup> enemyGroups;
    public float spawnInterval = 1.0f;
    public float timeBeforeWave = 3.0f;

    [Header("Configuración de Duración")]
    public float waveDuration = 60.0f;
}