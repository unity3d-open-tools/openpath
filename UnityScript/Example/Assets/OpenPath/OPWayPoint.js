#pragma strict

class OPWayPoint extends MonoBehaviour {
	public var layerMask : LayerMask;
	public var node : OPNode = new OPNode();
	
	public function FindNeighbors ( allNodes : OPWayPoint [] ) {
		var tempList : List.< OPNode > = new List.< OPNode > ();
		
		node.position = this.transform.position;
		
		for ( var nodeContainer : OPWayPoint in allNodes ) {
			if ( nodeContainer != this ) {
				var hit : RaycastHit;
				var here : Vector3 = this.transform.position + new Vector3 ( 0, 0.5, 0 );
				var there : Vector3 = nodeContainer.transform.position;
				var direction : Vector3 = there - here;
				var distance : float = ( here - there ).magnitude;
								
				Physics.Raycast ( here, direction, hit, distance, layerMask );
				
				if ( hit != null && hit.transform == nodeContainer.transform ) {
					tempList.Add ( nodeContainer.node );
				}
			}
		}
		
		node.neighbors = tempList;
	}
	
	function Update () {
		node.position = this.transform.position;
	}
}
