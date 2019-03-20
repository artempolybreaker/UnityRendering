#if !defined(MY_LIGHTING_INCLUDED)
#define MY_LIGHTING_INCLUDED

#include "AutoLight.cginc"
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

UnityLight CreateLight(Interpolators i) {
    UnityLight light;
    
    #if defined(POINT) || defined(SPOT) || defined(POINT_COOKIE)
        light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
    #else
        light.dir = _WorldSpaceLightPos0.xyz;
    #endif
    
    UNITY_LIGHT_ATTENUATION(attenuation, 0, i.worldPos);

    light.color = _LightColor0.rgb * attenuation;
    light.ndotl = DotClamped(i.normal, light.dir);
    return light;
}

float4 MyFragmentProgram (Interpolators i) : SV_TARGET {
    i.normal = normalize(i.normal);
    
    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

    float3 albedo = tex2D(_MainTex, i.uv).rgb * _Tint.rgb;
    float3 specularTint;
    float oneMinusReflectivity;
    albedo = DiffuseAndSpecularFromMetallic(albedo, _Metallic, specularTint, oneMinusReflectivity);
        
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
        CreateLight(i),
        indirectLight
    );
}
#endif