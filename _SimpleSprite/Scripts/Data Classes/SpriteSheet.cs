using UnityEngine;
using System;
using System.Collections;

public class SpriteSheet {
	Texture2D texture;
	String[] animation_names;		// Names of each animation
	Vector2[] animation_frames;		// For each animation, name start & end frame
	float[]	animation_fps;			// Default Frames per Second to play
	WrapMode[] animation_wrap_mode;	// Defualt Wrap Mode
	bool[] animation_play_on_wake;	// Play on wake?
	Rect[] animation_coordinates;	// Rect coordinates
	Vector2[] animation_dimensions;	// Each image's size in pixels
}
