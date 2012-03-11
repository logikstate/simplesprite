using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;

public class ReplaceWord : EditorWindow
{
	public static ReplaceWord win;	
	
	[MenuItem("Window/SimpleSprite/Replace Word")]
	static void Init () 
	{
		win = ScriptableObject.CreateInstance( typeof(ReplaceWord) ) as ReplaceWord;
		win.ShowUtility();			
	}
	
	string wordToReplace = "";
	string replaceWith = "";
	UnityEngine.Object[] files;
	bool prefix = true;
	void OnGUI()
	{
		wordToReplace = EditorGUILayout.TextField("Word To Remove", wordToReplace);
		
		replaceWith = EditorGUILayout.TextField("Replace With", replaceWith);
		
		prefix = EditorGUILayout.Toggle("Prefix" , prefix);
		
		GUI.backgroundColor = Color.white;
		if(GUILayout.Button("Replace"))
			Replace(Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets), wordToReplace, replaceWith);
		GUI.backgroundColor = Color.clear;
	}
	
	void Replace(UnityEngine.Object[] files, string wordToReplace, string replaceWith)
	{
		foreach(UnityEngine.Object f in files)
		{
			if(!prefix)
			{
				Debug.Log(f.name);
				f.name = f.name.Replace(wordToReplace, replaceWith);
				string n = AssetDatabase.GetAssetPath(f);
				n = n.Replace(wordToReplace, replaceWith);
				AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(f), n);
			}
			else
			{
				f.name = replaceWith + f.name;
			}
		}
	}
}