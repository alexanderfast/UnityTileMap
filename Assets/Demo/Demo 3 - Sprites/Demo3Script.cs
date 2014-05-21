using UnityEngine;
using UnityTileMap;

public class Demo3Script : MonoBehaviour
{
    private TileMapBehaviour m_tileMapBehaviour;
    private bool m_done;

    private void Awake()
    {
        m_tileMapBehaviour = GetComponent<TileMapBehaviour>();
        if (m_tileMapBehaviour == null)
            Debug.LogError("TileMapBehaviour not found");
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_done)
            return; // Tiles already set

        var tileSheet = m_tileMapBehaviour.TileSheet;
        if (tileSheet.Count == 0)
            return; // Waiting for user to add tiles

        // Every tile is assigned an id, this is for efficiency
        m_tileMapBehaviour[0, 0] = 0;
        m_tileMapBehaviour[1, 1] = 0;

        // Use this method to lookup the id for a named Sprite
        int redId = tileSheet.Lookup("Red");
        m_tileMapBehaviour[1, 0] = redId;
        m_tileMapBehaviour[0, 1] = redId;

        m_done = true;
    }
}
