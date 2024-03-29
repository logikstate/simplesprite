// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Custom/Unlit Transparent No Cull" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	
	ZWrite Off
	Cull off
	Blend SrcAlpha OneMinusSrcAlpha 

	Pass {
		Lighting Off
		SetTexture [_MainTex] { combine texture } 
	}
}
}