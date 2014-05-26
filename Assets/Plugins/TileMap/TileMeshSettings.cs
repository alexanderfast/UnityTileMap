using System;
using UnityEngine;

namespace UnityTileMap
{
    [Serializable]
    public class TileMeshSettings
    {
        /// <summary>
        /// The number of tiles on the x axis.
        /// </summary>
        [SerializeField]
        public int TilesX;

        /// <summary>
        /// The number of tiles on the y axis.
        /// </summary>
        [SerializeField]
        public int TilesY;

        /// <summary>
        /// The number of pixels along each axis on a tile.
        /// </summary>
        [SerializeField]
        public int TileResolution = 16;

        /// <summary>
        /// The size of one tile in Unity units.
        /// </summary>
        [SerializeField]
        public float TileSize = 1f;

        [SerializeField]
        public MeshMode MeshMode = MeshMode.SingleQuad;

        public TileMeshSettings()
        {
        }

        public TileMeshSettings(int tilesX, int tilesY) : this(tilesX, tilesY, 16)
        {
        }

        public TileMeshSettings(int tilesX, int tilesY, int tileResolution) : this(tilesX, tilesY, tileResolution, 1f)
        {
        }

        public TileMeshSettings(int tilesX, int tilesY, int tileResolution, float tileSize) : this(tilesX, tilesY, tileResolution, tileSize, MeshMode.SingleQuad)
        {
        }

        public TileMeshSettings(int tilesX, int tilesY, int tileResolution, float tileSize, MeshMode meshMode)
        {
            TilesX = tilesX;
            TilesY = tilesY;
            TileResolution = tileResolution;
            TileSize = tileSize;
            MeshMode = meshMode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            var o = obj as TileMeshSettings;
            if (o == null)
                return false;
            return TilesX == o.TilesX && TilesY == o.TilesY && TileResolution == o.TileResolution && TileSize == o.TileSize && MeshMode == o.MeshMode;
        }

        public override int GetHashCode()
        {
            return TilesX.GetHashCode() ^ TilesY.GetHashCode() ^ TileResolution.GetHashCode() ^ TileSize.GetHashCode() ^ MeshMode.GetHashCode();
        }
    }
}
