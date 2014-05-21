using UnityEngine;
using UnityTileMap;

public class Demo2Script : MonoBehaviour
{
    private TileMapBehaviour m_tileMapBehaviour;

    private void Awake()
    {
        m_tileMapBehaviour = GetComponent<TileMapBehaviour>();
        if (m_tileMapBehaviour == null)
            Debug.LogError("TileMapBehaviour not found");
    }

    // Use this for initialization
    private void Start()
    {
        // Create settings
        var meshSettings = new TileMeshSettings
        {
            // The number of tiles on the x axis
            TilesX = 2,
            // The number of tiles on the y axis
            TilesY = 2,
            // The number of pixels along each axis on a tile
            TileResolution = 16,
            // The size of one tile in Unity units
            TileSize = 1f
        };

        // Apply settings, resizing the TileMap
        m_tileMapBehaviour.MeshSettings = meshSettings;

        // Draw a checker pattern
        m_tileMapBehaviour.PaintTile(0, 0, Color.white);
        m_tileMapBehaviour.PaintTile(1, 0, Color.black);
        m_tileMapBehaviour.PaintTile(0, 1, Color.black);
        m_tileMapBehaviour.PaintTile(1, 1, Color.white);
    }
}
