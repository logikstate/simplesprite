// Parabox LLC
// Last Update : 12/05/2011

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class ssEditorTools
{

	public static Texture2D[] removeNonImages(UnityEngine.Object[] objs)
	{
		List<Texture2D> imgs = new List<Texture2D>();
		foreach(UnityEngine.Object p in objs)
		{
			if(ObjectNames.GetClassName(p) == "Texture2D")
				imgs.Add(p as Texture2D);
		}
		return(imgs.ToArray());
	}
	
	public static int sortAlphabetically(Texture2D a, Texture2D b)
	{
		string x, y;
		if(a != null)
			y = a.name;
			else
			y = "zzzzzzz";
		if(b != null)
			x = b.name;
			else
			x = "zzzzzzz";
			
		int boo = string.Compare(y,x);
		return(boo);
	}
	
	public static Texture2D[] hashtableListToArray(List<Texture2D> i)
	{
		List<Texture2D> ass = (List<Texture2D>)i;

		return(ass.ToArray() as Texture2D[]);	
	}
	
	public static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width*height];

        for(int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }
	
	public static Texture2D[] MaxImportSettings(Texture2D[] imgs)
	{
		for(int s = 0; s < imgs.Length; s++)
		{
			if(AssetDatabase.GetAssetPath( (Texture2D)imgs[s]) != null)
			{
				TextureImporter tempImporter = TextureImporter.GetAtPath( AssetDatabase.GetAssetPath( (Texture2D)imgs[s]) ) as TextureImporter;
				tempImporter.isReadable = true;
				tempImporter.textureFormat = TextureImporterFormat.ARGB32;
				tempImporter.npotScale = TextureImporterNPOTScale.None;
				tempImporter.textureType = TextureImporterType.GUI;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((Texture2D)imgs[s]), ImportAssetOptions.ForceUpdate);
			}
		}
		return(imgs);
	}	
		
	public static Texture2D MaxImportSettings(Texture2D img)
	{
		if(AssetDatabase.GetAssetPath( (Texture2D)img) != null)
		{
			TextureImporter tempImporter = TextureImporter.GetAtPath( AssetDatabase.GetAssetPath( (Texture2D)img) ) as TextureImporter;
			tempImporter.isReadable = true;
			tempImporter.textureFormat = TextureImporterFormat.ARGB32;
			tempImporter.npotScale = TextureImporterNPOTScale.None;
			tempImporter.textureType = TextureImporterType.GUI;
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((Texture2D)img), ImportAssetOptions.ForceUpdate);
		}
		
		return(img);
	}	

	public static Texture2D MakeReadable(Texture2D img)
	{
		if(AssetDatabase.GetAssetPath( (Texture2D)img) != null)
		{
			TextureImporter tempImporter = TextureImporter.GetAtPath( AssetDatabase.GetAssetPath( (Texture2D)img) ) as TextureImporter;
			tempImporter.isReadable = true;
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((Texture2D)img), ImportAssetOptions.ForceUpdate);
		}
		
		return(img);
	}	
	
	public static Vector2[] stringToVector2(string[] str)
	{
		string[] tHold;
		Vector2[] tVector = new Vector2[str.Length];
		for(int i = 0; i < str.Length; i++)
		{
			tHold = str[i].Split(","[0]);
			tVector[i] = new Vector2(float.Parse(tHold[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
								 float.Parse(tHold[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat) );		
		}
		return(tVector); 
	}
	
	public static float[] stringToFloat(string[] s)
	{
		float[] tFloat = new float[s.Length];
		for(int b = 0; b < s.Length; b++)
			tFloat[b] = float.Parse(s[b]);
		
		return( tFloat );
	}
	
	public static int[] stringToInt(string[] s)
	{
		int[] tInt = new int[s.Length];
		for(int b = 0; b < s.Length; b++)
			tInt[b] = int.Parse(s[b]);
		
		return( tInt );
	}
	
	public static bool[] stringToBool(string[] s)
	{
		bool[] tBool = new bool[s.Length];
		for(int b = 0; b < s.Length; b++)
		{			
			if(s[b] == "True")
				tBool[b] = true;
				else
				tBool[b] = false;
		}
		return( tBool );
	}
	
	public static Vector2 findLargestVector2Values(Vector2[] v)
	{
		Vector2 largest = new Vector2(0f, 0f);

		for(int s = 0; s < v.Length; s++)
		{			
			if(v[s].y > largest.y)
				largest.y = v[s].y;
			
			if(v[s].x > largest.x)
				largest.x = v[s].x;
		}
		return(largest);
	}
	
	public static Vector2[] getRange(Vector2[] arr, int x, int y)
	{
		Vector2[] newArray = new Vector2[ (y - x) + 1];
		int count = 0;
		
		for(int d = x; d <= y; d++)
		{
			newArray[count] = arr[d];
			count++;
		}
		return(newArray);
	}
	
}