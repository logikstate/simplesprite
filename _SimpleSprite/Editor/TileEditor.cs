// Parabox LLC
// Last Update : 12/05/2011

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;

[CustomEditor(typeof(Tile))]
[System.Serializable]
public class TileEditor : Editor
{
	int curFrame = 0;
	Vector2 scrollPosition = new Vector2(0f, 0f);
	GUIStyle highlight;
	[UnityEngine.SerializeField]
	int highlightAt = -1;
	int iconSize = 64;
	int offsetX = 0;
	int offsetY = 0;
	int border = 2;
	int previewLength{
		get{
			return( ((tile.animation_frames.Length / (Screen.width / (iconSize + border) )) * (iconSize + border)) + iconSize + 10 );
		}
	}
	bool generateCollider = false;
	Bounds colliderBounds;
	int layer = 0;
	List<GameObject> selected = new List<GameObject>();	// Selected Tile
	string tag = "Tile";
	Material t_mat;
	bool resetBounds = true;
	int w = 0;
	Texture2D bakedTex;
	
	float sweepArea = 1f;
	
	Tile tile{
		get { return (Tile) target; }
	}
	
	public enum Mode
	{
		Placement,	// New tiles are being placed
		Edit,		// A tile is selected and being moved or modified
		Navigation	// When Q is pressed, allow Scene movement.
	}
	
	Mode currentMode = Mode.Edit;
	
	GameObject previewMesh;

	public override void OnInspectorGUI()
	{		
		//=================================================================
		//	Set initial variables, and make changes from preview event.
		//=================================================================
		if(highlight == null)
			SetHighlight();
		
		if(resetBounds)
		{
			resetBounds = false;
			colliderBounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(tile.tileSize * 2, tile.tileSize * 2, tile.tileSize * 2));
			DestroyPreviewMesh();
		}
		
		if(tile.gameObject.transform.position != Vector3.zero)
			tile.gameObject.transform.position = Vector3.zero;
		//=================================================================
		w = Screen.width;
		GUILayout.Label("Material");
		tile.mat = (Material)EditorGUILayout.ObjectField(tile.mat, typeof(Material), false, GUILayout.MaxWidth(w) );

		GUILayout.Label("\nAtlas Data");

		tile.data = (TextAsset)EditorGUILayout.ObjectField(tile.data, typeof(TextAsset), false, GUILayout.MaxWidth(w) );

		if(GUILayout.Button("Load Data", GUILayout.MaxWidth(w)))
			readAtlasData(tile.data);

		iconSize = EditorGUILayout.IntSlider("Preview Image Size", iconSize, 16, 128, GUILayout.MaxWidth(w));

		tile.gridSize = EditorGUILayout.IntSlider("Grid Size", tile.gridSize, 1, 128, GUILayout.MaxWidth(w));
		tile.tileSize = EditorGUILayout.IntSlider("Tile Size", tile.tileSize, 1, 128, GUILayout.MaxWidth(w));
		
		generateCollider = EditorGUILayout.BeginToggleGroup("Generate Colliders", generateCollider);	
			colliderBounds = EditorGUILayout.BoundsField("Bounds", colliderBounds, GUILayout.MaxWidth(w));
		
			if(colliderBounds.size == new Vector3(0f, 0f, 0f) || GUILayout.Button("Reset") && Event.current.type != EventType.Repaint)
				resetBounds = true;
		EditorGUILayout.EndToggleGroup();
		
		tag = EditorGUILayout.TagField("Tag", tag);
		
		layer = EditorGUILayout.IntPopup("Layer Depth", layer, new string[]{"-4", "-3", "-2", "-1", "0", "1", "2", "3", "4"}, new int[]{-4, -3, -2, -1, 0, 1, 2, 3, 4});
		
		if( GUILayout.Button("Clean Up Overlapping Tiles") )
			RemoveOverlappingTiles();
	
		sweepArea = EditorGUILayout.FloatField("Scan Area", sweepArea);
	
		GUILayout.BeginHorizontal();
			GUILayout.Label("Mode");
			GUILayout.Label( currentMode.ToString(), EditorStyles.boldLabel );
		GUILayout.EndHorizontal();
		
		Rect prevRect = GUILayoutUtility.GetLastRect();

		/** Begin Drawing Tile Preview **/
		if(tile.mat == null)
			return;
		
		if(tile.animation_offset == null || tile.animation_scale == null)
		{
			EditorGUI.DrawPreviewTexture(new Rect(0,prevRect.y+24,iconSize, iconSize), tile.mat.mainTexture, tile.mat);
			return;
		}

