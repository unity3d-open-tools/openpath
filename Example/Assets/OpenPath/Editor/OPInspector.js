#pragma strict

@CustomEditor ( OPScanner )
public class OPInspector extends Editor {
	override function OnInspectorGUI () {
		var scanner : OPScanner = target as OPScanner;

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
