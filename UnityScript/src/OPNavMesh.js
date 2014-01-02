#pragma strict

import System.Collections.Generic;

class OPNavMesh extends MonoBehaviour {	
	private class Triangle {
		var indices : int[];
		
		function Triangle ( v0 : int, v1 : int, v2 : int ) {
			indices = new int[3];
			indices[0] = v0;
			indices[1] = v1;
			indices[2] = v2;
		}
		
		private function IsNeighborTo ( t : Triangle, vertices : Vector3[] ) : boolean {
			var similarVertices : int = 0;
			
			for ( var thisVertex : int in indices ) {
				for ( var thatVertex : int in t.indices ) {
					if ( vertices[thisVertex] == vertices[thatVertex] ) {
						similarVertices++;
					}
				}
			}
			
			return similarVertices > 1;
		}
		
		public function GetNeighbors ( triangles : Triangle[], vertices : Vector3[] ) : List.< int > {
			var tempList : List.< int > = new List.< int > ();
			
			for ( var i : int = 0; i < triangles.Length; i++ ) {
				if ( IsNeighborTo ( triangles[i], vertices ) ) {
					tempList.Add ( i );
				}
			}
			
			return tempList;
		}
		
		public function GetMedianPoint ( mesh : Mesh ) : Vector3 {
			var result : Vector3;
			
			for ( var i : int = 0; i < indices.Length; i++ ) {
				result += mesh.vertices[indices[i]];
			}
			
			result /= indices.Length;
		
			return result;
		}
	}
	
	private function MakeNeighbors ( a : OPNode, b : OPNode ) {
		if ( !a.neighbors.Contains ( b ) ) {
			a.neighbors.Add ( b );
		}
		
		if ( !b.neighbors.Contains ( a ) ) {
			b.neighbors.Add ( a );
		}
	}
	
	public function GetNodes () : OPNode[] {
		var mesh : Mesh = this.GetComponent(MeshFilter).mesh;
		var triangleList : List.< Triangle > = new List.< Triangle > ();
		var allNodes : List.< OPNode > = new List.< OPNode > ();
		
		var i : int = 0;
		var nb : int = 0;

		// Create triangles
		for ( i = 0; i < mesh.triangles.Length; i += 3 ) {			
			var triangle : Triangle = new Triangle (
				mesh.triangles [ i ],
				mesh.triangles [ i + 1 ],
				mesh.triangles [ i + 2 ]
			);
						
			triangleList.Add ( triangle );
			
			// Create median node
			var mn : OPNode = new OPNode ();
			mn.position = this.transform.TransformPoint ( triangle.GetMedianPoint ( mesh ) );
			
			// Add median node to list
			allNodes.Add ( mn );
		}
		
		var triangleArray : Triangle[] = triangleList.ToArray();
		var vertices : Vector3[] = mesh.vertices;
		
		// Connect median nodes
		for ( i = 0; i < triangleArray.Length; i++ ) {		
			for ( nb in triangleArray[i].GetNeighbors ( triangleArray, vertices ) ) {
				MakeNeighbors ( allNodes [ i ], allNodes [ nb ] );
			}
		}
		
		// Return
		return allNodes.ToArray();
	}
}
