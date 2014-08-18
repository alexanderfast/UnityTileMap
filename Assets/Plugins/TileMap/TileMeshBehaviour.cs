using System;
using UnityEngine;

namespace UnityTileMap
{
    /// <summary>
    /// The base class for behaviours that holds and renders the actual mesh that is the TileMap.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public abstract class TileMeshBehaviour : MonoBehaviour
    {
        private bool m_initialized;
        private Material m_material;
        private TileMeshSettings m_settings;
        private Mesh m_mesh;

        public virtual TileMeshSettings Settings
        {
            get { return m_settings; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (m_settings != null && m_settings.Equals(value))
                {
                    //Debug.Log("Settings equal, doing nothing");
                    return;
                }

                if (value.TilesX < 0)
                    throw new ArgumentException("TilesX cannot be less than zero");
                if (value.TilesY < 0)
                    throw new ArgumentException("TilesY cannot be less than zero");
                if (value.TileResolution < 0)
                    throw new ArgumentException("TilesResolution cannot be less than zero");
                if (value.TileSize < 0f)
                    throw new ArgumentException("TileSize cannot be less than zero");

                m_settings = value;

                if (m_material == null)
                    m_material = new Material(Shader.Find("Sprites/Default")) {color = Color.white};

                m_mesh = CreateMesh();

                var meshFilter = GetComponent<MeshFilter>();
                meshFilter.mesh = m_mesh;

                var meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.material = m_material;
            }
        }

        protected Material Material
        {
            get { return m_material ?? (m_material = new Material(Shader.Find("Sprites/Default")) {color = Color.white}); }
        }

        protected Texture MaterialTexture
        {
            get { return Material.GetTexture("_MainTex"); }
            set { Material.SetTexture("_MainTex", value); }
        }

        public abstract void SetTile(int x, int y, Sprite sprite);

        public Rect GetTileBoundsLocal(int x, int y)
        {
            var size = m_settings.TileSize;
            return new Rect(
                x * size,
                y * size,
                size,
                size);
        }

        public Rect GetTileBoundsWorld(int x, int y)
        {
            var rect = GetTileBoundsLocal(x, y);
            var position = transform.position;
            rect.x += position.x;
            rect.y += position.y;
            return rect;
        }

        protected abstract Mesh CreateMesh();
    }
}
