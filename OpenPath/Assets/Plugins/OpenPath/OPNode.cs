using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

public class OPNode : IComparable {
	public Vector3 position;
	public float estimatedTotalCost;
	public float costSoFar;
	public int size;	
	public OPNode parent;
	public bool active = false;
	public List<OPNode> neighbors = new List<OPNode>();
	
	public OPNode () {
		estimatedTotalCost = 0.0f;
		costSoFar = 1.0f;
		parent = null;
	}
	
	public OPNode ( Vector3 v ) {
		position.x = v.x;
		position.y = v.y;
		position.z = v.z;
		estimatedTotalCost = 0.0f;
		costSoFar = 1.0f;
		parent = null;
	}
	
	public OPNode ( float x, float y, float z ) {
		position.x = x;
		position.y = y;
		position.z = z;
		estimatedTotalCost = 0.0f;
		costSoFar = 1.0f;
		parent = null;
	}
	
	public int CompareTo ( System.Object obj ) { 
		OPNode mn = (OPNode) obj;	    
	    
		if ( this.estimatedTotalCost < mn.estimatedTotalCost ) {
			return -1;
		} else if ( this.estimatedTotalCost > mn.estimatedTotalCost ) {
			return 1;
		} else {
			return 0;
		}
	} 
	
}
