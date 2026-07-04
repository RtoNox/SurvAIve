using System.Collections.Generic;
using UnityEngine;

public class GroundPathNode : MonoBehaviour
{
    [System.Serializable]
    public class Connection
    {
        public GroundPathNode targetNode;
        public bool requiresJump;
    }

    [SerializeField] private List<Connection> connections = new List<Connection>();

    public IReadOnlyList<Connection> Connections => connections;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.15f);

        foreach (Connection connection in connections)
        {
            if (connection == null || connection.targetNode == null) continue;

            Gizmos.color = connection.requiresJump ? Color.yellow : Color.cyan;
            Gizmos.DrawLine(transform.position, connection.targetNode.transform.position);
        }
    }
}