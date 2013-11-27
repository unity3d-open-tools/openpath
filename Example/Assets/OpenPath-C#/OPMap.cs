using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum OPMapType
{
    Grid, WayPoint, NavMesh
}

public class OPMap : MonoBehaviour {

    public OPNode[] nodes = null;

    OPNode GetNode(Vector3 position)
    {
        foreach (OPNode node in nodes)
        {
            if (node.position == position) { return node; }
        }
        return null;
    }

    OPNode[] ListToArray(List<OPNode> list)
    {
        OPNode[] newArray = new OPNode[list.Count];

        for (int i = 0; i < list.Count; i++)
        {
            newArray[i] = list[i];
        }
        return newArray;
    }

    int GetIndex(OPNode node)
    {
        return System.Array.IndexOf(nodes, node);
    }

    List<OPNode> GetNeighbors(OPNode node)
    {
        return node.neighbors;
    }

    void Reset()
    {
        foreach ( OPNode n in nodes ) {
			if ( n != null) {
				n.parent = null;
			}
		}
    }
}

public class OPNavMeshMap : OPMap
{
    void OPNavMeshMap(OPNavMesh navMesh)
    {
        nodes = navMesh.GetNodes();

        MonoBehaviour.Destroy(navMesh.gameObject);
    }
}

public class OPWayPointMap : OPMap
{
    void OPWayPointMap(OPWayPoint[] nodeContainers)
    {
        List<OPNode> tempList = new List<OPNode>();

        foreach (OPWayPoint n in nodeContainers)
        {
            n.FindNeighbors(nodeContainers);
            tempList.Add(n.node);
        }

        nodes = tempList.ToArray();

        foreach (OPWayPoint n in GameObject.FindObjectsOfType<OPWayPoint>())
        {
            MonoBehaviour.Destroy(n.gameObject);
        }
    }
}

public class OPGridMap : OPMap
{
    float spacing;

    void OPGridMap(Vector3 start, Vector3 size, float gridSpacing, LayerMask layerMask)
    {
        List<OPNode> tempList = new List<OPNode>();

        spacing = gridSpacing;

        int x;
        int y;
        int z;

        for (x = 0; x < size.z; x++)
        {
            for (z = 0; z < size.z; z++)
            {
                Vector3 from = new Vector3(start.x + (x * spacing), start.y + (size.y * spacing), start.z + (z * spacing));
                RaycastHit[] hits = RaycastContinuous(from, layerMask);

                foreach(RaycastHit h in hits){
                    Vector3 p = h.point;
                    OPNode m = new OPNode(p.x, p.y, p.z);
                    tempList.Add(m);
                }
            }
        }

        nodes = tempList.ToArray();

        foreach (OPNode a in tempList)
        {
            FindNeighbors(a);
        }
    }

    private RaycastHit[] RaycastContinuous(Vector3 from, LayerMask layerMask)
    {
        List<RaycastHit> hits = new List<RaycastHit>();
        RaycastHit currentHit;

        if (Physics.Raycast(from, Vector3.down, out currentHit, Mathf.Infinity, layerMask))
        {
            hits.Add(currentHit);

            for (int i = 0; i < 50; i++)
            {
                if (Physics.Raycast(currentHit.point + Vector3.down, Vector3.down, out currentHit, Mathf.Infinity, layerMask))
                {
                    bool left = Physics.Raycast(currentHit.point, Vector3.left, spacing / 2, layerMask);
                    bool right = Physics.Raycast(currentHit.point, Vector3.right, spacing / 2, layerMask);
                    bool forward = Physics.Raycast(currentHit.point, Vector3.forward, spacing / 2, layerMask);
                    bool back = Physics.Raycast(currentHit.point, -Vector3.forward, spacing / 2, layerMask);
                    bool up = Physics.Raycast(currentHit.point, Vector3.up, spacing / 2, layerMask);

                    if (!left && !right && !forward && !back && !up)
                    {
                        hits.Add(currentHit);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        return hits.ToArray();
    }

    private void FindNeighbors(OPNode thisNode)
    {
        thisNode.neighbors.Clear();

        foreach (OPNode thatNode in nodes)
        {
            if ((thisNode.position - thatNode.position).sqrMagnitude <= spacing * 2.1f)
            {
                thisNode.neighbors.Add(thatNode);
            }
        }
    }
}