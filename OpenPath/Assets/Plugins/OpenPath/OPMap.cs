using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum OPMapType {
	Grid,
	WayPoint,
	NavMesh
}

[System.Serializable]
public class OPMap {
	[System.NonSerialized] public OPNode[] nodes = null;

	public OPNode GetNode ( Vector3 position ) {
		foreach ( OPNode node in nodes ) {
			if ( node.position == position ) {
				return node;
			}
		}
		
		return null;
	}
	
	public int GetIndex ( OPNode node ) {
		return System.Array.IndexOf ( nodes, node );
	}
	
	public List<OPNode> GetNeighbors ( OPNode node ) {
		return node.neighbors;
	}
	
	public void Reset () {
		foreach ( OPNode n in nodes ) {
			if ( n != null ) {
				n.parent = null;
			}
		}
	}
}


////////////////////
// NavMesh
////////////////////
[System.Serializable]
public class OPNavMeshMap : OPMap {
	public OPNavMeshMap ( OPNavMesh navMesh ) {
		if ( navMesh == null ) {
			Debug.LogError ( "OPMap | No active NavMesh in scene!" );
		} else {
			nodes = navMesh.GetNodes ();
		}
	}
}


//////////////////
// Waypoint
//////////////////
[System.Serializable]
public class OPWayPointMap : OPMap {
	public OPWayPointMap ( OPWayPoint[] nodeContainers ) {
		List<OPNode> tempList = new List<OPNode>();
		
		foreach ( OPWayPoint n in nodeContainers ) {
			n.FindNeighbors ( nodeContainers );
			
			tempList.Add ( n.node );
		}
		
		nodes = tempList.ToArray();
				
		foreach ( OPWayPoint n in GameObject.FindObjectsOfType(typeof(OPWayPoint)) ) {
			MonoBehaviour.Destroy ( n.gameObject ); 
		}
	}
}


//////////////////
// Grid
//////////////////
[System.Serializable]
public class OPGridMap : OPMap {	
	float spacing;
	//private int count = 0;

	public OPGridMap ( Vector3 start, Vector3 size, float gridSpacing, LayerMask layerMask ) {
		List< OPNode > tempList = new List< OPNode >();
		
		spacing = gridSpacing;
								
		int x;
		int z;
		
		// Raycast from every point in a horizontal grid
		for ( x = 0; x < size.x; x++ ) {
			for ( z = 0; z < size.z; z++ ) {
				Vector3 from = new Vector3 ( start.x + (x*spacing), start.y + (size.y*spacing), start.z + (z*spacing) );
				RaycastHit[] hits = RaycastContinuous ( from, layerMask );
				
				// Add all hits to the list
				for ( int r = 0; r < hits.Length; r++ ) {										
					Vector3 p = hits[r].point; 
					OPNode n = new OPNode ( p.x, p.y, p.z );
					tempList.Add ( n );
				}
			}
		}
		
		nodes = tempList.ToArray();

		FindNeighbors ();	
	}
	
	// Raycast continuously through several objects
	private RaycastHit[] RaycastContinuous ( Vector3 from, LayerMask layerMask ) {
		List< RaycastHit > hits = new List< RaycastHit > ();
		RaycastHit currentHit = new RaycastHit();
		
		if ( Physics.Raycast ( from, Vector3.down, out currentHit, Mathf.Infinity, layerMask ) ) {
			hits.Add ( currentHit );
			
			// We're allowing maximum 10 consecutive hits to be detected
			for ( int i = 0; i < 10; i++ ) {
				if ( Physics.Raycast ( currentHit.point + Vector3.down, Vector3.down, out currentHit, Mathf.Infinity, layerMask ) ) {
					bool left = Physics.Raycast ( currentHit.point, Vector3.left, spacing/2, layerMask );
					bool right = Physics.Raycast ( currentHit.point, -Vector3.left, spacing/2, layerMask );
					bool forward = Physics.Raycast ( currentHit.point, Vector3.forward, spacing/2, layerMask );
					bool back = Physics.Raycast ( currentHit.point, -Vector3.forward, spacing/2, layerMask );
					bool up = Physics.Raycast ( currentHit.point, -Vector3.down, spacing/2, layerMask );
					
					if ( !left && !right && !forward && !back && !up ) {
						hits.Add ( currentHit );
					}
				
				} else {
					break;

				}
			}
		}
	
		return hits.ToArray();
	}

	// Locate neighbouring nodes
	private void FindNeighbors () {
		for ( int o = 0; o < nodes.Length; o++ ) {
			OPNode thisNode = nodes[o];
			
			for ( int i = 0; i < nodes.Length; i++ ) {			
				OPNode thatNode = nodes[i];

				if ( ( thisNode.position - thatNode.position ).sqrMagnitude <= spacing * 2.1 ) {
					thisNode.neighbors.Add ( thatNode );
				}
			}
		}
	}
}
