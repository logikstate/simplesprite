// Parabox LLC
// Last Update : 12/05/2011

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System;

[CustomEditor(typeof(Sprite))]

[System.Serializable]
public class SpriteInterface : Editor 
{	
	private Sprite ss;
	private GameObject obj;
	//========================================================================================
	//	Temporary variables, used to check if there has been a change.  Prevents Unity from 
	//	constantly calling functions that are only needed to be run in the event of a change.
	//	Prefaced with "t_" to easily pick out temporary variables.
	//========================================================================================
	private int t_currentFrame = 0;
	private TextAsset t_atlasData = new TextAsset();
	private int t_anchor = 0;
	
	[ExecuteInEditMode]
	override public void OnInspectorGUI()
	{
		if(ss == null)
			ss = (Sprite)target;
			
		if(obj == null)
			obj = (GameObject)ss.gameObject;				
		
		if(!((MeshFilter)obj.GetComponent<MeshFilter>()).sharedMesh)
			ss.AssignVertices(ss.anchorOptions[ss.anchor], false);			
		
		ss.atlasData = (TextAsset)EditorGUILayout.ObjectField(ss.atlasData, typeof(TextAsset), true);
		
		GUILayout.BeginHorizontal();	
			if(GUILayout.Button("Find Atlas Data") && obj.renderer.sharedMaterial != null)
				findAtlas();
				
			if(GUILayout.Button("Find Material") && t_atlasData != null)
				findMaterial();
		GUILayout.EndHorizontal();

		if(!t_atlasData)
			t_atlasData = ss.atlasData;
		
		if(t_atlasData)
		{		
			if( (t_atlasData.text != ss.atlasData.text || ss.animation_names == null || ss.animation_names.Length <= 0 || GUILayout.Button("Reset To Defaults")) && Event.current.type != EventType.Layout)
				readData();
			
			if(ss.animation_names != null)
			{
				if( GUILayout.Button( new GUIContent("Make Unique", "This creates a new mesh for the object.  Use this if you duplicate a sprite and still want it to have it's own UV properties.")))
					makeUnique();
	
				EditorGUILayout.PrefixLabel("Set Frame");
				ss.currentFrame = EditorGUILayout.IntSlider(ss.currentFrame, 0, ss.animation_offset.Length - 1);
	
				if(ss.currentFrame != t_currentFrame)
					setFrame();
				
				ss.squashOn = (Sprite.squash)EditorGUILayout.EnumPopup( new GUIContent("Scale On", "Scales the mesh to the Local Scale of this axis"), ss.squashOn);
				
				ss.anchor = EditorGUILayout.Popup("Anchor", ss.anchor ,ss.anchorOptions);
				if(ss.anchor != t_anchor)
					setAnchor();
				
				ss.pixelperfect = EditorGUILayout.Toggle(new GUIContent("Pixel Perfect", "If enabled, SimpleSprite will automatically scale the mesh to reflect the exact dimensions of the image.  Takes into account the anchor point as well as which axis to scale to."), ss.pixelperfect);
				
				ss.hideOnInactive = EditorGUILayout.Toggle("Hide Inactive", ss.hideOnInactive);
				
				ss.createMeshAtRuntime = EditorGUILayout.Toggle("Mesh @ Run", ss.createMeshAtRuntime);
				
				EditorGUILayout.Space();
				
				GUILayout.BeginHorizontal();
					GUILayout.Label("Animation Name", EditorStyles.boldLabel, GUILayout.MaxWidth(110), GUILayout.MinWidth(110));
						GUILayout.Label("|", GUILayout.MaxWidth(7), GUILayout.MinWidth(7));
					GUILayout.Label("Wake", EditorStyles.boldLabel, GUILayout.MaxWidth(40), GUILayout.MinWidth(40));
						GUILayout.Label("|", GUILayout.MaxWidth(7), GUILayout.MinWidth(7));
					GUILayout.Label("FPS", EditorStyles.boldLabel, GUILayout.MaxWidth(40), GUILayout.MinWidth(40));
						GUILayout.Label("|", GUILayout.MaxWidth(7), GUILayout.MinWidth(7));
					GUILayout.Label("Wrap", EditorStyles.boldLabel, GUILayout.MaxWidth(40), GUILayout.MinWidth(40));
				GUILayout.EndHorizontal();
				
				for(int a = 0; a < ss.animation_names.Length; a++)
				{
					GUILayout.BeginHorizontal();
						GUILayout.Label(ss.animation_names[a], GUILayout.MaxWidth(133), GUILayout.MinWidth(133));
						ss.animation_playOnWake[a] = EditorGUILayout.Toggle(ss.animation_playOnWake[a], GUILayout.MaxWidth(40), GUILayout.MinWidth(40));
						ss.animation_fps[a] = EditorGUILayout.FloatField(ss.animation_fps[a], GUILayout.MaxWidth(40), GUILayout.MinWidth(40));
						GUILayout.Space(5);
						ss.animation_wrap[a] = EditorGUILayout.IntPopup(ss.animation_wrap[a], new string[]{"Once", "Loop", "PingPong", "Static"}, new int[]{0,1,2,3}, GUILayout.MaxWidth(60), GUILayout.MinWidth(50));
					GUILayout.EndHorizontal();
				}				
			}	
		}
		else
		{						
			if(t_atlasData == null && obj.renderer.sharedMaterial != null && Event.current.type != EventType.Layout)
				findAtlas();
		}

		if(t_atlasData != null && obj.renderer.sharedMaterial == null && Event.current.type != EventType.Layout)
			findMaterial();	
	
	}
		
