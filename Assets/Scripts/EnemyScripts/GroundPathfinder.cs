using System.Collections.Generic;
using UnityEngine;

public class GroundPathfinder : MonoBehaviour
{
    [SerializeField] private List<GroundPathNode> nodes = new List<GroundPathNode>();

    public List<GroundPathNode.Connection> FindPath(Vector2 startPosition, Vector2 targetPosition)
    {
        GroundPathNode startNode = GetClosestNode(startPosition);
        GroundPathNode targetNode = GetClosestNode(targetPosition);

        if (startNode == null || targetNode == null)
        {
            return new List<GroundPathNode.Connection>();
        }

        if (startNode == targetNode)
        {
            return new List<GroundPathNode.Connection>();
        }

        Dictionary<GroundPathNode, GroundPathNode> cameFrom = new Dictionary<GroundPathNode, GroundPathNode>();
        Dictionary<GroundPathNode, GroundPathNode.Connection> connectionUsed = new Dictionary<GroundPathNode, GroundPathNode.Connection>();
        Dictionary<GroundPathNode, float> costSoFar = new Dictionary<GroundPathNode, float>();

        List<GroundPathNode> openSet = new List<GroundPathNode>
        {
            startNode
        };

        cameFrom[startNode] = null;
        costSoFar[startNode] = 0f;

        while (openSet.Count > 0)
        {
            GroundPathNode current = GetLowestCostNode(openSet, costSoFar, targetNode);

            if (current == targetNode)
            {
                break;
            }

            openSet.Remove(current);

            foreach (GroundPathNode.Connection connection in current.Connections)
            {
                if (connection == null || connection.targetNode == null) continue;

                GroundPathNode nextNode = connection.targetNode;

                float jumpCost = connection.requiresJump ? 2f : 0f;
                float newCost = costSoFar[current] +
                                Vector2.Distance(current.transform.position, nextNode.transform.position) +
                                jumpCost;

                if (!costSoFar.ContainsKey(nextNode) || newCost < costSoFar[nextNode])
                {
                    costSoFar[nextNode] = newCost;
                    cameFrom[nextNode] = current;
                    connectionUsed[nextNode] = connection;

                    if (!openSet.Contains(nextNode))
                    {
                        openSet.Add(nextNode);
                    }
                }
            }
        }

        if (!cameFrom.ContainsKey(targetNode))
        {
            return new List<GroundPathNode.Connection>();
        }

        return ReconstructPath(startNode, targetNode, cameFrom, connectionUsed);
    }

    private GroundPathNode GetClosestNode(Vector2 position)
    {
        GroundPathNode closestNode = null;
        float closestDistance = Mathf.Infinity;

        foreach (GroundPathNode node in nodes)
        {
            if (node == null) continue;

            float distance = Vector2.Distance(position, node.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    private GroundPathNode GetLowestCostNode(
        List<GroundPathNode> openSet,
        Dictionary<GroundPathNode, float> costSoFar,
        GroundPathNode targetNode)
    {
        GroundPathNode bestNode = openSet[0];
        float bestCost = GetTotalEstimatedCost(bestNode, costSoFar, targetNode);

        for (int i = 1; i < openSet.Count; i++)
        {
            GroundPathNode node = openSet[i];
            float cost = GetTotalEstimatedCost(node, costSoFar, targetNode);

            if (cost < bestCost)
            {
                bestCost = cost;
                bestNode = node;
            }
        }

        return bestNode;
    }

    private float GetTotalEstimatedCost(
        GroundPathNode node,
        Dictionary<GroundPathNode, float> costSoFar,
        GroundPathNode targetNode)
    {
        float currentCost = costSoFar[node];
        float heuristicCost = Vector2.Distance(node.transform.position, targetNode.transform.position);

        return currentCost + heuristicCost;
    }

    private List<GroundPathNode.Connection> ReconstructPath(
        GroundPathNode startNode,
        GroundPathNode targetNode,
        Dictionary<GroundPathNode, GroundPathNode> cameFrom,
        Dictionary<GroundPathNode, GroundPathNode.Connection> connectionUsed)
    {
        List<GroundPathNode.Connection> path = new List<GroundPathNode.Connection>();

        GroundPathNode current = targetNode;

        while (current != startNode)
        {
            GroundPathNode.Connection connection = connectionUsed[current];
            path.Add(connection);

            current = cameFrom[current];
        }

        path.Reverse();

        return path;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;

        foreach (GroundPathNode node in nodes)
        {
            if (node == null) continue;

            Gizmos.DrawWireSphere(node.transform.position, 0.25f);
        }
    }
}