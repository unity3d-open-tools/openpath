#pragma strict

public enum OPMapType {
	Grid,
	WayPoint,
	NavMesh
}

public class OPMap {
	@NonSerialized public var nodes : OPNode[] = null;

	function GetNode ( position : Vector3 ) : OPNode {
		for ( var node : OPNode in nodes ) {
			if ( node.position == position ) {
				return node;
			}
		}
		
		return null;
	}
	
	function GetIndex ( node : OPNode ) : int {
		return System.Array.IndexOf ( nodes, node );
	}
	
	function GetNeighbors ( node : OPNode ) : List.<OPNode> {
		return node.neighbors;
	}
	
	function Reset () {
		for ( var n : OPNode in nodes ) {
			if ( n ) {
				n.parent = null;
			}
		}
	}
}


////////////////////
// NavMesh
////////////////////
public class OPNavMeshMap extends OPMap {
	function OPNavMeshMap ( navMesh : OPNavMesh ) {
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
public class OPWayPointMap extends OPMap {
	function OPWayPointMap ( nodeContainers : OPWayPoint[] ) {
		var tempList : List.<OPNode> = new List.<OPNode>();
		
		for ( var n : OPWayPoint in nodeContainers ) {
			n.FindNeighbors ( nodeContainers );
			
			tempList.Add ( n.node );
		}
		
		nodes = tempList.ToArray();
				
		for ( var n : OPWayPoint in GameObject.FindObjectsOfType(OPWayPoint) ) {
			MonoBehaviour.Destroy ( n.gameObject ); 
		}
	}
}


//////////////////
// Grid
//////////////////
public class OPGridMap extends OPMap {	
	var spacing : float;
	private var count : int = 0;

	function OPGridMap ( start : Vector3, size : Vector3, gridSpacing : float, layerMask : LayerMask ) {
		var tempList : List.< OPNode > = new List.< OPNode >();
		
		spacing = gridSpacing;
								
		var x : int;
		var z : int;
		
		// Raycast from every point in a horizontal grid
		for ( x = 0; x < size.x; x++ ) {
			for ( z = 0; z < size.z; z++ ) {
				var from : Vector3 = new Vector3 ( start.x + (x*spacing), start.y + (size.y*spacing), start.z + (z*spacing) );
				var hits : RaycastHit[] = RaycastContinuous ( from, layerMask );
				
				// Add all hits to the list
				for ( var r : int = 0; r < hits.Length; r++ ) {										
					var p : Vector3 = hits[r].point; 
					var n : OPNode = new OPNode ( p.x, p.y, p.z );
					tempList.Add ( n );
				}
			}
		}
		
		nodes = tempList.ToArray();

		FindNeighbors ();	
	}
	
	// Raycast continuously through several objects
	private function RaycastContinuous ( from : Vector3, layerMask : LayerMask ) : RaycastHit[] {
		var hits : List.< RaycastHit > = new List.< RaycastHit > ();
		var currentHit : RaycastHit;
		
		if ( Physics.Raycast ( from, Vector3.down, currentHit, Mathf.Infinity, layerMask ) ) {
			hits.Add ( currentHit );
			
			// We're allowing maximum 10 consecutive hits to be detected
			for ( var i : int = 0; i < 10; i++ ) {
				if ( Physics.Raycast ( currentHit.point + Vector3.down, Vector3.down, currentHit, Mathf.Infinity, layerMask ) ) {
					var left : boolean = Physics.Raycast ( currentHit.point, Vector3.left, spacing/2, layerMask );
					var right : boolean = Physics.Raycast ( currentHit.point, -Vector3.left, spacing/2, layerMask );
					var forward : boolean = Physics.Raycast ( currentHit.point, Vector3.forward, spacing/2, layerMask );
					var back : boolean = Physics.Raycast ( currentHit.point, -Vector3.forward, spacing/2, layerMask );
					var up : boolean = Physics.Raycast ( currentHit.point, -Vector3.down, spacing/2, layerMask );
					
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
	private function FindNeighbors () {
		for ( var o : int = 0; o < nodes.Length; o++ ) {
			var thisNode : OPNode = nodes[o];
			
			for ( var i : int = 0; i < nodes.Length; i++ ) {			
				var thatNode : OPNode = nodes[i];

				if ( ( thisNode.position - thatNode.position ).sqrMagnitude <= spacing * 2.1 ) {
					thisNode.neighbors.Add ( thatNode );
				}
			}
		}
	}
}
