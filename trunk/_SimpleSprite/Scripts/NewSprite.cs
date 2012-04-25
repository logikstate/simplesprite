using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class NewSprite : MonoBehaviour
{
	// Create a generic sprite object
	public static GameObject Create()
	{
		return Create("New Sprite", SS.Pivot.CenterMiddle, new Vector2(10, 10), null, null);
	}
	
	// Master Ovverride
	public static GameObject Create(string name, SS.Pivot pivot, Vector2 size, Material material, TextAsset data)
	{			
		GameObject go = new GameObject();
		go.AddComponent<MeshFilter>().sharedMesh = NewMesh(pivot, size);
		go.AddComponent<MeshRenderer>().material = material;
		Sprite sprite = go.AddComponent<Sprite>();
		SpriteSheet sheetInfo = new SpriteSheet(material, data);
		Debug.Log("Names " + sheetInfo.AnimationNames()[0]);		
		return go;
	}
	
	// TODO - Modify this to accept different planes (most urgently X Z for top down)
	public static Mesh NewMesh(SS.Pivot pivot, Vector2 dimensions)
	{
		Mesh m = new Mesh();
		
		Vector3 p0 = new Vector3();
		Vector3 p1 = new Vector3();
		Vector3 p2 = new Vector3();
		Vector3 p3 = new Vector3();
		
		// TODO This method triangulates vertices in a weird way- fix it
		switch(pivot)
		{
			case SS.Pivot.CenterMiddle:
				p0 = new Vector3(-(dimensions.x/2), -(dimensions.y/2), 0);
				p1 = new Vector3((dimensions.x/2), -(dimensions.y/2), 0);
				p2 = new Vector3(-(dimensions.x/2), (dimensions.y/2), 0);
				p3 = new Vector3((dimensions.x/2), (dimensions.y/2), 0);	
				break;
			
			case SS.Pivot.BottomLeft:
				p0 = new Vector3(0, 0, 0);
				p1 = new Vector3(dimensions.x, 0, 0);
				p2 = new Vector3(0, dimensions.y, 0);
				p3 = new Vector3(dimensions.x, dimensions.y, 0);
				break;		
			
			case SS.Pivot.BottomMiddle:
				p0 = new Vector3(-(dimensions.x/2), 0, 0);
				p1 = new Vector3((dimensions.x/2), 0, 0);
				p2 = new Vector3(-(dimensions.x/2), dimensions.y, 0);
				p3 = new Vector3((dimensions.x/2), dimensions.y, 0);
				break;				
				
			case SS.Pivot.BottomRight:
				p0 = new Vector3(-dimensions.x, 0, 0);
				p1 = new Vector3(0, 0, 0);
				p2 = new Vector3(-dimensions.x, dimensions.y, 0);
				p3 = new Vector3(0, dimensions.y, 0);
				break;			
			
			case SS.Pivot.CenterLeft:
				p0 = new Vector3(0, -(dimensions.y/2), 0);
				p1 = new Vector3(dimensions.x, -(dimensions.y/2), 0);
				p2 = new Vector3(0, (dimensions.y/2), 0);
				p3 = new Vector3(dimensions.x, (dimensions.y/2), 0);
				break;	

			case SS.Pivot.CenterRight:
				p0 = new Vector3(-dimensions.x, -(dimensions.y/2), 0);
				p1 = new Vector3(0, -(dimensions.y/2), 0);
				p2 = new Vector3(-dimensions.x, (dimensions.y/2), 0);
				p3 = new Vector3(0, (dimensions.y/2), 0);
				break;
			
			case SS.Pivot.TopLeft:
				p0 = new Vector3(0, -dimensions.y, 0);
				p1 = new Vector3(dimensions.x, -dimensions.y, 0);
				p2 = new Vector3(0, 0, 0);
				p3 = new Vector3(dimensions.x, 0, 0);
				break;
		
			case SS.Pivot.TopMiddle:
				p0 = new Vector3(-(dimensions.x/2), -dimensions.y, 0);
				p1 = new Vector3((dimensions.x/2), -dimensions.y, 0);
				p2 = new Vector3(-(dimensions.x/2), 0, 0);
				p3 = new Vector3((dimensions.x/2), 0, 0);
				break;

			case SS.Pivot.TopRight:
				p0 = new Vector3(-dimensions.x, -dimensions.y, 0);
				p1 = new Vector3(0, -dimensions.y, 0);
				p2 = new Vector3(-dimensions.x, 0, 0);
				p3 = new Vector3(0, 0, 0);
				break;				
		}

		m.vertices = new Vector3[]
		{
			p1, p0, p2,
			p1, p2, p3
		};
		
		m.triangles = new int[]
		{
			0, 1, 2,
			3, 4, 5
		};	
		
		Vector2 uv0 = new Vector2(0f,0f);
		Vector2 uv1 = new Vector2(1f,0f);
		Vector2 uv2 = new Vector2(0f,1f);
		Vector2 uv3 = new Vector2(1f,1f);
		
		m.uv = new Vector2[]
		{
			uv1, uv0, uv2,
			uv1, uv2, uv3
		};
		
		m.name = "SpriteMesh";	
		
		m.RecalculateNormals();
		m.RecalculateBounds();
		m.Optimize();
		return m;
	}
}