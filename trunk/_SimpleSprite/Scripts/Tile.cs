using UnityEngine;
using System;
using System.IO;

public class Tile : MonoBehaviour
{	
	public Material mat;
	public TextAsset data;
	
	public string[] animation_names;			// Names of all animations
	public Vector2[] animation_frames;			// Animation frame locations w/in offset/scale coords
	public float[] animation_fps;				// Default FPS
	public int[] animation_wrap;				// Wrap mode
												// 0 - Once
												// 1 - Loop
												// 2 - Ping Pong
												// 3 - Static
												
	public bool[] animation_playOnWake;			// Play on wake
	public Vector2[] animation_offset;			// Animation material offset
	public Vector2[] animation_scale;			// Aniamtion material scale
	public Vector2[] animation_imgSize;			// The actual pixel size of the animation

	public int gridSize = 8;
	public int tileSize = 4;
	
	public GameObject CreatePlane(string name, Vector3 location, Vector2 scale, Vector2 offset, Material mat, int size, bool generateCollider, Bounds colBounds, string tag, Quaternion rot, int layer)
	{	
		GameObject newSpriteObject = new GameObject();	
		newSpriteObject.name = name;
		
		if(name != "Preview Tile")
		{
			GameObject tileParent = GameObject.Find("Layer " + layer.ToString() );
			if( tileParent == null )
			{
				tileParent = new GameObject();
				tileParent.name = "Layer " + layer.ToString();
				tileParent.transform.position = new Vector3(0f,0f,0f);
				tileParent.transform.parent = gameObject.transform;
				newSpriteObject.transform.parent = tileParent.transform;
			}
			else
				newSpriteObject.transform.parent = tileParent.transform;
		}
		else
			newSpriteObject.transform.parent = gameObject.transform;
				
		newSpriteObject.transform.tag = tag;
		newSpriteObject.transform.rotation = rot;
		
		newSpriteObject.AddComponent("MeshFilter");
		MeshFilter meshFilter = newSpriteObject.GetComponent("MeshFilter") as MeshFilter;
		if (meshFilter==null)
		{
			Debug.LogError("MeshFilter not found! " + layer);
			return null;
		}
		
		Vector3 p0 = new Vector3(-size, -size, 0);
		Vector3 p1 = new Vector3(size, -size, 0);
		Vector3 p2 = new Vector3(-size, size, 0);
		Vector3 p3 = new Vector3(size, size, 0);
		
		Mesh mesh = new Mesh();
		meshFilter.mesh = mesh;
		
		mesh.Clear();
		
		mesh.vertices = new Vector3[]
		{
			p1, p0, p2,
			p1, p2, p3
		};
		
		mesh.triangles = new int[]
		{
			0, 1, 2,
			3, 4, 5
		};
		
		Vector2 uv0 = new Vector2(0f,0f);
		Vector2 uv1 = new Vector2(1f,0f);
		Vector2 uv2 = new Vector2(0f,1f);
		Vector2 uv3 = new Vector2(1f,1f);
		
		mesh.uv = new Vector2[]
		{
			uv1, uv0, uv2,
			uv1, uv2, uv3
		};
		
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();
		
		MeshRenderer mr = (MeshRenderer)newSpriteObject.AddComponent("MeshRenderer");
		mr.sharedMaterial = mat;
		mesh.name = "Meshy McMesherson Jr.";
		
		if(newSpriteObject.name != "Preview Tile")
			location.z = layer;
		newSpriteObject.transform.position = location;
	//	newSpriteObject.transform.rotation = rotation;
		SetUV(scale, offset, mesh);
		
		if(generateCollider)
		{
			BoxCollider col = (BoxCollider)newSpriteObject.AddComponent(typeof(BoxCollider));
			col.center = colBounds.center;
			col.size = colBounds.size;
		}
		
		return(newSpriteObject);
	}
	
	void SetUV(Vector2 scale, Vector2 offset, Mesh m)
	{		
		if(m == null)	// It shouldn't be, but just in case.
			return;
			
		Vector2[] newUV = m.uv;	
		
		for(int p = 0; p < m.uv.Length; p++)
		{
			newUV[p] = new Vector2( m.uv[p].x * scale.x + offset.x , m.uv[p].y * scale.y + offset.y);
		}
		m.uv = newUV;
	}	
}