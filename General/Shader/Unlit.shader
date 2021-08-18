Shader "47E/Unlit"
{
    Properties
    {
        [Header(Config)]
        [Enum(UnityEngine.Rendering.CullMode)] _Culling("Culling", Float) = 2
        [Toggle] _ZWrite ("ZWrite", Float) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("_SrcBlend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("_DstBlend", Float) = 0.0
        
        [Header(Stencil)]
        [IntRange] _StencilRef("Stencil Ref Value", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comp", Float) = 8
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilPassOp("Stencil Op", Float) = 0

        [Header(Properties)]
		[Toggle(VERTEX_COLOR)] _VertexColor("Use Vertex Color", Float) = 0
        [HDR]_Color ("Main Color", Color) = (1,1,1,1)
        _AlphaMultiplier ("Alpha Multiplier", Float) = 1
        _MainTex ("Main Texture", 2D) = "white" {}
        [Toggle(ALPHA_CLIP)] _UseAlphaClip ("Use Alpha Clip", Float) = 0
        _AlphaClip ("Alpha Clip", Float) = 0.01
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Culling]
            Blend [_SrcBlend] [_DstBlend]

			Stencil
			{
				Ref [_StencilRef]
				Comp [_StencilComp]
				Pass [_StencilPassOp]
			}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #pragma shader_feature ALPHA_CLIP
            #pragma shader_feature VERTEX_COLOR

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				#ifdef VERTEX_COLOR
				fixed4 color : COLOR;
				#endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				#ifdef VERTEX_COLOR
				fixed4 color : COLOR;
				#endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _MainTex_ST;
            fixed4 _Color;
            fixed _AlphaClip;
            fixed _AlphaMultiplier;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				#ifdef VERTEX_COLOR
				o.color = v.color;
				#endif
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                fixed4 col = tex2D(_MainTex, i.uv);
                col = col * _Color;
                col.a *= _AlphaMultiplier;

				#ifdef VERTEX_COLOR
				col *= i.color;
				#endif

                #ifdef ALPHA_CLIP
                clip(col.a - _AlphaClip);
                #endif

                return col;
            }
            ENDCG
        }
    }
}