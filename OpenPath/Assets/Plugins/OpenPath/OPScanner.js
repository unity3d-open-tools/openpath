#pragma strict

class OPScanner extends MonoBehaviour {
	public var scanOnEnable : boolean = true;
	public var searching : boolean = false;
	public var layerMask : LayerMask;
	public var mapType : OPMapType;
	public var map : OPMap = null;
	public var heuristic : float = 10.0;
	public var spacing : float = 1.0;
	public var maxCycles : int = 50;
	
	private var bounds : Bounds;
	private var gridSize : Vector3;
	private var generated : boolean = false;
	private var astar : OPAStar = new OPAStar ();

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
		generated = false;

		this.transform.position = Vector3.zero;
	}

	public function Scan () {
		if ( generated ) {
			Debug.LogWarning ( "OPScanner | Need to clear nodes first!" );
			return;
		} else {
			generated = true;
		}

		GenerateBounds ();
		GenerateMap ();
	}
	
	function Start () {
		if ( scanOnEnable ) {
			Scan ();
		}
	}
	
	public function FindPath ( start : Vector3, goal : Vector3, list : List.< OPNode > ) : IEnumerator {
		if ( !searching ) {
			searching = true;
			
			var here : OPNode = GetClosestNode ( start );
			var there : OPNode = GetClosestNode ( goal );

			yield StartCoroutine ( astar.Search ( here, there, map, heuristic, list, maxCycles ) );
		
			map.Reset ();

			searching = false;
		}
	}
	
	function OnDrawGizmos () {
		if ( map == null ) { return; }
		if ( map.nodes == null ) { return; }
		
		if ( bounds != null ) {
			Gizmos.color = Color.white;
		
			Gizmos.DrawWireCube ( bounds.center, bounds.size );
		}
		
		for ( var i : int = 0; i < map.nodes.Length; i++ ) {
			var n : OPNode = map.nodes[i];

			if ( n == null ) { continue; }
			
			Gizmos.color = new Color ( 0, 0.8, 1, 1 );
			
			if ( n.parent ) { Gizmos.color = Color.red; }
			if ( n.active ) { Gizmos.color = Color.green; }
			if ( n.neighbors.Count < 1 ) { Gizmos.color = Color.red; }
			
			Gizmos.DrawCube ( n.position, new Vector3 ( 0.25, 0.25, 0.25 ) );
			
			Gizmos.color = Color.green;
			
			for ( var o : int = 0; o < n.neighbors.Count; o++ ) {
				var nb : OPNode = n.neighbors[o];
				
				if ( n.active && nb.active ) {
					Gizmos.DrawLine ( n.position, nb.position );
				}
			}
			
			Gizmos.color = Color.white;
		}
	}
	
	public function GetClosestNode ( pos : Vector3 ) : OPNode {
		var shortestDistance : float = Mathf.Infinity;
		var node : OPNode;
		
		for ( var i : int = 0; i < map.nodes.Length; i++ ) {
			var n : OPNode = map.nodes[i];

			if ( n == null ) { continue; }
			
			var currentDistance : float = Vector3.Distance ( pos, n.position );
			
			if ( currentDistance < 0.5 ) {
				node = n;
				break;
		  	
			} else if ( currentDistance < shortestDistance ) {
				shortestDistance = currentDistance;
				node = n;
			}
		}
		
		return node;
	}
}
