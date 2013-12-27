#pragma strict

class OPScanner extends MonoBehaviour {
	public var scanOnEnable : boolean = true;
	public var layerMask : LayerMask;
	public var mapType : OPMapType;
	public var map : OPMap = null;
	public var heuristic : float = 10.0;
	public var spacing : float = 1.0;
	
	private var bounds : Bounds;
	private var gridSize : Vector3;
	
	public function GenerateBounds () {
	    var bounds : Bounds = new Bounds ( Vector3.zero, Vector3.zero );
	    var pos : Vector3 = Vector3.zero;
	    
	    for ( var r : Renderer in FindObjectsOfType ( Renderer ) ) {
	    	bounds.Encapsulate ( r.bounds );
	    }
	    
	    pos = bounds.min;
	    
	    transform.position = pos;
		gridSize = new Vector3 ( Mathf.Round ( bounds.size.x / spacing ) + 1, Mathf.Round ( bounds.size.y / spacing ) + 1, Mathf.Round ( bounds.size.z / spacing ) + 1 );
	}
	
	public function SetMap ( nodes : OPNode[] ) {
		map = new OPMap ();
		map.nodes = nodes;
	}
	
	public function GenerateMap () {
		if ( mapType == OPMapType.Grid ) {
			map = new OPGridMap ( transform.position, gridSize, spacing, layerMask );
		
		} else if ( mapType == OPMapType.NavMesh ) {
			var navMesh : OPNavMesh = GameObject.FindObjectOfType ( OPNavMesh );
			
			map = new OPNavMeshMap ( navMesh );
		
		} else {
			map = new OPWayPointMap ( GameObject.FindObjectsOfType ( OPWayPoint ) );
		
		}
	}

	public function Clear () {
		map = null;

		this.transform.position = Vector3.zero;

		Debug.Log ( "OPScanner | Cleared nodes" );
	}

	public function Scan () {
		Clear ();
		
		Debug.Log ( "OPScanner | Scanning for navigation nodes as " + mapType + "..." );
		
		GenerateBounds ();
		GenerateMap ();
		
		Debug.Log ( "OPScanner | ...scan completed" );
	}
	
	function Start () {
		if ( scanOnEnable ) {
			Scan ();
		}
	}
	
	public function FindPath ( start : Vector3, goal : Vector3 ) : List.<OPNode> {
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
			
			Gizmos.color = new Color ( 0, 0.8, 1, 1 );
			
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
	
	public function GetClosestNode ( pos : Vector3 ) : OPNode {
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
