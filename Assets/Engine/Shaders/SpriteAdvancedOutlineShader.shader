Shader "Sprites/AdvancedOutlineShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Outline Settings)]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size", Range(0, 20)) = 2
        _OutlineQuality ("Outline Quality", Range(4, 16)) = 8
        
        [Header(Outline Style)]
        [Toggle] _OnlyOutline ("Only Show Outline", Float) = 0
        [Toggle] _InnerOutline ("Inner Outline", Float) = 0
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.01
        
        [Header(Glow Effect)]
        [Toggle] _EnableGlow ("Enable Glow", Float) = 0
        _GlowColor ("Glow Color", Color) = (1,1,1,0.5)
        _GlowSize ("Glow Size", Range(0, 10)) = 3
        _GlowIntensity ("Glow Intensity", Range(0, 2)) = 1
        
        [Header(Animation)]
        [Toggle] _AnimateOutline ("Animate Outline", Float) = 0
        _AnimSpeed ("Animation Speed", Range(0, 10)) = 2
        _AnimAmplitude ("Animation Amplitude", Range(0, 5)) = 1
        [KeywordEnum(Pulse, Wave, Rainbow)] _AnimType ("Animation Type", Float) = 0
        
        [Header(Distortion)]
        [Toggle] _EnableDistortion ("Enable Distortion", Float) = 0
        _DistortionStrength ("Distortion Strength", Range(0, 0.1)) = 0.02
        _DistortionSpeed ("Distortion Speed", Range(0, 5)) = 1
        
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }
    
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        
        Pass
        {
            Name "AdvancedSpriteOutline"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma multi_compile_local _ _ONLYOUTLINE_ON
            #pragma multi_compile_local _ _INNEROUTLINE_ON
            #pragma multi_compile_local _ _ENABLEGLOW_ON
            #pragma multi_compile_local _ _ANIMATEOUTLINE_ON
            #pragma multi_compile_local _ _ENABLEDISTORTION_ON
            #pragma multi_compile_local _ANIMTYPE_PULSE _ANIMTYPE_WAVE _ANIMTYPE_RAINBOW
            
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_SpriteRendererColorArray)
                UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
            UNITY_INSTANCING_BUFFER_END(PerDrawSprite)
            
            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _RendererColor;
            fixed4 _OutlineColor;
            fixed4 _GlowColor;
            float _OutlineSize;
            float _OutlineQuality;
            float _AlphaThreshold;
            float _GlowSize;
            float _GlowIntensity;
            float _AnimSpeed;
            float _AnimAmplitude;
            float _DistortionStrength;
            float _DistortionSpeed;
            
            // 噪声函数
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            // HSV转RGB
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }
            
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.vertex = UnityFlipSprite(IN.vertex, unity_SpriteFlipArray);
                OUT.worldPos = mul(unity_ObjectToWorld, OUT.vertex).xy;
                OUT.vertex = UnityObjectToClipPos(OUT.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color * unity_SpriteRendererColorArray;
                
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                
                return OUT;
            }
            
            fixed4 SampleSpriteTexture(float2 uv)
            {
                fixed4 color = tex2D(_MainTex, uv);
                
                #if ETC1_EXTERNAL_ALPHA
                fixed4 alpha = tex2D(_AlphaTex, uv);
                color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
                #endif
                
                return color;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                
                #ifdef _ENABLEDISTORTION_ON
                // 添加扭曲效果
                float time = _Time.y * _DistortionSpeed;
                float2 distortion = float2(
                    sin(time + uv.y * 10) * _DistortionStrength,
                    cos(time + uv.x * 10) * _DistortionStrength
                );
                uv += distortion;
                #endif
                
                fixed4 c = SampleSpriteTexture(uv) * IN.color;
                
                // 计算动画参数
                float animValue = 1.0;
                #ifdef _ANIMATEOUTLINE_ON
                float time = _Time.y * _AnimSpeed;
                
                #ifdef _ANIMTYPE_PULSE
                animValue = 1.0 + sin(time) * _AnimAmplitude;
                #elif _ANIMTYPE_WAVE
                animValue = 1.0 + sin(time + IN.worldPos.x + IN.worldPos.y) * _AnimAmplitude;
                #elif _ANIMTYPE_RAINBOW
                animValue = 1.0;
                float hue = frac(time * 0.1 + (IN.worldPos.x + IN.worldPos.y) * 0.01);
                _OutlineColor.rgb = hsv2rgb(float3(hue, 1.0, 1.0));
                #endif
                #endif
                
                // 计算描边
                float outlineSize = _OutlineSize * animValue;
                float2 texelSize = _MainTex_TexelSize.xy * outlineSize;
                
                // 高质量描边采样
                fixed outline = 0;
                float angleStep = 6.28318530718 / _OutlineQuality;
                
                for (int i = 0; i < _OutlineQuality; i++)
                {
                    float angle = angleStep * i;
                    float2 offset = float2(cos(angle), sin(angle)) * texelSize;
                    outline += SampleSpriteTexture(uv + offset).a;
                }
                outline = min(outline, 1.0);
                
                // 发光效果
                fixed glow = 0;
                #ifdef _ENABLEGLOW_ON
                float glowSize = _GlowSize * animValue;
                float2 glowTexelSize = _MainTex_TexelSize.xy * glowSize;
                
                for (int j = 0; j < _OutlineQuality; j++)
                {
                    float angle = angleStep * j;
                    float2 offset = float2(cos(angle), sin(angle)) * glowTexelSize;
                    glow += SampleSpriteTexture(uv + offset).a;
                }
                glow = min(glow, 1.0) * _GlowIntensity;
                #endif
                
                // 内描边或外描边
                bool shouldOutline;
                #ifdef _INNEROUTLINE_ON
                shouldOutline = c.a > _AlphaThreshold && outline < 1.0;
                #else
                shouldOutline = c.a < _AlphaThreshold && outline > _AlphaThreshold;
                #endif
                
                #ifdef _ONLYOUTLINE_ON
                // 只显示描边
                if (shouldOutline)
                {
                    c = _OutlineColor * IN.color;
                    c.a *= outline;
                }
                else
                {
                    c.a = 0;
                }
                #else
                // 描边 + 原图
                if (shouldOutline)
                {
                    fixed4 outlinePixel = _OutlineColor * IN.color;
                    outlinePixel.a *= outline;
                    
                    #ifdef _ENABLEGLOW_ON
                    // 混合发光效果
                    fixed4 glowPixel = _GlowColor;
                    glowPixel.a *= glow;
                    c = lerp(glowPixel, outlinePixel, outline);
                    #else
                    c = outlinePixel;
                    #endif
                }
                #ifdef _ENABLEGLOW_ON
                else if (glow > _AlphaThreshold && c.a < _AlphaThreshold)
                {
                    // 只有发光，没有描边
                    c = _GlowColor;
                    c.a *= glow;
                }
                #endif
                #endif
                
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
