using System;
using UnityEngine;

namespace UnityTileMap
{
    [ExecuteInEditMode]
    [Serializable]
    public class TileMapBehaviour : MonoBehaviour
    {
        [SerializeField]
        private TileMapData m_tileMapData;

        [SerializeField]
        private TileMeshSettings m_tileMeshSettings;

        [SerializeField]
        private TileSheet m_tileSheet;

        private TileMeshGrid m_meshGrid;

        public TileMeshSettings MeshSettings
        {
            get { return m_tileMeshSettings; }
            set
            {
                //Debug.Log("Applying settings");

                MeshGrid.Settings = value;
                m_tileMeshSettings = value;
                m_tileMapData.SetSize(m_tileMeshSettings.TilesX, m_tileMeshSettings.TilesY);
            }
        }

        public TileSheet TileSheet
        {
            get { return m_tileSheet; }
        }

        private TileMeshGrid MeshGrid
        {
            get
            {
                if (m_meshGrid == null)
                {
                    Debug.Log("Recreating TileMeshGrid");
                    m_meshGrid = new TileMeshGrid();
                    m_meshGrid.Initialize(this, m_tileMeshSettings);
                }
                return m_meshGrid;
            }
        }

        protected virtual void Awake()
        {
            if (m_tileMeshSettings == null)
                m_tileMeshSettings = new TileMeshSettings(2, 2, 16, 1f);

            if (m_tileSheet == null)
                m_tileSheet = ScriptableObject.CreateInstance<TileSheet>();

            if (m_meshGrid == null)
            {
                m_meshGrid = new TileMeshGrid();
                m_meshGrid.Initialize(this, m_tileMeshSettings);
            }
            else
            {
                m_meshGrid.Settings = m_tileMeshSettings;
            }

            if (m_tileMapData == null)
            {
                m_tileMapData = new TileMapData();
                m_tileMapData.SetSize(m_tileMeshSettings.TilesX, m_tileMeshSettings.TilesY);
            }
            else
            {
                // restore tilemap data
                for (int x = 0; x < m_tileMapData.SizeX; x++)
                {
                    for (int y = 0; y < m_tileMapData.SizeY; y++)
                    {
                        var id = m_tileMapData[x, y];
                        if (id < 0)
                            continue;
                        var sprite = m_tileSheet.Get(id);
                        SetTile(x, y, sprite);
                    }
                }
            }
        }

        public int this[int x, int y]
        {
            get { return m_tileMapData[x, y]; }
            set
            {
                //Debug.Log(string.Format("Setting tile ({0}, {1}) = {2}", x, y, value));

                var sprite = m_tileSheet.Get(value);
                SetTile(x, y, sprite);

                m_tileMapData[x, y] = value;
            }
        }

        /// <summary>
        /// Get the bounding box of a single tile in local coordinates.
        /// This is useful for positioning GameObjects that are children of the Tilemap.
        /// </summary>
        public Rect GetTileBoundsLocal(int x, int y)
        {
            return MeshGrid.GetTileBoundsLocal(x, y);
        }

        /// <summary>
        /// Get the bounding box of a single tile in world/scene coordinates.
        /// This is useful for positioning GameObjects that are not children of the Tilemap.
        /// </summary>
        public Rect GetTileBoundsWorld(int x, int y)
        {
            return MeshGrid.GetTileBoundsWorld(x, y);
        }

        /// <summary>
        /// Paint a tile in a solid color.
        /// </summary>
        /// <remarks>
        /// Painted tiles cannot be serialized.
        /// </remarks>
        public void PaintTile(int x, int y, Color color)
        {
            MeshGrid.SetTile(x, y, color);
        }

        private void SetTile(int x, int y, Sprite sprite)
        {
            if (sprite == null)
                throw new ArgumentNullException("sprite");
            Rect rect = sprite.rect;
            Color[] colors = sprite.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            MeshGrid.SetTile(x, y, colors);
        }
    }
}
