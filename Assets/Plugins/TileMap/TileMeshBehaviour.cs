using System;
using System.Linq;
using UnityEngine;

namespace UnityTileMap
{
    // TODO would it be easier to understand if this class was refactored into two, one for each mode?

    /// <summary>
    /// The class that holds and renders the actual mesh that is the TileMap.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class TileMeshBehaviour : MonoBehaviour
    {
        private bool m_initialized;
        private Texture2D m_texture;
        private Material m_material;
        private bool m_textureDirty;
        private TileMeshSettings m_settings;
        private Mesh m_mesh;

        public TileMeshSettings Settings
        {
            get { return m_settings; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (m_settings != null && m_settings.Equals(value))
                {
                    Debug.Log("Settings equal, doing nothing");
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
                    m_material = new Material(Shader.Find("Sprites/Default")) { color = Color.white };

                if (m_settings.MeshMode == MeshMode.SingleQuad)
                {
                    CreateTexture();
                    CreateMeshSingleQuad();
                }
                else if (m_settings.MeshMode == MeshMode.QuadGrid)
                {
                    m_texture = null;
                    CreateMeshQuadGrid();
                }
                else
                {
                    throw new ArgumentException("Unsupported MeshMode: " + m_settings.MeshMode);
                }

                var meshFilter = GetComponent<MeshFilter>();
                meshFilter.mesh = m_mesh;

                var meshRenderer = GetComponent<MeshRenderer>();
                meshRenderer.material = m_material;
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

        public void SetTile(int x, int y, Sprite sprite)
        {
            if (m_settings == null)
                throw new InvalidOperationException("Settings have not been set");

            //Debug.Log(string.Format("Setting tile {0},{1} to {2}", x, y, sprite.name));

            switch (m_settings.MeshMode)
            {
                case MeshMode.SingleQuad:
                    var rect = sprite.rect;
                    var colors = sprite.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
                    SetTile(x, y, colors);
                    break;
                case MeshMode.QuadGrid:
                    // TODO should be flagged in inspector when sprites are set up
                    var currentTexture = m_material.GetTexture("_MainTex");
                    if (currentTexture != null && currentTexture != sprite.texture)
                        throw new ArgumentException("Sprites from different textures is not supported in QuadGrid mode.");

                    m_material.SetTexture("_MainTex", sprite.texture);

                    int quadIndex = (y * m_settings.TilesX) + x;
                    SetTile(quadIndex, sprite);
                    break;
                default:
                    throw new ArgumentException("Unsupported MeshMode: " + m_settings.MeshMode);
            }
        }

        /// <summary>
        /// Paint a tile with a solid color.
        /// </summary>
        public void SetTile(int x, int y, Color color)
        {
            var colors = Enumerable.Repeat(color, m_settings.TileResolution * m_settings.TileResolution).ToArray();
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

            var resolution = m_settings.TileResolution;

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

        private void SetTile(int quadIndex, Sprite sprite)
        {
            // TODO seems like i cant modify the uv coordinates in place

            quadIndex *= 4;
            var uv = GetComponent<MeshFilter>().sharedMesh.uv;
            var r = sprite.textureRect;

            // vertices going counter clockwise
            // 2--3
            // | /|
            // |/ |
            // 0--1
            uv[quadIndex] = ToUv(new Vector2(r.xMin, r.yMin), sprite.texture);
            uv[quadIndex + 1] = ToUv(new Vector2(r.xMax, r.yMin), sprite.texture);
            uv[quadIndex + 2] = ToUv(new Vector2(r.xMin, r.yMax), sprite.texture);
            uv[quadIndex + 3] = ToUv(new Vector2(r.xMax, r.yMax), sprite.texture);
            GetComponent<MeshFilter>().sharedMesh.uv = uv;
        }

        public Rect GetTileBoundsLocal(int x, int y)
        {
            var size = m_settings.TileSize;
            return new Rect(
                x * size,
                (y + 1) * size,
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

        private void CreateTexture()
        {
            m_texture = new Texture2D(
                m_settings.TilesX * m_settings.TileResolution,
                m_settings.TilesY * m_settings.TileResolution,
                TextureFormat.RGBA32,
                false);
            m_texture.name = "TileMapTexture";

            // create a material holding the texture
            if (m_material == null)
                m_material = new Material(Shader.Find("Sprites/Default")) {color = Color.white};
            m_material.SetTexture("_MainTex", m_texture);
        }

        private void CreateMeshSingleQuad()
        {
            var vertices = new Vector3[4];
            var triangles = new int[6];
            var normals = new Vector3[4];
            var uv = new Vector2[4];
            float sizeX = m_settings.TilesX * m_settings.TileSize;
            float sizeY = m_settings.TilesY * m_settings.TileSize;

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

            m_mesh = new Mesh();
            m_mesh.vertices = vertices;
            m_mesh.triangles = triangles;
            m_mesh.normals = normals;
            m_mesh.uv = uv;
            m_mesh.name = "TileMapMesh";
        }

        private void CreateMeshQuadGrid()
        {
            var tileSize = m_settings.TileSize;
            var tilesX = m_settings.TilesX;
            var tilesY = m_settings.TilesY;
            var quads = tilesX * tilesY; // one quad per tile

            var vertices = new Vector3[quads * 4];
            var triangles = new int[quads * 6];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];

            for (int y = 0; y < tilesY; y++)
            {
                for (int x = 0; x < tilesX; x++)
                {
                    var i = (y * tilesX) + x; // quad index
                    var qi = i * 4; // vertex index
                    var ti = i * 6;

                    // vertices going clockwise
                    // 2--3
                    // | /|
                    // |/ |
                    // 0--1
                    var vx = x * tileSize;
                    var vy = y * tileSize;
                    vertices[qi]     = new Vector3(vx, vy, 0);
                    vertices[qi + 1] = new Vector3(vx + tileSize, vy, 0);
                    vertices[qi + 2] = new Vector3(vx, vy + tileSize, 0);
                    vertices[qi + 3] = new Vector3(vx + tileSize, vy + tileSize, 0);

                    triangles[ti]     = qi;
                    triangles[ti + 1] = qi + 2;
                    triangles[ti + 2] = qi + 3;

                    triangles[ti + 3] = qi;
                    triangles[ti + 4] = qi + 3;
                    triangles[ti + 5] = qi + 1;
                }
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                normals[i] = Vector3.forward;
                uv[i] = Vector2.zero; // uv are set by assigning a tile
            }

            m_mesh = new Mesh();
            m_mesh.vertices = vertices;
            m_mesh.triangles = triangles;
            m_mesh.normals = normals;
            m_mesh.uv = uv;
            m_mesh.name = "TileMapMesh";
        }

        private static Vector2 ToUv(Vector2 xy, Texture2D texture)
        {
            return new Vector2(xy.x / texture.width, xy.y / texture.height);
        }
    }
}
