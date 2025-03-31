Shader "Custom/TVStatic"
{
    Properties
    {
        _StaticIntensity("Static Intensity", Range(0,1)) = 1
        _ScanlineOffset("Scanline Offset", Range(-1,1)) = 0
        _NoiseScale("Noise Scale", Range(0.1, 10)) = 1
        _ScanlineWidth("Scanline Width", Range(0.01, 0.1)) = 0.05
        _Color("Color", Color) = (1,1,1,1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Lambert alpha:fade
        
        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
        };
        
        float _StaticIntensity;
        float _ScanlineOffset;
        float _NoiseScale;
        float _ScanlineWidth;
        float4 _Color;
        
        // Simple noise function
        float rand(float2 co)
        {
            return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
        }
        
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Create static noise
            float noise = rand(IN.screenPos.xy * _NoiseScale + _Time.y);
            noise = lerp(0, noise, _StaticIntensity);
            
            // Add scanlines
            float scanline = frac(IN.screenPos.y * _ScanlineWidth + _ScanlineOffset);
            scanline = smoothstep(0.4, 0.6, scanline);
            
            // Combine effects
            float3 finalColor = _Color.rgb * noise * scanline;
            
            o.Albedo = finalColor;
            o.Alpha = _Color.a * noise;
        }
        ENDCG
    }
    FallBack "Diffuse"
}