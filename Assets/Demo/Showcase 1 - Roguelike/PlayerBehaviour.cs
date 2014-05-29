using UnityEngine;
using UnityTileMap;

public class PlayerBehaviour : MonoBehaviour
{
    private TileMapBehaviour m_tileMap;
    private LevelBehaviour m_levelBehaviour;
    private SceneFadeInOut m_sceneFadeInOut;
    private int m_x;
    private int m_y;

    // Use this for initialization
    private void Start()
    {
        var tileMapGameObject = GameObject.Find("TileMap");
        m_tileMap = tileMapGameObject.GetComponent<TileMapBehaviour>();
        if (m_tileMap == null)
            Debug.LogError("TileMapBehaviour not found");
        m_levelBehaviour = tileMapGameObject.GetComponent<LevelBehaviour>();
        if (m_levelBehaviour == null)
            Debug.LogError("LevelBehaviour not found");
        m_sceneFadeInOut = GameObject.Find("ScreenFader").GetComponent<SceneFadeInOut>();
        if (m_sceneFadeInOut == null)
            Debug.LogError("SceneFadeInOut not found");
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            TryMoveTo(m_x, m_y + 1);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            TryMoveTo(m_x, m_y - 1);
        if (Input.GetKeyDown(KeyCode.RightArrow))
            TryMoveTo(m_x + 1, m_y);
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            TryMoveTo(m_x - 1, m_y);
    }

    private void TryMoveTo(int x, int y)
    {
        if (m_levelBehaviour.IsWalkeable(x, y))
            SetTilePosition(x, y);
    }

    public void SetTilePosition(int x, int y)
    {
        m_x = x;
        m_y = y;
        var tileBounds = m_tileMap.GetTileBoundsWorld(x, y);
        transform.position = new Vector3(tileBounds.xMin, tileBounds.yMin, transform.position.z);

        // If we walk onto the stairs down...
        if (m_levelBehaviour.GetTile(m_x, m_y) == TileType.StairsDown)
            OnStairsDown();
    }

    private void OnStairsDown()
    {
        enabled = false;
        m_sceneFadeInOut.FadeOutThenIn(() =>
            {
                m_levelBehaviour.StartLevel();
                enabled = true;
            });
    }
}
