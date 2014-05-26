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
                m_tileMeshSettings = new TileMeshSettings(2, 2, 16, 1f, MeshMode.SingleQuad);

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
                        SetTile(x, y, id);
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
                SetTile(x, y, value);
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

        public void PaintTile(int x, int y, Color color)
        {
            if (m_tileMeshSettings.MeshMode != MeshMode.SingleQuad)
                throw new InvalidOperationException("Painting tiles is only supported in SingleQuad MeshMode");
            var child = MeshGrid.Child;
            if (child == null)
                throw new InvalidOperationException("MeshGrid has not yet been created.");
            child.SetTile(x, y, color);
        }

        private void SetTile(int x, int y, Sprite sprite)
        {
            if (sprite == null)
                throw new ArgumentNullException("sprite");
            MeshGrid.SetTile(x, y, sprite);
        }

        private void SetTile(int x, int y, int id)
        {
            var sprite = m_tileSheet.Get(id);
            MeshGrid.SetTile(x, y, sprite);
        }
    }
}
