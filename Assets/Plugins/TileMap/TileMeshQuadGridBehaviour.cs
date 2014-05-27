using System;
using UnityEngine;

namespace UnityTileMap
{
    /// <summary>
    /// The mesh behaviour used for MeshMode.QuadGrid.
    /// </summary>
    public class TileMeshQuadGridBehaviour : TileMeshBehaviour
    {
        public override void SetTile(int x, int y, Sprite sprite)
        {
            // TODO should be flagged in inspector when sprites are set up
            var currentTexture = MaterialTexture;
            if (currentTexture != null && currentTexture != sprite.texture)
                throw new ArgumentException("Sprites from different textures is not supported in QuadGrid mode.");

            MaterialTexture = sprite.texture;

            int quadIndex = (y * base.Settings.TilesX) + x;
            SetTile(quadIndex, sprite);
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

        protected override Mesh CreateMesh()
        {
            var tileSize = base.Settings.TileSize;
            var tilesX = base.Settings.TilesX;
            var tilesY = base.Settings.TilesY;
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
                    vertices[qi] = new Vector3(vx, vy, 0);
                    vertices[qi + 1] = new Vector3(vx + tileSize, vy, 0);
                    vertices[qi + 2] = new Vector3(vx, vy + tileSize, 0);
                    vertices[qi + 3] = new Vector3(vx + tileSize, vy + tileSize, 0);

                    triangles[ti] = qi;
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

        private static Vector2 ToUv(Vector2 xy, Texture2D texture)
        {
            return new Vector2(xy.x / texture.width, xy.y / texture.height);
        }
    }
}
