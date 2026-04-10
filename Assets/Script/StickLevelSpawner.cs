using System.Collections.Generic;
using UnityEngine;

public class StickLevelSpawner : MonoBehaviour
{
    [SerializeField] private GameObject stickPrefab;
    [SerializeField] private LevelData levelData;
    [SerializeField] private ColorMaterialConfig colorConfig;

    [SerializeField] private float horizontalSpacing = 1.5f;
    [SerializeField] private float stickSpacing = 1.5f;
    [SerializeField] private GameObject disappearParticlePrefab;
    [SerializeField] private Vector3 startPosition = new Vector3(0f, 0f, 0f);

    [SerializeField] private List<GameObject> spawnedSticks = new List<GameObject>();

    // Unity'nin otomatik çağırdığı ilk frame metodudur. Seviyedeki tüm sopa kolonlarını sahneye yerleştirir.
    private void Start()
    {
        SpawnSticks();
    }

    // LevelData'daki stickColumns listesini okur.
    // Her kolon için boş bir GameObject oluşturur, StickColumnController component'i ekler ve Init() ile başlatır.
    // Kolonlar yatay olarak ortaya hizalanmış şekilde eşit aralıklarla yerleştirilir.
    // disappearParticlePrefab her kolona iletilir — sopa yok olduğunda particle orada spawn olur.
    private void SpawnSticks()
    {
        if (levelData == null || levelData.stickColumns.Count == 0) return;

        int count = levelData.stickColumns.Count;
        float totalWidth = (count - 1) * horizontalSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = startPosition + new Vector3(startX + i * horizontalSpacing, 0f, 0f);

            GameObject columnRoot = new GameObject("Column_" + i);
            columnRoot.transform.position = spawnPos;

            StickColumnController column = columnRoot.AddComponent<StickColumnController>();
            column.Init(levelData.stickColumns[i].stickColors, stickPrefab, colorConfig, stickSpacing, disappearParticlePrefab);

            spawnedSticks.Add(columnRoot);
        }
    }
}
