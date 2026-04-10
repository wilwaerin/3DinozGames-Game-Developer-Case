using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickColumnController : MonoBehaviour
{
    private GameObject stickPrefab;
    private ColorMaterialConfig colorConfig;
    private Vector3 columnPosition;
    private float stickSpacing = 1.5f;
    private float moveSpeed = 5f;
    private GameObject disappearParticlePrefab;
    private Vector3 stickNaturalScale = Vector3.one;

    private readonly List<GameObject> columnSticks = new List<GameObject>();

    // Kolonun tüm sopalarını oluşturur ve konumlandırır.
    // İlk sopa (index 0) aktif ve görünür olarak başlar; diğerleri scale=0 (görünmez) bekler.
    // İlk sopanın doğal scale değeri stickNaturalScale'ıne kaydedilir — animasyonlarda baz olarak kullanılır.
    public void Init(List<RingColor> colors, GameObject prefab, ColorMaterialConfig config, float spacing = 1.5f, GameObject particlePrefab = null)
    {
        stickPrefab = prefab;
        colorConfig = config;
        stickSpacing = spacing;
        disappearParticlePrefab = particlePrefab;
        columnPosition = transform.position;

        for (int i = 0; i < colors.Count; i++)
        {
            Vector3 pos = columnPosition + new Vector3(0f, -i * stickSpacing, 0f);
            GameObject go = Instantiate(stickPrefab, pos, Quaternion.identity);

            StickController sc = go.GetComponentInChildren<StickController>();
            if (sc != null)
                sc.Init(colors[i], colorConfig, i == 0 ? this : null);

            if (i == 0)
                stickNaturalScale = go.transform.localScale;
            else
                go.transform.localScale = Vector3.one * 0.3f;

            columnSticks.Add(go);
        }
    }

    // Aktif (en üst) sopa dolduğunda StickController tarafından çağrılır.
    // Dolu sopayı listeden çıkarır ve yok olma animasyon zincirini başlatır.
    public void OnStickFull()
    {
        if (columnSticks.Count == 0) return;

        GameObject filledStick = columnSticks[0];
        columnSticks.RemoveAt(0);

        StartCoroutine(OnStickFullSequence(filledStick));
    }

    // Animasyonları sıralı çalıştıran sarıcı coroutine'dir.
    // Önce DisappearRoutine tamamen biter, ondan sonra SlideUpAll başlar.
    // yield return ile bu sıra kesinlikle korunur — iki animasyon üst üste binmez.
    private IEnumerator OnStickFullSequence(GameObject filledStick)
    {
        yield return StartCoroutine(DisappearRoutine(filledStick));

        if (columnSticks.Count == 0) yield break;

        StartCoroutine(SlideUpAll());
    }

    // Dolu sopayı animasyonlu olarak yok eder.
    // Önce 1→1.25 büyür (punch, 0.12s), ardından 1.25→0 küçülr (ease-in hızlanarak, 0.22s).
    // Sıfırlandıktan sonra particle efekti spawn eder ve objeyi Destroy eder.
    private IEnumerator DisappearRoutine(GameObject stickRoot)
    {
        if (stickRoot == null) yield break;

        Vector3 originalScale = stickRoot.transform.localScale;

        float t = 0f;
        float punchDuration = 0.12f;
        while (t < punchDuration)
        {
            t += Time.deltaTime;
            float progress = t / punchDuration;
            stickRoot.transform.localScale = originalScale * Mathf.Lerp(1f, 1.25f, progress);
            yield return null;
        }

        t = 0f;
        float shrinkDuration = 0.22f;
        while (t < shrinkDuration && stickRoot != null)
        {
            t += Time.deltaTime;
            float progress = t / shrinkDuration;
            float scale = Mathf.Lerp(1.25f, 0f, progress * progress);
            stickRoot.transform.localScale = originalScale * scale;
            yield return null;
        }

        if (stickRoot != null)
        {
            if (disappearParticlePrefab != null)
                Instantiate(disappearParticlePrefab, stickRoot.transform.position, Quaternion.identity);

            Destroy(stickRoot);
        }
    }

    // Listede kalan tüm sopaları bir üst pozisyona MoveTowards ile kaydırır.
    // Kaydırma sırasında yeni öne gelen sopanın scale'ı 0→1 büyür (gizliden görünüre).
    // Hepsi yerine gelince üstteki sopayı Activate eder ve AppearBounce çalar.
    private IEnumerator SlideUpAll()
    {
        List<Vector3> targets = new List<Vector3>();
        for (int i = 0; i < columnSticks.Count; i++)
            targets.Add(columnPosition + new Vector3(0f, -i * stickSpacing, 0f));

        float totalDist = columnSticks.Count > 0 && columnSticks[0] != null
            ? Vector3.Distance(columnSticks[0].transform.position, targets[0])
            : stickSpacing;
        if (totalDist < 0.01f) totalDist = stickSpacing;

        bool moving = true;
        while (moving)
        {
            moving = false;
            for (int i = 0; i < columnSticks.Count; i++)
            {
                if (columnSticks[i] == null) continue;
                columnSticks[i].transform.position = Vector3.MoveTowards(
                    columnSticks[i].transform.position,
                    targets[i],
                    moveSpeed * Time.deltaTime
                );
                if (Vector3.Distance(columnSticks[i].transform.position, targets[i]) > 0.01f)
                    moving = true;
            }

            if (columnSticks.Count > 0 && columnSticks[0] != null)
            {
                float remaining = Vector3.Distance(columnSticks[0].transform.position, targets[0]);
                float progress = Mathf.Clamp01(1f - (remaining / totalDist));
                columnSticks[0].transform.localScale = stickNaturalScale * Mathf.Lerp(0.3f, 1f, progress);
            }

            yield return null;
        }

        for (int i = 0; i < columnSticks.Count; i++)
            if (columnSticks[i] != null)
                columnSticks[i].transform.position = targets[i];

        if (columnSticks.Count > 0 && columnSticks[0] != null)
        {
            StickController sc = columnSticks[0].GetComponentInChildren<StickController>();
            if (sc != null)
                sc.Activate(this);

            StartCoroutine(AppearBounce(columnSticks[0]));
        }
    }

    // Yeni aktif olan sopa yerine geldiğinde çalınan ziplama efekti.
    // 1→1.25 (0.12s) büyütme, ardından 1.25→1 (0.1s) settle — sopa doğal boyutuna kararlı iner.
    private IEnumerator AppearBounce(GameObject stickRoot)
    {
        if (stickRoot == null) yield break;

        float t = 0f;
        float punchDuration = 0.12f;
        while (t < punchDuration && stickRoot != null)
        {
            t += Time.deltaTime;
            float progress = t / punchDuration;
            stickRoot.transform.localScale = stickNaturalScale * Mathf.Lerp(1f, 1.25f, progress);
            yield return null;
        }

        t = 0f;
        float settleDuration = 0.1f;
        while (t < settleDuration && stickRoot != null)
        {
            t += Time.deltaTime;
            float progress = t / settleDuration;
            stickRoot.transform.localScale = stickNaturalScale * Mathf.Lerp(1.25f, 1f, progress);
            yield return null;
        }

        if (stickRoot != null)
            stickRoot.transform.localScale = stickNaturalScale;
    }
}
