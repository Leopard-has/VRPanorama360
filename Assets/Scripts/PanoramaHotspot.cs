using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Pasang di root prefab Hotspot (World Space Canvas berisi 1 tombol).
/// Saat tombol diklik dengan kontroler VR, aplikasi berpindah ke panorama tujuan.
public class PanoramaHotspot : MonoBehaviour
{
    public Button button;
    public TMP_Text label;

    PanoramaScene target;
    Transform head;

    public void Setup(PanoramaScene targetScene, string text)
    {
        target = targetScene;

        if (label != null) label.text = text;

        // Canvas World Space butuh kamera agar sinar kontroler bisa mengenainya
        var canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null) canvas.worldCamera = Camera.main;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        if (PanoramaManager.Instance != null) PanoramaManager.Instance.GoTo(target);
    }

    // Hotspot selalu menghadap pengguna
    void LateUpdate()
    {
        if (head == null)
        {
            if (Camera.main == null) return;
            head = Camera.main.transform;
        }
        Vector3 dir = transform.position - head.position;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}
