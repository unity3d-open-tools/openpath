#pragma strict

public enum OPMapType {
	Grid,
	WayPoint,
	NavMesh
}

class OPMap {
	var nodes : OPNode[] = null;

	function GetNode ( position : Vector3 ) : OPNode {
		for ( var node : OPNode in nodes ) {
			if ( node.position == position ) {
				return node;
			}
		}
		
		return null;
	}
	
	function ListToArray ( list : List.<OPNode> ) : OPNode[] {
		var newArray : OPNode[] = new OPNode[list.Count];
		
		for ( var i : int = 0; i < list.Count; i++ ) {
			newArray[i] = list[i];
		}
		
		return newArray;
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

public class OPNavMeshMap extends OPMap {
	function OPNavMeshMap ( navMesh : OPNavMesh ) {
		nodes = navMesh.GetNodes ();
	
		MonoBehaviour.Destroy ( navMesh.gameObject );
	}
}

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

public class OPGridMap extends OPMap {	
	var spacing : float;
	
	function OPGridMap ( start : Vector3, size : Vector3, gridSpacing : float, layerMask : LayerMask ) {
		var tempList : List.< OPNode > = new List.< OPNode >();
		
		spacing = gridSpacing;
								
		var x : int;
		var y : int;
		var z : int;
		
		for ( x = 0; x < size.x; x++ ) {
			for ( z = 0; z < size.z; z++ ) {
				var from : Vector3 = new Vector3 ( start.x + (x*spacing), start.y + (size.y*spacing), start.z + (z*spacing) );
				var hits : RaycastHit[] = RaycastContinuous ( from, layerMask );
				
				for ( var h : RaycastHit in hits ) {										
					var p : Vector3 = h.point; 
					var m : OPNode = new OPNode ( p.x, p.y, p.z );
					tempList.Add ( m );
				}
			}
		}
		
		nodes = tempList.ToArray();	
		
		for ( var a : OPNode in tempList ) {
			FindNeighbors ( a );
		}
	}
	
	private function RaycastContinuous ( from : Vector3, layerMask : LayerMask ) : RaycastHit[] {
		var hits : List.< RaycastHit > = new List.< RaycastHit > ();
		var currentHit : RaycastHit;
		
		if ( Physics.Raycast ( from, Vector3.down, currentHit, Mathf.Infinity, layerMask ) ) {
			hits.Add ( currentHit );
			
			for ( var i : int = 0; i < 50; i++ ) {
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
	
	private function FindNeighbors ( thisNode : OPNode ) {
		thisNode.neighbors.Clear ();
		
		for ( var thatNode : OPNode in nodes ) {			
			if ( ( thisNode.position - thatNode.position ).sqrMagnitude <= spacing * 2.1 ) {
				thisNode.neighbors.Add ( thatNode );
			}
		}
	}
}