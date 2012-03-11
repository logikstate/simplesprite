// Parabox LLC
// Last Update : 12/05/2011

using UnityEngine;
using UnityEditor;
using System.Collections;

public class DrawSprite : EditorWindow 
{	
	[MenuItem ("Window/SimpleSprite/Create Sprite _%k")]
	
	static void CreateMesh()
	{		
		GameObject newSpriteObject = new GameObject();		
		newSpriteObject.name = "Sprite";

		newSpriteObject.AddComponent("MeshFilter");
		MeshFilter meshFilter = newSpriteObject.GetComponent("MeshFilter") as MeshFilter;
		if (meshFilter==null)
		{
			Debug.LogError("MeshFilter not found!");
			return;
		}
		
		Vector3 p0 = new Vector3(-5, -5, 0);
		Vector3 p1 = new Vector3(5, -5, 0);
		Vector3 p2 = new Vector3(-5, 5, 0);
		Vector3 p3 = new Vector3(5, 5, 0);
		
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
		
		newSpriteObject.AddComponent("MeshRenderer");
		newSpriteObject.AddComponent(typeof(Sprite));
		mesh.name = "SpriteMesh";	
	}
}