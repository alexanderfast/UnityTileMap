using UnityEngine;
using System;

namespace UnityTileMap
{
    // TODO this class is supposed to handle the logic for splitting up the tilemap into parts, but its currently just an empty shell
    public class TileMeshGrid
    {
        private TileMapBehaviour m_parent;
        private TileMeshSettings m_settings;
        private TileMeshBehaviour m_child; // TODO should support more than one

        public bool Initialized { get; private set; }

        public TileMeshSettings Settings
        {
            get { return m_child == null ? null : m_child.Settings; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
        
                m_settings = value;
                RecreateChild();
            }
        }

        public TileMeshBehaviour Child
        {
            get { return m_child; }
        }

        public void Initialize(TileMapBehaviour parent, TileMeshSettings settings)
        {
            m_parent = parent;
            m_settings = settings;
            Initialized = true;
            RecreateChild();
        }

        public void DeleteAllChildren()
        {
            for (int i = 0; i < m_parent.transform.childCount; i++)
            {
                Transform child = m_parent.transform.GetChild(i);
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        private void RecreateChild()
        {
            DeleteAllChildren();

            if (m_parent.transform.childCount == 0)
            {
                // add the one and only mesh
                var type = typeof(TileMeshBehaviour);
                var mesh = new GameObject("Mesh@0,0", type);
                mesh.transform.parent = m_parent.transform;
                mesh.transform.localPosition = Vector3.zero;
                m_child = (TileMeshBehaviour)mesh.GetComponent(type);
                m_child.Settings = m_settings;
            }
            else
            {
                throw new ArgumentException("Didn't cleane up children");
            }
        }

        public void SetTile(int x, int y, Sprite sprite)
        {
            m_child.SetTile(x, y, sprite);
        }

        public Rect GetTileBoundsLocal(int x, int y)
        {
            return m_child.GetTileBoundsLocal(x, y);
        }

        public Rect GetTileBoundsWorld(int x, int y)
        {
            return m_child.GetTileBoundsWorld(x, y);
        }
    }
}
