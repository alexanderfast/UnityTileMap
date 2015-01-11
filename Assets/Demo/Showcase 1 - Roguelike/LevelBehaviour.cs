using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTileMap;
using Random = UnityEngine.Random;

public class LevelBehaviour : MonoBehaviour
{
    public int SizeX = 16;
    public int SizeY = 16;

    private TileMapBehaviour m_tileMap;
    private Grid<TileType> m_grid;
    private TileEnumMapper<TileType> m_tiles;

    private void Awake()
    {
        // Find the tilemap game object and cache the behaviour.
        m_tileMap = GameObject.Find("TileMap").GetComponent<TileMapBehaviour>();
        if (m_tileMap == null)
        {
            Debug.LogError("TileMapBehaviour not found");
            return;
        }

        // Rather than hardcode the resolution of a tile, use the size of the first sprite.
        var tileSheet = m_tileMap.TileSheet;
        if (tileSheet.Count == 0)
        {
            Debug.LogError("Add some sprites before running the game");
            return;
        }
        var spriteSize = tileSheet.Get(0).rect;

        // Define and apply the settings for the tilemap.
        var settings = new TileMeshSettings(SizeX, SizeY, (int)spriteSize.width);
        m_tileMap.MeshSettings = settings;

        // Map type of tile to sprite
        m_tiles = new TileEnumMapper<TileType>(m_tileMap);
        m_tiles.Map(TileType.Wall, "Wall2");
        m_tiles.Map(TileType.Floor, "Floor");
        m_tiles.Map(TileType.StairsUp, "StairsUp");
        m_tiles.Map(TileType.StairsDown, "StairsDown");
    }

    private void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        m_grid = new DungeonGenerator().Generate(SizeX, SizeY);

        // Change two random floor tiles to stairs
        var position = GetRandomTileOfType(TileType.Floor);
        m_grid[(int)position.x, (int)position.y] = TileType.StairsUp;
        position = GetRandomTileOfType(TileType.Floor);
        m_grid[(int)position.x, (int)position.y] = TileType.StairsDown;

        // Set all tiles
        for (int y = 0; y < m_grid.SizeY; y++)
        {
            for (int x = 0; x < m_grid.SizeX; x++)
            {
                var type = m_grid[x, y];
                if (type == TileType.Empty)
                    m_tileMap.PaintTile(x, y, new Color(0, 0, 0, 0));
                else
                    m_tileMap[x, y] = m_tiles.GetId(type);
            }
        }

        // Move player to stairs
        var stairs = FindTile(TileType.StairsUp);
        var playerBehaviour = GameObject.Find("Player").GetComponent<PlayerBehaviour>();
        playerBehaviour.SetTilePosition((int)stairs.x, (int)stairs.y);
        playerBehaviour.renderer.sortingOrder = 1;

        TileMeshBehaviour mesh = m_tileMap.GetComponentInChildren<TileMeshBehaviour>();

        if (mesh != null)
        {
            mesh.renderer.sortingOrder = 0;
        }
    }

    public TileType GetTile(int x, int y)
    {
        return m_tiles.GetType(m_tileMap[x, y]);
    }

    // Returns true if the player can walk on this tile.
    public bool IsWalkeable(int x, int y)
    {
        int tile = m_tileMap[x, y];
        return tile != m_tiles.GetId(TileType.Wall);
    }

    public Vector2 FindTile(TileType tileType)
    {
        for (int y = 0; y < m_grid.SizeY; y++)
        {
            for (int x = 0; x < m_grid.SizeX; x++)
            {
                var type = m_grid[x, y];
                if (type == tileType)
                    return new Vector2(x, y);
            }
        }
        return Vector2.zero;
    }

    public IEnumerable<Vector2> FindAllTiles(TileType tileType)
    {
        for (int y = 0; y < m_grid.SizeY; y++)
        {
            for (int x = 0; x < m_grid.SizeX; x++)
            {
                var type = m_grid[x, y];
                if (type == tileType)
                    yield return new Vector2(x, y);
            }
        }
    }

    public Vector2 GetRandomTileOfType(TileType tileType)
    {
        var candidates = FindAllTiles(tileType).ToList();
        return candidates[Random.Range(0, candidates.Count)];
    }
}
