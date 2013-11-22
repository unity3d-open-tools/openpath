#pragma strict

class OPPathFinder extends MonoBehaviour {
	var currentNode : int = 0;
	var scanner : OPScanner;
	var speed : float = 4.0;
	var stoppingDistance : float = 0.1;
	var nodeDistance : float = 1.0;
	var target : Transform;
	var autoChase : boolean = false;
	
	@HideInInspector var nodes : List.<OPNode> = new List.<OPNode>();
	@HideInInspector var goal : Vector3;
	
	function SetGoalAfterSeconds ( s : float ) : IEnumerator {
		yield WaitForSeconds ( s );
				
		SetGoal ( target );
	}
	
	function Start () {
		if ( target != null ) {
			StartCoroutine ( SetGoalAfterSeconds ( 2 ) );
		}
	}
	
	function ClearNodes () {
		for ( var node : OPNode in nodes ) {
			node.active = false;
			node.parent = null;
		}
		
		nodes.Clear ();	
	}
	
	function SetGoal ( t : Transform ) {
		// If there is a goal, create the nodes
		if ( t ) {
			SetGoal ( t.position );
		
		} else {
			ClearNodes ();
		
		}
	}
	
	function SetGoal ( v : Vector3 ) {
		if ( goal == v ) { return; }
				
		ClearNodes ();
		goal = v;
		UpdatePosition ();
	}
	
	function GetGoal () : Vector3 {
		return goal;
	}
		
	function UpdatePosition () {
		var start : Vector3 = this.transform.position;
					
		//Loom.RunAsync ( function () {
			var newNodes : List.<OPNode> = scanner.FindPath ( start, goal );
									
			for ( var node : OPNode in newNodes ) {
				node.active = true;
			}
			
		//	Loom.QueueOnMainThread ( function () {
				nodes = newNodes;
				currentNode = 0;
		//	} );
		//} );
	}
	
	function Update () {
		if ( scanner == null ) {
			if ( GameObject.FindObjectOfType(OPScanner) ) {
				scanner = GameObject.FindObjectOfType(OPScanner);
			} else {
				Debug.LogError ( "No scanner found! Attach an OPScanner to the scanner variable." );
				return;
			}
		}
			
		// If there are nodes to follow		
		if ( nodes && nodes.Count > 0 ) {
			if ( ( transform.position - ( nodes[currentNode] as OPNode ).position ).magnitude < nodeDistance && currentNode < nodes.Count - 1 ) {
				currentNode++;
			}
			
			if ( autoChase ) {
				var lookPos : Vector3 = ( nodes[currentNode] as OPNode ).position - transform.position;
				lookPos.y = 0;
				
				transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.LookRotation( lookPos ), 8 * Time.deltaTime );			
				transform.localPosition += transform.forward * speed * Time.deltaTime;
			}
		
			if ( ( transform.position - goal ).magnitude <= stoppingDistance ) {
				ClearNodes ();
			}
		}
	}
}