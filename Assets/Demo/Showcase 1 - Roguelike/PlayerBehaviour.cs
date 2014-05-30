using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTileMap;

public class PlayerBehaviour : MonoBehaviour
{
    private TileMapBehaviour m_tileMap;
    private LevelBehaviour m_levelBehaviour;
    private SceneFadeInOut m_sceneFadeInOut;
    private int m_x;
    private int m_y;
    private bool m_walking;

    // Use this for initialization
    private void Start()
    {
        var tileMapGameObject = GameObject.Find("TileMap");
        m_tileMap = tileMapGameObject.GetComponent<TileMapBehaviour>();
        if (m_tileMap == null)
            Debug.LogError("TileMapBehaviour not found");
        m_levelBehaviour = tileMapGameObject.GetComponent<LevelBehaviour>();
        if (m_levelBehaviour == null)
            Debug.LogError("LevelBehaviour not found");
        m_sceneFadeInOut = GameObject.Find("SceneFader").GetComponent<SceneFadeInOut>();
        if (m_sceneFadeInOut == null)
            Debug.LogError("SceneFadeInOut not found");
    }

    // Update is called once per frame
    private void Update()
    {
        ProcessInput();
    }

    private void ProcessInput()
    {
        if (m_walking)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            TryMoveTo(m_x, m_y + 1);
        if (Input.GetKeyDown(KeyCode.DownArrow))
            TryMoveTo(m_x, m_y - 1);
        if (Input.GetKeyDown(KeyCode.RightArrow))
            TryMoveTo(m_x + 1, m_y);
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            TryMoveTo(m_x - 1, m_y);
        if (Input.GetMouseButtonDown(0))
        {
            // we can make this assumption since the TileMap is on position 0, 0
            // TODO create a world coordinate to tile coordinate lookup

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var clicked = new Vector2Int((int)ray.origin.x, (int)ray.origin.y);
            m_walking = true;
            StartCoroutine(WalkTo(clicked, .2f));
        }
    }

    // TODO refactor into a reusable "TileWalker" behaviour
    private IEnumerator WalkTo(Vector2Int destination, float stepIntervalSeconds)
    {
        if (m_levelBehaviour.IsWalkeable(destination.x, destination.y))
        {
            var astar = new AStar(m_levelBehaviour);
            var path = astar.Search(new Vector2Int(m_x, m_y), destination).ToList();
            if (path.Count == 0)
            {
                Debug.Log("No path found");
                m_walking = false;
                yield break;
            }

            foreach (var i in path)
            {
                SetTilePosition(i.x, i.y);
                yield return new WaitForSeconds(stepIntervalSeconds);
            }
        }
        m_walking = false;
    }

    private void TryMoveTo(int x, int y)
    {
        if (m_levelBehaviour.IsWalkeable(x, y))
            SetTilePosition(x, y);
    }

    public void SetTilePosition(int x, int y)
    {
        m_x = x;
        m_y = y;
        var tileBounds = m_tileMap.GetTileBoundsWorld(x, y);
        transform.position = new Vector3(tileBounds.xMin, tileBounds.yMin, transform.position.z);

        // If we walk onto the stairs down...
        if (m_levelBehaviour.GetTile(m_x, m_y) == TileType.StairsDown)
            OnStairsDown();
    }

    private void OnStairsDown()
    {
        enabled = false;
        m_sceneFadeInOut.FadeOutThenIn(() =>
            {
                m_levelBehaviour.StartLevel();
                enabled = true;
            });
    }

    // TODO move this class to outer scope and refine logic specifically for grid
    private class AStarGrid : IAStar<Vector2Int>
    {
        public virtual int HeuristicCostEstimate(Vector2Int a, Vector2Int b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        public virtual IEnumerable<Vector2Int> GetNeighbourNodes(Vector2Int node)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                    yield return new Vector2Int(node.x + x, node.y + y);
            }
        }
    }

    private class AStar : AStarGrid
    {
        private readonly LevelBehaviour m_levelBehaviour;

        public AStar(LevelBehaviour levelBehaviour)
        {
            m_levelBehaviour = levelBehaviour;
        }

        public override IEnumerable<Vector2Int> GetNeighbourNodes(Vector2Int node)
        {
            // only return neighbour tiles that are walkable
            return base.GetNeighbourNodes(node).Where(x => m_levelBehaviour.IsWalkeable(x.x, x.y));
        }
    }
}
