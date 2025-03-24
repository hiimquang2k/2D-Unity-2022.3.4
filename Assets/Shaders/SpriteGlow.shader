Shader "Custom/SpriteGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 1.0
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01
    }
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _OutlineWidth;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Sample the texture around the current pixel for outline
                float2 offsets[4] = {
                    float2(0, _OutlineWidth),
                    float2(_OutlineWidth, 0),
                    float2(0, -_OutlineWidth),
                    float2(-_OutlineWidth, 0)
                };
                
                float outline = 0;
                for (int j = 0; j < 4; j++)
                {
                    float4 neighbor = tex2D(_MainTex, i.uv + offsets[j]);
                    outline = max(outline, neighbor.a - col.a);
                }
                
                // Add glow to the sprite
                float3 glow = _GlowColor.rgb * _GlowIntensity * col.a;
                col.rgb = lerp(col.rgb, glow, _GlowColor.a * 0.5);
                
                // Add outline
                col.rgb = lerp(col.rgb, _GlowColor.rgb, outline * _GlowColor.a);
                
                return col;
            }
            ENDCG
        }
    }
}