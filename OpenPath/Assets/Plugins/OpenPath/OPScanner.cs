using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OPScanner : MonoBehaviour {
	public bool scanOnEnable = true;
	public bool searching = false;
	public LayerMask layerMask;
	public OPMapType mapType;
	public OPMap map = null;
	public float heuristic = 10.0f;
	public float spacing = 1.0f;
	public int maxCycles = 50;
	
	private Bounds bounds;
	private Vector3 gridSize;
	private bool generated = false;
	private OPAStar astar = new OPAStar ();

	public static OPScanner instance;

	public static OPScanner GetInstance () {
		return instance;
	}

	public void GenerateBounds () {
	    Bounds bounds = new Bounds ( Vector3.zero, Vector3.zero );
	    Vector3 pos = Vector3.zero;
	    
	    foreach ( Renderer r in FindObjectsOfType ( typeof(Renderer) ) ) {
	    	bounds.Encapsulate ( r.bounds );
	    }
	    
	    pos = bounds.min;
	    
	    transform.position = pos;
		gridSize = new Vector3 ( Mathf.Round ( bounds.size.x / spacing ) + 1, Mathf.Round ( bounds.size.y / spacing ) + 1, Mathf.Round ( bounds.size.z / spacing ) + 1 );
	}
	
	public void SetMap ( OPNode[] nodes ) {
		map = new OPMap ();
		map.nodes = nodes;
	}
	
	public void GenerateMap () {
		if ( mapType == OPMapType.Grid ) {
			map = new OPGridMap ( transform.position, gridSize, spacing, layerMask );
		
		} else if ( mapType == OPMapType.NavMesh ) {
			OPNavMesh navMesh = (OPNavMesh)GameObject.FindObjectOfType ( typeof(OPNavMesh) );
		
			map = new OPNavMeshMap ( navMesh );
		
		} else {
			map = new OPWayPointMap ( (OPWayPoint[])GameObject.FindObjectsOfType ( typeof(OPWayPoint) ) );
		
		}
	}

	public void Clear () {
		map = null;
		generated = false;

		this.transform.position = Vector3.zero;
	}

	public void Scan () {
		if ( generated ) {
			Debug.LogWarning ( "OPScanner | Need to clear nodes first!" );
			return;
		} else {
			generated = true;
		}

		GenerateBounds ();
		GenerateMap ();
	}
	
	void Start () {
		if ( scanOnEnable ) {
			Scan ();
		}
	}
	
	public IEnumerator FindPath ( Vector3 start, Vector3 goal, List< OPNode > list ) {
        if ( !searching ) {
			searching = true;
			
			OPNode here = GetClosestNode ( start );
			OPNode there = GetClosestNode ( goal );

			yield return StartCoroutine ( astar.Search ( here, there, map, heuristic, list, maxCycles ) );
		
			map.Reset ();

			searching = false;
		}
	}
	
	void OnDrawGizmos () {
		if ( map == null ) { return; }
		if ( map.nodes == null ) { return; }
		
        Gizmos.color = Color.white;
    
        Gizmos.DrawWireCube ( bounds.center, bounds.size );
		
		for ( int i = 0; i < map.nodes.Length; i++ ) {
			OPNode n = map.nodes[i];

			if ( n == null ) { continue; }
			
			Gizmos.color = new Color ( 0f, 0.8f, 1f, 1f );
			
			if ( n.parent != null ) { Gizmos.color = Color.red; }
			if ( n.active ) { Gizmos.color = Color.green; }
			if ( n.neighbors.Count < 1 ) { Gizmos.color = Color.red; }
			
			Gizmos.DrawCube ( n.position, new Vector3 ( 0.25f, 0.25f, 0.25f ) );
			
			Gizmos.color = Color.green;
			
			for ( int o = 0; o < n.neighbors.Count; o++ ) {
				OPNode nb = n.neighbors[o];
				
				if ( n.active && nb.active ) {
					Gizmos.DrawLine ( n.position, nb.position );
				}
			}
			
			Gizmos.color = Color.white;
		}
	}
	
	public void Awake () {
		instance = this;
	}

	public OPNode GetClosestNode ( Vector3 pos ) {
		float shortestDistance = Mathf.Infinity;
		OPNode node = null;
		
		for ( int i = 0; i < map.nodes.Length; i++ ) {
			OPNode n = map.nodes[i];

			if ( n == null ) { continue; }
			
			float currentDistance = Vector3.Distance ( pos, n.position );
			
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
