using System.Linq;
using System.Collections.Generic;

public interface IAStar<TNode>
{
    int HeuristicCostEstimate(TNode a, TNode b);

    IEnumerable<TNode> GetNeighbourNodes(TNode node);
}

/// <summary>
/// http://en.wikipedia.org/wiki/A*_search_algorithm#Pseudocode
/// </summary>
public static class AStarExtensions
{
    public static IEnumerable<TNode> Search<TNode>(this IAStar<TNode> astar, TNode start, TNode goal)
    {
        var closedSet = new HashSet<TNode>(); // nodes already evaluated
        var openSet = new HashSet<TNode> {start}; // nodes to be evaluated
        var cameFrom = new Dictionary<TNode, TNode>(); // the map of navigated nodes
        var g = new Dictionary<TNode, int>(); // g score for each node, cost from start along best path
        g[start] = 0;
        var f = new Dictionary<TNode, int>(); // f score for each node, estimated total cost from start to goal
        f[start] = g[start] + astar.HeuristicCostEstimate(start, goal);

        while (openSet.Count > 0)
        {
            // current is the node in openSet with lowest f score
            var current = openSet.First();
            foreach (var node in openSet)
            {
                if (f[node] < f[current])
                    current = node;
            }

            // back track and yield path if we have reached the destination);
            if (current.Equals(goal))
            {
                IList<TNode> path;
                ReconstructPath(cameFrom, goal, out path);
                return path;
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbour in astar.GetNeighbourNodes(current))
            {
                if (closedSet.Contains(neighbour))
                    continue;
                var tentativeG = g[current] + astar.HeuristicCostEstimate(current, neighbour); // TODO distance?

                if (!openSet.Contains(neighbour) || tentativeG < g[neighbour])
                {
                    cameFrom[neighbour] = current;
                    g[neighbour] = tentativeG;
                    f[neighbour] = g[neighbour] + astar.HeuristicCostEstimate(neighbour, goal);

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }

        return new TNode[] {};
    }

    // TODO convert this method to an iterative solution
    private static void ReconstructPath<TNode>(IDictionary<TNode, TNode> cameFrom, TNode current, out IList<TNode> path)
    {
        if (cameFrom.ContainsKey(current))
        {
            ReconstructPath(cameFrom, cameFrom[current], out path);
            path.Insert(path.Count, current);
        }
        else
        {
            path = new List<TNode> {current};
        }
    }
}
