Shader "47E/Unlit"
{
    Properties
    {
        [Header(Config)]
        [Enum(UnityEngine.Rendering.CullMode)] _Culling("Culling", Float) = 2
        [Toggle] _ZWrite ("ZWrite", Float) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
    	[Enum(UnityEngine.Rendering.BlendOp)] _ColorBlendOp ("ColorBlendOp", Float) = 0.0
    	[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcColorBlend", Float) = 1.0
    	[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstColorBlend", Float) = 0.0
        [Enum(UnityEngine.Rendering.BlendOp)] _AlphaBlendOp ("AlphaBlendOp", Float) = 0.0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendAlpha ("SrcAlphaBlend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendAlpha ("DstAlphaBlend", Float) = 0.0
        
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
	    Pass
        {
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Culling]
        	BlendOp [_ColorBlendOp], [_AlphaBlendOp]
            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]

			Stencil
			{
				Ref [_StencilRef]
				Comp [_StencilComp]
				Pass [_StencilPassOp]
			}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            #pragma multi_compile ALPHA_CLIP
            #pragma multi_compile VERTEX_COLOR

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
            	float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
				#ifdef VERTEX_COLOR
				fixed4 color : COLOR;
				#endif
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
			UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            #ifdef ALPHA_CLIP
            fixed _AlphaClip;
            #endif
            fixed _AlphaMultiplier;
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
            	//https://forum.unity.com/threads/gpu-instancing-with-single-pass-stereo-rendering.1222533/
            	//#ifdef UNITY_STEREO_INSTANCING_ENABLED
				//InstanceID = InstanceID/2;
				//#endif
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