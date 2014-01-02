#pragma strict

import System;

class OPNode implements IComparable {
	public var position : Vector3;
	public var estimatedTotalCost : float;
	public var costSoFar : float;
	public var size : int;	
	public var parent : OPNode;
	public var active : boolean = false;
	public var neighbors : List.<OPNode> = new List.<OPNode>();
	
	function OPNode () {
		estimatedTotalCost = 0.0;
		costSoFar = 1.0;
		parent = null;
	}
	
	function OPNode ( v : Vector3 ) {
		OPNode ( v.x, v.y, v.z );
	}
	
	function OPNode ( x : float, y : float, z : float ) {
		position.x = x;
		position.y = y;
		position.z = z;
		estimatedTotalCost = 0.0;
		costSoFar = 1.0;
		parent = null;
	}
	
	function CompareTo ( obj : System.Object ) : int { 
		var mn : OPNode = obj as OPNode;	    
	    
		if ( this.estimatedTotalCost < mn.estimatedTotalCost ) {
			return -1;
		} else if ( this.estimatedTotalCost > mn.estimatedTotalCost ) {
			return 1;
		} else {
			return 0;
		}
	} 
	
}
