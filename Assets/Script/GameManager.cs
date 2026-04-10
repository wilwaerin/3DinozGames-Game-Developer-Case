using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private LevelData levelData;

    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    private float currentTime;
    private bool gameOver = false;
    private ChainController[] chainControllers;

    // Oyun başladığında çalışır. LevelData'dan timer süresini okur, panelleri gizler
    // ve bir frame sonra ChainController'ları bulmak için InitAfterSpawn coroutine'ini başlatır.
    private void Start()
    {
        currentTime = levelData != null ? levelData.levelTime : 60f;

        if (winPanel != null)  winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        StartCoroutine(InitAfterSpawn());
    }

    // Bir frame bekler, ardından sahnedeki tüm ChainController'ları bulur.
    // Start()'ta değil bir frame sonra aranmasının nedeni: ChainSceneSetup aynı frame'de Instantiate yapar,
    // obje hemen hazır olmayabilir. Bir frame beklenerek güvenli bulunur.
    private IEnumerator InitAfterSpawn()
    {
        yield return null;
        chainControllers = FindObjectsOfType<ChainController>();
    }

    // Her frame çalışır. gameOver true ise hiçbir şey yapmaz.
    // Timer'u azaltır, ekranı günceller. Süre sıfırlanırsa kaybetme, tüm chain'ler boşalmışsa kazanma tetikler.
    private void Update()
    {
        if (gameOver) return;

        currentTime = Mathf.Max(0f, currentTime - Time.deltaTime);
        UpdateTimerDisplay();

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            TriggerLose();
            return;
        }

        if (CheckWin())
            TriggerWin();
    }

    // Timer metnini MM:SS formatında günceller.
    // Süreye göre renk değiştirir: 30 saniyenin üzerinde beyaz, 30s ve altı turuncu, 10s ve altı kırmızı.
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";

        if (currentTime <= 10f)
            timerText.color = Color.red;
        else if (currentTime <= 30f)
            timerText.color = new Color(1f, 0.6f, 0f);
        else
            timerText.color = Color.white;
    }

    // Tüm ChainController'ların boş olup olmadığını kontrol eder.
    // Herhangi bir zincirde ring kaldıysa false döner. Hepsi boşalmışsa true döner — oyun kazanıldı.
    private bool CheckWin()
    {
        if (chainControllers == null || chainControllers.Length == 0) return false;

        foreach (ChainController cs in chainControllers)
        {
            if (cs == null) continue;
            if (!cs.IsEmpty()) return false;
        }

        return true;
    }

    // Kazanma durumunu tetikler.
    // gameOver true yapılır, chain collider'ları kapatılır (artık tıklanamazlar) ve win panel animasyonlu açılır.
    private void TriggerWin()
    {
        gameOver = true;
        DisableChainColliders();
        if (winPanel != null) StartCoroutine(ShowPanelAnimated(winPanel));
        Debug.Log("[GameManager] WIN!");
    }

    // Kaybetme durumunu tetikler.
    // gameOver true yapılır, chain collider'ları kapatılır ve lose panel animasyonlu açılır.
    private void TriggerLose()
    {
        gameOver = true;
        DisableChainColliders();
        if (losePanel != null) StartCoroutine(ShowPanelAnimated(losePanel));
        Debug.Log("[GameManager] LOSE!");
    }

    // Verilen panel objesini scale animasyonuyla açar.
    // Önce scale 0'dan 1.15'e Sine ease ile büyür (0.25s), sonra 1.15'ten 1.0'e settle iner (0.1s).
    // Time.unscaledDeltaTime kullanılır — Time.timeScale = 0 olsa bile animasyon çalışır.
    private IEnumerator ShowPanelAnimated(GameObject panel)
    {
        panel.SetActive(true);
        Transform t = panel.transform;
        t.localScale = Vector3.zero;

        float growDuration = 0.25f;
        float elapsed = 0f;
        while (elapsed < growDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / growDuration;
            float scale = Mathf.Sin(progress * Mathf.PI * 0.5f);
            t.localScale = Vector3.one * Mathf.Lerp(0f, 1.15f, scale);
            yield return null;
        }

        elapsed = 0f;
        float settleDuration = 0.1f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / settleDuration;
            t.localScale = Vector3.one * Mathf.Lerp(1.15f, 1f, progress);
            yield return null;
        }

        t.localScale = Vector3.one;
    }

    // Oyun bittiğinde tüm ChainController'ların BoxCollider'ını devre dışı bırakır.
    // Bu sayede game over sonrasında zincire tıklanarak ring serbest bırakılamaz.
    private void DisableChainColliders()
    {
        if (chainControllers == null) return;
        foreach (ChainController cs in chainControllers)
        {
            if (cs == null) continue;
            BoxCollider col = cs.GetComponent<BoxCollider>();
            if (col != null) col.enabled = false;
        }
    }
}
