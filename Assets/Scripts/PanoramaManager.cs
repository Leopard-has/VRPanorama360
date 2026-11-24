using System.Collections;
using TMPro;
using UnityEngine;

/// Pengatur utama aplikasi panorama 360:
/// menampilkan panorama (lewat Skybox), membuat hotspot, dan berpindah lokasi dengan efek fade.
/// Pasang di objek kosong "PanoramaManager".
public class PanoramaManager : MonoBehaviour
{
    public static PanoramaManager Instance;

    [Header("Skybox")]
    [Tooltip("Material dengan shader Skybox/Panoramic.")]
    public Material skyboxMaterial;

    [Header("Panorama awal")]
    public PanoramaScene startScene;

    [Header("Hotspot")]
    public PanoramaHotspot hotspotPrefab;
    [Tooltip("Objek kosong tempat hotspot dibuat (child dari PanoramaManager).")]
    public Transform hotspotRoot;
    [Tooltip("Titik pusat hotspot (kira-kira posisi kepala pengguna).")]
    public Vector3 hotspotCenter = new Vector3(0f, 1.4f, 0f);
    public float hotspotDistance = 3.5f;
    [Tooltip("Hotspot mengikuti posisi kepala pengguna sehingga selalu setinggi mata (disarankan aktif).")]
    public bool followHead = true;

    [Header("Transisi (opsional)")]
    public CanvasGroup fadeGroup;
    public float fadeDuration = 0.25f;

    [Header("Banner nama lokasi (opsional)")]
    public TMP_Text locationLabel;
    public CanvasGroup bannerGroup;
    public float bannerSeconds = 2.5f;

    PanoramaScene current;
    bool busy;
    Coroutine bannerRoutine;
    Transform head;

    public PanoramaScene Current { get { return current; } }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (hotspotRoot != null)
        {
            hotspotRoot.position = hotspotCenter;
            hotspotRoot.rotation = Quaternion.identity;
        }
        if (fadeGroup != null) fadeGroup.alpha = 0f;
        if (bannerGroup != null) bannerGroup.alpha = 0f;

        if (startScene == null) startScene = FindFirstObjectByType<PanoramaScene>();
        if (startScene == null)
        {
            Debug.LogError("Tidak ada PanoramaScene di scene. Buat objek Pano_... dan pasang script Panorama Scene.");
            return;
        }
        Show(startScene);
    }

    // Menjaga hotspot tetap setinggi mata walaupun tinggi kamera berubah
    void LateUpdate()
    {
        if (!followHead || hotspotRoot == null) return;

        if (head == null)
        {
            if (Camera.main == null) return;
            head = Camera.main.transform;
        }
        hotspotRoot.position = head.position;
    }

    // Dipanggil hotspot saat diklik
    public void GoTo(PanoramaScene target)
    {
        if (target == null || target == current || busy) return;
        StartCoroutine(Transition(target));
    }

    IEnumerator Transition(PanoramaScene target)
    {
        busy = true;
        yield return Fade(1f);
        Show(target);
        yield return Fade(0f);
        busy = false;
    }

    IEnumerator Fade(float to)
    {
        if (fadeGroup == null || fadeDuration <= 0f) yield break;

        float from = fadeGroup.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = to;
    }

    void Show(PanoramaScene scene)
    {
        current = scene;

        if (skyboxMaterial != null && scene.panorama != null)
        {
            skyboxMaterial.SetTexture("_MainTex", scene.panorama);
            skyboxMaterial.SetFloat("_Rotation", scene.skyboxRotation);
            RenderSettings.skybox = skyboxMaterial;
        }

        BuildHotspots(scene);
        ShowBanner(scene.displayName);
    }

    void BuildHotspots(PanoramaScene scene)
    {
        if (hotspotRoot == null) return;

        for (int i = hotspotRoot.childCount - 1; i >= 0; i--)
            Destroy(hotspotRoot.GetChild(i).gameObject);

        if (hotspotPrefab == null) return;

        foreach (var link in scene.links)
        {
            if (link.target == null) continue;

            Vector3 dir = PanoramaScene.Direction(link.yaw, link.pitch);

            var h = Instantiate(hotspotPrefab, hotspotRoot);
            h.transform.localPosition = dir * hotspotDistance;
            h.transform.localRotation = Quaternion.identity;
            h.Setup(link.target, string.IsNullOrEmpty(link.label) ? link.target.displayName : link.label);
        }
    }

    void ShowBanner(string text)
    {
        if (locationLabel != null) locationLabel.text = text;
        if (bannerGroup == null) return;

        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerRoutine = StartCoroutine(BannerRoutine());
    }

    IEnumerator BannerRoutine()
    {
        bannerGroup.alpha = 1f;
        yield return new WaitForSeconds(bannerSeconds);

        float t = 0f;
        const float fadeOut = 0.5f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            bannerGroup.alpha = 1f - t / fadeOut;
            yield return null;
        }
        bannerGroup.alpha = 0f;
    }
}
