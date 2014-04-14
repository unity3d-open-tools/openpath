#pragma strict

import System.Collections.Generic;

class OPPriorityQueue implements IComparer {
	var nodes : List.< OPNode > = new List.< OPNode >();
		
	function GetAllNodes () : List.< OPNode > {
		return nodes;
	}
		
	function GetLength () : int {
		return nodes.Count;
	}
	
	function Compare ( nodeA : OPNode, nodeB : OPNode ) : int { 
		if ( nodeA.estimatedTotalCost < nodeB.estimatedTotalCost) {
			return -1;
		} else if ( nodeA.estimatedTotalCost > nodeB.estimatedTotalCost ) {
			return 1;
		} else {
			return 0;
		}
	}
	
	function Push ( node : OPNode ) : int {
		nodes.Add ( node );
		nodes.Sort();
		return nodes.Count;
	}
	
	function Front () : OPNode {
		if ( nodes.Count > 0 ) {
			return nodes[0] as OPNode;
		} else {
			return null;
		}
	}
	
	function Pop () : OPNode {
		if ( nodes.Count == 0 ) {
			return null;
		}
		
		var mn : OPNode = nodes[0] as OPNode;
		nodes.RemoveAt ( 0 );
		
		return mn;
	}
	
	function Contains ( node : OPNode ) : boolean {
		return nodes.Contains ( node );
	}
	
	function Remove ( node : OPNode ) {
		nodes.Remove ( node );	
		nodes.Sort();
	}
}
