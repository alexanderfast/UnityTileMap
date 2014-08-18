using UnityEngine;
using UnityTileMap;
using System.Collections.Generic;

public class EyeBehaviour : MonoBehaviour
{
    private static readonly Vector2Int m_initialPosition = new Vector2Int(15, 10);

    private TileMapBehaviour m_tileMap;
    private TileMapVisibilityBehaviour m_visibility;
    private KeyboardInputBehaviour m_input;
    private int m_visionRange = int.MaxValue;
    private bool m_spawned;

    void Awake()
    {
        m_tileMap = GameObject.Find("TileMap").GetComponent<TileMapBehaviour>();

        // TODO add keyboard input, calling UpdateVision after each change in position
        //m_input = GetComponent<KeyboardInputBehaviour>();
    }

    // Use this for initialization
    void Start()
    {
        transform.position = m_tileMap.GetTileBoundsWorld(m_initialPosition.x, m_initialPosition.y).center;
    }


    // Update is called once per frame
    void Update()
    {
        if (!m_spawned) // TODO there is probably a better way to do init
        {
            m_visibility = m_tileMap.EnableVisibility();
            UpdateVision();
            m_spawned = true;
        }
    }

    // TODO its time to start think about tile meta data
    private bool IsTileBlockingSight(int x, int y)
    {
        if (m_tileMap.IsInBounds(x, y))
            return m_tileMap[x, y] == 0;
        return true;
    }

    private void UpdateVision()
    {
        var visible = new HashSet<Vector2Int>();

        FieldOfView.Run(
            m_initialPosition,
            m_tileMap.MeshSettings.TilesX,
            m_tileMap.MeshSettings.TilesY,
            m_visionRange,
            i => visible.Add(i),
            i => IsTileBlockingSight(i.x, i.y));

        // render light/shadow
        foreach (var tile in visible)
        {
            //Debug.Log(tile.Key + " " + !tile.Value);
            m_visibility.SetTileVisible(tile.x, tile.y, true);
        }
    }
}
