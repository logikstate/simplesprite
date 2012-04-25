using UnityEngine;
using System.Collections;

public class InstantiateTest : MonoBehaviour {
	public Material mat;
	public TextAsset text;
	void OnGUI()
	{
		if(GUILayout.Button("Create New Sprite"))
			NewSprite.Create("New", SS.Pivot.CenterMiddle, new Vector2(10, 10), mat, text);
	}
}
