using UnityEngine;
using System;
using System.Collections;

public class SpriteSheet {
	Texture2D texture;
	Material material;
	String[] animation_names;		// Names of each animation
	Vector2[] animation_frames;		// For each animation, name start & end frame
	float[]	animation_fps;			// Default Frames per Second to play
//	WrapMode[] animation_wrap_mode;	// Defualt Wrap Mode
	int[] animation_wrap_mode;	// Defualt Wrap Mode	TODO make WrapMode enum global
	bool[] animation_play_on_wake;	// Play on wake?
	Vector2[] animation_offset;		// Rect coordinates
	Vector2[] animation_dimensions;	// Each image's size in pixels
	Vector2[] animation_scale;		// dim.x/dim.y
	
	// Constructor
	public SpriteSheet(Material mat, TextAsset data) {
		material = mat;
				// Create an instance of StreamReader to read from a file.
		string lines = data.ToString();
		string[] line = lines.Split("\n"[0]);
		int curLine = 0;
		
		// Read and display lines from the file until the end of the file is reached.
		animation_names = line[curLine].Split(","[0]);
		
		curLine++;
		animation_frames = ssTools.stringToVector2(line[curLine].Split("-"[0]));
		
		curLine++;
		animation_fps = ssTools.stringToFloat(line[curLine].Split(","[0]));
		
		// Wrap mode
		curLine++;
		animation_wrap_mode = ssTools.stringToInt(line[curLine].Split(","[0]));
		
		curLine++;
		animation_play_on_wake = ssTools.stringToBool(line[curLine].Split(","[0]));

		curLine++;
		animation_offset = ssTools.stringToVector2(line[curLine].Split("-"[0]));			
		
		// Get xScale and yScale values
		curLine++;
		animation_scale = ssTools.stringToVector2(line[curLine].Split("-"[0]));
		
		// Get image pixel dimensions.  Used for scaling mesh at runtime.
		curLine++;
		animation_dimensions = ssTools.stringToVector2(line[curLine].Split("-"[0]));

	}
	
	// info seeking
	public string[] AnimationNames()
	{
		return animation_names;
	}
	
	/*
	public SpriteSheet ReadSpritesheet(TextAsset text)
	{
	//	SpriteSheet spritesheet = new SpriteSheet();

		// Create an instance of StreamReader to read from a file.
		string line = text.ToString();
		int curLine = 0;
		// Read and display lines from the file until the end of the file is reached.
		ss.animation_names = line[curLine].Split(","[0]);
		
		curLine++;
		ss.animation_frames = ssTools.stringToVector2(line[curLine].Split("-"[0]));
		
		curLine++;
		ss.animation_fps = ssTools.stringToFloat(line[curLine].Split(","[0]));
		
		// Wrap mode
		curLine++;
		ss.animation_wrap = ssTools.stringToInt(line[curLine].Split(","[0]));
		
		curLine++;
		line = sr.ReadLine();
		ss.animation_playOnWake = ssTools.stringToBool(line[curLine].Split(","[0]));

		curLine++;
		ss.animation_offset = ssTools.stringToVector2(line[curLine].Split("-"[0]));			
		
		// Get xScale and yScale values
		curLine++;
		ss.animation_scale = ssTools.stringToVector2(line[curLine].Split("-"[0]));
		
		// Get image pixel dimensions.  Used for scaling mesh at runtime.
		curLine++;
		ss.animation_imgSize = ssTools.stringToVector2(line[curLine].Split("-"[0]));

		return spritesheet;
	}
	*/
}
