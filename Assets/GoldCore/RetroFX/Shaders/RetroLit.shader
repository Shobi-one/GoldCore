// Vertex lit (Gouraud) material for the retro presets, for URP and the built-in pipeline. It reads the globals RetroCamera sets:
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
    // URP version: vertex lit like the built-in one below, with snapping and affine textures in every pass
    // that writes depth, so depth priming and shadows line up with the wobbling geometry.
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }
        Cull [_Cull]

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "RetroFX.cginc"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            half4 _Color;
            half4 _EmissionColor;
            float _VertexSnapOverride;
        CBUFFER_END
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma shader_feature_local RETRO_UNLIT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 affineUV : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half3 diffuse : TEXCOORD3;
                half3 ambient : TEXCOORD4;
                half fogFactor : TEXCOORD5;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs pos = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = RetroSnapVertex(pos.positionCS, _VertexSnapOverride);
                o.positionWS = pos.positionWS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.affineUV = RetroAffineUV(o.uv, o.positionCS);
                o.diffuse = 0;
                o.ambient = 1;

            #ifndef RETRO_UNLIT
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                Light mainLight = GetMainLight();
                o.diffuse = mainLight.color * saturate(dot(normalWS, mainLight.direction));
                o.ambient = SampleSH(normalWS);
                #if defined(_ADDITIONAL_LIGHTS_VERTEX) || defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                for (uint i = 0u; i < count; i++)
                {
                    Light light = GetAdditionalLight(i, pos.positionWS);
                    o.ambient += light.color * light.distanceAttenuation * saturate(dot(normalWS, light.direction));
                }
                #endif
            #endif

                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = RetroResolveUV(i.uv, i.affineUV, _MainTex_TexelSize);
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Color;
            #ifdef RETRO_UNLIT
                half3 lit = albedo.rgb;
            #else
                half shadow = MainLightRealtimeShadow(TransformWorldToShadowCoord(i.positionWS));
                half3 lit = albedo.rgb * (i.diffuse * shadow + i.ambient);
            #endif
                half3 col = MixFog(lit + _EmissionColor.rgb, i.fogFactor);
                return half4(col, albedo.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            float4 vert(Attributes v) : SV_POSITION
            {
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDir = normalize(_LightPosition - positionWS);
            #else
                float3 lightDir = _LightDirection;
            #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDir));
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                return positionCS;
            }

            half4 frag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 vert(float4 positionOS : POSITION) : SV_POSITION
            {
                return RetroSnapVertex(TransformObjectToHClip(positionOS.xyz), _VertexSnapOverride);
            }

            half frag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            Varyings vert(float4 positionOS : POSITION, float3 normalOS : NORMAL)
            {
                Varyings o;
                o.positionCS = RetroSnapVertex(TransformObjectToHClip(positionOS.xyz), _VertexSnapOverride);
                o.normalWS = TransformObjectToWorldNormal(normalOS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return half4(NormalizeNormalPerPixel(i.normalWS), 0);
            }
            ENDHLSL
        }
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
