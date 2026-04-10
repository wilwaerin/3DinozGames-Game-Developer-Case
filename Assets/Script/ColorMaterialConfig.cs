using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorMaterialConfig", menuName = "Game/Color Material Config")]
public class ColorMaterialConfig : ScriptableObject
{
    [Serializable]
    public struct ColorMaterialEntry
    {
        public RingColor color;
        public Material material;
    }

    [SerializeField] private List<ColorMaterialEntry> entries = new List<ColorMaterialEntry>();

    // Verilen RingColor değerine karşılık gelen Material'ı entries listesinden arar ve döner.
    // Liste sırayla taranarak eşleşen ilk girdi döndürülür. Hiç eşleşme bulunamazsa null döner.
    // RingController ve StickController bu metodu materyallerini uygulamak için kullanır.
    public Material GetMaterial(RingColor color)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].color == color)
                return entries[i].material;
        }
        return null;
    }
}
