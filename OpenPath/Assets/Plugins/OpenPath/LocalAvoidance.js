#pragma strict

class LocalAvoidance extends MonoBehaviour {
	public var activationDistance : float = 1;
	public var offset : Vector3;
	public var detecting : boolean = false;
	public var targetDirection : Vector3;
	public var layerMask : LayerMask;
	
	private function CheckDetecting () : boolean {
		for ( var i = 0; i < 5; i++ ) {
			var direction : Vector3 = Quaternion.AngleAxis(-(67.5+i*11.25), Vector3.up) * this.transform.right;
			var ray : Ray = new Ray ( this.transform.position + offset, direction );
	        var hit : RaycastHit;
	        
	        if( Physics.Raycast ( ray, hit, activationDistance ) ) {
	           return true;
	        
	        }
		}
		
		return false;
	}
	
	function Update () {
		detecting = CheckDetecting ();
		
		if ( detecting ) {
			var longestDistance : float = 0;
			var newDirection : Vector3;
		
			for ( var i = 0; i < 5; i++ ) {
				var direction : Vector3 = Quaternion.AngleAxis(-(i*45), Vector3.up) * this.transform.right;
				var ray : Ray = new Ray ( this.transform.position + offset, direction );
		        var hit : RaycastHit;
		        
		        if( Physics.Raycast ( ray, hit, Mathf.Infinity ) ) {
		            if ( hit.distance > longestDistance ) {
		            	longestDistance = hit.distance;
		            	newDirection = direction;
		            }
		            	            
		            Debug.DrawRay ( this.transform.position + offset, direction * hit.distance, Color.red );
		        
		        } else {
	        		Debug.DrawRay ( this.transform.position + offset, direction * Mathf.Infinity, Color.green);
		        	
		        }
			}
			
			targetDirection = newDirection;
		}
	}
}