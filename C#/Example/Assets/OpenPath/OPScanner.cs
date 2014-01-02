using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OPScanner : MonoBehaviour {

    public bool scanOnEnable = true;
    public LayerMask layerMask;
    public Vector3 gridSize;
    public OPMapType mapType;
    public OPMap map = null;
    public float heuristic = 10f;
    public float spacing = 1f;
    public Bounds bounds;

    void Start()
    {
        if (scanOnEnable) { Scan(); }
    }

    void GetBounds()
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        Vector3 pos = Vector3.zero;

        foreach (Renderer r in FindObjectsOfType<Renderer>())
        {
            bounds.Encapsulate(r.bounds);
        }

        pos = bounds.min;

        transform.position = pos;
        gridSize = new Vector3(Mathf.Round(bounds.size.x / spacing) + 1, Mathf.Round(bounds.size.y / spacing) + 1, Mathf.Round(bounds.size.z / spacing) + 1);
    }

    void SetMap(OPNode[] nodes)
    {
        map = new OPMap();
        map.nodes = nodes;
    }

    void SetMap()
    {
        if (mapType == OPMapType.Grid)
        {
            map = new OPGridMap(transform.position, gridSize, spacing, layerMask);
        }
        else if ( mapType == OPMapType.NavMesh ) {
			OPNavMesh navMesh = GameObject.FindObjectOfType <OPNavMesh>( );
			
			map = new OPNavMeshMap ( navMesh );
		
		} 
        else {
			map = new OPWayPointMap ( GameObject.FindObjectsOfType<OPWayPoint>());
		}
    }

    IEnumerator Init()
    {
        Debug.Log("OPScanner | Scanning for navigation nodes as " + mapType + "...");

        float timeTaken = Time.time;

        yield return new WaitForEndOfFrame();

        GetBounds();
        SetMap();

        yield return new WaitForEndOfFrame();

        timeTaken = (Time.time - timeTaken) * 10;

        Debug.Log("OPScanner | ... scane complete in " + timeTaken + " seconds");
    }

    void Scan()
    {
        StartCoroutine(Init());
    }

    public List<OPNode> FindPath(Vector3 start, Vector3 goal)
    {
        OPNode here = GetClosestNode(start);
        OPNode there = GetClosestNode(goal);
        List<OPNode> list = OPAStar.Search(here, there, map, heuristic);

        map.Reset();

        return list;
    }

    void OnDrawGizmos()
    {
        if (map == null) { return; }
        if (map.nodes == null) { return; }

        if (bounds != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        foreach (OPNode n in map.nodes)
        {
            if (n == null) { continue; }

            Gizmos.color = new Color(1f, 1f, 1f, 0.5f);

            if (n.parent != null) { Gizmos.color = Color.red; }
            if (n.active) { Gizmos.color = Color.green; }
            if (n.neighbors.Count < 1) { Gizmos.color = Color.red; }

            Gizmos.DrawCube(n.position, new Vector3(0.1f, 0.1f, 0.1f));

            Gizmos.color = Color.green;

            foreach (OPNode nb in n.neighbors)
            {
                if (n.active && nb.active)
                {
                    Gizmos.DrawLine(n.position, nb.position);
                }
            }

            Gizmos.color = Color.white;
        }
    }

    OPNode GetClosestNode(Vector3 pos)
    {
        float shortestDistance = 100f;
        OPNode node = new OPNode();
        //print("Length " + map.nodes.Length);

        foreach (OPNode n in map.nodes)
        {
            if ( n == null ) { continue; }
			
			float currentDistance = ( pos - (n as OPNode).position ).magnitude;
            //print(currentDistance);
			
			if ( currentDistance < shortestDistance && ( n as OPNode ).neighbors.Count > 0 ) {
				shortestDistance = currentDistance;
				node = n as OPNode;
				
			}
        }
        //print(node.position);
        return node;
    }
}