		offsetX = 0;
		offsetY = 0;
		Rect scrollRect = new Rect(0,prevRect.y+24,Screen.width - 3, Screen.height - (prevRect.y + 24));
		scrollPosition = GUI.BeginScrollView (scrollRect, scrollPosition, new Rect(0, 0, Screen.width - 8, previewLength));
		for(int it = 0; it < tile.animation_offset.Length; it++)
		{
			curFrame = it;
			t_mat = tile.mat;

			t_mat.mainTextureOffset = tile.animation_offset[curFrame];
			t_mat.mainTextureScale = tile.animation_scale[curFrame];

			Rect rect = new Rect( offsetX * (iconSize + border), offsetY, iconSize, iconSize);
			Rect highlightRect = new Rect( (offsetX * (iconSize + border)) + border, offsetY + border, iconSize - border * 2, iconSize - border * 2);
			if(highlightAt == it)
			{
				GUI.Label(rect, "", highlight);
			//	EditorGUI.DrawPreviewTexture(highlightRect, t_mat.mainTexture, t_mat);
				GUI.DrawTextureWithTexCoords(highlightRect, t_mat.mainTexture, new Rect(tile.animation_offset[curFrame].x,
					tile.animation_offset[curFrame].y, tile.animation_scale[curFrame].x, tile.animation_scale[curFrame].y), true );
			}
			else
			{
			//	EditorGUI.DrawPreviewTexture(rect, t_mat.mainTexture, t_mat);

				GUI.DrawTextureWithTexCoords(highlightRect, t_mat.mainTexture, new Rect(tile.animation_offset[curFrame].x,
					tile.animation_offset[curFrame].y, tile.animation_scale[curFrame].x, tile.animation_scale[curFrame].y), true );
			}

			offsetX++;

			if(offsetX * (iconSize + border) >= Screen.width - iconSize)// - 16)
			{
				offsetX = 0;
				offsetY += iconSize + border;
			}

			if(rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
			{
				if(highlightAt == it)
					highlightAt = -1;
					else
					{
						highlightAt = it;
						DestroyPreviewMesh();						
						selected.Clear();
						currentMode = Mode.Placement;
					}
				Repaint();
			}
		}
		GUI.EndScrollView();

		if(!scrollRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
		{
			highlightAt = -1;
			DestroyPreviewMesh();
			Repaint();
		}

		tile.mat.mainTextureOffset = new Vector2(0f, 0f);
		tile.mat.mainTextureScale = new Vector2(1f, 1f);

		if(GUI.changed)
		{
			DestroyPreviewMesh();
		}
	}

	void OnSceneGUI()
	{
	//	Debug.Log(currentMode);
		if(currentMode == Mode.Edit)
			ToolsSupport.Hidden = true;
			else
			ToolsSupport.Hidden = false;
		
		Handles.BeginGUI();
			GUILayout.Label(currentMode.ToString(), EditorStyles.whiteLargeLabel);
		Handles.EndGUI();
		
		Event e = Event.current;
		KeyCheck();
		if(e.alt)
			return;
		
		// This prevents us from selecting other objects in the scene
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		HandleUtility.AddDefaultControl(controlID);
		
		switch(currentMode)
		{
			case Mode.Navigation:
				break;
			
			case Mode.Placement:
				if(highlightAt < 0)
					break;
					
				// Show Preview
				if(previewMesh == null)
					previewMesh = tile.CreatePlane("Preview Tile", Snap(PreviewTilePosition, tile.gridSize), tile.animation_scale[highlightAt], tile.animation_offset[highlightAt], tile.mat, tile.tileSize, generateCollider, colliderBounds, tag, new Quaternion(0f, 0f, 0f, 0f), layer);
			
				previewMesh.transform.position = Snap(PreviewTilePosition, tile.gridSize);
				
				if(e.type == EventType.MouseDrag)
				{
					if(!TileCheck(true))
						tile.CreatePlane("Tile", Snap(WorldMousePosition, tile.tileSize * 2), tile.animation_scale[highlightAt], tile.animation_offset[highlightAt], tile.mat , tile.tileSize, generateCollider, colliderBounds, tag, previewMesh.transform.rotation, layer);
					break;
				}
			
				if(e.type == EventType.MouseDown)
				{
					if(!TileCheck(false))
						tile.CreatePlane("Tile", Snap(WorldMousePosition, tile.gridSize), tile.animation_scale[highlightAt], tile.animation_offset[highlightAt], tile.mat , tile.tileSize, generateCollider, colliderBounds, tag, previewMesh.transform.rotation, layer);
						else
						currentMode = Mode.Edit;
				}
				break;
			
			case Mode.Edit:
				if(selected != null)
				{
					for(int g = 0; g < selected.Count; g++)
					{
						selected[g].transform.position = Handles.PositionHandle(Snap(selected[g].transform.position, tile.gridSize), selected[g].transform.rotation);
						if(selected[g].transform.position.z != layer)
							SetLayer(selected[g], layer);
					}
				}
				
				if(MouseDown())
				{
					if(e.shift)
						EditTile(false);
						else
						EditTile(true);
				}
				break;
		}
	}
	
	void EditTile(bool clearSelected)
	{
		GameObject c = HandleUtility.PickGameObject(Event.current.mousePosition, true);
		if( c != null && c.name != "Preview Tile")
		{
			if(Event.current.clickCount > 1)
			{
				RotateMesh(selected);
				return;
			}
			
			if(selected != null)
			if(!selected.Contains(c))
			{
				if(clearSelected == false)
				{
					selected.Add(c);
					layer = (int)selected[selected.Count-1].transform.position.z;
				}
				else
				{
					selected.Clear();
					selected.Add(c);
					layer = (int)selected[selected.Count -1 ].transform.position.z;
				}
				
				Repaint();
			}
		}
		else
		{
			selected.Clear();
			currentMode = Mode.Placement;
		}	
	}
	
	bool TileCheck(bool dragging)
	{
		GameObject c = HandleUtility.PickGameObject(Event.current.mousePosition, true);
		if( c != null && c.name != "Preview Tile" )
		{
			if(selected != null)	
			if(!selected.Contains(c) && !dragging)
			{
				DestroyPreviewMesh();
				layer = (int)c.transform.position.z;
				selected.Clear();
				selected.Add(c);
				return true;
			}
			
			selected.Clear();
			return true;
		}
		else
			return false;
	}
	
	void RotateMesh(GameObject zeTile)
	{
		List<GameObject> l = new List<GameObject>();
		l.Add(zeTile);
		RotateMesh( l );
	}
	
	void RotateMesh(List<GameObject> tilesToRotate)
	{
		Undo.RegisterUndo(tilesToRotate.ToArray(), "Rotate Tile(s)");
		foreach(GameObject go in tilesToRotate)
		{
			Vector3 t_rot = go.transform.rotation.eulerAngles;
			t_rot.z += 90;
			t_rot.z = t_rot.z - (t_rot.z % 90f);			// Because sometimes Unity decides to add .07 to rotations.
			go.transform.rotation = Quaternion.Euler(t_rot);
		}
	}

	void SetLayer(GameObject sel, int l)
	{
		sel.transform.position = new Vector3(sel.transform.position.x, sel.transform.position.y, l);
		
		GameObject tileParent = GameObject.Find("Layer " + l.ToString() );
		if( tileParent == null )
		{
			tileParent = new GameObject();
			tileParent.name = "Layer " + layer.ToString();
			tileParent.transform.position = new Vector3(0f,0f,0f);
			tileParent.transform.parent = tile.gameObject.transform;
			sel.transform.parent = tileParent.transform;
		}
		else
			sel.transform.parent = tileParent.transform;
	}
	
	void KeyCheck()
	{
		if(Event.current.type == EventType.KeyUp)
		{
			if(Event.current.keyCode == KeyCode.W)
			{
				switch(currentMode)
				{
					case Mode.Placement:
						currentMode = Mode.Edit;
						break;
					case Mode.Edit:
						selected.Clear();
						currentMode = Mode.Placement;
						break;
					case Mode.Navigation:
						currentMode = Mode.Placement;
						break;
				}
			}
			
			if(Event.current.keyCode == KeyCode.D || Event.current.keyCode == KeyCode.E)
			{
				Event.current.Use();
				switch(currentMode)
				{
				case Mode.Placement:
					RotateMesh(previewMesh);
					break;
				case Mode.Edit:
					RotateMesh(selected);
					break;
				}		
			}
			
			if(Event.current.keyCode == KeyCode.Q)
			{
				DestroyPreviewMesh();
				currentMode = Mode.Navigation;
			}
			
			if(Event.current.functionKey)
			{
				if( (Event.current.keyCode == KeyCode.Backspace || Event.current.keyCode == KeyCode.Delete) && selected != null)
				{
					Undo.SetSnapshotTarget(selected[0].transform.parent, "Delete Tile");
					Undo.CreateSnapshot();
					Undo.RegisterSnapshot();
					for(int i = selected.Count - 1; i > -1; i--)
					{
						DestroyImmediate(selected[i]);
					}
					selected.Clear();
				}
			}
			
			Repaint();
		}
	}

	void DestroyPreviewMesh()
	{
		GameObject[] lostPreviewMeshes = GameObject.FindGameObjectsWithTag("Tile");
		for(int i = 0; i < lostPreviewMeshes.Length; i++)
		{
			if(lostPreviewMeshes[i].name == "Preview Tile")
				DestroyImmediate(lostPreviewMeshes[i]);
		}
	}
	
	//=========================================================================
	//	Takes a Vector3 ( World Position) and an Integer (Snap Grid size).
	//=========================================================================
	Vector3 Snap(Vector3 input, int g)
    {
		// Don't snap Z axis. This allows for layering without having massive gaps.
        return(new Vector3(g * Mathf.Round((input.x / g)), g * Mathf.Round((input.y / g)), input.z));// g * Mathf.Round((input.z / g))));
    }
	
	bool Clicking()
	{
		return( Event.current.type == EventType.MouseDown && Event.current.button == 0 );
	}

	Vector3 WorldMousePosition{
		get{
			return ScreenToWorld(Event.current.mousePosition, 0f);
		}
	}
	
	bool MouseUp()
	{
		return(Event.current.type == EventType.MouseUp);
	}	
	
	bool MouseDown()
	{
		return(Event.current.type == EventType.MouseDown);
	}

	Vector3 PreviewTilePosition{
		get{
			return ScreenToWorld(Event.current.mousePosition, layer + tile.tileSize);
		}
	}

	// Converts a screen point to a world point
	Vector3 ScreenToWorld(Vector2 screen, float Z)
	{
		// Z is Z position
		Handles.SetCamera(Camera.current);
		if(Camera.current !=null)
		{
			Ray ray = HandleUtility.GUIPointToWorldRay(screen);
			return new Vector3(ray.origin.x, ray.origin.y, Z);
		}
		return new Vector3(0f, 0f, 0f);
	}

	void SetHighlight()
	{
		highlight = new GUIStyle();
		highlight.normal.background = MakeTex(600, 1, new Color(1f, 0f, 0f, 1f) );
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
	
	void RemoveOverlappingTiles()
	{		
		selected.Clear();
	    List<GameObject> tiles = new List<GameObject>( GetTiles() );
		Undo.RegisterSceneUndo("Undo Tile Cleanup");
		
		for(int i = 0; i < tiles.Count; i++)
		{
			GameObject closest = GetClosestTile(tiles[i], sweepArea);		
			if( closest != null && closest.name != "Preview Tile" )
			{
				if(tiles[i].transform.position.z == closest.transform.position.z)
				{
					DestroyImmediate(closest);
					i = 0;
				}
			}
		}
		EditorUtility.UnloadUnusedAssets();
	}
	
	GameObject GetClosestTile(GameObject anchor, float scanDistance)
	{
	  	GameObject closest = null;
		GameObject[] tiles = GetTiles();
		float prevDistance = scanDistance;	// How close to scan
		
		if(anchor == null)
			return null;
		
		for(int i = 0; i < tiles.Length; i++)
		{
	        float distance = (tiles[i].transform.position - anchor.transform.position).sqrMagnitude;
	
	        if (distance < prevDistance && tiles[i] != anchor) 
			{
	            closest = tiles[i];
	            prevDistance = distance;
	        }
	    }
	
	    return closest;
	}
	
	GameObject[] GetTiles()
	{
		return GameObject.FindGameObjectsWithTag("Tile");
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
			tile.animation_names = line.Split(","[0]);

			line = sr.ReadLine();
			tile.animation_frames = ssTools.stringToVector2(line.Split("-"[0]));

			line = sr.ReadLine();
			tile.animation_fps = ssTools.stringToFloat(line.Split(","[0]));

			// Wrap mode
			line = sr.ReadLine();
			tile.animation_wrap = ssTools.stringToInt(line.Split(","[0]));

			// Play on wake
			line = sr.ReadLine();
			tile.animation_playOnWake = ssTools.stringToBool(line.Split(","[0]));

			// Get xMin and yMin offset values
			line = sr.ReadLine();
			tile.animation_offset = ssTools.stringToVector2(line.Split("-"[0]));

			// Get xScale and yScale values
			line = sr.ReadLine();
			tile.animation_scale = ssTools.stringToVector2(line.Split("-"[0]));

			// Get image pixel dimensions.  Used for scaling mesh at runtime.
			line = sr.ReadLine();
			tile.animation_imgSize = ssTools.stringToVector2(line.Split("-"[0]));

			sr.Close();
		}
		catch(Exception e)
		{
			// Let the user know what went wrong.
			Debug.LogError("The SimpleSprite sheet index could not be read.  Try loading a different sheet, or re-packing the currently selected sheet.  " + e);
		}
	}
}

public class ToolsSupport {

	public static bool Hidden {
		get {
			Type type = typeof (Tools);
			FieldInfo field = type.GetField ("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
			return ((bool) field.GetValue (null));
		}
		set {
			Type type = typeof (Tools);
			FieldInfo field = type.GetField ("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
			field.SetValue (null, value);
		}
	}
}