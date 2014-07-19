#pragma strict

class OPPathFinder extends MonoBehaviour {
	public var currentNode : int = 0;
	public var scanner : OPScanner;
	public var speed : float = 4.0;
	public var stoppingDistance : float = 0.5;
	public var nodeDistance : float = 1.0;
	public var target : Transform;
	public var autoChase : boolean = false;
	public var raycastToGoal : boolean = true;
	public var raycastDistance : float = 100;
	public var updateDelay : float = 1;
		
	@NonSerialized public var nodes : OPNode[] = new OPNode[0];
	
	private var goal : Vector3;
	private var updateTimer : float = 0;
	private var controller : CharacterController;

	public function get atEndOfPath () : boolean {
		return currentNode >= nodes.Length || ( transform.position - goal ).magnitude <= stoppingDistance;
	}

	public function get hasPath () : boolean { 
		return nodes.Length > 0;
	}

	function Start () {
		controller = this.GetComponent.< CharacterController > ();
		scanner = GameObject.FindObjectOfType(OPScanner);
	
		if ( target && autoChase ) {
			SetGoal ( target );
		}
	}
	
	public function ClearNodes () {
		for ( var node : OPNode in nodes ) {
			node.active = false;
			node.parent = null;
		}
		
		nodes = new OPNode[0];	
	}
	
	public function GetCurrentNode () : Vector3 {
		if ( currentNode < nodes.Length ) {
			return nodes[currentNode].position;
		
		} else {
			return this.transform.position;

		}
	}

	private function DoRaycast ( here : Vector3, there : Vector3, rayDistance : float ) : RaycastHit {
		var hit : RaycastHit;
		
		if ( collider ) {
			Physics.CapsuleCast ( here, here + new Vector3 ( 0, collider.bounds.extents.x, 0 ), collider.bounds.extents.x, there - here, hit, rayDistance );
		
		} else {
			Physics.Raycast ( here, there - here, hit, rayDistance );

		}

		if ( hit != null && hit.collider != null ) {
			Debug.DrawLine ( here, hit.point, Color.yellow );
		
		} else {
			Debug.DrawLine ( here, here + ( there - here ) * rayDistance, Color.yellow );

		}	

		return hit;
	}

	public function GetCurrentGoal () : Vector3 {
		if ( !raycastToGoal ) { return GetCurrentNode (); }
		
		var here : Vector3 = this.transform.position + Vector3.up + this.transform.forward;
		var there : Vector3 = goal + Vector3.up;
		var realDistance : float = Vector3.Distance ( here, there );
		var rayDistance : float = realDistance > raycastDistance ? raycastDistance : realDistance;
		var hit : RaycastHit;
	
		// If the goal is at a different height (>2 meters), return the current node
		if ( Mathf.Abs ( here.y - there.y ) > 2 ) { 
			return GetCurrentNode ();

		// Limit the length of the ray to the maximum ray distance
		} else if ( realDistance <= rayDistance ) {
			hit = DoRaycast ( here, there, rayDistance );
			
			// We hit something
			if ( hit != null && hit.collider != null ) {
				// The hit is this object, try again
				if ( hit.collider.gameObject == this.gameObject ) {
					here = hit.point;
					hit = DoRaycast ( here, there, rayDistance );
					
					// We hit something (again), return the current node
					if ( hit != null ) {
						return GetCurrentNode ();			

					// The goal is in plain sight
					} else {
						return goal;

					}
				
				// The hit is another object, return current node
				} else {
					return GetCurrentNode ();

				}

			// We hit nothing, the goal is in plain sight
			} else {
				return goal;	
			
			}
		
		// Out of raycast reach, return current node
		} else {
			return GetCurrentNode ();
		
		}	
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
		SetGoal ( v, false );
	}
	
	public function SetGoal ( v : Vector3, persist : boolean ) {
		if ( goal == v ) { return; }
				
		goal = v;
		UpdatePosition ( persist );
	}
	
	public function GetGoal () : Vector3 {
		return goal;
	}
		
	public function UpdatePosition ( persist : boolean ) {
		if ( !scanner ) { return; }

		StartCoroutine ( function () : IEnumerator {
			if ( persist ) {
				while ( updateTimer > 0 ) {
					yield null;
				}

				while ( scanner.searching ) {
					yield null;
				}
			}
			
			if ( !scanner.searching && updateTimer <= 0 ) {
				var start : Vector3 = this.transform.position;
			
				var tempNodes : List.< OPNode > = new List.< OPNode > ();
				
				yield scanner.FindPath ( start, goal, tempNodes );
				
				ClearNodes ();
			
				nodes = tempNodes.ToArray ();

				for ( var i : int = 0; i < nodes.Length; i++ ) {
					nodes[i].active = true;
				}
				
				if ( nodes.Length > 1 ) {
					currentNode = 1;
				
				} else {
					currentNode = 0;

				}

				updateTimer = updateDelay;
			}
		} () );
	}

	function Update () {
		if ( updateTimer > 0 ) {
			updateTimer -= Time.deltaTime;
		}
		
		if ( scanner ) {
			// If there are nodes to follow		
			if ( nodes && nodes.Length > 0 && currentNode < nodes.Length ) {
				if ( ( transform.position - nodes[currentNode].position ).magnitude <= stoppingDistance ) {
					currentNode++;
				}

				if ( autoChase ) {
					transform.LookAt ( ( nodes[currentNode] as OPNode ).position );
					
					if ( controller ) {
						controller.Move ( transform.forward * speed * Time.deltaTime );

					} else {
						transform.position += transform.forward * speed * Time.deltaTime;
					}
				}
			
				if ( ( transform.position - goal ).magnitude <= stoppingDistance ) {
					ClearNodes ();
				}
			}
		}
	}
}
