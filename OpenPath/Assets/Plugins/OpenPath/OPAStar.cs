using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class OPAStar {
	// Lists
	[System.NonSerialized] private OPPriorityQueue openList;
	[System.NonSerialized] private OPPriorityQueue closedList;
			
	// Find a path and return a list of each step
	public IEnumerator Search ( OPNode start, OPNode goal, OPMap map, float heuristicWeight, List< OPNode > list, int maxCycles ) {
		Debug.Log(start.ToString() + ", " + goal.ToString());
        
        if ( start != null && goal != null ) {
            // Add the starting node to the open list
            openList = new OPPriorityQueue ();
            openList.Push ( start );
            start.costSoFar = 0;
            start.estimatedTotalCost = HeuristicEstimate ( start, goal, heuristicWeight );
            
            closedList = new OPPriorityQueue ();
            
            OPNode currentNode = null;
            int cycles = 0;

            // While the open list is not empty
            while ( openList.GetLength() != 0 ) {
                
                // Current node = node from the open list with the lowest cost
                currentNode = openList.Front();
                
                if ( currentNode == goal ) {
                    break;
                }
                
                // Examine each node adjacent to the current node
                List <OPNode> neighbors = map.GetNeighbors ( currentNode );

                for ( int nIndex = 0; nIndex != neighbors.Count; nIndex++ ) {		
                    // Get the cost estimate for the end node
                    OPNode endNode = (OPNode) neighbors[nIndex];
                    float incrementalCost = GetCost ( currentNode, endNode );
                    float endNodeCost = currentNode.costSoFar + incrementalCost;
                    
                    // If the node is closed we may have to skip or remove it from the closed list.
                    if ( closedList.Contains ( endNode ) ) {
                        // If we didn't find a shorter route, skip.
                        if ( endNode.costSoFar <= endNodeCost ) {
                            continue;
                        }
                        
                        // Otherwise remove it from the closed list
                        closedList.Remove( endNode );

                    // Skip if the node is open and we haven't found a better route
                    } else if ( openList.Contains ( endNode ) ) {
                        // If our route is no better, then skip
                        if(endNode.costSoFar <= endNodeCost ) {
                            continue;
                        }
                    }
                    
                    float endNodeHeuristic = HeuristicEstimate ( endNode, goal, heuristicWeight );
                    // We are here if we need to update the node
                    // Update the cost and estimate
                    endNode.costSoFar = endNodeCost;
                    endNode.parent = currentNode;
                    endNode.estimatedTotalCost = endNodeCost + endNodeHeuristic;
                    
                    // And add it to the open list
                    if ( !openList.Contains ( endNode ) ) {
                        openList.Push(endNode);
                    }
                }
                
                // We've finished looking at the neighbors for the current node, so add it to the closed list and remove it from the open list
                closedList.Push ( currentNode );
                openList.Remove ( currentNode );

                if ( cycles > maxCycles ) {
                    cycles = 0;
                    yield return null;
                
                } else {
                    cycles++;
                
                }			
            }
            
            if ( currentNode.Equals ( goal ) ) {
                // Path complete			
                GetPath ( currentNode, list );

            }
        }
	}
	
	private float HeuristicEstimate ( OPNode currNode, OPNode goal, float heuristicWeight ) {
		return ( currNode.position - goal.position ).magnitude * heuristicWeight;
	}
	
	private float GetCost ( OPNode node0, OPNode node1 ) {
		return ( node0.position - node1.position ).magnitude;
	}
		
	// Helper void used to build path for AStar search
	private void GetPath ( OPNode node, List< OPNode > list ) {
		// Traverse the path from goal to start
		int counter = 0;
		
		while ( node != null ) {
			if ( counter > 100 ) {
				Debug.LogError ( "OpenPath | Screech! Failsafe engaged." );
				return;
			}
			
			list.Add  ( node );
			node = node.parent;
			counter++;
		}
		
		// Reverse it
		list.Reverse();
	}
	
	// Get straight line distance between two points
	public float EuclideanDistance ( Vector3 point1, Vector3 point2 ) {
		return ( point1 - point2 ).magnitude;
	}
	
	// Get manhattan distance between two points
	public float ManhattanDistance ( Vector3 point1, Vector3 point2 ) {
		return Mathf.Abs ( point1.x - point2.x ) + Mathf.Abs ( point1.y - point2.y ) + Mathf.Abs ( point1.z - point2.z );
	}
}
