using System.Linq;
using UnityEngine;
using System;

namespace UnityTileMap
{
    // TODO this class is supposed to handle the logic for splitting up the tilemap into parts, but its currently just an empty shell
    // TODO rename to TileChunkManager or something better
    public class TileChunkManager
    {
        private const string ChunkNameFormat = "Mesh@{0},{1}";

        /// <summary>
        /// All chunks are children of the TileMapBehaviour GameObject transform.
        /// </summary>
        private TileMapBehaviour m_parent;

        /// <summary>
        /// All chunks use the same settings.
        /// </summary>
        private TileMeshSettings m_settings;

        /// <summary>
        /// Chunk cache, these should be able to be created and destroyed at will since the actual data
        /// (stating which tile id is on which coordinate) lives in the TileMapData class.
        /// </summary>
        private readonly Grid<TileMeshBehaviour> m_chunks = new Grid<TileMeshBehaviour>();

        /// <summary>
        /// The number of tiles on each chunk.
        /// </summary>
        public Vector2 ChunkSize
        {
            get
            {
                if (m_settings == null)
                    return Vector2.zero;

                // the size of each chunk is defined in settings
                return new Vector2(m_settings.TilesX, m_settings.TilesY);
            }
        }

        public bool Initialized { get; private set; }

        // TODO temporary utility property to use while the grid only supports one chunk
        public TileMeshBehaviour Chunk
        {
            get
            {
                var chunk = GetChunk(0, 0);

                // TODO horrible hack to make sure settings are applied, shouldnt be needed
                if (chunk.Settings == null)
                {
                    //Debug.Log("Force applying settings");
                    chunk.Settings = m_settings;
                }
                return chunk;
            }
        }

        public TileMeshSettings Settings
        {
            get { return m_settings; }
            set
            {
                if (!Initialized)
                    throw new InvalidOperationException("The TileMeshGrid needs to be initialized before applying settings");

                m_settings = value;

                var chunks = m_chunks.Where(x => x != null).ToList();
                if (chunks.Count == 0)
                {
                    // setup a single chunk
                    SetNumChunks(1, 1);
                    GetChunk(0, 0).Settings = value;
                }
                else
                {
                    // apply the settings to all children);
                    foreach (TileMeshBehaviour child in chunks)
                        child.Settings = m_settings;
                }
            }
        }

        public void Initialize(TileMapBehaviour parent, TileMeshSettings settings)
        {
            if (Initialized)
                throw new InvalidOperationException("Already initialized");
            m_parent = parent;
            m_settings = settings;
            Initialized = true;
        }

        public void DeleteAllChunks()
        {
            for (int i = 0; i < m_parent.transform.childCount; i++)
            {
                Transform child = m_parent.transform.GetChild(i);

                // TODO should children of the TileMap thats not chunks be allowed?
                if (!child.name.StartsWith("Mesh"))
                    continue;
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            m_chunks.Clear();
        }

        public void DeleteChunk(int x, int y)
        {
            var chunkName = string.Format(ChunkNameFormat, x, y);
            for (int i = 0; i < m_parent.transform.childCount; i++)
            {
                Transform child = m_parent.transform.GetChild(i);
                if (child.name == chunkName)
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                    m_chunks[x, y] = null;
                    return;
                }
            }
        }

        // TODO make public once more than one is supported
        /// <summary>
        /// Creates the array that holds the chunks.
        /// </summary>
        private void SetNumChunks(int x, int y)
        {
            // TODO this currently means that the number of chunks is known which doesnt fit the usecase of infinite levels
            m_chunks.SetSize(x, y, null);
        }

        /// <summary>
        /// Finds the chunk that holds the specified tile.
        /// </summary>
        internal TileMeshBehaviour GetChunk(int tileX, int tileY)
        {
            int chunkX = tileX / m_settings.TilesX;
            int chunkY = tileY / m_settings.TilesY;

            //Debug.Log(string.Format("Finding chunk: {0}, {1}", chunkX, chunkY));

            // check cache for chunk
            var chunk = m_chunks[chunkX, chunkY];
            if (chunk != null)
                return chunk;

            //Debug.Log("Children: " + m_parent.transform.childCount);

            // traverse children of parent
            var chunkName = string.Format(ChunkNameFormat, chunkX, chunkY);
            for (int i = 0; i < m_parent.transform.childCount; i++)
            {
                var child = m_parent.transform.GetChild(i);
                if (child.name == chunkName)
                {
                    var mesh = child.GetComponent<TileMeshBehaviour>();
                    if (mesh == null)
                    {
                        Debug.LogError(string.Format("Child '{0}' was found but didn't have a TileMeshBehaviour", child.name));
                        return null;
                        // TODO create and add it?
                    }
                    m_chunks[chunkX, chunkY] = mesh;
                    return mesh;
                }
            }

            // otherwise create it
            return CreateChunk(chunkX, chunkY);
        }

        private TileMeshBehaviour CreateChunk(int x, int y)
        {
            // TODO should this be moved to a factory?
            Type behaviourType;
            switch (m_settings.MeshMode)
            {
                case MeshMode.SingleQuad:
                    behaviourType = typeof(TileMeshSingleQuadBehaviour);
                    break;
                case MeshMode.QuadGrid:
                    behaviourType = typeof(TileMeshQuadGridBehaviour);
                    break;
                default:
                    throw new InvalidOperationException("Mesh mode not implemented: " + m_settings.MeshMode);
            }

            var gameObject = new GameObject(string.Format(ChunkNameFormat, x, y), behaviourType);
            gameObject.transform.parent = m_parent.transform;

            // TODO calculate proper position based on chunksize and tilesize
            gameObject.transform.localPosition = Vector3.zero;

            // apply settings
            var mesh = gameObject.GetComponent<TileMeshBehaviour>();
            mesh.Settings = m_settings;

            // cache
            m_chunks[x, y] = mesh;
            return mesh;
        }
    }
}
