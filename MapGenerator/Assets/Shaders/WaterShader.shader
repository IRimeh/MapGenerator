Shader "Unlit/WaterShader"
{
    Properties
    {
        [Header(Base Colors)]
        _DeepColor("Deep Color", Color) = (1,1,1,1)
        _ShallowColor("Shallow Color", Color) = (1,1,1,1)
        _MaxDepth("Max Depth", Range(0, 1)) = 0.5
        _AlphaDepth("Alpha Depth", Range(0, 1)) = 0.25

        [Header(Additional Colors)]
        _ShorelineColor("Shoreline Color", Color) = (1,1,1,1)
        _AdditionalSpecs("Additional Specs", float) = -0.0025

        [Header(Environment)]
        _ShadowStrength("Shadow Strenght", Range(0, 1)) = 0.5
        _EnvStrength("Environment Strength", Range(0, 1)) = 0.75
        [NoScaleOffset]_EnvTex("Environnement Tex", Cube) = "" {}

        [Header(Normals)]
        _NormalStrength("Normal Strength", Range(0, 2)) = 1
        [NoScaleOffset]_NormalTex("Normal", 2D) = "bump" {}

        [Header(Specular)]
        [HDR]_SpecularColor("Specular Color", Color) = (1,1,1,1)
        _Gloss("Glossiness", Range(0,1)) = 1

        [Header(Wave Variables)]
        _WaveSize("Wave Size", float) = 1
        _DetailWaveSize("Detail Wave Size", float) = 2
        _WaveScrollSpeed("Wave Scroll Speed", float) = 1
        _WaveScrollDir("Wave Scroll Direction", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalRenderPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                half2 uv : TEXCOORD0;
                half3 normal : NORMAL;
                half4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half3 uv : TEXCOORD0;
                half4 normal : ATTR1; // fogcoord in w
                half4 tspace0 : ATTR2; // wpos x in w
                half4 tspace1 : ATTR3; // wpos y in w
                half4 tspace2 : ATTR4; // wpos z in w
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_NormalTex);
            SAMPLER(sampler_NormalTex);
            float4 _NormalTex_ST;
            float _NormalStrength;
            samplerCUBE _EnvTex;
            float _EnvStrength;

            float4 _SpecularColor;
            float _Gloss;

            float _WaveSize;
            float _DetailWaveSize;
            float2 _WaveScrollDir;
            float _WaveScrollSpeed;

            float4 _DeepColor;
            float4 _ShallowColor;
            float _MaxDepth;
            float _AlphaDepth;

            float3 _PlayerPosition;
            float _ShadowStrength;

            float4 _ShorelineColor;
            float _AdditionalSpecs;

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.vertex.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normal.xyz, v.tangent);

                o.tspace0.xyz = half3(normalInputs.tangentWS.x, normalInputs.bitangentWS.x, normalInputs.normalWS.x);
                o.tspace1.xyz = half3(normalInputs.tangentWS.y, normalInputs.bitangentWS.y, normalInputs.normalWS.y);
                o.tspace2.xyz = half3(normalInputs.tangentWS.z, normalInputs.bitangentWS.z, normalInputs.normalWS.z);

                //Follow player
                positionInputs.positionWS.xz += _PlayerPosition.xz;
                positionInputs = GetVertexPositionInputs(mul(unity_WorldToObject, positionInputs.positionWS));

                o.vertex = positionInputs.positionCS;
                o.normal.xyz = normalInputs.normalWS;
                o.uv.xy = v.uv;
                o.uv.z = -TransformWorldToView(positionInputs.positionWS).z;

                // Pack vars
                o.normal.w = ComputeFogFactor(positionInputs.positionCS.z);
                o.tspace0.w = positionInputs.positionWS.x;
                o.tspace1.w = positionInputs.positionWS.y;
                o.tspace2.w = positionInputs.positionWS.z;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float3 wPos = half3(i.tspace0.w, i.tspace1.w, i.tspace2.w);
                float2 screenUvs = i.vertex.xy / _ScaledScreenParams.xy;

                // Normal Textures  
                float3 normalTex1 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, (wPos.xz + _WaveScrollDir * _Time.y * _WaveScrollSpeed) * _WaveSize * 0.01f));
                float3 normalTex2 = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, (wPos.xz - _WaveScrollDir * _Time.y * _WaveScrollSpeed) * _DetailWaveSize * 0.01f));
                float3 normalTex = (normalTex1 + normalTex2) * 0.5f;

                // Light
                float4 shadowCoord = TransformWorldToShadowCoord(wPos.xyz);
                Light light = GetMainLight(shadowCoord);

                // Construct normal
                float3 normal;
                normal.x = dot(i.tspace0.xyz, normalTex);
                normal.y = dot(i.tspace1.xyz, normalTex);
                normal.z = dot(i.tspace2.xyz, normalTex);
                normal = normalize(lerp(i.normal.xyz, normal, _NormalStrength));

                //Light calculation
                float3 lightDir = light.direction;
                float3 lightReflectDir = reflect(-lightDir, normal);
                float3 viewDir = normalize(_WorldSpaceCameraPos - wPos);
                float NDotL = dot(i.normal.xyz, lightDir);
                float NDotLNorm = dot(normal, lightDir);
                float fresnel = dot(viewDir, i.normal.xyz);
                float shadow = lerp(light.shadowAttenuation * light.distanceAttenuation, 1, 1.0f - _ShadowStrength);

                //Specular calculation
                float specular = saturate(dot(lightReflectDir, viewDir));
                specular = pow(specular, exp2(_Gloss * 12.0f)) * _SpecularColor;

                //Env
                float3 env = texCUBE(_EnvTex, reflect(-viewDir, i.normal.xyz));

                //Depth
                float rawDepth = SampleSceneDepth(screenUvs);
                float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float distToPlane = i.uv.z;
                float depth = (linearDepth - distToPlane) * 0.01f;

                //Line around geometry
                float shoreLine = step(depth - NDotLNorm * 0.015f, 0.001f);
                float additionalSpecs = saturate(step(NDotLNorm, 0.1f) - shoreLine) * _AdditionalSpecs;

                //Composition
                float alpha = lerp(_ShallowColor.a, _DeepColor.a, saturate(depth / _AlphaDepth));
                float3 baseTex = lerp(_ShallowColor, _DeepColor, saturate(depth / _MaxDepth));

                float4 output = float4((baseTex * NDotL + specular + env * _EnvStrength + (shoreLine * _ShorelineColor) + additionalSpecs) * shadow, saturate(alpha + specular + shoreLine));
                return output;
            }
        ENDHLSL
        }
    }
}