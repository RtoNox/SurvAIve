using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager2D : MonoBehaviour
{
    public static PathfindingManager2D Instance { get; private set; }

    [Header("Pathfinding Settings")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private bool blockPathIfObstacleBetweenNodes = true;

    private PathNode2D[] allNodes;

    private void Awake()
    {
        Instance = this;
        allNodes = FindObjectsOfType<PathNode2D>();
    }

    public List<PathNode2D> FindPath(Vector2 startPosition, Vector2 targetPosition, bool isFlyingEnemy)
    {
        PathNode2D startNode = GetClosestUsableNode(startPosition, isFlyingEnemy);
        PathNode2D targetNode = GetClosestUsableNode(targetPosition, isFlyingEnemy);

        if (startNode == null || targetNode == null)
        {
            return null;
        }

        return RunAStar(startNode, targetNode, isFlyingEnemy);
    }

    private PathNode2D GetClosestUsableNode(Vector2 position, bool isFlyingEnemy)
    {
        PathNode2D closestNode = null;
        float closestDistance = Mathf.Infinity;

        foreach (PathNode2D node in allNodes)
        {
            if (node == null) continue;

            bool usable = isFlyingEnemy
                ? node.CanBeUsedByFlyingEnemy()
                : node.CanBeUsedByGroundedEnemy();

            if (!usable) continue;

            float distance = Vector2.Distance(position, node.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    private List<PathNode2D> RunAStar(PathNode2D startNode, PathNode2D targetNode, bool isFlyingEnemy)
    {
        List<PathNode2D> openSet = new List<PathNode2D>();
        HashSet<PathNode2D> closedSet = new HashSet<PathNode2D>();

        Dictionary<PathNode2D, PathNode2D> cameFrom = new Dictionary<PathNode2D, PathNode2D>();
        Dictionary<PathNode2D, float> gCost = new Dictionary<PathNode2D, float>();
        Dictionary<PathNode2D, float> fCost = new Dictionary<PathNode2D, float>();

        openSet.Add(startNode);

        gCost[startNode] = 0f;
        fCost[startNode] = GetHeuristic(startNode, targetNode);

        while (openSet.Count > 0)
        {
            PathNode2D currentNode = GetLowestFCostNode(openSet, fCost);

            if (currentNode == targetNode)
            {
                return ReconstructPath(cameFrom, currentNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (PathNode2D neighbor in currentNode.Neighbors)
            {
                if (neighbor == null) continue;
                if (closedSet.Contains(neighbor)) continue;

                bool usable = isFlyingEnemy
                    ? neighbor.CanBeUsedByFlyingEnemy()
                    : neighbor.CanBeUsedByGroundedEnemy();

                if (!usable) continue;

                if (blockPathIfObstacleBetweenNodes && IsBlocked(currentNode, neighbor))
                {
                    continue;
                }

                float tentativeGCost = gCost[currentNode] + Vector2.Distance(
                    currentNode.transform.position,
                    neighbor.transform.position
                );

                if (!gCost.ContainsKey(neighbor) || tentativeGCost < gCost[neighbor])
                {
                    cameFrom[neighbor] = currentNode;
                    gCost[neighbor] = tentativeGCost;
                    fCost[neighbor] = tentativeGCost + GetHeuristic(neighbor, targetNode);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private bool IsBlocked(PathNode2D fromNode, PathNode2D toNode)
    {
        Vector2 fromPosition = fromNode.transform.position;
        Vector2 toPosition = toNode.transform.position;

        Vector2 direction = toPosition - fromPosition;
        float distance = direction.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(
            fromPosition,
            direction.normalized,
            distance,
            obstacleLayer
        );

        return hit.collider != null;
    }

    private float GetHeuristic(PathNode2D a, PathNode2D b)
    {
        return Vector2.Distance(a.transform.position, b.transform.position);
    }

    private PathNode2D GetLowestFCostNode(List<PathNode2D> nodes, Dictionary<PathNode2D, float> fCost)
    {
        PathNode2D bestNode = nodes[0];
        float bestCost = fCost.ContainsKey(bestNode) ? fCost[bestNode] : Mathf.Infinity;

        for (int i = 1; i < nodes.Count; i++)
        {
            PathNode2D node = nodes[i];
            float cost = fCost.ContainsKey(node) ? fCost[node] : Mathf.Infinity;

            if (cost < bestCost)
            {
                bestNode = node;
                bestCost = cost;
            }
        }

        return bestNode;
    }

    private List<PathNode2D> ReconstructPath(Dictionary<PathNode2D, PathNode2D> cameFrom, PathNode2D currentNode)
    {
        List<PathNode2D> path = new List<PathNode2D>();
        path.Add(currentNode);

        while (cameFrom.ContainsKey(currentNode))
        {
            currentNode = cameFrom[currentNode];
            path.Add(currentNode);
        }

        path.Reverse();
        return path;
    }
}