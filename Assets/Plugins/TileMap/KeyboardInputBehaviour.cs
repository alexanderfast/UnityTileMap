using UnityEngine;
using UnityTileMap;
using System.Collections;

/// <summary>
/// A utility class for basic keyboard input.
/// </summary>
public class KeyboardInputBehaviour : MonoBehaviour
{
    private TileMapBehaviour m_tileMap;
    private Vector2Int m_tilePosition = new Vector2Int(0, 0);

    public Vector2Int TilePosition
    {
        get { return m_tilePosition; }
        set
        {
            if (m_tilePosition == value)
                return;

            m_tilePosition = value;
            transform.position = m_tileMap.GetTileBoundsWorld(value.x, value.y).center;
        }
    }

    void Awake()
    {
        // TODO a general way to find and attach to a tile map
        m_tileMap = GameObject.Find("TileMap").GetComponent<TileMapBehaviour>();
    }

    void Update()
    {
        ProcessInput();
    }

    protected void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            MoveNorth();
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            MoveEast();
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            MoveSouth();
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            MoveWest();
    }

    public void MoveNorth()
    {
        MoveTo(m_tilePosition.x, m_tilePosition.y + 1);
    }

    public void MoveEast()
    {
        MoveTo(m_tilePosition.x + 1, m_tilePosition.y);
    }

    public void MoveSouth()
    {
        MoveTo(m_tilePosition.x, m_tilePosition.y - 1);
    }

    public void MoveWest()
    {
        MoveTo(m_tilePosition.x - 1, m_tilePosition.y);
    }

    public void MoveTo(int x, int y)
    {
        if (!IsTileBlockingMovement(x, y))
            TilePosition = new Vector2Int(x, y);
    }

    // TODO need a way to handle meta data for tiles
    protected virtual bool IsTileBlockingMovement(int x, int y)
    {
        if (m_tileMap.IsInBounds(x, y))
        {
            Debug.Log(m_tileMap[x, y]);
            return m_tileMap[x, y] == 0; // TODO assumes tile id 0 is walls and everything else is walkable
        }
        return true;
    }
}
