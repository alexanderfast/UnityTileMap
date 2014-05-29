using UnityEditor;
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PixelPerfectCameraBehaviour : MonoBehaviour
{
    public float pixelsPerUnit;
    public int zoom = 1;

    void Start()
    {
        UpdateSize();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
            UpdateSize();
    }

    void UpdateSize()
    {
        Camera.main.orthographicSize = Screen.height / pixelsPerUnit / zoom / 2f;
    }
}
