// Vertex lit (Gouraud) material for the retro presets. It reads the globals RetroCamera sets:
// vertex snapping, affine texture warping and point sampling. With the Modern preset, or no
// RetroCamera at all, it is a plain vertex lit shader.
Shader "GoldCore/RetroLit"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission", Color) = (0, 0, 0, 1)
        _VertexSnapOverride ("Vertex Snap Override (0 = use preset)", Float) = 0
        [Toggle(RETRO_UNLIT)] _Unlit ("Unlit", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Cull [_Cull]

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma shader_feature_local RETRO_UNLIT
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "RetroFX.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _EmissionColor;
            float _VertexSnapOverride;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 affineUV : TEXCOORD1;
                fixed3 diffuse : COLOR0;
                fixed3 ambient : COLOR1;
                SHADOW_COORDS(2)
                UNITY_FOG_COORDS(3)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = RetroSnapVertex(UnityObjectToClipPos(v.vertex), _VertexSnapOverride);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.affineUV = RetroAffineUV(o.uv, o.pos);

                o.diffuse = 0;
                o.ambient = 1;
            #ifndef RETRO_UNLIT
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                o.diffuse = _LightColor0.rgb * saturate(dot(worldNormal, lightDir));
                o.ambient = ShadeSH9(float4(worldNormal, 1));
                #ifdef VERTEXLIGHT_ON
                o.ambient += Shade4PointLights(unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                    unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                    unity_4LightAtten0, worldPos, worldNormal);
                #endif
            #endif

                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = RetroResolveUV(i.uv, i.affineUV, _MainTex_TexelSize);
                fixed4 albedo = tex2D(_MainTex, uv) * _Color;
            #ifdef RETRO_UNLIT
                fixed3 lit = albedo.rgb;
            #else
                fixed3 lit = albedo.rgb * (i.diffuse * SHADOW_ATTENUATION(i) + i.ambient);
            #endif
                fixed4 col = fixed4(lit + _EmissionColor.rgb, albedo.a);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        // Extra per-pixel lights (point and spot lights set to Important), lit per vertex.
        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma shader_feature_local RETRO_UNLIT
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "RetroFX.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _VertexSnapOverride;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 affineUV : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                fixed3 diffuse : COLOR0;
                UNITY_LIGHTING_COORDS(3, 4)
                UNITY_FOG_COORDS(5)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = RetroSnapVertex(UnityObjectToClipPos(v.vertex), _VertexSnapOverride);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.affineUV = RetroAffineUV(o.uv, o.pos);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 lightDir = normalize(UnityWorldSpaceLightDir(o.worldPos));
                o.diffuse = _LightColor0.rgb * saturate(dot(worldNormal, lightDir));
                UNITY_TRANSFER_LIGHTING(o, v.uv);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
            #ifdef RETRO_UNLIT
                return 0;
            #else
                float2 uv = RetroResolveUV(i.uv, i.affineUV, _MainTex_TexelSize);
                fixed4 albedo = tex2D(_MainTex, uv) * _Color;
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                fixed4 col = fixed4(albedo.rgb * i.diffuse * atten, 0);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0, 0, 0, 0));
                return col;
            #endif
            }
            ENDCG
        }
    }
    Fallback "Legacy Shaders/VertexLit"
}
