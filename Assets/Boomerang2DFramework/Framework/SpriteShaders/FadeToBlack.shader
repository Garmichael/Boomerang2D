// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Modified for the Boomerang2D Framework. Copyright (c) 2020 StormGarden Games. MIT license

Shader "Boomerang2D/FadeToBlack"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        [HideInInspector] _StartTime ("StartTime", float) = 0
        _TransitionTime ("Speed", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFragFlash
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            
            float _TransitionTime;
            float _StartTime;
            
            fixed4 SpriteFragFlash(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                
                float currentTime = _Time[1] - _StartTime;
                float percentageDone = currentTime / _TransitionTime;
                 
                if(percentageDone >= 1){
                    percentageDone = 1;
                }
                
                c.r = c.r - c.r * percentageDone;
                c.g = c.g - c.g * percentageDone;
                c.b = c.b - c.b * percentageDone;
                c.a = c.a + (1 - c.a) * percentageDone;
                
                c.rgb *= c.a;
                
                return c;
            }
        ENDCG
        }
    }
}
