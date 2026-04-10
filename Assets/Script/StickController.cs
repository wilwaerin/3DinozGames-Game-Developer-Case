using System.Collections.Generic;
using UnityEngine;

public class StickController : MonoBehaviour
{
    private RingColor stickColor;
    [SerializeField] private int maxCapacity = 3;
    [SerializeField] private List<Transform> slotPoints = new List<Transform>();
    private ColorMaterialConfig colorConfig;

    private List<RingController> ringsOnStick = new List<RingController>();
    private int reservedCount = 0;
    private StickColumnController columnController;
    private bool isActive = false;

    // Sopanın rengini döner.
    public RingColor StickColor => stickColor;

    // Sopanın rengini, materyalini ve bağlı olduğu kolon kontrolcüsünü ayarlar.
    // column null verilirse sopa pasif başlar (ring kabul etmez). İlk sopa aktif, diğerleri pasif başlar.
    public void Init(RingColor color, ColorMaterialConfig config, StickColumnController column = null)
    {
        stickColor = color;
        colorConfig = config;
        columnController = column;
        isActive = column != null;
        ApplyMaterial();
    }

    // ColorMaterialConfig'den sopanın rengine ait Material'ı alır ve parent Renderer'a uygular.
    // Renderer GetComponentInParent ile aranır — sopa prefabında mesh üst objededir.
    private void ApplyMaterial()
    {
        if (colorConfig == null) return;
        Material mat = colorConfig.GetMaterial(stickColor);
        if (mat == null) return;
        Renderer rend = GetComponentInParent<Renderer>();
        if (rend != null)
            rend.material = mat;
    }

    // Bu sopanın verilen renkteki bir ring'i kabul edip edemeyeceğini kontrol eder.
    // Üç koşul birden sağlanmalı: sopa aktif olmalı, renk eşleşmeli, kapasite dolmamış olmalı.
    public bool CanAccept(RingColor ringColor)
    {
        if (!isActive) return false;
        if (ringColor != stickColor) return false;
        if (ringsOnStick.Count + reservedCount >= maxCapacity) return false;
        return true;
    }

    // Bir sonraki boş slot'un dünya pozisyonunu döner.
    // Ring yola çıkmadan önce bu pozisyonu hedef olarak alır. slotPoints listesi yoksa sopanın merkezi döner.
    public Vector3 GetNextSlotPosition()
    {
        int index = ringsOnStick.Count + reservedCount;

        if (index < slotPoints.Count)
            return slotPoints[index].position;

        return transform.position;
    }

    // Sopayı aktif hale getirir ve kolon kontrolcüsünü atar.
    // SlideUpAll() sonunda yeni öne gelen (en üst) sopa için çağrılır.
    public void Activate(StickColumnController column)
    {
        columnController = column;
        isActive = true;
    }

    // Bir sonraki slotu mantıksal olarak rezerve eder.
    // Ring yola çıktığında hemen çağrılır. Böylece ring henüz gelmeden aynı slot başka bir ring'e açık görünmez.
    public void ReserveSlot()
    {
        reservedCount++;
    }

    // Ring gerçekten sopaya ulaştığında çağrılır. Listeye ekler, rezervasyonu düşürür.
    // Kapasite dolunca StickColumnController.OnStickFull() tetikler — yok olma+yeni sopa animasyonu başlar.
    public void AddRing(RingController ring)
    {
        if (!ringsOnStick.Contains(ring))
        {
            ringsOnStick.Add(ring);
            reservedCount = Mathf.Max(0, reservedCount - 1);

            if (ringsOnStick.Count >= maxCapacity && columnController != null)
                columnController.OnStickFull();
        }
    }

    // Sopada şu an bulunan ring sayısını döner. (Aktif olarak bir yerde kullanılmıyor, ileride debug için kalabilir.)
    public int GetCurrentCount()
    {
        return ringsOnStick.Count;
    }

    // Sopanın tamamen dolu olup olmadığını kontrol eder.
    // Hem gerçekten gelen ring'leri hem de rezerve edilmiş ama henüz gelmeyen slot'ları hesaba katar.
    public bool IsFull()
    {
        return ringsOnStick.Count + reservedCount >= maxCapacity;
    }
}