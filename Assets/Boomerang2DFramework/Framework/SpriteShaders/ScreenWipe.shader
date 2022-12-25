Shader "Boomerang2D/ScreenWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _TransitionTex("Transition Texture", 2D) = "white" {}
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
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            float _TransitionTime;
            float _StartTime;
            sampler2D _TransitionTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, IN.texcoord);
                fixed4 transitionColor = tex2D(_TransitionTex, IN.texcoord);
                const float current_time = _Time[1] - _StartTime;
                float percentage_done = current_time / _TransitionTime;

                if (percentage_done >= 1)
                {
                    percentage_done = 1;
                }

                if (transitionColor.r >= percentage_done)
                {
                    if (color.a == 0)
                    {
                        transitionColor.a = 0;
                    } else
                    {
                        transitionColor.r = color.r;
                        transitionColor.g = color.g;
                        transitionColor.b = color.b;
                        transitionColor.a = color.a;
                    }
                }
                else
                {
                    transitionColor.r = 0;
                    transitionColor.b = 0;
                    transitionColor.g = 0;
                    transitionColor.a = 1;
                }

                return transitionColor;
            }
            ENDCG
        }
    }
}