using System;
using System.Collections.Generic;
using UnityEngine;

/// Satu tautan (hotspot) dari panorama ini ke panorama lain.
[Serializable]
public class PanoramaLink
{
    public PanoramaScene target;

    [Tooltip("Arah horizontal (derajat). 0 = depan (+Z), 90 = kanan (+X), 180 = belakang, 270 = kiri.")]
    public float yaw;

    [Tooltip("Arah vertikal (derajat). 0 = setinggi mata, positif = ke atas, negatif = ke bawah.")]
    public float pitch;

    [Tooltip("Teks pada tombol. Kosongkan untuk memakai nama lokasi tujuan.")]
    public string label;
}

/// Pasang di setiap objek Pano_... (satu objek per gambar panorama 360).
/// Objek ini hanya menyimpan data: gambar, rotasi skybox, dan daftar tautan ke lokasi lain.
public class PanoramaScene : MonoBehaviour
{
    [Header("Data panorama")]
    public string displayName = "Lokasi";
    public Texture panorama;

    [Tooltip("Memutar gambar agar bagian yang diinginkan berada di depan saat panorama dibuka.")]
    [Range(0f, 360f)] public float skyboxRotation = 0f;

    [Header("Tautan ke lokasi lain")]
    public List<PanoramaLink> links = new List<PanoramaLink>();

    public static Vector3 Direction(float yaw, float pitch)
    {
        return Quaternion.Euler(-pitch, yaw, 0f) * Vector3.forward;
    }

#if UNITY_EDITOR
    // Menampilkan posisi hotspot di Scene view saat objek ini dipilih.
    void OnDrawGizmosSelected()
    {
        var m = FindFirstObjectByType<PanoramaManager>();
        Vector3 center = m != null ? m.hotspotCenter : new Vector3(0f, 1.4f, 0f);
        float dist = m != null ? m.hotspotDistance : 3.5f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, 0.1f);

        foreach (var l in links)
        {
            Vector3 p = center + Direction(l.yaw, l.pitch) * dist;
            Gizmos.DrawLine(center, p);
            Gizmos.DrawSphere(p, 0.15f);
            string name = l.target != null ? l.target.displayName : "(kosong)";
            UnityEditor.Handles.Label(p + Vector3.up * 0.3f, name);
        }
    }

    // Klik kanan pada komponen ini -> "Preview panorama ini di Scene view"
    [ContextMenu("Preview panorama ini di Scene view")]
    void PreviewInScene()
    {
        var m = FindFirstObjectByType<PanoramaManager>();
        if (m == null || m.skyboxMaterial == null || panorama == null)
        {
            Debug.LogWarning("Preview gagal: pastikan PanoramaManager, Skybox Material, dan gambar Panorama sudah terisi.");
            return;
        }
        m.skyboxMaterial.SetTexture("_MainTex", panorama);
        m.skyboxMaterial.SetFloat("_Rotation", skyboxRotation);
        RenderSettings.skybox = m.skyboxMaterial;
        UnityEditor.SceneView.RepaintAll();
    }
#endif
}
