using UnityEngine;
using System.Collections.Generic;
using UnityTileMap;

public class GameSessionBehaviour : MonoBehaviour
{
    private TileMapBehaviour m_tileMap;

    private void Awake()
    {
        // Find the tilemap game object and cache the behaviour.
        m_tileMap = GameObject.Find("TileMap").GetComponent<TileMapBehaviour>();
        if (m_tileMap == null)
            Debug.LogError("TileMapBehaviour not found");
    }

    private void Start()
    {
        // Size of dungeon.
        int sizeX = 20;
        int sizeY = 8;

        // Rather than hardcode the resolution of a tile, use the size of the first sprite.
        var tileSheet = m_tileMap.TileSheet;
        var spriteSize = tileSheet.Get(0).rect;

        // Define and apply the settings for the tilemap.
        var settings = new TileMeshSettings(sizeX, sizeY, (int)spriteSize.width);
        m_tileMap.MeshSettings = settings;

        // The level generator uses an enum to define each type of tile.
        // UnityTileMap doesn't (and shouldn't) know about the enum,
        // so we need a way to translate each enum value (type of tile) to a sprite.
        var tileTypes = new Dictionary<LevelGenerator.TileType, int>();
        tileTypes[LevelGenerator.TileType.Wall]  = tileSheet.Lookup("Green");
        tileTypes[LevelGenerator.TileType.Floor] = tileSheet.Lookup("Blue");

        // Generate the level.
        var level = LevelGenerator.Generate(sizeX, sizeY);

        // Loop through the level and set each tile.
        // This might look very different depending on how the generator/loader returns the level.
        foreach (KeyValuePair<Vector2, LevelGenerator.TileType> pair in level)
        {
            var x = (int)pair.Key.x;
            var y = (int)pair.Key.y;
            var tile = pair.Value;

            // Use our cached lookup, which sprite represents this type of tile?
            var spriteId = tileTypes[tile];

            // Set one tile.
            m_tileMap[x, y] = spriteId;
        }

        // Done!
    }

    private class LevelGenerator
    {
        public enum TileType
        {
            Wall,
            Floor
        }

        /// <summary>
        /// Actually generating something like a roguelike dungeon is outside the scope of this demo,
        /// this method just returns random data.
        /// </summary>
        public static IDictionary<Vector2, TileType> Generate(int sizeX, int sizeY)
        {
            var map = new Dictionary<Vector2, TileType>();
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    var position = new Vector2(x, y);
                    var tile = Random.Range(0, 2) == 0 ? TileType.Wall : TileType.Floor;
                    map[position] = tile;
                }
            }
            return map;
        }
    }
}
