using System;
using System.Linq;
using UnityEngine;

namespace UnityTileMap
{
    /// <summary>
    /// The mesh behaviour used for MeshMode.SingleQuad.
    /// </summary>
    public class TileMeshSingleQuadBehaviour : TileMeshBehaviour
    {
        private Texture2D m_texture;
        private bool m_textureDirty;

        public override TileMeshSettings Settings
        {
            get { return base.Settings; }
            set
            {
                // TODO a bit copy and paste code, but we only want to recreate the texture if settings changed
                if (value == null)
                    throw new ArgumentNullException("value");
                if (base.Settings != null && base.Settings.Equals(value))
                    return;
                base.Settings = value;
                CreateTexture();
            }
        }

        private void LateUpdate()
        {
            if (m_textureDirty && m_texture != null)
            {
                m_texture.Apply();
                m_textureDirty = false;
            }
        }

        public override void SetTile(int x, int y, Sprite sprite)
        {
            var rect = sprite.rect;
            var colors = sprite.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            SetTile(x, y, colors);
        }

        /// <summary>
        /// Paint a tile with a solid color.
        /// </summary>
        public void SetTile(int x, int y, Color color)
        {
            var colors = Enumerable.Repeat(color, base.Settings.TileResolution * base.Settings.TileResolution).ToArray();
            SetTile(x, y, colors);
        }

        /// <summary>
        /// Paint a tile with custom colors, usually a sprite.
        /// </summary>
        private void SetTile(int x, int y, Color[] colors)
        {
            if (m_texture == null)
            {
                Debug.LogError("Texture has not been created");
                return;
            }

            var resolution = base.Settings.TileResolution;

            // the texture has 0,0 in the bottom left, flip y to put it at upper left
            m_texture.SetPixels(
                x * resolution,
                y * resolution,
                resolution,
                resolution,
                colors);

            if (Application.isPlaying)
                m_textureDirty = true;
            else
                m_texture.Apply();
        }

        protected override Mesh CreateMesh()
        {
            var vertices = new Vector3[4];
            var triangles = new int[6];
            var normals = new Vector3[4];
            var uv = new Vector2[4];
            float sizeX = base.Settings.TilesX * base.Settings.TileSize;
            float sizeY = base.Settings.TilesY * base.Settings.TileSize;

            // vertices going clockwise
            // 2--3
            // | /|
            // |/ |
            // 0--1
            vertices[0] = new Vector3(0, 0, 0);
            vertices[1] = new Vector3(sizeX, 0, 0);
            vertices[2] = new Vector3(0, sizeY, 0);
            vertices[3] = new Vector3(sizeX, sizeY, 0);

            triangles[0] = 0;
            triangles[1] = 2;
            triangles[2] = 3;

            triangles[3] = 0;
            triangles[4] = 3;
            triangles[5] = 1;

            normals[0] = Vector3.forward;
            normals[1] = Vector3.forward;
            normals[2] = Vector3.forward;
            normals[3] = Vector3.forward;

            uv[0] = new Vector2(0, 0);
            uv[1] = new Vector2(1, 0);
            uv[2] = new Vector2(0, 1);
            uv[3] = new Vector2(1, 1);

            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                normals = normals,
                uv = uv,
                name = "TileMapMesh"
            };
            return mesh;
        }

        private void CreateTexture()
        {
            m_texture = new Texture2D(
                base.Settings.TilesX * base.Settings.TileResolution,
                base.Settings.TilesY * base.Settings.TileResolution,
                TextureFormat.RGBA32,
                false);
            m_texture.name = "TileMapTexture";

            MaterialTexture = m_texture;
        }
    }
}
