#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

#include "UnityPBSLighting.cginc"

float4 _Tint;
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

    float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
    float3 specularTint;
    float oneMinusReflectivity;
    albedo = DiffuseAndSpecularFromMetallic(albedo, _Metallic, specularTint, oneMinusReflectivity);
    
    UnityLight light;
    light.color = lightColor;
    light.dir = lightDir;
    light.ndotl = DotClamped(i.normal, lightDir);
    
    UnityIndirect indirectLight;
    indirectLight.diffuse = 0;
    indirectLight.specular = 0;
    
    return UNITY_BRDF_PBS(
        albedo, 
        specularTint,
        oneMinusReflectivity,
        _Smoothness,
        i.normal,
        viewDir,
        light,
        indirectLight
    );
    
    /*
    float3 specular = specularTint * lightColor * pow(DotClamped(halfVector, i.normal),_Smoothness * 100);
    float3 diffuse = albedo * lightColor * DotClamped(lightDir, i.normal);
    
    return float4(diffuse + specular, 1);
    */
}
#endif