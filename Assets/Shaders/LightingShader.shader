// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/LightingShader" {
	Properties {
		_Tint ("Tint", Color) = (1, 1, 1, 1)
		_MainTex ("Albedo", 2D) = "white" {}
		_SpecularTint ("Specular", Color) = (1, 1, 1, 1)
		_Smoothness ("Smoothness", Range (0, 1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0
	}
	SubShader {
		Pass {
		
		    Tags {
		        "LightMode" = "ForwardBase"
		    }
		
            CGPROGRAM
			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "UnityStandardBRDF.cginc"
			#include "UnityStandardUtils.cginc"
			
			float4 _Tint;
			float4 _SpecularTint;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Smoothness;
			float _Metallic;
			
			struct VertexData {
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};
			
			struct Interpolators {
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
			};
			
            Interpolators MyVertexProgram (VertexData v) {
				Interpolators i;
                i.position = UnityObjectToClipPos(v.position);
                i.worldPos = mul(unity_ObjectToWorld, v.position);
                i.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                //i.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
                i.normal = mul(
                    transpose((float3x3)unity_WorldToObject),
                    v.normal
                );
                i.normal = normalize(i.normal);
                return i;
			}

			float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
			    i.normal = normalize(i.normal);
			    
			    float3 lightColor = _LightColor0.rgb;
			    float3 lightDir = _WorldSpaceLightPos0.xyz;
			    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
			    float3 reflectionDir = reflect(-lightDir, i.normal);
			    float3 halfVector = normalize(lightDir + viewDir);

			    float3 specular = _SpecularTint.rgb * lightColor * pow(DotClamped(halfVector, i.normal),_Smoothness * 100);
                float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
                float oneMinusReflectivity;
                albedo = EnergyConservationBetweenDiffuseAndSpecular(albedo, _SpecularTint.rgb, oneMinusReflectivity);
			    float3 diffuse = albedo * lightColor * DotClamped(lightDir, i.normal);
                
                return float4(diffuse + specular, 1);
			}

			ENDCG
		}
	}
}