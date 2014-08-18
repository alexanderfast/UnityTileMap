using UnityEngine;
using System.Collections;
using UnityTileMap;
using System;
using System.Linq;

/// <summary>
/// Helper class for drawing boxes, lines and stuff on the TileMap. Useful for debugging.
/// All tile id's used are assumed to exist in the TileMaps tile sheet.
/// </summary>
public class TilePainter
{
    private TileMapBehaviour m_tileMap;

    public TilePainter(TileMapBehaviour tileMap)
    {
        if (tileMap == null)
            throw new ArgumentNullException("tileMap");
        m_tileMap = tileMap;
    }

    public void Fill(int id)
    {
        DrawFilledRectangle(0, 0, m_tileMap.MeshSettings.TilesX, m_tileMap.MeshSettings.TilesY, id);
    }

    public void DrawRectangle(int x, int y, int sizeX, int sizeY, int id)
    {
        for (int ix = x; ix < (x + sizeX); ix++)
        {
            m_tileMap[ix, y] = id;
            m_tileMap[ix, y + sizeY - 1] = id;
        }
        for (int iy = y; iy < (y + sizeY); iy++)
        {
            m_tileMap[x, iy] = id;
            m_tileMap[x + sizeX - 1, iy] = id;
        }
    }

    public void DrawFilledRectangle(int x, int y, int sizeX, int sizeY, int id)
    {
        for (int ix = x; ix < (x + sizeX); ix++)
            for (int iy = y; iy < (y + sizeY); iy++)
                m_tileMap[ix, iy] = id;
    }

    // TODO this method probably belongs somewhere else
    public static Sprite CreateTileSprite(Color color, int size)
    {
        var texture = new Texture2D(size, size);
        var colors = Enumerable.Repeat<Color>(color, size * size).ToArray();
        texture.SetPixels(0, 0, size, size, colors);

        var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
        sprite.name = color.ToString();
        return sprite;
    }
}
