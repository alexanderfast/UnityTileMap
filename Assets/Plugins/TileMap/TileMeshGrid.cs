using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTileMap
{
    public class TileMeshGrid
    {
        private Grid<TileMeshBehaviour> m_meshes;
        private TileMapBehaviour m_parent;

        public TileMeshSettings Settings
        {
            get { return GetTileMeshBehaviour().Settings; }
            set { GetTileMeshBehaviour().Settings = value; }
        }

        internal void Initialize(TileMapBehaviour parent, TileMeshSettings tileMeshSettings)
        {
            if (m_parent != null)
                throw new InvalidOperationException("Already initialized");
            m_parent = parent;
            BuildMeshCache();
            Settings = tileMeshSettings;
        }

        public void SetTile(int x, int y, Color color)
        {
            GetTileMeshBehaviour().SetTile(x, y, color);
        }

        public void SetTile(int x, int y, Color[] colors)
        {
            // TODO find out which mesh in grid x, y belongs to and set tile on correct mesh
            GetTileMeshBehaviour().SetTile(x, y, colors);
        }

        public Rect GetTileBoundsLocal(int x, int y)
        {
            return GetTileMeshBehaviour().GetTileBoundsLocal(x, y);
        }

        public Rect GetTileBoundsWorld(int x, int y)
        {
            return GetTileMeshBehaviour().GetTileBoundsWorld(x, y);
        }

        private TileMeshBehaviour GetTileMeshBehaviour()
        {
            // TODO returns the one and only mesh, should create and use grid
            return GetTileMeshBehaviour(0, 0);
        }

        private TileMeshBehaviour GetTileMeshBehaviour(int x, int y)
        {
            BuildMeshCache();
            return m_meshes[x, y];
        }

        private void BuildMeshCache()
        {
            if (m_meshes != null)
                return; // already done
            m_meshes = new Grid<TileMeshBehaviour>();

            if (m_parent.transform.childCount == 0)
            {
                // add the one and only mesh
                //Debug.Log("Creating default mesh");
                var mesh = new GameObject("Mesh@0,0", typeof(TileMeshBehaviour));
                mesh.transform.parent = m_parent.transform;
                mesh.transform.position = Vector3.zero;
            }

            var found = new Dictionary<Vector2, TileMeshBehaviour>();
            for (int i = 0; i < m_parent.transform.childCount; i++)
            {
                Transform child = m_parent.transform.GetChild(i);
                if (!child.name.StartsWith("Mesh@"))
                {
                    // Debug.LogWarning("Unable to attach mesh: " + child.name);
                    continue;
                }

                var behaviour = child.GetComponentInChildren<TileMeshBehaviour>();
                if (behaviour == null)
                {
                    Debug.LogError(string.Format("Child '{0}' doesnt have a TileMeshBehaviour", child.name));
                    continue;
                }

                int x, y;
                try
                {
                    string[] coord = child.name.Split('@')[1].Split(',');
                    x = int.Parse(coord[0]);
                    y = int.Parse(coord[1]);
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Unable to parse '{0}': {1}", child.name, e.Message));
                    continue;
                }

                found[new Vector2(x, y)] = behaviour;
            }

            m_meshes = new Grid<TileMeshBehaviour>();
            m_meshes.SetSize(
                found.Keys.Select(x => (int)x.x).Max() + 1,
                found.Keys.Select(x => (int)x.y).Max() + 1,
                null);
            foreach (var pair in found)
            {
                m_meshes[(int)pair.Key.x, (int)pair.Key.y] = pair.Value;
            }
        }
    }
}
