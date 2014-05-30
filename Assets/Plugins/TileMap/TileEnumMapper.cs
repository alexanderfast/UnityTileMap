using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityTileMap
{
    /// <summary>
    /// If you prefer to add an Enum in your code to define each type of tile this class will help you map the
    /// enum values to the internal id's used by UnityTileMap.
    /// This class is created and configured entirely through code and isn't visible anywhere in the Unity GUI.
    /// </summary>
    /// <typeparam name="T">The type/enum that defines a type of tile.</typeparam>
    public class TileEnumMapper<T>
    {
        /// <summary>
        /// The TileMapBehaviour that this TileEnumMapper wraps.
        /// </summary>
        private readonly TileMapBehaviour m_tileMap;

        /// <summary>
        /// The map from type of tile to internal id.
        /// </summary>
        private readonly Dictionary<T, int> m_tiles = new Dictionary<T, int>();

        public TileEnumMapper(TileMapBehaviour tileMap)
        {
            if (tileMap == null)
                throw new ArgumentNullException("tileMap");
            m_tileMap = tileMap;
        }

        /// <summary>
        /// Map a type of tile to a named sprite you have added to the TileSheet in the TileMapBehaviour.
        /// </summary>
        /// <param name="tileType">The type of tile.</param>
        /// <param name="nameOfSprite">The name of the sprite the tile should be mapped to.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the sprite with that name has not been added.</exception>
        public void Map(T tileType, string nameOfSprite)
        {
            try
            {
                m_tiles[tileType] = m_tileMap.TileSheet.Lookup(nameOfSprite);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError(string.Format("No Sprite named '{0}', did you forget to add it?", nameOfSprite));
                throw;
            }
        }

        /// <summary>
        /// Get the internal id that this type of tile is mapped to.
        /// </summary>
        public int GetId(T tileType)
        {
            return m_tiles[tileType];
        }

        /// <summary>
        /// Get the type of tile that has been mapped to this internal id.
        /// </summary>
        public T GetType(int id)
        {
            foreach (var pair in m_tiles)
            {
                if (pair.Value == id)
                    return pair.Key;
            }
            throw new KeyNotFoundException(id.ToString());
        }

        /// <summary>
        /// Helper method to set a tile on the TileMap without having to think about internal ids.
        /// </summary>
        public void SetTile(int x, int y, T tileType)
        {
            m_tileMap[x, y] = m_tiles[tileType];
        }

        /// <summary>
        /// Helper method to get a tile on the TileMap without having to think about internal ids.
        /// </summary>
        public T GetTile(int x, int y)
        {
            return GetType(m_tileMap[x, y]);
        }
    }
}
