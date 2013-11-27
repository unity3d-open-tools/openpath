using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OPAStar : MonoBehaviour
{

    //Lists
    static OPPriorityQueue openList;
    static OPPriorityQueue closedList;

    static List<OPNode> Search(OPNode start, OPNode goal, OPMap map, float heuristicWeight)
    {
        Debug.Log("OPAStar | Searching for best route from " + start.position + " to " + goal.position);

        //Add the starting node to the open list
        openList = new OPPriorityQueue();
        openList.Push(start);
        start.costSoFar = 0;
        start.estimatedTotalCost = HeuristicEstimate(start, goal, heuristicWeight);

        closedList = new OPPriorityQueue();

        OPNode currentNode = null;

        //While the open list is not empty
        while (openList.GetLength() != 0)
        {
            //current node = node from the open list with the lowest cost
            currentNode = openList.Front();

            if (currentNode == goal) { break; }

            //examine each node adjacent to the current node

            List<OPNode> neighbors = map.GetNeighbors(currentNode);

            for (int nIndex = 0; nIndex != neighbors.Count; nIndex++)
            {
                //get the cost estimate for the end node
                OPNode endNode = (OPNode)neighbors[nIndex];
                float incrementalCost = GetCost(currentNode, endNode);
                float endNodeCost = currentNode.costSoFar + incrementalCost;

                //if the node is closed we may have to skip or remove it from the closed list
                if (closedList.Contains(endNode))
                {
                    //if we didn't find a shorter route; skip
                    if (endNode.costSoFar <= endNodeCost) { continue; }

                    //otherwise remove it from the closed list
                    closedList.Remove(endNode);
                }
                //skip if the node is open and we haven't foudn a better route
                else if (openList.Contains(endNode))
                {
                    //if our route is no better, then skip
                    if (endNode.costSoFar <= endNodeCost) { continue; }
                }

                float endNodeHeuristic = HeuristicEstimate(endNode, goal, heuristicWeight);

                //we are here if we need to update the node
                //update the cost and estimate
                endNode.costSoFar = endNodeCost;
                endNode.parent = currentNode;
                endNode.estimatedTotalCost = endNodeCost + endNodeHeuristic;

                //and add it to the open list
                if (!openList.Contains(endNode))
                {
                    openList.Push(endNode);
                }
            }

            // We've finished looking at the neighbors for the current node, so add it to the closed list and remove it from the open list
            closedList.Push(currentNode);
            openList.Remove(currentNode);
        }

        if (!currentNode.Equals(goal))
        {
            Debug.LogError("OpenPath | Path not found!");
            //return the empty array
            return new List<OPNode>();
        }
        else
        {
            //path complete
            Debug.Log("OPAStar | Path found!");
        }
    }

    private static float HeuristicEstimate(OPNode currNode, OPNode goal, float heuristicWeight)
    {
        return (((((currNode.position - goal.position)).magnitude) * heuristicWeight));
    }

    private static float GetCost(OPNode node0, OPNode node1)
    {
        return ((node0.position - node1.position).magnitude);
    }

    //helper method used to build path for AStar search
    private static List<OPNode> GetPath(OPNode node)
    {
        List<OPNode> path = new List<OPNode>();

        //transverse the path from goal to start

        int counter = 0;

        while (node != null)
        {
            if (counter > 100)
            {
                Debug.LogError("OpenPath | Screech! Failsafe Engaged!");
                return new List<OPNode>();
            }
            path.Add(node);
            node = node.parent;
            counter++;
        }

        //reverse it
        path.Reverse();
        return path;
    }

    //get straight line distance between two points
    public static float EnclideanDistance(Vector3 point1, Vector3 point2)
    {
        return ((point1 - point2).magnitude);
    }

    //get manhatten between two points
    public static float ManhattanDistance(Vector3 point1, Vector3 point2)
    {
        return (Mathf.Abs(point1.x - point2.x) + Mathf.Abs(point1.y - point2.y) + Mathf.Abs(point1.z - point2.z));
    }

}