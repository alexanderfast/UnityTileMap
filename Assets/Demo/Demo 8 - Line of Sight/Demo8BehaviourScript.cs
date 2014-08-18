using UnityEngine;
using UnityTileMap;

public class Demo8BehaviourScript : MonoBehaviour {

    const int m_tileSize = 8;
    const int m_obstacleCount = 25;

	// Use this for initialization
	void Start () {

        var tileMap = GetComponent<TileMapBehaviour>();
        if (tileMap == null)
        {
            Debug.LogError("TileMap not found");
            return;
        }

        tileMap.MeshSettings = new TileMeshSettings
        {
            TileResolution = m_tileSize,
            TilesX = 30,
            TilesY = 20
        };

        // enable line of sight graphics
        tileMap.EnableVisibility();
        
        // create two tiles
        int wallId = tileMap.TileSheet.Add(TilePainter.CreateTileSprite(Color.black, m_tileSize));
        int floorId = tileMap.TileSheet.Add(TilePainter.CreateTileSprite(Color.gray, m_tileSize));

        // create walls and floor
        var painter = new TilePainter(tileMap);
        painter.Fill(floorId);
        painter.DrawRectangle(0, 0, tileMap.MeshSettings.TilesX, tileMap.MeshSettings.TilesY, wallId);

        // create some random blocks that will block sight
        for (int i = 0; i < m_obstacleCount; i++)
        {
            var x = Random.Range(1, tileMap.MeshSettings.TilesX - 2);
            var y = Random.Range(1, tileMap.MeshSettings.TilesY - 2);
            tileMap[x, y] = wallId;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
