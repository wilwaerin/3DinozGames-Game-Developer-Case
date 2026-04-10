using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChainData
{
    public List<RingColor> ringColors = new List<RingColor>();
}

[Serializable]
public class StickColumnData
{
    public List<RingColor> stickColors = new List<RingColor>();
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public const int MaxChains = 3;
    public const int MaxColumns = 5;

    [Header("Timer")]
    public float levelTime;

    [Header("Chains")]
    public List<ChainData> chains = new List<ChainData>();

    [Header("Sticks")]
    public List<StickColumnData> stickColumns = new List<StickColumnData>();

    // Unity Editor'de Inspector'da herhangi bir değer değiştirildiğinde otomatik çalışır (sadece editorde, runtime'da çalışmaz).
    // MaxChains ve MaxColumns limitlerini aşan girdişleri otomatik olarak kırpar ve konsola uyarı yazar.
    private void OnValidate()
    {
        if (chains.Count > MaxChains)
        {
            chains.RemoveRange(MaxChains, chains.Count - MaxChains);
            Debug.LogWarning($"[LevelData] Maksimum {MaxChains} chain eklenebilir.");
        }

        if (stickColumns.Count > MaxColumns)
        {
            stickColumns.RemoveRange(MaxColumns, stickColumns.Count - MaxColumns);
            Debug.LogWarning($"[LevelData] Maksimum {MaxColumns} kolon eklenebilir.");
        }
    }
}