	// This exists because it's an easy way to set everything
	// should something go missing in the asset (cough, mesh cough)
	void setFrame()
	{
		t_currentFrame = ss.currentFrame;
		ss.SetFrame(ss.currentFrame);
	}
	
	void setAnchor()
	{
		t_anchor = ss.anchor;
		ss.AssignVertices(ss.anchorOptions[ss.anchor], false);
		setFrame();
	}	
	
	void makeUnique()
	{
		if(EditorUtility.DisplayDialog("Make Sprite Mesh Unique",
"This creates a new mesh for the object.  Use this if you duplicate a sprite and still want it to have it's own UV properties.  Warning - If the object is already unique it will leak a mesh into the scene.  To get rid of the leak you will have to restart Unity.", "Create Mesh", "Cancel"))
		{
			ss.AssignVertices(ss.anchorOptions[ss.anchor], true);
			setFrame();					
		}
	}
	void readData()
	{
		t_atlasData = ss.atlasData;
		
		if(!Application.isPlaying)
		{
			readAtlasData(ss.atlasData);
			setAnchor();
			Debug.Log("Spritesheet Data Updated");
		}
	}
	
	void findMaterial()
	{		
		foreach(UnityEngine.Object i in Resources.FindObjectsOfTypeAll(typeof(Material)) )
		{			
			if( i.name + "_data" == ss.atlasData.name)
			{
				obj.renderer.sharedMaterial = (Material)i;
				break;
			}
		}	
	}
	
	void findAtlas()
	{
	
		foreach(UnityEngine.Object i in Resources.FindObjectsOfTypeAll(typeof(TextAsset)) )
		{
			if( i.name == obj.renderer.sharedMaterial.name + "_data" || i.name == obj.renderer.sharedMaterial.name + "_index")
			{
				ss.atlasData = (TextAsset)i;
				break;
			}
		}
	}
	
	public void readAtlasData(TextAsset aD)
	{
		// Read from .ss file and transcribe to animFrames[]
		try 
		{
			// Create an instance of StreamReader to read from a file.
			StreamReader sr = new StreamReader( AssetDatabase.GetAssetPath(aD) );
			string line = "";
			
			// Read and display lines from the file until the end of the file is reached.
			line = sr.ReadLine();
			ss.animation_names = line.Split(","[0]);
			
			line = sr.ReadLine();
			ss.animation_frames = ssTools.stringToVector2(line.Split("-"[0]));
			
			line = sr.ReadLine();
			ss.animation_fps = ssTools.stringToFloat(line.Split(","[0]));
			
			// Wrap mode
			line = sr.ReadLine();
			ss.animation_wrap = ssTools.stringToInt(line.Split(","[0]));
			
			// Play on wake
			line = sr.ReadLine();
			ss.animation_playOnWake = ssTools.stringToBool(line.Split(","[0]));

			// Get xMin and yMin offset values
			line = sr.ReadLine();
			ss.animation_offset = ssTools.stringToVector2(line.Split("-"[0]));			
			
			// Get xScale and yScale values
			line = sr.ReadLine();
			ss.animation_scale = ssTools.stringToVector2(line.Split("-"[0]));
			
			// Get image pixel dimensions.  Used for scaling mesh at runtime.
			line = sr.ReadLine();
			ss.animation_imgSize = ssTools.stringToVector2(line.Split("-"[0]));
						
			sr.Close();
		}
		catch(Exception e)
		{
			// Let the user know what went wrong.
			Debug.LogError("The SimpleSprite sheet index could not be read.  Try loading a different sheet, or re-packing the currently selected sheet.  " + e);	
		}
	}	
}