// Parabox LLC
// Last Update : 12/05/2011

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class AtlasBuilder : EditorWindow
{
	public static AtlasBuilder aB;
	
	//======================================================================== 
	// Workspace variables.  These should all be cleared and re-initialized
	// when starting a new atlas.
	//========================================================================	
	Hashtable		atlasIndex = new Hashtable();				// All Atlas indexes found in the atlas index folder
	string[]		atlases = new string[]{""};					// All atlas names - use only for GUI purposes.
	int				currentAnimation = 0;						// Current atlas, as index of atlas array - atlasIndex[currentAnimation];
	string			atlasName = "New Atlas";					// Current atlas name.
	
	// GUI Variables
	Vector2 scroll = new Vector2();				// Sprite Preview Scrollbar
	Vector2 optionsScroll = new Vector2();		// Options Scrollbar
	int maxIconSize = 128;
	string[] wraps = new string[]{"Once", "Loop", "Ping Pong", "Static"};
	GUIStyle highlight = new GUIStyle();
	GUIStyle highlight2 = new GUIStyle();
	bool importAsMenu = false;					// If true, SS will seperate images dragged into workspace into their own animations w/ static wrap.
	
	// Atlas Animation Data
	List<string> animation_names = new List<string>();				// An array containing the keys for the animation hashtables
	Hashtable animation_sprites = new Hashtable();					// ( string, List<Texture2D> ) 	- Name of atlas, Sprite images
	Hashtable animation_fps = new Hashtable();						// ( string, float )			- Name of atlas, default FPS
	Hashtable animation_wrapMode = new Hashtable();					// ( string, int )				- Name of atlas, default WrapMode
	Hashtable animation_playOnWake = new Hashtable();				// ( string, bool )				- Name of atlas, play one wake bool
	
	int sheetPadding = 4;
	int maxSheetSize = 1024;
	string buildTo = "Spritesheets";
	FilterMode filtermode = FilterMode.Point;
	TextureWrapMode texturewrapmode = TextureWrapMode.Clamp;
	TextureImporterFormat texFormat = TextureImporterFormat.AutomaticCompressed;
	string saveDirectory = "_SimpleSprite/Atlases/";
	
	[MenuItem("Window/SimpleSprite/AtlasBuilder _%j")]
	static void Init () 
	{
		aB = (AtlasBuilder)EditorWindow.GetWindow( typeof(AtlasBuilder) );
		aB.Show();		
	}
	
	void OnFocus()
	{
		atlasIndex = findAtlases();
		if(currentAnimation < atlases.Length && currentAnimation > -1)
			setWorkspace(atlases[currentAnimation]);
		
		setHighlightStyle();	
	}

	void OnGUI()
	{		
		//======================================================================== 
		// The GUI popup menu for selecting which atlas to work on.
		//========================================================================
	GUILayout.BeginHorizontal();
		optionsScroll = GUILayout.BeginScrollView (optionsScroll, GUILayout.MinWidth(312), GUILayout.MaxHeight(Screen.height - 8));
			GUILayout.BeginVertical();
				if(GUILayout.Button("New Atlas", GUILayout.MaxWidth(300) ))
					clearWorkspace();			
			
				string t_atlasName = atlasName;
				t_atlasName = EditorGUILayout.TextField("Atlas Name", t_atlasName, GUILayout.MaxWidth(300));
				if(t_atlasName != atlasName)
					atlasName = t_atlasName;
				
				GUILayout.BeginHorizontal();
					GUILayout.Label("Atlases", GUILayout.MaxWidth(60));	
					
					int t_cA = currentAnimation;	
					if(atlases.Length <= 0)
						atlasIndex = findAtlases();	
					t_cA = EditorGUILayout.Popup(t_cA, atlases, GUILayout.MaxWidth(200));
					if(t_cA != currentAnimation)
					{
						currentAnimation = t_cA;
						setWorkspace(atlases[currentAnimation]);
					}
				
				if(GUILayout.Button("Refresh", GUILayout.MaxWidth(60)))
					atlasIndex = findAtlases();	
				GUILayout.EndHorizontal();
				
				importAsMenu = EditorGUILayout.Toggle(new GUIContent("Seperate Images", "When enabled, any images dragged into the workspace will be automatically split into seperate animations with Static settings, making it ideal for creating GUI spritesheets."), importAsMenu);
				
				GUILayout.Label("Saved Atlases Directory", GUILayout.MaxWidth(300));
				saveDirectory = EditorGUILayout.TextField("Assets /", saveDirectory, GUILayout.MaxWidth(300));
		
				if(GUILayout.Button("Save Atlas", GUILayout.MaxWidth(300), GUILayout.MinHeight(32)))
					saveAtlas(true);			
				
				maxIconSize = EditorGUILayout.IntSlider("Preview Size", maxIconSize, 16, 256, GUILayout.MaxWidth(300));			
				
				// Sheet build options
				
				EditorGUILayout.Space();
				
				GUILayout.Label("Spritesheet Build Settings");
				
				sheetPadding = EditorGUILayout.IntSlider("Padding", sheetPadding, 0, 16, GUILayout.MaxWidth(300));
				maxSheetSize = EditorGUILayout.IntPopup("Max Sheet Size", maxSheetSize, new string[]{"8", "16", "32", "64", "128", "256", "512", "1024", "2048", "4096"}, 
					new int[]{8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096}, GUILayout.MaxWidth(300));
				texFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("Texture Format", texFormat, GUILayout.MaxWidth(300));
				filtermode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", filtermode, GUILayout.MaxWidth(300));
				texturewrapmode = (TextureWrapMode)EditorGUILayout.EnumPopup("Texture Wrap", texturewrapmode, GUILayout.MaxWidth(300));
				
				if(GUILayout.Button("Set Build Directory", GUILayout.MaxWidth(300))) {
					buildTo = EditorUtility.OpenFolderPanel("Directory to save Spritesheets to.", "/", "");
				}		
				GUILayout.Label(buildTo);

				GUI.backgroundColor = Color.green;
				if(GUILayout.Button("Build", GUILayout.MaxWidth(300),GUILayout.MinHeight(64) ))
				{
					if(File.Exists("Assets/" + buildTo + atlasName + ".png"))
					{
						if(EditorUtility.DisplayDialog("Spritesheet Already Exists",
	"A Spritesheet with that name currently exists.  Would you like to overwrite it?", "Overwrite", "Cancel"))
						{
							TexturePacker.pack(animation_sprites, atlasName, animation_names.ToArray(), animation_fps, animation_wrapMode, animation_playOnWake, maxSheetSize, sheetPadding, buildTo, filtermode, texturewrapmode, texFormat);				
						}
					}
					else
					{
						TexturePacker.pack(animation_sprites, atlasName, animation_names.ToArray(), animation_fps, animation_wrapMode, animation_playOnWake, maxSheetSize, sheetPadding, buildTo, filtermode, texturewrapmode, texFormat);				
					}				
						
				}
				GUI.backgroundColor = new Color(1, 1, 1, 1);
			GUILayout.EndVertical();	
		GUILayout.EndScrollView();
		
		//======================================================================== 
		// Display Animations
		//========================================================================
		if(animation_sprites.Count > 0)
		{			
		//	Debug.Log("Current event detected: " + Event.current.type);
		scroll = GUILayout.BeginScrollView (scroll, GUILayout.MinWidth(Screen.width - 300), GUILayout.MaxHeight(Screen.height - 8));
		GUILayout.BeginVertical();
			if(animation_sprites.Count != 0)
			{
				for(int i = 0; i < animation_names.Count; i++)
				{
					if(i % 2 == 0)
					GUILayout.BeginVertical( highlight2 );
					else
					GUILayout.BeginVertical( highlight );
					
						GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.MinWidth(Screen.width - 300) );
							GUILayout.BeginHorizontal( GUILayout.MaxWidth(800) );
								if(GUILayout.Button("Edit", EditorStyles.toolbarButton))
								{
									editExistingAnimation(animation_names[i]);
									break;
								}
								
								if(GUILayout.Button("Delete", EditorStyles.toolbarButton))
								{
									deleteAnimation( animation_names[i] );
									break;
								}		
									GUILayout.Space(10);			
								GUILayout.Label( animation_names[i], EditorStyles.miniLabel, GUILayout.MaxWidth(120), GUILayout.MinWidth(120));								
									GUILayout.Space(5);
									GUILayout.Label("|", EditorStyles.miniLabel);								
									GUILayout.Space(5);
								GUILayout.Label("Frames per Second", EditorStyles.miniLabel);
								GUILayout.Label( (animation_fps[ animation_names[i] ]).ToString(), EditorStyles.miniLabel);
									GUILayout.Space(5);
									GUILayout.Label("|", EditorStyles.miniLabel);								
									GUILayout.Space(5);
								GUILayout.Label("Wrap Mode - ", EditorStyles.miniLabel); 
								GUILayout.Label(wraps[ (int)animation_wrapMode[ animation_names[i] ] ], EditorStyles.miniLabel);
									GUILayout.Space(5);
									GUILayout.Label("|", EditorStyles.miniLabel);								
									GUILayout.Space(5);
								GUILayout.Label("Play On Wake - ", EditorStyles.miniLabel);
								GUILayout.Label( (animation_playOnWake[ animation_names[i] ]).ToString(), EditorStyles.miniLabel);
							GUILayout.EndHorizontal();
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal(GUILayout.MaxWidth(120));
							foreach(Texture2D j in (List<Texture2D>)animation_sprites[ animation_names[i] ])
							{
								GUILayout.Label(j, GUILayout.MaxWidth(maxIconSize), GUILayout.MaxHeight(maxIconSize) );
							}
							
						GUILayout.EndHorizontal();
							
					GUILayout.EndVertical();
					
					GUILayout.Space(5);
				}
			}
			GUILayout.EndScrollView();
		}
		else	
		//======================================================================== 
		// If no Atlas is currently in the workspace, display "Drag Images"
		// and check to see if there is an atlas to be set.  Will not set images
		// during a layout event. 
		//========================================================================
		if(animation_sprites.Count < 1)
		{
			GUI.Label( new Rect( ((Screen.width/2) - 60) + 150, Screen.height/2, 120, 64) , "Drag Images Here");

			if(currentAnimation < atlases.Length && currentAnimation > -1 && Event.current.type != EventType.Layout)
			{
				setWorkspace(atlases[currentAnimation]);	
				Repaint();
			}
		}
		GUILayout.EndHorizontal();	
		
		
		//======================================================================== 
		// Accept drag and drop
		//========================================================================		
		if(Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
		{	
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			if(Event.current.type == EventType.DragPerform)
			{					
				DragAndDrop.AcceptDrag();
				acceptDrag(DragAndDrop.objectReferences);
			}
		}
		
		if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed") 
		{
			Repaint();		
		}
		
	}
	
	// Use this function to add animations from external scripts
	public static void addAnimationToAtlas(string n, List<Texture2D> imgs, float fps, int wrap, bool playonwake)
	{
		aB = (AtlasBuilder)EditorWindow.GetWindow( typeof(AtlasBuilder) );
		
		if(imgs.Count > 0)
			aB.addAnimation(n, imgs, fps, wrap, playonwake);
	}
	
	// For internal use
	void addAnimation(string name, List<Texture2D> imgs, float fps, int wrap, bool playonwake)
	{								
		if(animation_names.Contains(name))
			if(!EditorUtility.DisplayDialog("Overwrite Previous Animation?",
"An animation already exists with this name.  Would you like to overwrite it?", "Overwrite", "Do Not Overwrite"))
				{
					name = name + "_new";
				}
				else
				{
					deleteAnimation(name);
				}
				
		// Actual animation data
		animation_sprites.Add(name, imgs);		
		animation_fps.Add(name, fps);					// ( string, float )			- Name of atlas, default FPS		
		animation_wrapMode.Add(name, wrap);				// ( string, int )				- Name of atlas, default WrapMode		
		animation_playOnWake.Add(name, playonwake);		// ( string, bool )				- Name of atlas, play one wake bool
		
		// For GUI use
		animation_names.Add(name);
		
		saveAtlas(true);
		
		Repaint();		
	}
	
	//======================================================================== 
	// Accept a Drag, then populate the current workspace with the images.
	// This function takes an array of objects, and also catches any non-image
	// files.
	//========================================================================
	void acceptDrag(UnityEngine.Object[] images)
	{		
		Undo.RegisterUndo(this, "Add sprites");
		createNewAnimation(images);
		Repaint();
	}
	
	void ImportAsMenu(UnityEngine.Object[] p)
	{
		foreach(Texture2D m in (Texture2D[])ssEditorTools.removeNonImages(p))
		{
			string newAnimName = m.name;
			if(animation_names.Contains(m.name))

			newAnimName = m.name + "_0";

			addAnimation(newAnimName, (List<Texture2D>)new List<Texture2D>(){m}, 0f, 3, false);
		}
		
	}
	
	private void createNewAnimation(UnityEngine.Object[] p)
	{
		if(importAsMenu)
		{
			ImportAsMenu( p );
		}
		else
		{
			AnimationEditor.editAnimation( ssEditorTools.removeNonImages(p) as Texture2D[], "New Animation", 4f, 0, false );
		}
	}
		
	void editExistingAnimation(string animation_name)
	{			
		Undo.RegisterUndo(aB, "Edit Animation");
		AnimationEditor.editAnimation( ssEditorTools.hashtableListToArray(animation_sprites[animation_name] as List<Texture2D>), animation_name, (float)animation_fps[animation_name],
			(int)animation_wrapMode[animation_name],
			(bool)animation_playOnWake[animation_name]);

	//	deleteAnimation(animation_name);		
	
		Repaint();
	}
	
	private void deleteAnimation(string asf)
	{
	
		if(EditorUtility.DisplayDialog("Delete Animation?",
"This Action cannot be undone.  Continue?", "Delete", "Cancel"))
		{
			animation_names.Remove(asf);
			animation_sprites.Remove(asf);
			animation_fps.Remove(asf);
			animation_wrapMode.Remove(asf);
			animation_playOnWake.Remove(asf);
			
			saveAtlas(true);
			
			findAtlases();
				
			Repaint();
		}
	}
	
	// Finds all atlases stored in TxtAssets folder and returns a Hashtable of [name of sheet, textAsset]
	Hashtable findAtlases()
	{		
		string os = "windows";
		
		// The necessity of this strikes me as silly, surely there's a way to just deal with 
		// relative file paths.
		//
		// And stop calling me Shirley.
		if(Environment.OSVersion.ToString().Contains("Windows"))
			os = "windows";
			else
			os = "mac";
			
		if( Directory.Exists("Assets/" + saveDirectory) || Directory.Exists("Assets\\" + saveDirectory) )
		{
			List<string> storedAtlasNames;
			if(os == "windows")
				storedAtlasNames = new List<string>(Directory.GetFiles("Assets\\" + saveDirectory));
				else
				storedAtlasNames = new List<string>(Directory.GetFiles("Assets/" + saveDirectory));
			
			Hashtable atlasIndex_hash = new Hashtable();
			for(int o = 0; o < storedAtlasNames.Count; o++)
			{			
				// Load the name of the atlas
				TextAsset i = (TextAsset)AssetDatabase.LoadAssetAtPath(storedAtlasNames[o], typeof(TextAsset));
	
				if(os == "windows")
					storedAtlasNames[o] = storedAtlasNames[o].Replace("Assets\\" + saveDirectory, "");
					else
					storedAtlasNames[o] = storedAtlasNames[o].Replace("Assets/" + saveDirectory, "");
					
				storedAtlasNames[o] = storedAtlasNames[o].Replace(".txt", "");
				storedAtlasNames[o] = storedAtlasNames[o].Replace("SSData_", "");
		
				// Associate the two in a hashtable
				atlasIndex_hash.Add(storedAtlasNames[o], i);
			}
			
			// Refresh the Atlas index
			atlases = storedAtlasNames.ToArray();	
			
			return(atlasIndex_hash);
		}
		else
			return(null);
	}
	
	void saveAtlas(bool overwrite)
	{
//	if(animation_sprites.Count > 0)
//	{
		if(atlasName == "")
		{
			atlasName = "New Atlas";	
		}	
		
		if(!saveDirectory.EndsWith("/"))
			saveDirectory = saveDirectory + "/";
			
		if(!Directory.Exists("Assets/" + saveDirectory))
	    {
	    	Directory.CreateDirectory("Assets/" + saveDirectory);
	    }
		
		if(!overwrite)
		{
			if(atlases.Contains(atlasName))
			{
				if(!EditorUtility.DisplayDialog("Overwrite Previous Atlas?",
"Are you sure you want to overwrite your previous workspace of the same name?", "Overwrite", "Do Not Overwrite"))
					atlasName = atlasName + "_1";
			}
		}
		
		// Write the txt file that contains the stored animation data.
		StreamWriter sw = new StreamWriter("Assets/" + saveDirectory + "SSData_" + atlasName + ".txt");
		
		foreach(string i in animation_names)
		{
			// Write image locations
			sw.Write(i + "*****");
			foreach(Texture2D j in (List<Texture2D>)animation_sprites[i])
			{
				sw.Write(AssetDatabase.AssetPathToGUID( AssetDatabase.GetAssetPath(j) ) + "--");
			}
			
			// Write fps
			sw.Write("*****");
				sw.Write(animation_fps[i]);
		
			// Write wrap mode
			sw.Write("*****");
				sw.Write(animation_wrapMode[i]);
			
			// Write play on wake
			sw.Write("*****");
				sw.Write(animation_playOnWake[i]);
				
			sw.WriteLine("");
		}		
		sw.Close();

		AssetDatabase.Refresh();
		atlasIndex = findAtlases();
	}
	//}
	
	void readAtlasData(TextAsset n)
	{
		if(n != null){
		StreamReader sr = new StreamReader( AssetDatabase.GetAssetPath(n) );
			
			string line;
			while(sr.Peek() > -1)
			{
				line = sr.ReadLine();
				string[] lineSplit = (string[])line.Split(new string[]{(string)"*****"}, 0);
				
				string[] spriteGUID = (string[])lineSplit[1].Split(new string[]{(string)"--"}, 0);
				
				float t_fps = float.Parse(lineSplit[2]);							
				
				int t_wrapMode = int.Parse(lineSplit[3]);							
				
				bool t_playOnWake = Convert.ToBoolean(lineSplit[4]);
				
				List<Texture2D> t_animation_sprites = new List<Texture2D>();
				foreach(string t in spriteGUID)
				{
					Texture2D img = (Texture2D)AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath(t), typeof(Texture2D));
					if(img != null)
						t_animation_sprites.Add( img );
				}
				animation_sprites.Add(lineSplit[0], t_animation_sprites);
				animation_fps.Add(lineSplit[0], t_fps);
				animation_wrapMode.Add(lineSplit[0], t_wrapMode);
				animation_playOnWake.Add(lineSplit[0], t_playOnWake);	
			}
			
			string[] oa = new string[animation_sprites.Count];	
			animation_sprites.Keys.CopyTo(oa, 0);
			animation_names = new List<string>(oa);
		
			sr.Close();
		}
	}
	
	//========================================================================	
	// Set the Workspace to whatever is currently selected.
	//========================================================================
	void setWorkspace(string h)
	{
		if( atlases.Contains(h) && h != "")
		{
			clearWorkspace();		// Clean out the workspace
			
			atlasName = h;
			
			if( atlasIndex != null )
			if(File.Exists( AssetDatabase.GetAssetPath( atlasIndex[h] as TextAsset ) ))
				readAtlasData(atlasIndex[h] as TextAsset);		// Read and load appropriate data, if any
			
			currentAnimation = Array.IndexOf(atlases, h);

			Repaint();
		}
	}
	
	void clearWorkspace()
	{
//		Debug.Log("Clear Workspace called");
		animation_names.Clear();
		animation_sprites.Clear();
		animation_fps.Clear();
		animation_wrapMode.Clear();
		animation_playOnWake.Clear();
		
		atlasIndex = findAtlases();
		
		currentAnimation = -1;
		
		atlasName = "New Atlas";
	}
	
	void setHighlightStyle()
	{	
		EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences(); 			

		highlight.normal.background = ssEditorTools.MakeTex(600, 1, new Color(0f, 0f, 0f, .3f) );
		highlight2.normal.background = ssEditorTools.MakeTex(600, 1, new Color(0f, 0f, 0f, .1f) );
	}	
}
