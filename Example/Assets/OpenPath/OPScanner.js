#pragma strict

class OPScanner extends MonoBehaviour {
	var scanOnEnable : boolean = true;
	var layerMask : LayerMask;
	var gridSize : Vector3;
	var mapType : OPMapType;
	var map : OPMap = null;
	var heuristic : float = 10.0;
	var spacing : float = 1.0;
	var bounds : Bounds;
	
	function GetBounds () {
	    var bounds : Bounds = new Bounds ( Vector3.zero, Vector3.zero );
	    var pos : Vector3 = Vector3.zero;
	    
	    for ( var r : Renderer in FindObjectsOfType ( Renderer ) ) {
	    	bounds.Encapsulate ( r.bounds );
	    }
	    
	    pos = bounds.min;
	    
	    transform.position = pos;
		gridSize = new Vector3 ( Mathf.Round ( bounds.size.x / spacing ) + 1, Mathf.Round ( bounds.size.y / spacing ) + 1, Mathf.Round ( bounds.size.z / spacing ) + 1 );
	}
	
	function SetMap ( nodes : OPNode[] ) {
		map = new OPMap ();
		map.nodes = nodes;
	}
	
	function SetMap () {
		if ( mapType == OPMapType.Grid ) {
			map = new OPGridMap ( transform.position, gridSize, spacing, layerMask );
		
		} else if ( mapType == OPMapType.NavMesh ) {
			var navMesh : OPNavMesh = GameObject.FindObjectOfType ( OPNavMesh );
			
			map = new OPNavMeshMap ( navMesh );
		
		} else {
			map = new OPWayPointMap ( GameObject.FindObjectsOfType ( OPWayPoint ) );
		
		}
	}
	
	function Init () : IEnumerator {
		Debug.Log ( "OPScanner | Scanning for navigation nodes as " + mapType + "..." );
		
		var timeTaken : float = Time.time;
		
		yield WaitForEndOfFrame ();
	
		GetBounds ();
		SetMap ();
		
		yield WaitForEndOfFrame ();
		
		timeTaken = ( Time.time - timeTaken ) * 10;
		
		Debug.Log ( "OPScanner | ...scan completed in " + timeTaken + " seconds" );
	}
	
	function Scan () {
		StartCoroutine ( Init () );
	}
	
	function Start () {
		if ( scanOnEnable ) {
			Scan ();
		}
	}
	
	function FindPath ( start : Vector3, goal : Vector3 ) : List.<OPNode> {
		var here : OPNode = GetClosestNode ( start );
		var there : OPNode = GetClosestNode ( goal );
		var list : List.<OPNode> = OPAStar.Search ( here, there, map, heuristic );
	
		map.Reset ();
	
		return list;
	}
	
	function OnDrawGizmos () {
		if ( map == null ) { return; }
		if ( map.nodes == null ) { return; }
		
		if ( bounds != null ) {
			Gizmos.color = Color.white;
		
			Gizmos.DrawWireCube ( bounds.center, bounds.size );
		}
		
		for ( var n : OPNode in map.nodes ) {		
			if ( n == null ) { continue; }
			
			Gizmos.color = new Color ( 1, 1, 1, 0.5 );
			
			if ( n.parent ) { Gizmos.color = Color.red; }
			if ( n.active ) { Gizmos.color = Color.green; }
			if ( n.neighbors.Count < 1 ) { Gizmos.color = Color.red; }
			
			Gizmos.DrawCube ( n.position, new Vector3 ( 0.1, 0.1, 0.1 ) );
			
			Gizmos.color = Color.green;
			
			for ( var nb : OPNode in n.neighbors ) {
				if ( n.active && nb.active ) {
					Gizmos.DrawLine ( n.position, nb.position );
				}
			}
			
			Gizmos.color = Color.white;
		}
	}
	
	function GetClosestNode ( pos : Vector3 ) : OPNode {
		var shortestDistance : float = 100;
		var node : OPNode;
		
		for ( var n : OPNode in map.nodes ) {
			if ( n == null ) { continue; }
			
			var currentDistance : float = ( pos - (n as OPNode).position ).magnitude;
			
			if ( currentDistance < shortestDistance && ( n as OPNode ).neighbors.Count > 0 ) {
				shortestDistance = currentDistance;
				node = n as OPNode;
				
			}
		}
		
		return node;
	}
}