Shader "Custom/SpriteLit"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _LightIntensity ("Light Intensity", Range(0, 3)) = 1.0
        _AmbientLight ("Ambient Light", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "LightMode"="ForwardBase"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 tspace0 : TEXCOORD2;
                float3 tspace1 : TEXCOORD3;
                float3 tspace2 : TEXCOORD4;
            };
            
            sampler2D _MainTex;
            sampler2D _NormalMap;
            float4 _MainTex_ST;
            float _LightIntensity;
            float _AmbientLight;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // Calculate tangent space for normal mapping
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                float3 worldBitangent = cross(worldNormal, worldTangent) * v.tangent.w;
                
                o.tspace0 = float3(worldTangent.x, worldBitangent.x, worldNormal.x);
                o.tspace1 = float3(worldTangent.y, worldBitangent.y, worldNormal.y);
                o.tspace2 = float3(worldTangent.z, worldBitangent.z, worldNormal.z);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample normal map and convert from [0,1] to [-1,1] range
                half3 tnormal = UnpackNormal(tex2D(_NormalMap, i.uv));
                
                // Transform normal from tangent to world space
                half3 worldNormal;
                worldNormal.x = dot(i.tspace0, tnormal);
                worldNormal.y = dot(i.tspace1, tnormal);
                worldNormal.z = dot(i.tspace2, tnormal);
                
                // Basic diffuse lighting
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                
                // Sample texture and apply lighting
                fixed4 albedo = tex2D(_MainTex, i.uv);
                fixed3 lightColor = _LightColor0.rgb * _LightIntensity;
                
                fixed3 diffuse = lightColor * nl + _AmbientLight;
                fixed4 col = fixed4(albedo.rgb * diffuse, albedo.a);
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
