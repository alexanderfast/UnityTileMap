using UnityEngine;
using UnityTileMap;
using System;
using System.Linq;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class TileMapVisibilityBehaviour : MonoBehaviour {

    const float alpha = .2f;

    private SpriteRenderer m_spriteRenderer;
    private TileMapBehaviour m_tileMap;
    private Texture2D m_texture;
    private bool m_textureDirty;
    private Grid<bool> m_visible;

    public void SetTileVisible(int x, int y, bool visible)
    {
        if (!m_visible.IsInBounds(x, y))
            return;
        if (m_visible[x, y] == visible)
            return;

        // TODO better colors
        var color = visible
            ? new Color(0, 1, 0, alpha)
            : new Color(0, 0, 1, alpha);

        m_visible[x, y] = visible;
        m_texture.SetPixel(x, y, color);
        m_textureDirty = true;
    }

    void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

	// Use this for initialization
	void Start () {
        // The TileMapVisibilityBehaviour should be created by the TileMapBehaviour as a child
        m_tileMap = transform.parent.GetComponent<TileMapBehaviour>();

        if (m_tileMap == null)
            throw new InvalidOperationException("TileMapVisibility GameObject is not a child of TileMap, did you create it yourself?");

        AttachToTileMap(m_tileMap);
	}
	
	// Update is called once per frame
	void Update () {
	}

    private void LateUpdate()
    {
        if (m_textureDirty && m_texture != null)
        {
            m_texture.Apply();
            m_textureDirty = false;
        }
    }

    public void AttachToTileMap(TileMapBehaviour tileMap)
    {
        m_tileMap = tileMap;

        // position the visibility graphics to match the tile map
        // TODO doesnt support moving the tile map after visibility has been attached
        transform.position = m_tileMap.transform.position;

        // create the texture that will cover the unseen parts of the map with black, one pixel per tile
        // TODO add support for resizing map after its been attached
        m_texture = new Texture2D(
            m_tileMap.MeshSettings.TilesX,
            m_tileMap.MeshSettings.TilesY,
            TextureFormat.RGBA32,
            false);
        m_texture.name = "TileMapVisibilityTexture";
        m_texture.filterMode = FilterMode.Point; // TODO filter mode selected by user?
        m_texture.alphaIsTransparency = true;

        // setup the grid to cache status, so we dont draw on the texture when we dont have to
        m_visible = new Grid<bool>();
        m_visible.SetSize(m_texture.width, m_texture.height, true);

        // paint all tiles invisible initially
        for (int x = 0; x < m_texture.width; x++)
            for (int y = 0; y < m_texture.height; y++)
                SetTileVisible(x, y, false);
        m_texture.Apply();

        // create a sprite that will cover the map, upscaling each pixel to cover a tile
        var textureRect = new Rect(0, 0, m_texture.width, m_texture.height);
        var sprite = Sprite.Create(m_texture, textureRect, Vector2.zero);
        sprite.name = "TileMapVisibilitySprite";
        
        m_spriteRenderer.sprite = sprite;

        // scale to cover tilemap
        transform.localScale = new Vector3(100, 100, 0f); // TODO is this because 100 pixels per unit?
        
        // setup the grid to cache status, so we dont draw on the texture when we dont have to
        m_visible = new Grid<bool>();
        m_visible.SetSize(m_texture.width, m_texture.height, false);
    }
}
