// Parabox LLC
// Last Update : 12/05/2011

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TexturePacker : EditorWindow
{
	public static void pack(Hashtable images, string sheetName, string[] animationNames, Hashtable fps, Hashtable wrapmode, Hashtable playonwake, int maxSheetSize, int sheetPadding, string buildDirectory, FilterMode filtermode, TextureWrapMode texturewrapmode, TextureImporterFormat texFormat)
	{			
		// Prevents user from building to a sub-directory with a super long file name
		// instead of the intended behaviour.
		if(buildDirectory.EndsWith("/") && Environment.OSVersion.ToString().Contains("Windows"))
			buildDirectory = buildDirectory.Remove(buildDirectory.Length-1);
		
			
		if(Environment.OSVersion.ToString().Contains("Windows"))
			buildDirectory = ("Assets\\" + buildDirectory);
			else
			buildDirectory = ("Assets/" +buildDirectory);
		
		if(!buildDirectory.EndsWith("/") && (Environment.OSVersion.ToString().Contains("Mac") || Environment.OSVersion.ToString().Contains("Unix")) )
			buildDirectory = buildDirectory + "/";
				
		if(!buildDirectory.EndsWith("\\") && Environment.OSVersion.ToString().Contains("Windows"))
			buildDirectory = buildDirectory + "\\";
	
		Texture2D[] textures = new Texture2D[images.Count];
		List<Texture2D> textusres = new List<Texture2D>();
		float floatLength = 0f;
		float prog = 0f;	
			
		images.Keys.CopyTo(animationNames, 0);
		
		foreach(string i in animationNames)
		{			
			foreach(Texture2D j in (List<Texture2D>)images[i])
			{
				textusres.Add(j);
			}
		}	
		textusres.TrimExcess();
		textures =  new Texture2D[textusres.Count];
		textures = textusres.ToArray();
	
		for(int s = 0; s < textures.Length; s++)
		{		
			floatLength = textures.Length;
			prog += 1f / floatLength;
			EditorUtility.DisplayProgressBar("Creating Spritesheet", "Fanagling Image Properties for Maximum Sprite-age", prog);
			// Make readable
			textures[s] = ssEditorTools.MaxImportSettings(textures[s]);
		}
	
		EditorUtility.ClearProgressBar();
		
		Texture2D newSheet = new Texture2D(maxSheetSize, maxSheetSize); 
		Rect[] coords = newSheet.PackTextures((Texture2D[])textures, Mathf.Abs(sheetPadding), maxSheetSize);
		
		prog = 0.0f;
		floatLength = textures.Length + (textures.Length / 2);
		for(int k = 0; k < textures.Length; k++)
		{
			prog += (1.0f / floatLength);
			EditorUtility.DisplayProgressBar("Creating Spritesheet", "Packing Sprites", prog);

			// Set back to unreadable if necessary
			if(AssetDatabase.GetAssetPath((Texture2D)textures[k]) != null)
			{
				TextureImporter tempImporter2 = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath((Texture2D)textures[k]));
				tempImporter2.isReadable = false;
				tempImporter2.textureFormat = TextureImporterFormat.ARGB32;
				tempImporter2.textureType = TextureImporterType.Image;
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath((Texture2D)textures[k]));
			}
		}
		
		floatLength = (1.0f / floatLength);		

		if(!Directory.Exists(buildDirectory))
		{
			Directory.CreateDirectory(buildDirectory);
		}

		// Write to PNG
		byte[] byt = newSheet.EncodeToPNG();
		string path = "";
		
		path = buildDirectory + sheetName + ".png";
		
		if (path != "") 
		{
			System.IO.File.WriteAllBytes(path, byt);
			AssetDatabase.Refresh();
			
			//	Import Settings for new Spritesheet
			if(path != null)
			{
				TextureImporter tempImporter3 = (TextureImporter)TextureImporter.GetAtPath(path);
				tempImporter3.filterMode = filtermode;
				tempImporter3.wrapMode = texturewrapmode;
				tempImporter3.maxTextureSize = maxSheetSize;
				tempImporter3.textureFormat = texFormat;
				AssetDatabase.ImportAsset(path);
			}		
		}
		prog += floatLength;

		// Write the .ss file containing relevant data
		StreamWriter sw;
	    sw = new StreamWriter(buildDirectory + sheetName + "_data" + ".txt");
			
		// Write Animation Names
		for(int i = 0; i < animationNames.Length; i++)
		{
			if(i < animationNames.Length - 1)
				sw.Write(animationNames[i] + ",");
				else
				sw.WriteLine(animationNames[i]);
		}		
		
					
		// Write User Animation Frames
		List<int> animFrames = new List<int>();
		int n = 0;
		for(int i = 0; i < images.Count; i++)
		{
			animFrames.Add( ((List<Texture2D>)images[animationNames[i]]).Count );
			
			if( i < images.Count - 1)
			{
				sw.Write( n.ToString() + "," + (animFrames[i] + n - 1).ToString() + "-");
				n += animFrames[i];
			}	
			else
			{
				sw.WriteLine( n.ToString() + "," + (animFrames[i] + n -1).ToString() );
			}
		}		

		// Write default FPS
		for(int i = 0; i < fps.Count; i++)
		{
			if(i < fps.Count - 1)
				sw.Write(fps[ animationNames[i] ] + ",");
				else
				sw.WriteLine(fps[ animationNames[i] ]);
		}	
		
		// Write wrap mode
		for(int i = 0; i < wrapmode.Count; i++)
		{
			if(i < wrapmode.Count - 1)
				sw.Write(wrapmode[ animationNames[i] ] + ",");
				else
				sw.WriteLine(wrapmode[ animationNames[i] ]);
		}	
		
		// Write play on wake
		for(int i = 0; i < playonwake.Count; i++)
		{
			if(i < playonwake.Count - 1)
				sw.Write(playonwake[ animationNames[i] ] + ",");
				else
				sw.WriteLine(playonwake[ animationNames[i] ]);
		}
	
		// Write UV coordinates
	 	for(int f = 0; f < coords.Length; f++)
		    if(f < coords.Length - 1)
				sw.Write(coords[f].xMin + "," + coords[f].yMin + "-");			
				else
				sw.WriteLine(coords[f].xMin + "," + coords[f].yMin);			
				
		for(int f = 0; f < coords.Length; f++)
			if(f < coords.Length - 1)
				sw.Write(coords[f].width + "," + coords[f].height + "-");	
				else
				sw.WriteLine(coords[f].width + "," + coords[f].height);	
	
		// Write scale - image size specific
		for(int f = 0; f < textures.Length; f++)
			if(f < textures.Length - 1)
				sw.Write(textures[f].width + "," + textures[f].height + "-");	
				else
				sw.Write(textures[f].width + "," + textures[f].height);		

		sw.Close();

	
		// Create new material for the sheet
		Material material = new Material(Shader.Find("Diffuse"));
		material.mainTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
		AssetDatabase.CreateAsset(material, path.Replace(".png", "") + ".mat");
	
	
		EditorUtility.ClearProgressBar();
		
	    AssetDatabase.Refresh();	
		AssetDatabase.SaveAssets();
		
		Debug.Log("Spritesheet Build Success!  Saved to location : " + buildDirectory);

	}
//	catch(Exception e)
//	{
//		Debug.LogError("Spritesheet build failed.  Delete existing Spritesheet PNG and try again. " + e);
//		EditorUtility.ClearProgressBar();
//	}
//	}
}