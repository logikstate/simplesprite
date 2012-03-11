using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Sprite : MonoBehaviour
{

	// Internal variables
	public bool isPlayingInternal = false;		// Should the animation be playing?  Used in anim loop
	private float timer = 0f;					// Used for keeping time during animation loop.
	private string currentAnimation = "";		// What Animation is Currently Playing
	public string currentlyPlaying
	{
		get{
			return(currentAnimation);
		}
	}
	private int currentAnimationIndex = 0;		// Index of currently playing Animation
	private int posneg = 1;						// Frame advancer - positive or negative depending on FPS and loop mode
	public int currentFrame = 0;				// What frame is currently being displayed?
	private Vector2[] baseScale;				// Largest X & Y values for all images in each animation.  Used for scaling mesh.
	private Mesh m;
	
	//========================================================================================
	//	Atlas coordinates - Essentially just all the data from the generated txt file.
	//	Left unitialized as it should either be initialized via Editor script or at runtime
	//	based on the current material assigned.
	//========================================================================================
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
	public float fps = 0;						// Frames per second. Only use as current FPS. 
												// Do not alter outside of runtime.
	
	public bool isPlaying
	{
		get 
		{
			return isPlayingInternal; 
		}
	}	
	// Atlas Information and Runtime Settings
	public TextAsset atlasData;
	public squash squashOn;
	public bool pixelperfect = true;
	public bool hideOnInactive = false;
	public int anchor = 0;
	public string[] anchorOptions = new string[]{"Center-Middle", "Bottom-Left", "Bottom-Middle", "Bottom-Right", "Center-Left", "Center-Right", "Top-Left", "Top-Middle", "Top-Right"};
	public enum squash
	{
		X, Y, Z
	}
	public bool createMeshAtRuntime = false;
	public Vector3 meshScale;					// Inspector defined localScale.  Used for scaling mesh - adjust this in 
												// script as opposed to localScale, as modifying localScale will screw 
												// with the mesh scaling.	


	// The reason this is hard set instead of grabbing it at runtime is because the UVs are adjusted 
	// in Editor mode.  For the time being this means that only SimpleSprite generated meshes will work
	// unless you specifically modify originalUV to match that of your mesh. To do this, simply find the uv
	// coords for your mesh (I just run a quick for loop around Debug.Log(mesh.uv[]) to get them.  Alternatively,
	// you may wish to avoid this hassle and simply set it at runtime.  If you do this, don't set the frame in Editor 
	// mode.  I've left the originalUV = mesh.uv in the script, just un-comment it to use other meshes with this script. 
	public Vector2[] originalUV = new Vector2[]{new Vector2(1,0), new Vector2(0,0), new Vector2(0,1),
												new Vector2(1,0), new Vector2(0,1), new Vector2(1,1) };
		
	void Start()
	{
		if(createMeshAtRuntime)
			AssignVertices(anchorOptions[anchor], false);
		
		// Calling Mesh so as not to interfere with any other sprites that may sharedMesh
		// the same mesh.  You could change this to sharedMesh, but only do so if you either
		// want to adjust the UVs across every sprite object uniformly, or do not use the same
		// same mesh for any other sprites anyhow (If you use Create Sprite this isn't an issue).	
		m = ((MeshFilter)GetComponent<MeshFilter>()).mesh as Mesh;		
		if(m.vertices == null)
			AssignVertices(anchorOptions[anchor], false);
			
		// Set Mesh and base mesh scale - used in Pixel Perfect Mesh scaling	
		baseScale = new Vector2[animation_frames.Length];
		for(int u = 0; u < animation_frames.Length; u++)
		{
			baseScale[u] = ssTools.findLargestVector2Values(ssTools.getRange(animation_imgSize, (int)animation_frames[u].x, (int)animation_frames[u].y));
		}
		
		meshScale = transform.localScale;
				
		// Only allow this to be called if you have not set the frame in Editor or adjusted the UVs
		// in any manner
		/* originalUV = m.uv;
		*/
		
		// If more than one animation is marked to Play On Wake, the last
		// one in the array will ultimately win out and play.
		for(int e = 0; e < animation_playOnWake.Length; e++)
		{
			if(animation_playOnWake[e] == true)
			{
				Play(animation_names[e]);	
			}
		}
	}
	
	//========================================================================================	
	//	User called functions
	//========================================================================================
	public void Play()
	{
		play();
	}

	public void Play(string animName)
	{
		if(Array.Exists(animation_names, element => element == animName))
		{
			if( isPlayingInternal && currentAnimation != animName )
			{
				currentAnimation = animName;
				currentAnimationIndex = Array.IndexOf(animation_names, currentAnimation);
				currentFrame = (int)animation_frames[currentAnimationIndex].x;
				fps = (1 / animation_fps[currentAnimationIndex]);
				posneg = (int)(fps/Mathf.Abs(fps));
			}
			else
			if( !isPlayingInternal )
			{		
				isPlayingInternal = true;
				currentAnimation = animName;
				currentAnimationIndex = Array.IndexOf(animation_names, currentAnimation);
				currentFrame = (int)animation_frames[currentAnimationIndex].x;
				
				pixelPerfect();

				setUV(animation_scale[currentFrame], animation_offset[currentFrame]);
				
				fps = (1 / animation_fps[currentAnimationIndex]);
				posneg = (int)(fps/Mathf.Abs(fps));

				StartCoroutine( play() );
			}
		}
		else
		{
			Debug.LogWarning("No animation with the name " + animName + " exists in this Spritesheet.");
		}
	}
	
	public void Stop()
	{
		isPlayingInternal = false;	
	}
	
	public void Speed(float newFPS)
	{
		fps = (1f / newFPS);	
	}	
	
	public void Speed(string animName, float newFPS)
	{
		animation_fps[Array.IndexOf(animation_names, animName)] = 1f / newFPS;
	//	fps = (1f / newFPS);	
	}
	
	
	public void SetFrame(int frame)
	{					
		frame = Mathf.Clamp(frame, 0, animation_offset.Length - 1);

		if(Application.isPlaying)
			pixelPerfect();
		
		setUV(animation_scale[frame], animation_offset[frame]);
	}
	
	public void SetAnchor(string newAnchor)
	{
		// newAnchor should correspond to a string in anchorOptions
		AssignVertices(newAnchor, false);	
	}
	
	//========================================================================================
	//	Internal functions
	//========================================================================================
	private IEnumerator play()
	{
		if(animation_wrap[currentAnimationIndex] == 3)
		{
			isPlayingInternal = false;
		}
		
		if(renderer.enabled == false)
			renderer.enabled = true;
		
		while(isPlayingInternal)
		{
			timer += 1f * Time.deltaTime;
			
			if(timer >= fps)
			{
				currentFrame += posneg;
				
				if(currentFrame > animation_frames[currentAnimationIndex].y)
				{
					switch(animation_wrap[currentAnimationIndex])
					{
						case 0 :
							currentFrame = (int)animation_frames[currentAnimationIndex].y;
							isPlayingInternal = false;
							break;
						
						case 1 :
							currentFrame = (int)animation_frames[currentAnimationIndex].x;
							break;
							
						case 2 :
							currentFrame = (int)animation_frames[currentAnimationIndex].y - 1;
							posneg = -1;
							break;
					}
				}
				if(currentFrame < animation_frames[currentAnimationIndex].x)
				{
					switch(animation_wrap[currentAnimationIndex])
					{
						case 0 :
							currentFrame = (int)animation_frames[currentAnimationIndex].x;
							isPlayingInternal = false;
							break;
					
						case 1 :
							currentFrame = (int)animation_frames[currentAnimationIndex].y;
							posneg = -1;
							break;
						
						case 2 : 
							currentFrame = (int)animation_frames[currentAnimationIndex].x + 1;
							posneg = 1;
							break;
					}				
				}
				
				timer = 0f;
				
				pixelPerfect();

				setUV(animation_scale[currentFrame], animation_offset[currentFrame]);
			}			
			
			yield return null;
		}

		if(hideOnInactive && animation_wrap[currentAnimationIndex] != 3)
			renderer.enabled = false;		

		yield return null;
	}
	
	void setUV(Vector2 scale, Vector2 offset)
	{		
		if(m == null)	// It shouldn't be, but just in case.
			m = ((MeshFilter)GetComponent<MeshFilter>()).sharedMesh as Mesh;
		//	assignVertices(anchorOptions[anchor], false);
			
		Vector2[] newUV = m.uv;	
		
		for(int p = 0; p < originalUV.Length; p++)
		{
			newUV[p] = new Vector2( originalUV[p].x * scale.x + offset.x , originalUV[p].y * scale.y + offset.y);
		}
		m.uv = newUV;
	}		
	
	void pixelPerfect()
	{
		if(pixelperfect)
		switch(squashOn)
		{
			case squash.X:
				transform.localScale = new Vector3(meshScale.x * (animation_imgSize[currentFrame].x / baseScale[currentAnimationIndex].y), meshScale.y * (animation_imgSize[currentFrame].y / baseScale[currentAnimationIndex].y), transform.localScale.z);
				break;
			case squash.Y:
				transform.localScale = new Vector3(meshScale.x * (animation_imgSize[currentFrame].x / baseScale[currentAnimationIndex].x), meshScale.y * (animation_imgSize[currentFrame].y / baseScale[currentAnimationIndex].x), transform.localScale.z);
				break;
			case squash.Z:
				transform.localScale = new Vector3(transform.localScale.x, meshScale.y * (animation_imgSize[currentFrame].y / baseScale[currentAnimationIndex].y), meshScale.z * (animation_imgSize[currentFrame].x / baseScale[currentAnimationIndex].y));
				break;
		}
	}
	
	// Reassign Vertices to adjust Pivot point - Don't call at runtime!  Or do, I don't care.
	public void AssignVertices(string windingOrder, bool createNew)
	{			
		if( ((MeshFilter)GetComponent<MeshFilter>()).sharedMesh && !createNew)
		{
			m = ((MeshFilter)GetComponent<MeshFilter>()).sharedMesh;
		}
		else
		{
			((MeshFilter)GetComponent<MeshFilter>()).sharedMesh = new Mesh();
			m = ((MeshFilter)GetComponent<MeshFilter>()).sharedMesh;
		}
		
		Vector3 p0 = new Vector3();
		Vector3 p1 = new Vector3();
		Vector3 p2 = new Vector3();
		Vector3 p3 = new Vector3();
		
		switch(windingOrder)
		{
			case "Center-Middle":
				p0 = new Vector3(-5, -5, 0);
				p1 = new Vector3(5, -5, 0);
				p2 = new Vector3(-5, 5, 0);
				p3 = new Vector3(5, 5, 0);	
				break;
			
			case "Bottom-Left":
				p0 = new Vector3(0, 0, 0);
				p1 = new Vector3(10, 0, 0);
				p2 = new Vector3(0, 10, 0);
				p3 = new Vector3(10, 10, 0);
				break;		
			
			case "Bottom-Middle":
				p0 = new Vector3(-5, 0, 0);
				p1 = new Vector3(5, 0, 0);
				p2 = new Vector3(-5, 10, 0);
				p3 = new Vector3(5, 10, 0);
				break;				
				
			case "Bottom-Right":
				p0 = new Vector3(-10, 0, 0);
				p1 = new Vector3(0, 0, 0);
				p2 = new Vector3(-10, 10, 0);
				p3 = new Vector3(0, 10, 0);
				break;			
			
			case "Center-Left":
				p0 = new Vector3(0, -5, 0);
				p1 = new Vector3(10, -5, 0);
				p2 = new Vector3(0, 5, 0);
				p3 = new Vector3(10, 5, 0);
				break;	

			case "Center-Right":
				p0 = new Vector3(-10, -5, 0);
				p1 = new Vector3(0, -5, 0);
				p2 = new Vector3(-10, 5, 0);
				p3 = new Vector3(0, 5, 0);
				break;
			
			case "Top-Left":
				p0 = new Vector3(0, -10, 0);
				p1 = new Vector3(10, -10, 0);
				p2 = new Vector3(0, 0, 0);
				p3 = new Vector3(10, 0, 0);
				break;
		
			case "Top-Middle":
				p0 = new Vector3(-5, -10, 0);
				p1 = new Vector3(5, -10, 0);
				p2 = new Vector3(-5, 0, 0);
				p3 = new Vector3(5, 0, 0);
				break;

			case "Top-Right":
				p0 = new Vector3(-10, -10, 0);
				p1 = new Vector3(0, -10, 0);
				p2 = new Vector3(-10, 0, 0);
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
	}
}