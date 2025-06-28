Shader "Sprites/OutlineShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size", Range(0, 10)) = 1
        
        [Header(Advanced)]
        [Toggle] _OnlyOutline ("Only Outline", Float) = 0
        [Toggle] _UseAlpha ("Use Alpha Channel", Float) = 1
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.1
        
        [Header(Animation)]
        [Toggle] _AnimateOutline ("Animate Outline", Float) = 0
        _AnimSpeed ("Animation Speed", Range(0, 10)) = 1
        _AnimAmplitude ("Animation Amplitude", Range(0, 2)) = 0.5
        
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
            Name "SpriteOutline"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma multi_compile_local _ _ONLYOUTLINE_ON
            #pragma multi_compile_local _ _USEALPHA_ON
            #pragma multi_compile_local _ _ANIMATEOUTLINE_ON
            
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
            float _OutlineSize;
            float _AlphaThreshold;
            float _AnimSpeed;
            float _AnimAmplitude;
            
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.vertex = UnityFlipSprite(IN.vertex, unity_SpriteFlipArray);
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
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
                
                // 计算描边大小（支持动画）
                float outlineSize = _OutlineSize;
                #ifdef _ANIMATEOUTLINE_ON
                float time = _Time.y * _AnimSpeed;
                outlineSize += sin(time + IN.texcoord.x * 10 + IN.texcoord.y * 10) * _AnimAmplitude;
                #endif
                
                // 计算纹理采样偏移
                float2 texelSize = _MainTex_TexelSize.xy * outlineSize;
                
                // 采样8个方向的像素
                fixed outline = 0;
                outline += SampleSpriteTexture(IN.texcoord + fixed2(texelSize.x, 0)).a;
                outline += SampleSpriteTexture(IN.texcoord + fixed2(-texelSize.x, 0)).a;
                outline += SampleSpriteTexture(IN.texcoord + fixed2(0, texelSize.y)).a;
                outline += SampleSpriteTexture(IN.texcoord + fixed2(0, -texelSize.y)).a;
                outline += SampleSpriteTexture(IN.texcoord + fixed2(texelSize.x, texelSize.y)).a;
                outline += SampleSpriteTexture(IN.texcoord + fixed2(-texelSize.x, texelSize.y)).a;
                outline += SampleSpriteTexture(IN.texcoord + fixed2(texelSize.x, -texelSize.y)).a;
                outline += SampleSpriteTexture(IN.texcoord + fixed2(-texelSize.x, -texelSize.y)).a;
                
                outline = min(outline, 1.0);
                
                #ifdef _USEALPHA_ON
                // 使用Alpha通道判断是否需要描边
                bool shouldOutline = c.a < _AlphaThreshold && outline > _AlphaThreshold;
                #else
                // 简单的透明度判断
                bool shouldOutline = c.a < 0.1 && outline > 0.1;
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
                    c = outlinePixel;
                }
                #endif
                
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
