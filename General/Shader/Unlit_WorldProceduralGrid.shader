Shader "47E/ProceduralWorldspaceGrid"
{
    Properties
    {
        [Header(General Config)]
        [Enum(UnityEngine.Rendering.CullMode)] _Culling("Culling", Float) = 2
        [Toggle] _ZWrite ("ZWrite", Float) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("_SrcBlend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("_DstBlend", Float) = 0.0
        [Toggle(ALPHA_CLIP)] _UseAlphaClip ("Use Alpha Clip", Float) = 0
        _AlphaClip ("Alpha Clip", Float) = 0.01
        
        [Header(Properties)]
        [HDR] _GridColor ("Grid Color", Color) = (1, 1, 1, 1.0) 
        [HDR] _FillColor ("Fill Color", Color) = (1, 0.0, 0.0, 1.0) 
        _GridScale ("Grid Scale", Range(1, 32)) = 2
        _Width ("Width", Range(0, 0.75)) = 0.1
        _Smoothness("Smoothness", Range(0, 1)) = 0.1
        _Connection("Connection", Range(-100, 1)) = 1
        [Toggle(DISTANCE_FADE)] _UseCameraDistanceFade ("Use Camera Distance Fade", Float) = 0
        _DistanceClose ("Distance Close", Range(0,10)) = 0.4
        _DistanceFar ("Distance Far", Range(0,30)) = 3.4

    }
    SubShader
    {
        LOD 100
        Pass
        {
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Culling]
            Blend [_SrcBlend] [_DstBlend]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma shader_feature ALPHA_CLIP
            #pragma shader_feature DISTANCE_FADE
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                half3 normal: NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD2;
                half3 worldNormal: NORMAL; 
                fixed4 color : COLOR0;
                UNITY_VERTEX_OUTPUT_STEREO
            };


            half _GridScale;
            fixed4 _GridColor;
            fixed4 _FillColor;
            half _Width;
            half _Smoothness;
            half _Connection;
            fixed _AlphaClip;
            half _DistanceClose;
            half _DistanceFar;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                #ifdef DISTANCE_FADE
                half l = min(1.0, max(0.0, (length(WorldSpaceViewDir(v.vertex)) - _DistanceClose) / (_DistanceFar - _DistanceClose)));
                #else
                half l = 0;
                #endif
                o.color.a = lerp(1, 0.0, l);
                return o;
            }

            half4 fracSquare (float4 input)
            {
                float4 value = frac(input);
                value *= value;
                return value;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                half4 gridPos = _GridScale * i.worldPos;
                gridPos = max (fracSquare(gridPos), fracSquare(-gridPos));
                fixed high1 = 1 - _Width;
                fixed low1 = high1 - _Smoothness;
                half4 col = (gridPos - low1) / (high1 - low1);

                col = clamp(col, 0, 1);

                fixed3 normalFade = clamp(i.worldNormal*i.worldNormal,0.0,1.0);

                col *= lerp(_Connection, 1, lerp(gridPos.x, 1, normalFade.x));
                col *= lerp(_Connection, 1, lerp(gridPos.y, 1, normalFade.y));
                col *= lerp(_Connection, 1, lerp(gridPos.z, 1, normalFade.z));
                
                col.rgb *= (1 - normalFade);
                col.a = max (max(col.r, col.g), col.b);
                col = lerp(_FillColor, _GridColor, col.a);
                col.a *= i.color.a; //object vertex alpha + distance fade
                #ifdef ALPHA_CLIP
                clip(col.a - _AlphaClip);
                #endif

                return col;
            }
            ENDCG
        }
    }
}