using System;

namespace UnityTileMap
{
    /// <summary>
    /// A container describing the TileMap used for serialization.
    /// </summary>
    [Serializable]
    internal class TileMapData : Grid<int>
    {
        public void SetSize(int x, int y)
        {
            base.SetSize(x, y, -1);
        }
    }
}
