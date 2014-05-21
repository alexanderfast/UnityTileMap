using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityTileMap;

[Serializable]
[CustomEditor(typeof(TileMapBehaviour))]
public class TileMapBehaviourInspector : Editor
{
    [SerializeField]
    private bool m_showTiles = false;

    [SerializeField]
    private bool m_showSprites;

    [SerializeField]
    private bool m_sortSpritesByName;

    private readonly Dictionary<string, Texture2D> m_thumbnailCache = new Dictionary<string, Texture2D>();

    private TileMapBehaviour m_tileMap;
    private TileSheet m_tileSheet;

    private int m_tilesX;
    private int m_tilesY;
    private int m_tileResolution;
    private float m_tileSize;

    private void OnEnable()
    {
        m_tileMap = (TileMapBehaviour)target;
        m_tileSheet = m_tileMap.TileSheet;

        var meshSettings = m_tileMap.MeshSettings;
        if (meshSettings != null)
        {
            m_tilesX = meshSettings.TilesX;
            m_tilesY = meshSettings.TilesY;
            m_tileResolution = meshSettings.TileResolution;
            m_tileSize = meshSettings.TileSize;
        }
    }

    public override void OnInspectorGUI()
    {
//		base.OnInspectorGUI();

        m_tilesX = EditorGUILayout.IntField(
            new GUIContent("Tiles X", "The number of tiles on the X axis"),
            m_tilesX);
        m_tilesY = EditorGUILayout.IntField(
            new GUIContent("Tiles Y", "The number of tiles on the Y axis"),
            m_tilesY);
        m_tileResolution = EditorGUILayout.IntField(
            new GUIContent("Tile Resolution", "The number of pixels along each axis on one tile"),
            m_tileResolution);
        m_tileSize = EditorGUILayout.FloatField(
            new GUIContent("Tile Size", "The size of one tile in Unity units"),
            m_tileSize);
        if (GUILayout.Button("Resize"))
            m_tileMap.MeshSettings = new TileMeshSettings(m_tilesX, m_tilesY, m_tileResolution, m_tileSize);

        // TODO make a proper tile editing gui
        m_showTiles = EditorGUILayout.Foldout(m_showTiles, "Tiles:");
        if (m_showTiles)
        {
            var nameMap = new Dictionary<int, string>();
            var tileIds = m_tileSheet.Ids.ToList();
            foreach (var id in tileIds)
                nameMap[id] = m_tileSheet.Lookup(id);

            var tileNames = nameMap.Values.ToArray();
            var settings = m_tileMap.MeshSettings;

            for (int y = 0; y < settings.TilesY; y++)
            {
                for (int x = 0; x < settings.TilesX; x++)
                {
                    int selected = -1;
                    var id = m_tileMap[x, y];
                    if (id >= 0 && nameMap.ContainsKey(id))
                        selected = Array.IndexOf(tileNames, nameMap[id]);

                    var changed = EditorGUILayout.Popup(string.Format("{0}, {1}", x, y), selected, tileNames);
                    if (changed != selected)
                        m_tileMap[x, y] = m_tileSheet.Lookup(nameMap[changed]);
                }
            }
        }

        bool prominentImportArea = m_tileSheet.Ids.Count() == 0;
        m_showSprites = EditorGUILayout.Foldout(m_showSprites || prominentImportArea, "Sprites:");
        if (m_showSprites || prominentImportArea)
        {
            ShowImportDropArea();
        }
        if (m_showSprites && !prominentImportArea)
        {
            if (GUILayout.Button("Delete all"))
            {
                foreach (var id in m_tileMap.TileSheet.Ids)
                    m_tileMap.TileSheet.Remove(id);
                m_thumbnailCache.Clear();
            }

            if (GUILayout.Button("Refresh thumbnails"))
                m_thumbnailCache.Clear();

            m_sortSpritesByName = GUILayout.Toggle(m_sortSpritesByName, "Sort sprites by name");

            var ids = m_tileSheet.Ids.ToList();
            ids.Sort();
            for (int i = 0; i < ids.Count; i++)
            {
                var sprite = m_tileSheet.Get(ids[i]);
                ShowSprite(sprite);

                // add separators, except below the last one
                // could probably find a better looking one
                if (i < (ids.Count - 1))
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            }
        }

        EditorUtility.SetDirty(this);
    }

    public void ShowSprite(Sprite sprite)
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(new GUIContent(sprite.name, GetThumbnail(sprite), sprite.textureRect.ToString()));

        GUILayout.FlexibleSpace();

        // TODO would be nice to vertically center this button when larger sprites are used
        if (GUILayout.Button("Remove"))
            m_tileSheet.Remove(m_tileSheet.Lookup(sprite.name));

        EditorGUILayout.EndHorizontal();
    }

    public void ShowImportDropArea()
    {
        Event evt = Event.current;
        Rect rect = GUILayoutUtility.GetRect(0.0f, 20.0f, GUILayout.ExpandWidth(true));
        GUI.Box(rect, "Drag and drop Texture/Sprite here");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!rect.Contains(evt.mousePosition))
                    return;
//			if (evt.type != EventType.DragPerform)
//				return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
//			DragAndDrop.AcceptDrag();

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    // I dont know of a way to multiselect stuff in the asset view
                    // to drag and drop, so assume only one
                    var dropped = DragAndDrop.objectReferences.FirstOrDefault();
                    TryImport(dropped);

                    Event.current.Use();
                }
                break;
        }
    }

    private void TryImport(object obj)
    {
        var texture = obj as Texture2D;
        var sprite = obj as Sprite;
        if (texture != null)
            ImportTexture(texture);
        else if (sprite != null)
            ImportSprite(sprite);
    }

    private void ImportTexture(Texture2D texture)
    {
        if (!IsTextureReadable(texture))
        {
            Debug.LogError(string.Format("Texture '{0}' must be marked as readable", texture.name));
            return;
        }
        var assetPath = AssetDatabase.GetAssetPath(texture);
        var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        var sprites = assets.OfType<Sprite>().ToList();
        if (sprites.Count > 0)
        {
            foreach (var sprite in sprites)
            {
                if (m_tileSheet.Contains(sprite.name))
                    continue;
                m_tileSheet.Add(sprite);
            }
            Debug.Log(string.Format("{0} sprites loaded from {1}", sprites.Count, assetPath));
        }
        else
        {
            Debug.LogWarning(string.Format("No sprites found on asset path: {0}", assetPath));
        }
    }

    private void ImportSprite(Sprite sprite)
    {
        if (!IsTextureReadable(sprite.texture))
        {
            Debug.LogError(string.Format("Texture '{0}' must be marked as readable", sprite.texture.name));
            return;
        }
        if (m_tileSheet.Contains(sprite.name))
            Debug.LogError(string.Format("TileSheet already contains a sprite named {0}", sprite.name));
        else
            m_tileSheet.Add(sprite);
    }

    private Texture GetThumbnail(Sprite sprite)
    {
        Texture2D texture = null;
        if (!m_thumbnailCache.TryGetValue(sprite.name, out texture))
        {
            var rect = sprite.textureRect;
            texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
            texture.SetPixels(sprite.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height));
            texture.Apply(false, true);

            m_thumbnailCache[sprite.name] = texture;
        }
        return texture;
    }

    private static bool IsTextureReadable(Texture2D texture)
    {
        var path = AssetDatabase.GetAssetPath(texture);
        var textureImporter = (TextureImporter)AssetImporter.GetAtPath(path);
        return textureImporter.isReadable;
    }
}
