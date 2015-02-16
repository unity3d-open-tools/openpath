using UnityEngine;
using UnityEditor;

[CustomEditor ( typeof(OPScanner) )]
public class OPInspector : Editor {
	override public void OnInspectorGUI () {
		OPScanner scanner = (OPScanner)target;

		if ( !scanner ) { return; }

		DrawDefaultInspector ();

		EditorGUILayout.Space  ();

		if ( GUILayout.Button ( "Scan", GUILayout.Height(30) ) ) {
			scanner.Scan ();
		}
		
		if ( GUILayout.Button ( "Clear", GUILayout.Height(20) ) ) {
			scanner.Clear ();
		}
	}
}
