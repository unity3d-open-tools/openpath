using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
 
public class ScriptMigrator : EditorWindow {
	public enum Mode
	{
		Convert,
		Replace
	}
 
	public static Mode m_Mode = Mode.Convert;
	public static bool m_BackupSource = true;
	public static int m_ActiveInput = 0;
	public static string m_ReplacementSuffix = ".rep";
 
	private static void DeleteFile (string path)
	{
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}
 
	private static string ReadFile(string path)
	{
	       	string output = "";
 
		if (File.Exists(path))
		{
			output = File.ReadAllText(path);
		}
 
		return output;
	}
 
	private static void EditFile(string content, string path)
	{
		File.WriteAllText(path, content);
	}
 
	private static void CreateFile(string content, string path)
	{
		DeleteFile(path);
		EditFile(path, content);
	}
 
	private static void RenameFile(string path, string extension)
	{
		string newPath = path.Replace(Path.GetExtension(path), extension);
		File.Move(path, newPath);
		File.Move(path + ".meta", newPath + ".meta");
	}
 
	private static void Replace (Object[] input)
	{
		for (int i = 0; i < input.Length; i++)
		{
			string path = AssetDatabase.GetAssetPath(input[i]);
			string replacementPath = path.Replace(".js",".cs") + m_ReplacementSuffix;
			string content = ReadFile(replacementPath);
 
			if (string.IsNullOrEmpty(content))
			{
				continue;
			}
 
			if (m_BackupSource)
			{
				File.Copy(path, path + ".bak");
			}
 
			EditFile(content, path);
			RenameFile(path, ".cs");
		}
 
		AssetDatabase.Refresh();
	}
 
	private static void Convert (Object[] input)
	{
		for (int i = 0; i < input.Length; i++)
		{
			string path = AssetDatabase.GetAssetPath(input[i]);
			string content = ConvertUStoCS(input[i].ToString());
 
			if (m_BackupSource)
			{
				File.Copy(path, path + ".bak");
			}
 
			EditFile(content, path);
			RenameFile(path, ".cs");
		}
 
		AssetDatabase.Refresh();
	}
 
	private static string ConvertUStoCS(string outputString)
	{
		// Make every public class not extending MonoBehaviour serializable
		outputString = Regex.Replace(outputString, "public class (?!.+ extends MonoBehaviour)", "[System.Serializable]\npublic class " );
 
		// Reorder generic expressions
		outputString = Regex.Replace(outputString, "(?<var>[0-9a-zA-Z]+) : (?<generic>[0-9a-zA-Z]+).< (?<type>[0-9a-zA-Z]+) >", "${generic}< ${type} > ${var}"); 
 
		// Remove remaining dots before generic expression brackets
		outputString = Regex.Replace(outputString, "\\.<", "<");
 
		// Reorder variable declarations
		outputString = Regex.Replace(outputString, "(?<var>[0-9a-zA-Z]+) : (?<type>[0-9a-zA-Z_\\[\\]]+)", "${type} ${var}");
 
		// Reorder function declarations
		outputString = Regex.Replace(outputString, "function (?<name>.+) : (?<return>.+) {", "${return} ${name} {");
 
		// Reorder compile flags
		outputString = Regex.Replace(outputString, "@(?<flag>.+) \\( (?<name>.+) \\)", "[${flag} (${name})]");
 
		// Reorder casting
		outputString = Regex.Replace(outputString, "= (?<var>.+) as (?<type>.+);", "= (${type}) ${var};");
 
		// Simple replace operations
		outputString = Regex.Replace(outputString, "boolean", "bool");
		outputString = Regex.Replace(outputString, "var ", "");
		outputString = Regex.Replace(outputString, "function", "void");
		outputString = Regex.Replace(outputString, "import", "using");
		outputString = Regex.Replace(outputString, "extends", ":");
		outputString = Regex.Replace(outputString, " String", " string");
		outputString = Regex.Replace(outputString, "@HideInInspector", "[HideInInspector]");
		outputString = Regex.Replace(outputString, "@System.NonSerialized", "[System.NonSerialized]");
		outputString = Regex.Replace(outputString, "@NonSerialized", "[System.NonSerialized]");
		outputString = Regex.Replace(outputString, "@NonSerialized", "[System.NonSerialized]");
 
		// Remove the #pragma strict statement
		outputString = Regex.Replace(outputString, "#pragma strict", "using UnityEngine;\nusing System.Collections;");
 
		// Correct foreach loops
		outputString = Regex.Replace(outputString, "for \\( (?<1>\\S+) (?<2>\\S+) in", "foreach ( ${1} ${2} in");
 
		return outputString;
	}
 
	private static void DrawConvertMode()
	{
		Object[] input = Selection.GetFiltered (typeof(Object), SelectionMode.DeepAssets);
 
		if (m_ActiveInput >= input.Length)
		{
			m_ActiveInput = input.Length - 1;
		}
 
		for (int i = 0; i < input.Length; i++)
		{
			if (input[i].name == "ScriptMigrator")
			{
				EditorGUILayout.LabelField("I cannot convert myself :)");
				return;
			}
		}
 
		if (input.Length > 0)
		{
			for (int i = 0; i < input.Length; i++)
			{
				bool isActiveInput = EditorGUILayout.Foldout(m_ActiveInput == i,input[i].name);
 
				if (isActiveInput)
				{
					m_ActiveInput = i;
 
					// Get input information
					string inputString = input[i].ToString();
 
					// Init output
					string outputString = ConvertUStoCS(inputString);
 
					// Show input + output	
					EditorGUILayout.BeginHorizontal();
 
					EditorGUILayout.LabelField("UnityScript");
					EditorGUILayout.LabelField("C#");
 
					EditorGUILayout.EndHorizontal();
 
					EditorGUILayout.BeginHorizontal();
 
					EditorGUILayout.TextField(inputString,GUILayout.Height(400));		
					EditorGUILayout.TextField(outputString,GUILayout.Height(400));
 
					EditorGUILayout.EndHorizontal();
				}
			}
 
			EditorGUILayout.Space();
 
			// Options
			m_BackupSource = EditorGUILayout.Toggle("Back up source", m_BackupSource);
 
			// Execute
			if (GUILayout.Button("Convert"))
			{
				Convert(input);
			}
		}
		else
		{
			EditorGUILayout.LabelField("Select some UnityScript (.js) files in the project view");
		}
	}
 
	private static void DrawReplaceMode()
	{
		m_ReplacementSuffix = EditorGUILayout.TextField("Replacement suffix:", m_ReplacementSuffix);
 
		m_BackupSource = EditorGUILayout.Toggle("Back up source", m_BackupSource);
 
		if (GUILayout.Button("Replace"))
		{
			Object[] input = Selection.GetFiltered (typeof(Object), SelectionMode.DeepAssets);
			Replace(input);
		}	
	}
 
	[MenuItem ("Window/Script Migrator")]
	static void Init ()
	{
		EditorWindow.GetWindow(typeof(ScriptMigrator), false, "Script Migrator", true);
	}
 
	void OnGUI ()
	{
		GUILayout.Space(40);
 
		m_Mode = (Mode)EditorGUILayout.Popup("Mode:", (int)m_Mode, System.Enum.GetNames(typeof(Mode)));
 
		EditorGUILayout.Space();
 
		if (m_Mode == Mode.Convert)
		{
			DrawConvertMode();
		}
		else
		{
			DrawReplaceMode();
		}
	}
}
