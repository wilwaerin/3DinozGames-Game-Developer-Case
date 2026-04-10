using System.Collections.Generic;
using UnityEngine;

public class ChainController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject ringPrefab;
    [SerializeField] private Transform visualRoot;

    [Header("Layout")]
    [SerializeField] private float ringSpacing = 0.5f;

    private List<RingColor> ringColorList;
    private readonly List<GameObject> rings = new List<GameObject>();

    // Dışarıdan renk listesi alır ve saklar. Start() çalışınca bu liste üzerinden ring'ler oluşturulur.
    // ChainSceneSetup tarafından prefab instantiate edildikten hemen sonra çağrılır.
    public void Init(List<RingColor> colors)
    {
        ringColorList = colors;
    }

    // Unity'nin otomatik çağırdığı ilk frame metodudur.
    // Init() ile renk listesi atanmışsa ring'leri spawn eder.
    private void Start()
    {
        if (ringColorList != null && ringColorList.Count > 0)
            SpawnRings();
    }

    // ringColorList'teki her renk için ring prefabını visualRoot altına oluşturur.
    // Her ring'e pozisyon, rotasyon, renk ve sallanma hareketi (IdleSway) atar.
    // Son olarak BoxCollider yüksekliğini ring sayısına eşitler — tıklama alanı ring sayısına göre büyür.
    private void SpawnRings()
    {
        rings.Clear();

        for (int i = 0; i < ringColorList.Count; i++)
        {
            GameObject newRing = Instantiate(ringPrefab, visualRoot);

            newRing.name = $"Ring_{i}";
            rings.Add(newRing);

            float yRotation = (i % 2 == 0) ? 50f : 120f;
            newRing.transform.localRotation = Quaternion.Euler(0f, yRotation, 90f);
            newRing.transform.localPosition = new Vector3(0f, -i * ringSpacing, 0f);

            RingController ringController = newRing.GetComponent<RingController>();
            if (ringController != null)
            {
                ringController.SetColor(ringColorList[i]);
                ringController.StartIdleSway(i);
            }
        }

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Vector3 size = col.size;
            size.y = ringColorList.Count;
            col.size = size;
        }
    }

    // Listenin sonundaki ring'i (zincirin en altındaki) serbest bırakır.
    // Aynı renkteki uygun bir sopa arar; sopa yoksa ring zincirde kalır, bir şey olmaz.
    // Sopa bulunursa ring parent'tan kopar, sopaya doğru hareket başlar ve BoxCollider küçülür.
    public void ReleaseBottomRing()
    {
        if (rings.Count == 0)
        {
            return;
        }

        GameObject bottomRingObject = rings[rings.Count - 1];
        RingController bottomRing = bottomRingObject.GetComponent<RingController>();

        if (bottomRing == null)
        {
            Debug.LogWarning("RingController bulunamadı.");
            return;
        }

        StickController validStick = FindValidStick(bottomRing.RingColor);

        if (validStick == null)
        {
            Debug.Log("Uygun stick yok, ring zincirde kalıyor.");
            return;
        }

        rings.RemoveAt(rings.Count - 1);
        bottomRing.transform.SetParent(null);

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            Vector3 size = col.size;
            size.y = Mathf.Max(0f, size.y - 1f);
            col.size = size;
        }

        Vector3 targetPos = validStick.GetNextSlotPosition();
        validStick.ReserveSlot();
        bottomRing.MoveTo(targetPos, validStick);

        Debug.Log(bottomRing.name + " -> " + validStick.name);
    }

    // Sahnedeki tüm StickController'ları tarar.
    // Verilen renkle uyuşan, aktif ve dolu olmayan sopalar arasından
    // en solda (en küçük X pozisyonu) olanı seçip döner. Uygun sopa yoksa null döner.
    private StickController FindValidStick(RingColor ringColor)
    {
        StickController[] allSticks = FindObjectsOfType<StickController>();

        StickController bestStick = null;
        float bestX = float.MaxValue;

        for (int i = 0; i < allSticks.Length; i++)
        {
            StickController stick = allSticks[i];

            if (!stick.CanAccept(ringColor))
                continue;

            float x = stick.transform.position.x;

            if (x < bestX)
            {
                bestX = x;
                bestStick = stick;
            }
        }

        return bestStick;
    }

    // Zincirde hiç ring kalmadıysa true döner.
    // GameManager her frame bu metodu çağırarak tüm zincirlerin boşalıp boşalmadığını kontrol eder (kazanma koşulu).
    public bool IsEmpty()
    {
        return rings.Count == 0;
    }

    // Zincirdeki ring sayısını döner.
    public int GetRingCount()
    {
        return rings.Count;
    }
}