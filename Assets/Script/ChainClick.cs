using UnityEngine;

public class ChainClick : MonoBehaviour
{
    [SerializeField] private ChainController chainSpawner;

    // Unity'nin component ilk eklendiğinde otomatik çağırdığı metoddur (editorde).
    // ChainController referansı boşsa aynı objedeki component'i otomatik atar, tekrar elle atamak gerekmez.
    private void Reset()
    {
        if (chainSpawner == null)
            chainSpawner = GetComponent<ChainController>();
    }

    // Kullanıcı bu objeye tıkladığında Unity tarafından otomatik çağrılır.
    // ChainController'a zincirin en altındaki ring'i serbest bırakmasını söyler.
    // Objenin BoxCollider'u olması şarttır, yoksa tıklama algılanmaz.
    private void OnMouseDown()
    {
        if (chainSpawner == null)
        {
            Debug.LogWarning("ChainSpawner referansı atanmadı.");
            return;
        }

        chainSpawner.ReleaseBottomRing();
    }
}