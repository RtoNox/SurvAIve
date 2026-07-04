using System.Collections.Generic;
using UnityEngine;

public class PathNode2D : MonoBehaviour
{
    public enum NodeType
    {
        Ground,
        Air,
        Both
    }

    [Header("Node Settings")]
    [SerializeField] private NodeType nodeType = NodeType.Both;

    [Header("Connections")]
    [SerializeField] private List<PathNode2D> neighbors = new List<PathNode2D>();

    public NodeType Type => nodeType;
    public List<PathNode2D> Neighbors => neighbors;

    public bool CanBeUsedByGroundedEnemy()
    {
        return nodeType == NodeType.Ground || nodeType == NodeType.Both;
    }

    public bool CanBeUsedByFlyingEnemy()
    {
        return nodeType == NodeType.Air || nodeType == NodeType.Both;
    }

    private void OnDrawGizmos()
    {
        switch (nodeType)
        {
            case NodeType.Ground:
                Gizmos.color = Color.green;
                break;

            case NodeType.Air:
                Gizmos.color = Color.cyan;
                break;

            case NodeType.Both:
                Gizmos.color = Color.yellow;
                break;
        }

        Gizmos.DrawWireSphere(transform.position, 0.25f);

        Gizmos.color = Color.white;

        foreach (PathNode2D neighbor in neighbors)
        {
            if (neighbor != null)
            {
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }
}