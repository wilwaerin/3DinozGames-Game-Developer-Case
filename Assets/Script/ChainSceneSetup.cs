using UnityEngine;
using System.Collections.Generic;

public class ChainSceneSetup : MonoBehaviour
{
    [SerializeField] private GameObject chainRootPrefab;
    [SerializeField] private LevelData levelData;

    [SerializeField] private float horizontalSpacing = 1.5f;
    [SerializeField] private Vector3 startPosition = new Vector3(0f, 4f, 0f);

    [SerializeField] private List<GameObject> spawnedChains = new List<GameObject>();

    // Unity'nin otomatik çağırdığı ilk frame metodudur. Seviyedeki tüm chain'leri sahneye yerleştirir.
    private void Start()
    {
        SpawnChains();
    }

    // LevelData'daki chain listesini okur ve her chain için chainRootPrefab'ı instantiate eder.
    // Chain'ler yatay olarak ortaya hizalanmış şekilde eşit aralıklarla yerleştirilir.
    // Her chain'deki ChainController'a renk listesi Init() ile iletilir.
    private void SpawnChains()
    {
        if (levelData == null || levelData.chains.Count == 0) return;

        int count = levelData.chains.Count;
        float totalWidth = (count - 1) * horizontalSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = startPosition + new Vector3(startX + i * horizontalSpacing, 0f, 0f);

            GameObject newChain = Instantiate(chainRootPrefab, spawnPos, Quaternion.identity);
            newChain.name = "Chain_" + i;

            ChainController chainController = newChain.GetComponent<ChainController>();
            if (chainController != null)
                chainController.Init(levelData.chains[i].ringColors);

            spawnedChains.Add(newChain);
        }
    }
}