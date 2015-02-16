using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class OPPathFinder : MonoBehaviour {
	public int currentNode = 0;
	public OPScanner scanner;
	public float speed = 4.0f;
	public float stoppingDistance = 0.5f;
	public float nodeDistance = 1.0f;
	public Transform target;
	public bool autoChase = false;
	public bool raycastToGoal = true;
	public float raycastDistance = 100f;
	public float updateDelay = 1f;
	public float maxDrop = 2f;
		
	[System.NonSerialized] public OPNode[] nodes = new OPNode[0];
	
	private Vector3 goal;
	private float updateTimer = 0;
	private CharacterController controller;

	public bool atEndOfPath {
		get {
            return currentNode >= nodes.Length || ( transform.position - goal ).magnitude <= stoppingDistance;
        }
	}

	public bool facingDrop {
        get {
            bool result = false;
            Vector3 here = this.transform.position;
            Vector3 there = goal;
            Vector3 middle = ( there + here ) / 2;

            // Is there a drop in between us and the target?
            if ( !Physics.Raycast ( middle, Vector3.down, maxDrop ) ) {
                result = true;

            // If not, is there a drop between us and the middle?
            } else if ( !Physics.Raycast ( ( here + middle ) / 2, Vector3.down, maxDrop ) ) {
                result = true;

            }
        
            return result;
        }
	}

	public bool hasPath { 
        get {
		    return nodes.Length > 0;
        }
	}

	public float localAvoidanceAngle {
        get {
            float result = 0;
            
            if ( controller ) {
                RaycastHit hit = new RaycastHit();

                // See if the object fits. If it doesn't, return an offset angle
                if ( Physics.SphereCast ( controller.bounds.center, controller.radius, this.transform.forward, out hit, 2 ) ) {
                    Vector3 localHit = transform.InverseTransformPoint ( hit.point );
                    
                    if ( Mathf.Atan2 ( localHit.x, localHit.z ) * Mathf.Rad2Deg > 0 ) {
                        result = -1;
                    
                    } else {
                        result = 1;
                    
                    }
                }
            }

            return result;
        }
	}

	void Start () {
		controller = this.GetComponent< CharacterController > ();
		scanner = (OPScanner)GameObject.FindObjectOfType(typeof(OPScanner));
	
		if ( target && autoChase ) {
			SetGoal ( target );
		}
	}
	
	public void ClearNodes () {
		foreach ( OPNode node in nodes ) {
			node.active = false;
			node.parent = null;
		}
		
		nodes = new OPNode[0];	
	}
	
	public Vector3 GetCurrentNode () {
		if ( currentNode < nodes.Length ) {
			return nodes[currentNode].position;
		
		} else {
			return this.transform.position;

		}
	}

	private RaycastHit DoRaycast ( Vector3 here, Vector3 there, float rayDistance ) {
		RaycastHit hit = new RaycastHit();
		
		Physics.Raycast ( here, there - here, out hit, rayDistance );

		if ( hit.collider != null ) {
			Debug.DrawLine ( here, hit.point, Color.yellow );
		
		} else {
			Debug.DrawLine ( here, here + ( there - here ) * rayDistance, Color.yellow );

		}	

		return hit;
	}

	public Vector3 GetCurrentGoal () {
		if ( !raycastToGoal ) { return GetCurrentNode (); }
		
		Vector3 result = new Vector3();

		Vector3 here = this.transform.position + Vector3.up + this.transform.forward;
		Vector3 there = goal + Vector3.up;
		float realDistance = Vector3.Distance ( here, there );
		float rayDistance = realDistance > raycastDistance ? realDistance : raycastDistance;
		RaycastHit hit = new RaycastHit();
	
		// If we are facing a drop, return the current node
		if ( facingDrop ) { 
			result = GetCurrentNode ();

		// Limit the length of the ray to the maximum ray distance
		} else if ( realDistance <= rayDistance ) {
			hit = DoRaycast ( here, there, rayDistance );
			
			// We hit something
			if ( hit.collider != null ) {
				// The hit is this object, try again
				if ( hit.collider.gameObject == this.gameObject ) {
					here = hit.point;
					hit = DoRaycast ( here, there, rayDistance );
					
					// We hit something again, return the current node
					if ( hit.collider != null ) {
						result = GetCurrentNode ();			

					// The goal is in plain sight
					} else {
						result = goal;

					}
				
				// The hit is another object, return current node
				} else {
					result = GetCurrentNode ();

				}

			// We hit nothing, the goal is in plain sight
			} else {
				result = goal;	
			
			}
		
		// Out of raycast reach, return current node
		} else {
			result = GetCurrentNode ();
		
		}

		return result;
	}

	public void SetGoal ( Transform t ) {
		// If there is a goal, create the nodes
		if ( t ) {
			SetGoal ( t.position );
		
		} else {
			ClearNodes ();
		
		}
	}
	
	public void SetGoal ( Vector3 v ) {
		SetGoal ( v, false );
	}
	
	public void SetGoal ( Vector3 v, bool persist ) {
		if ( goal == v ) { return; }
				
		goal = v;
		UpdatePosition ( persist );
	}
	
	public Vector3 GetGoal () {
		return goal;
	}
		
    private IEnumerator UpdatePositionRoutine ( bool persist ) {
        if ( persist ) {
            while ( updateTimer > 0 ) {
                yield return null;
            }

            while ( scanner.searching ) {
                yield return null;
            }
        }
        
        if ( !scanner.searching && updateTimer <= 0 ) {
            Vector3 start = this.transform.position;
        
            List< OPNode > tempNodes = new List< OPNode > ();
           
            yield return StartCoroutine(scanner.FindPath ( start, goal, tempNodes ));
            
            ClearNodes ();
        
            nodes = tempNodes.ToArray ();

            for ( int i = 0; i < nodes.Length; i++ ) {
                nodes[i].active = true;
            }
            
            if ( nodes.Length > 1 ) {
                currentNode = 1;
            
            } else {
                currentNode = 0;

            }

            updateTimer = updateDelay;
        }
    }
	
    public void UpdatePosition ( bool persist ) {
		if ( !scanner ) { return; }

		StartCoroutine ( UpdatePositionRoutine(persist) );
	}

	void Update () {
		if ( updateTimer > 0 ) {
			updateTimer -= Time.deltaTime;
		}
		
		if ( scanner ) {
			// If there are nodes to follow		
			if ( nodes != null && nodes.Length > 0 && currentNode < nodes.Length ) {
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
