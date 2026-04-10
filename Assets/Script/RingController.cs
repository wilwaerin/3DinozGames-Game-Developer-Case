using System.Collections;
using UnityEngine;

public class RingController : MonoBehaviour
{
    [SerializeField] private RingColor ringColor;
    [SerializeField] private ColorMaterialConfig colorConfig;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    private Coroutine moveCoroutine;
    private Coroutine idleSwayCoroutine;

    [SerializeField] private float swayAmplitude = 8f;
    [SerializeField] private float swayFrequency = 1f;
    [SerializeField] private float moveEndScale = 0.3f;

    // Ring'in rengini döner.
    public RingColor RingColor => ringColor;

    // Ring'in rengini ayarlar ve buna uygun materyali uygular.
    // ChainController.SpawnRings() tarafından her ring oluşturulunca çağrılır.
    public void SetColor(RingColor color)
    {
        ringColor = color;
        ApplyMaterial();
    }

    // ColorMaterialConfig üzerinden ringColor'ın Material'ını alır ve Renderer'a uygular.
    // Config veya material null ise sessizce çıkar, hata fırlatmaz.
    private void ApplyMaterial()
    {
        if (colorConfig == null) return;
        Material mat = colorConfig.GetMaterial(ringColor);
        if (mat == null) return;
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            rend.material = mat;
    }

    // Ring'in zincirde asılı dururken sallanma animasyonunu başlatır.
    // chainIndex faz farkı yaratır, böylece tüm ring'ler aynı anda değil, sıradışlı sallanır.
    // Zaten çalışan bir sway varsa önce onu durdurur.
    public void StartIdleSway(int chainIndex)
    {
        if (idleSwayCoroutine != null)
            StopCoroutine(idleSwayCoroutine);
        idleSwayCoroutine = StartCoroutine(IdleSwayRoutine(chainIndex));
    }

    // Süresiz dönen coroutine. Mathf.Sin ile X ekseninde salınım oluşturur.
    // phase değeri chainIndex'e göre farklılaştırılır — her ring farklı bir noktanın süreklemesinden başlar.
    private IEnumerator IdleSwayRoutine(int chainIndex)
    {
        float phase = chainIndex * 1.1f;
        Quaternion baseRotation = transform.localRotation;

        while (true)
        {
            float angle = Mathf.Sin(Time.time * swayFrequency + phase) * swayAmplitude;
            transform.localRotation = baseRotation * Quaternion.Euler(angle, 0f, 0f);
            yield return null;
        }
    }

    // Ring'i hedef pozisyona hareket ettirir. Önce idle sway'i durdurur.
    // stick parametresi verilirse ring hedefe varınca o sopaya eklenir ve child'a dönüştürülür.
    // Önceden devam eden bir hareket varsa onu iptal eder.
    public void MoveTo(Vector3 target, StickController stick = null)
    {
        if (idleSwayCoroutine != null)
        {
            StopCoroutine(idleSwayCoroutine);
            idleSwayCoroutine = null;
        }

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveRoutine(target, stick));
    }

    // Her frame'de ring'i MoveTowards ile hedefe yaklaştırır.
    // Hareket boyunca scale startScale'ınden moveEndScale'ıne lineer küçülr (0.4 → 0.3 varsayılan).
    // Rotasyon da aynı anda düzeltilir (Lerp ile). Hedefe ulaşınca stick.AddRing() çağrılır.
    private IEnumerator MoveRoutine(Vector3 target, StickController stick)
    {
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, 90f, 0f);
        float totalDistance = Vector3.Distance(transform.position, target);
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one * moveEndScale;

        while (Vector3.Distance(transform.position, target) >= 0.01f)
        {
            float remaining = Vector3.Distance(transform.position, target);
            float progress = totalDistance > 0f ? 1f - (remaining / totalDistance) : 1f;

            transform.localScale = Vector3.Lerp(startScale, endScale, progress);

            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = target;
        transform.rotation = targetRotation;
        transform.localScale = endScale;

        if (stick != null)
        {
            stick.AddRing(this);
            transform.SetParent(stick.transform);
        }

        moveCoroutine = null;
    }
}