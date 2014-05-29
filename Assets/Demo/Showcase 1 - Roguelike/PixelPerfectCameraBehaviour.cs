using UnityEngine;

//[ExecuteInEditMode]
public class PixelPerfectCameraBehaviour : MonoBehaviour
{
    public float pixelsPerUnit;
    public int zoom = 1;

    void Start()
    {
        UpdateSize();
    }

    //// Update is called once per frame
    //void Update()
    //{
    //    // Keep correct size in editor even if user resizes the window.
    //    if (Application.isEditor)
    //        UpdateSize();
    //}

    void UpdateSize()
    {
        var size = Screen.height / pixelsPerUnit / zoom / 2f;

        // Always setting the size would make the scene constantly marked as dirty,
        // only set if value has changed.
        if (Camera.main.orthographicSize != size)
            Camera.main.orthographicSize = size;
    }
}
