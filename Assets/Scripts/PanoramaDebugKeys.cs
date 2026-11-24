using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// OPSIONAL. Pintasan keyboard untuk menguji semua panorama tanpa headset:
/// Page Down = lokasi berikutnya, Page Up = lokasi sebelumnya, Home = kembali ke lokasi awal.
/// Urutan lokasi mengikuti urutan objek Pano_... di Hierarchy.
public class PanoramaDebugKeys : MonoBehaviour
{
    List<PanoramaScene> ordered;

    void Start()
    {
        ordered = new List<PanoramaScene>(
            FindObjectsByType<PanoramaScene>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        ordered.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
    }

    void Update()
    {
        var kb = Keyboard.current;
        var m = PanoramaManager.Instance;
        if (kb == null || m == null || ordered == null || ordered.Count == 0) return;

        int i = ordered.IndexOf(m.Current);
        int n = ordered.Count;

        if (kb.pageDownKey.wasPressedThisFrame) m.GoTo(ordered[(i + 1 + n) % n]);
        if (kb.pageUpKey.wasPressedThisFrame) m.GoTo(ordered[(i - 1 + n) % n]);
        if (kb.homeKey.wasPressedThisFrame && m.startScene != null) m.GoTo(m.startScene);
    }
}
