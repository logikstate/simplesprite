// Parabox LLC
// Last Update : 12/05/2011

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class AnimationEditor : EditorWindow
{
	public List<Texture2D> sprites;
	
	// These are all relative only to the current animation
	string animName = "New Animation";
	float fps = 12f;
	int wrapmode = 0;					// Wrap mode:
										// 0 - Once
										// 1 - Loop
										// 2 - Ping Pong
										// 3 - Static
	bool playOnWake	= false;			// Play on wake?
	
	
	public static AnimationEditor animationWindow;
	Vector2 scroll = new Vector2();
	private int imgSize = 128;	
	
	// Drag and drop organization
	private int insertAt = -1;
	private Texture2D imgToDrag;
	bool dragging = false;
		
	private int highlightAt = -1;
	GUIStyle highlight;
	private bool initialPress = true;
	
	private Texture2D q;
	private int currentPreviewFrame = 0;
	private int posneg = 1;							// This modifies the FPS to be either positive or negative.
	private float previewTimer = 0f;
	private bool playPreviewAnimation = false;
	private bool highlightCurrentFrame = true;		// Whether or not to highlight Sprite as preview animation plays.
	private int previewSize = 128;
	
	public static void editAnimation(Texture2D[] s, string n, float f, int w, bool p) 
	{
		AnimationEditor.animationWindow = (AnimationEditor)EditorWindow.GetWindow(typeof(AnimationEditor), true);
		
		animationWindow.setHighlight();
		
		AnimationEditor.animationWindow.ShowUtility();
	
		animationWindow.populateAnimation(s, n, f, w, p);
	}
	
	void populateAnimation(Texture2D[] ugs, string n, float f, int w, bool p)
	{
		Undo.RegisterUndo(this, "Create New Animation");
		sprites = new List<Texture2D>(ugs);
		animName = n;
		
		fps = f;
		wrapmode = w;
		playOnWake = p;
	}
		
	void OnGUI()
	{		
		// Name of animation
		string t_animName = animName;
		t_animName = EditorGUILayout.TextField("Animation Name", t_animName);
		if(t_animName != animName)
		{
			animName = t_animName;
			EditorUtility.SetDirty(this);
		}
		
		//========================================================
		//	Animation Toolbar
		//========================================================	
		GUILayout.BeginHorizontal();
			if(GUILayout.Button("Remove Duplicates"))
				sprites = removeDuplicates(sprites);
		
			if(GUILayout.Button("Sort Alphabetically"))
				sprites.Sort(ssEditorTools.sortAlphabetically);
			
			if(GUILayout.Button("Reverse Order"))
				sprites.Reverse();
				
			if(GUILayout.Button("Max Import Quality"))
				ssEditorTools.MaxImportSettings(sprites.ToArray() as Texture2D[]);
				
			if(GUILayout.Button("Clear Animation"))
				clearAnimation();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
			fps = EditorGUILayout.FloatField("Frames / Second", fps, GUILayout.MaxWidth(140) );
			wrapmode = EditorGUILayout.Popup("Wrap Mode", wrapmode, new string[] {"Once", "Loop", "Ping Pong", "Static"}, GUILayout.MaxWidth(200) );	
			playOnWake = EditorGUILayout.Toggle("Play on Wake", playOnWake);
			
			imgSize = EditorGUILayout.IntSlider("Display Size", imgSize, 16, 512, GUILayout.MaxWidth(300));
		GUILayout.EndHorizontal();
		
		//========================================================
		//	Save Animation
		//========================================================		
		if(GUILayout.Button("Save Animation", GUILayout.MinHeight(32)))
			saveAnimation();
		
		//========================================================
		//	Display Sprites
		//========================================================
		scroll = GUILayout.BeginScrollView(scroll, GUILayout.MinWidth(Screen.width-8), GUILayout.MaxHeight(imgSize + 16));
		GUILayout.BeginHorizontal(GUILayout.MaxHeight(imgSize + 12), GUILayout.MaxWidth( (sprites.Count * imgSize) + 4) );
		for(int u = 0; u < sprites.Count; u++)
		{
			if(u == insertAt)
				GUILayout.Space(imgSize);		
		
			if(u == highlightAt)
				GUILayout.Label(sprites[u], highlight, GUILayout.MaxWidth(imgSize), GUILayout.MaxHeight(imgSize));
				else
				GUILayout.Label(sprites[u], new GUIStyle(), GUILayout.MaxWidth(imgSize), GUILayout.MaxHeight(imgSize) );
	
			//========================================================
			//	Handle Event Input (Delete, Duplicate ect)
			//========================================================
			if(Event.current.type != EventType.Layout)
			{
				// Initialize drag
				if (Event.current.type == EventType.MouseDrag && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && !dragging)		
				{	
					Undo.RegisterUndo(this, "Move Sprite");					
					highlightAt = -1;
					dragging = true;
					imgToDrag = sprites[u];
					sprites.RemoveAt(u);
					break;
				}
	
				// If dragging, note where the current best insertion point is			
				if(dragging && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
				{
					insertAt = u;
				}
				
				// Take input for selecting a sprite
				if(!dragging && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
				{
				//	if(Event.current.modifiers == EventModifiers.Shift)
				//		selectMultiple(insertAt, u);
				//		else						
						highlightAt = u;
						currentPreviewFrame = u;
					Repaint();
				}
				
				if(Event.current.type == EventType.ValidateCommand && (highlightAt < sprites.Count && highlightAt > -1))
				{	
					Event.current.Use();
							
					switch(Event.current.commandName)
					{
						case "Delete":
							remove(highlightAt);
							break;
						case "Duplicate":
							duplicate(sprites[highlightAt], highlightAt);
							break;
					}
					
				}
				else
				{
					if(Event.current.type != EventType.used)
						initialPress = true;					
				}
				
				if(Event.current.keyCode == KeyCode.Backspace || Event.current.keyCode == KeyCode.Delete)
				{
					remove(highlightAt);
					
				}
				//	Debug.Log("Pressed + " + Event.current.keyCode);
			
			}			
		}
		GUILayout.EndHorizontal();

		if(Event.current.type != EventType.Layout)
		{
			// If dragged outside of all images, set insert point to last
			if(dragging & !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
				insertAt = -1;	
			
			if(!GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
			{
				highlightAt = -1;
				Repaint();
			}
		}
		
		GUILayout.EndScrollView();
		
		if(Event.current.type != EventType.Layout)
		{
			// If dragged outside of all images, set insert point to last
			if(dragging & !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
				insertAt = -1;	
			
			if(!GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
			{
				highlightAt = -1;
				Repaint();
			}
		}
		
		//========================================================
		//	Animation Preview
		//========================================================		
		if(sprites.Count > 0)
		{
			if(currentPreviewFrame < sprites.Count)
				q = sprites[currentPreviewFrame];
		}
		else
			playPreviewAnimation = false;
		
		if(playPreviewAnimation && sprites.Count > 0 && currentPreviewFrame < sprites.Count)
		{
			q = sprites[currentPreviewFrame];
		}
		
		GUILayout.BeginArea(new Rect( (Screen.width / 2) - (previewSize / 2), 150 + imgSize, previewSize, previewSize));
			
			GUILayout.Box(q, GUILayout.MinWidth(previewSize), GUILayout.MinHeight(previewSize));
				
		GUILayout.EndArea();

		GUILayout.BeginHorizontal();
			if(!playPreviewAnimation)
			{
				if(GUILayout.Button("Play"))
					playPreviewAnimation = true;
			}
			else
			{
				if(GUILayout.Button("Pause"))
					playPreviewAnimation = false;
			}
			
			if(GUILayout.Button("Step Backward"))
				stepBack();
			
			if(GUILayout.Button("Step Forward"))
				stepForward();
				
			highlightCurrentFrame = EditorGUILayout.Toggle("Highlight Frame", highlightCurrentFrame);
			
			previewSize = EditorGUILayout.IntSlider("Preview Size", previewSize, 16, 512);
			
		GUILayout.EndHorizontal();	
		
		//========================================================
		//	Event Handlers
		//========================================================		
		if(dragging)
		{
			GUI.Label( new Rect(Event.current.mousePosition.x - (imgSize/2), 
							Event.current.mousePosition.y - (imgSize/2), 
							imgSize, 
							imgSize), 
							imgToDrag);
			
			if(Event.current.type == EventType.MouseUp)
			{
				dragging = false;
				if(insertAt == -1)
				{
					sprites.Add(imgToDrag);
					highlightAt = sprites.Count - 1;
				}
				else
				{
					sprites.Insert(insertAt, imgToDrag);
					highlightAt = insertAt;	
				}
				
				insertAt = -1;
			}
			Repaint();

		}
		
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
			Repaint();		
			
	}
	
	void Update()
	{
		// Frame counter				
		if(playPreviewAnimation && (fps != 0) )
		{
			if( (Time.realtimeSinceStartup - previewTimer) >= (1 / Mathf.Abs(fps) ) )
			{
				previewTimer = Time.realtimeSinceStartup;
								
				currentPreviewFrame += posneg;
								
				if(currentPreviewFrame >= sprites.Count)
				{
					switch(wrapmode)
					{
						case 0 :
							playPreviewAnimation = false;
							currentPreviewFrame = 0;
							break;
						
						case 1 :
							currentPreviewFrame = 0;
							break;
							
						case 2 :
							currentPreviewFrame = sprites.Count - 2;
							posneg = -1;
							break;
					
					}	
				}
				if(currentPreviewFrame < 0)
				{
					switch(wrapmode)
					{
						case 0 :
							playPreviewAnimation = false;
							currentPreviewFrame = 0;
						//	StartCoroutine( pause() );
							break;
					
						case 1 :
							currentPreviewFrame = 0;
							break;
						
						case 2 : 
							currentPreviewFrame = 1;
							posneg = 1;
							break;
					}				
				}					
				
				Repaint();
			}
			
			if(highlightCurrentFrame)
				highlightAt = currentPreviewFrame;
		}
	}
	
	void stepBack()
	{
		currentPreviewFrame--;
		if(currentPreviewFrame < 0)
			currentPreviewFrame = sprites.Count - 1;
		
		highlightAt = currentPreviewFrame;
	}
	
	void stepForward()
	{
		currentPreviewFrame++;
		if(currentPreviewFrame >= sprites.Count)
			currentPreviewFrame = 0;
		
		highlightAt = currentPreviewFrame;
		
	}
	
	void saveAnimation()
	{
		
		EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences(); 		
		
		AtlasBuilder.addAnimationToAtlas(animName, sprites, fps, wrapmode, playOnWake);
		
		if(animationWindow)
			animationWindow.Close();		
			else
			{
				animationWindow = (AnimationEditor)EditorWindow.GetWindow( typeof(AnimationEditor), true);
				animationWindow.Close();
			}
	}
	
	// Clear out entire animation workspace
	void clearAnimation()
	{
		Undo.RegisterUndo(this, "Clear Animation");
		animName = "New Animation";
		fps = 16f;
		wrapmode = 0;
		playOnWake = false;	
		sprites.Clear();
	}
	
	void remove(int p)
	{
		if(p >= 0 && p < sprites.Count)
		{
			Undo.RegisterUndo(this, "Delete Sprite");
			highlightAt = -1;
			sprites.RemoveAt( p );
			Repaint();	
		}
	}
	
	void duplicate(Texture2D i, int p)
	{
		if(initialPress)
		{
			Undo.RegisterUndo(this, "Duplicate");
			sprites.Insert(p, i);
			Repaint();
			initialPress = false;
		}
	}
	
	List<Texture2D> removeDuplicates(List<Texture2D> s)
	{
		Undo.RegisterUndo(this, "Remove Duplicates");
		
		s = s.Distinct().ToList();
			
		Repaint();
				
		return(s);
	}	
	
	void acceptDrag(UnityEngine.Object[] images)
	{		
		Undo.RegisterUndo(this, "Add sprites");
		images = ssEditorTools.removeNonImages(images);
		foreach(Texture2D um in images)
			sprites.Add(um);
		Repaint();
	}
	
	void setHighlight()
	{
		highlight = new GUIStyle();
		highlight.normal.background = MakeTex(1, 1, new Color(.35f, .8f, .35f, 1f) );
	}	
	
	private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width*height];

        for(int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }	
}