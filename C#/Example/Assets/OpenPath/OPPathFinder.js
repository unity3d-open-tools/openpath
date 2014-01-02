#pragma strict

class OPPathFinder extends MonoBehaviour {
	public var currentNode : int = 0;
	public var scanner : OPScanner;
	public var speed : float = 4.0;
	public var stoppingDistance : float = 0.1;
	public var nodeDistance : float = 1.0;
	public var target : Transform;
	public var autoChase : boolean = false;
	
	private var nodes : List.<OPNode> = new List.<OPNode>();
	private var goal : Vector3;
	
	public function SetGoalAfterSeconds ( s : float ) : IEnumerator {
		yield WaitForSeconds ( s );
				
		SetGoal ( target );
	}
	
	function Start () {
		if ( target != null ) {
			StartCoroutine ( SetGoalAfterSeconds ( 2 ) );
		}
	}
	
	public function ClearNodes () {
		for ( var node : OPNode in nodes ) {
			node.active = false;
			node.parent = null;
		}
		
		nodes.Clear ();	
	}
	
	public function GetCurrentNode () : Vector3 {
		return nodes[currentNode].position;
	}

	public function SetGoal ( t : Transform ) {
		// If there is a goal, create the nodes
		if ( t ) {
			SetGoal ( t.position );
		
		} else {
			ClearNodes ();
		
		}
	}
	
	public function SetGoal ( v : Vector3 ) {
		if ( goal == v ) { return; }
				
		ClearNodes ();
		goal = v;
		UpdatePosition ();
	}
	
	public function GetGoal () : Vector3 {
		return goal;
	}
		
	public function UpdatePosition () {
		var start : Vector3 = this.transform.position;
					
		Loom.RunAsync ( function () {
			nodes = scanner.FindPath ( start, goal );
									
			for ( var i : int = 0; i < nodes.Count; i++ ) {
				nodes[i].active = true;
			}
			
			currentNode = 0;
		} );
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
