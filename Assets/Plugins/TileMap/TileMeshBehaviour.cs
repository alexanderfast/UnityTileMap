using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UnityTileMap
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class TileMeshBehaviour : MonoBehaviour
    {
        private bool m_initialized;
        private Texture2D m_texture;
        private Material m_material;
        private bool m_textureDirty;
        private int m_tileResolution = 16;
        private float m_tileSize = 1.0f;
        private int m_tilesX = -1;
        private int m_tilesY = -1;

        public TileMeshSettings Settings
        {
            get { return new TileMeshSettings(m_tilesX, m_tilesY, m_tileResolution, m_tileSize); }
            set
            {
                if (Settings.Equals(value))
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

                m_tilesX = value.TilesX;
                m_tilesY = value.TilesY;
                m_tileResolution = value.TileResolution;
                m_tileSize = value.TileSize;

                //Debug.Log("Recreating tile mesh");

                CreateTexture();
                CreateMesh();
            }
        }

        private void LateUpdate()
        {
            if (m_textureDirty)
            {
                m_texture.Apply();
                m_textureDirty = false;
            }
        }

        /// <summary>
        /// Paint a tile with a solid color.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void SetTile(int x, int y, Color color)
        {
            SetTile(x, y, Enumerable.Repeat(color, m_tileResolution * m_tileResolution).ToArray());
        }

        /// <summary>
        /// Paint a tile with custom colors, usually a sprite.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colors"></param>
        public void SetTile(int x, int y, Color[] colors)
        {
            if (m_texture == null)
            {
                Debug.LogError("Texture has not been created");
                return;
            }

            // the texture has 0,0 in the bottom left, flip y to put it at upper left
            m_texture.SetPixels(
                x * m_tileResolution,
                m_texture.height - ((y + 1) * m_tileResolution),
                m_tileResolution,
                m_tileResolution,
                colors);

            if (Application.isPlaying)
                m_textureDirty = true;
            else
                m_texture.Apply();
        }

        public Rect GetTileBoundsLocal(int x, int y)
        {
            return new Rect(
                x * m_tileSize,
                (y + 1) * m_tileSize,
                m_tileSize,
                m_tileSize);
        }

        public Rect GetTileBoundsWorld(int x, int y)
        {
            var rect = GetTileBoundsLocal(x, y);
            var position = transform.position;
            rect.x += position.x;
            rect.y += position.y;
            return rect;
        }

        private void CreateTexture()
        {
            m_texture = new Texture2D(
                m_tilesX * m_tileResolution,
                m_tilesY * m_tileResolution,
                TextureFormat.RGBA32,
                false);
            m_texture.name = "TileMapTexture";

            // create a material holding the texture
            if (m_material == null)
                m_material = new Material(Shader.Find("Sprites/Default")) {color = Color.white};
            m_material.SetTexture("_MainTex", m_texture);

            // apply material to mesh
            var meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = m_material;
            //renderer.sharedMaterial = material;
            //renderer.sharedMaterial.mainTexture = m_texture;
        }

        private void CreateMesh()
        {
            var vertices = new Vector3[4];
            var triangles = new int[6];
            var normals = new Vector3[4];
            var uv = new Vector2[4];
            float sizeX = m_tilesX * m_tileSize;
            float sizeY = m_tilesY * m_tileSize;

            // vertices going clockwise
            // 0--1
            // |\ |
            // | \|
            // 2--3
            vertices[0] = new Vector3(0, 0, 0);
            vertices[1] = new Vector3(sizeX, 0, 0);
            vertices[2] = new Vector3(0, sizeY, 0);
            vertices[3] = new Vector3(sizeX, sizeY, 0);

            triangles[0] = 0;
            triangles[1] = 3;
            triangles[2] = 2;

            triangles[3] = 0;
            triangles[4] = 1;
            triangles[5] = 3;

            normals[0] = Vector3.forward;
            normals[1] = Vector3.forward;
            normals[2] = Vector3.forward;
            normals[3] = Vector3.forward;

            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(1, 0);
            uv[2] = new Vector2(0, 1);
            uv[3] = new Vector2(1, 1);

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.name = "TileMapMesh";

            var meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
        }
    }
}
