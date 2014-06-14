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

        [SerializeField]
        private bool m_activeInEditMode;

        private TileChunkManager m_chunkManager;

        /// <summary>
        /// When ActiveInEditMode the mesh for the tilemap will be created and rendered in edit mode.
        /// This is useful if you want to use the map editing GUI.
        /// A benefit to disabling it is a smaller file size on the scene (especially when using MeshMode.SingleQuad),
        /// since the data is still stored and the mesh will be generated when entering play mode.
        /// </summary>
        public bool ActiveInEditMode
        {
            get { return m_activeInEditMode; }
            set
            {
                if (m_activeInEditMode == value)
                    return;
                m_activeInEditMode = value;

                if (Application.isEditor)
                {
                    if (m_activeInEditMode)
                        CreateMesh();
                    else
                        DestroyMesh();
                }
            }
        }

        public TileMeshSettings MeshSettings
        {
            get { return m_tileMeshSettings; }
            set
            {
                ChunkManager.Settings = value;
                m_tileMeshSettings = value;
                m_tileMapData.SetSize(m_tileMeshSettings.TilesX, m_tileMeshSettings.TilesY);
            }
        }

        public TileSheet TileSheet
        {
            get { return m_tileSheet; }
        }

        private TileChunkManager ChunkManager
        {
            get
            {
                if (m_chunkManager == null)
                {
                    Debug.Log("Recreating TileMeshGrid");
                    m_chunkManager = new TileChunkManager();
                    m_chunkManager.Initialize(this, m_tileMeshSettings);
                }
                return m_chunkManager;
            }
        }
        
        public bool HasMesh
        {
            get { return ChunkManager.Chunk != null; }
        }

        protected virtual void Awake()
        {
            if (m_tileMeshSettings == null)
                m_tileMeshSettings = new TileMeshSettings(2, 2, 16, 1f, MeshMode.SingleQuad);

            if (m_tileSheet == null)
                m_tileSheet = ScriptableObject.CreateInstance<TileSheet>();

            if (m_chunkManager == null)
            {
                m_chunkManager = new TileChunkManager();
                m_chunkManager.Initialize(this, m_tileMeshSettings);
            }

            if (m_tileMapData == null)
            {
                m_tileMapData = new TileMapData();
                m_tileMapData.SetSize(m_tileMeshSettings.TilesX, m_tileMeshSettings.TilesY);
            }

            if (Application.isPlaying || m_activeInEditMode)
                CreateMesh();
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

        public void CreateMesh()
        {
            // initialize mesh grid
            if (!ChunkManager.Initialized)
                ChunkManager.Initialize(this, m_tileMeshSettings);
            else
                ChunkManager.Settings = m_tileMeshSettings;

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

        public void DestroyMesh()
        {
            ChunkManager.DeleteAllChunks();
        }

        /// <summary>
        /// Get the bounding box of a single tile in local coordinates.
        /// This is useful for positioning GameObjects that are children of the Tilemap.
        /// </summary>
        public Rect GetTileBoundsLocal(int x, int y)
        {
            return ChunkManager.Chunk.GetTileBoundsLocal(x, y);
        }

        /// <summary>
        /// Get the bounding box of a single tile in world/scene coordinates.
        /// This is useful for positioning GameObjects that are not children of the Tilemap.
        /// </summary>
        public Rect GetTileBoundsWorld(int x, int y)
        {
            return ChunkManager.Chunk.GetTileBoundsWorld(x, y);
        }

        public void PaintTile(int x, int y, Color color)
        {
            var child = ChunkManager.Chunk;
            if (child == null)
                throw new InvalidOperationException("MeshGrid has not yet been created.");
            var singleQuad = child as TileMeshSingleQuadBehaviour;
            if (singleQuad == null)
                throw new InvalidOperationException("Painting tiles is only supported in SingleQuad MeshMode");
            singleQuad.SetTile(x, y, color);
        }

        private void SetTile(int x, int y, Sprite sprite)
        {
            if (sprite == null)
                throw new ArgumentNullException("sprite");
            ChunkManager.Chunk.SetTile(x, y, sprite);
        }

        private void SetTile(int x, int y, int id)
        {
            var sprite = m_tileSheet.Get(id);
            ChunkManager.Chunk.SetTile(x, y, sprite);
        }
    }
}
