Shader "47E/Unlit_CustomLight"
{
    Properties
    {
        [Header(Config)]
        [Enum(UnityEngine.Rendering.CullMode)] _Culling("Culling", Float) = 2
        [Toggle] _ZWrite ("ZWrite", Float) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("_SrcBlend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("_DstBlend", Float) = 0.0

        [Header(Properties)]
        _MainTex ("Main Texture", 2D) = "white" {}
        // Tint color.
        [HDR] _BaseColor ("Base Color", Color) = (1,1,1,1)
        // Highlight color.
        [HDR] _HighlightColor ("Highlight Color", Color) = (0,0.8,1,1)
        // Highlight Intensity.
        _Highlight ("Highlight Intensity", Float) = 0
        // Intensity of a virtual light.
        _LightIntensity ("Virtual Light Intensity", Float) = 0.5
        // World space position of a virtual light.
        _LightPos ("Virtual Light Position", Vector) = (0,15,0,0)
        // World space position of a virtual ground plane.
        _GroundPos ("Ground Plane Position (Y)", Float) = 0
        
        [Header(Shader Features)]
        [Toggle(ALPHA_CLIP)] _UseAlphaClip ("Use Alpha Clip", Float) = 0
        _AlphaClip ("Alpha Clip", Float) = 0.01

        [Toggle(USE_LIGHTMAP)] _UseLightmap ("Use Occlusion", Float) = 0
        _LightmapAlpha ("Lightmap Alpha", Range(0, 1)) = 0.5
        
        [Toggle(USE_OCCLUSION)] _UseOcclusion ("Use Occlusion", Float) = 0
        _OcclusionMap ("Occlusion Texture", 2D) = "white" {}
        _OcclusionStrength ("Occlusion Intensity", Range(0.0, 2.0)) = 1
    }
    SubShader
    {
        Pass
        {
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            Cull [_Culling]
            Blend [_SrcBlend] [_DstBlend]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature ALPHA_CLIP USE_OCCLUSION
            #pragma shader_feature USE_OCCLUSION
            #pragma shader_feature USE_LIGHTMAP

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                half4 color : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float2 uv1 : TEXCOORD2;
                float4 vertex : SV_POSITION;
                
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _OcclusionMap;
            float4 _OcclusionMap_ST;
            fixed _OcclusionStrength;

            fixed _AlphaClip;
            fixed _LightmapAlpha;
            
            half4 _BaseColor;
            half3 _HighlightColor;
            half _Highlight;
            float _LightIntensity;
            float3 _LightPos;
            float _GroundPos;

            float3 worldPos;

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                #ifdef USE_LIGHTMAP
                o.uv1 = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
                #else
                o.uv1 = v.uv1;
                #endif

                // Get vertex world position.
                worldPos = mul(unity_ObjectToWorld, (v.vertex));

                // Simple nDotL lighting.
                float3 normal = UnityObjectToWorldNormal(v.normal);
                float3 lightDir = normalize(worldPos - _LightPos);
                float nDotL = dot(normal, -lightDir);
                float3 viewDir = normalize(WorldSpaceViewDir(v.vertex));
                float nDotV = dot(normal, viewDir);

                float lighting = nDotL * _LightIntensity;

                // Apply simple lighting at different rates per channel.
                // Red is more diffuse, while green and blue are more emissive.
                o.color.r += 0.25 * lighting;
                o.color.g += 0.25 * lighting;
                o.color.b += 0.125 * lighting;

                // Darken vertices that are closer to a defined ground plane
                float groundOccStart = worldPos.y - _GroundPos;
                float groundOccEnd = groundOccStart + 1;
                half groundMask = saturate(clamp(worldPos.y, groundOccStart, groundOccEnd));
                o.color.rgb = o.color.rgb * groundMask + 0.5 * o.color.rgb * (1 - groundMask);

                // Multiply by tint color.
                o.color.rgb *= _BaseColor;

                // Add highlight.
                float rim = 0.5 * (1 - nDotV);
                float highlightMask = saturate(_Highlight * rim);
                o.color.rgb = lerp(o.color.rgb, _HighlightColor.rgb, highlightMask);
                o.color.rgb += _HighlightColor.rgb * 0.5 * highlightMask;
                o.color.a = _BaseColor.a;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                fixed4 col = tex2D(_MainTex, i.uv);

                #ifdef ALPHA_CLIP
                clip(col.a - _AlphaClip);
                #endif

                #ifdef USE_LIGHTMAP
                fixed3 lightmapColor = lerp(fixed3(1,1,1), DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv1)), _LightmapAlpha);
                col *= fixed4(lightmapColor, 1);
                #endif

                #ifdef  USE_OCCLUSION
                fixed3 occlusion = lerp(1, tex2D(_OcclusionMap, i.uv), _OcclusionStrength);
                col *= fixed4(occlusion, 1);
                #endif
                
                return i.color * col;
            }
            ENDCG
        }
    }
}