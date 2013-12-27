using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OPWayPoint : MonoBehaviour
{

    public LayerMask layerMask;
    public OPNode node = new OPNode();

    public void FindNeighbors(OPWayPoint[] allnodes)
    {
        List<OPNode> tempList = new List<OPNode>();

        node.position = this.transform.position;

        foreach (OPWayPoint nodeContainer in allnodes)
        {
            if (nodeContainer != this)
            {
                RaycastHit hit;
                Vector3 here = this.transform.position + new Vector3(0f, 0.5f, 0f);
                Vector3 there = nodeContainer.transform.position;
                Vector3 direction = there - here;
                float distance = (here - there).magnitude;

                Physics.Raycast(here, direction, out hit, distance, layerMask);

                if ((hit.point != null) && hit.transform == nodeContainer.transform)
                {
                    tempList.Add(nodeContainer.node);
                }
            }
        }

        node.neighbors = tempList;
    }

    void Update()
    {
        node.position = this.transform.position;
    }
}