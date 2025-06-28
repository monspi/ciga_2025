Shader "Custom/AdvancedOutlineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        
        [Header(Outline Settings)]
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01
        [Toggle] _OutlineOnly ("Outline Only", Float) = 0
        
        [Header(Outline Animation)]
        [Toggle] _AnimateOutline ("Animate Outline", Float) = 0
        _AnimationSpeed ("Animation Speed", Range(0, 10)) = 1
        _AnimationAmplitude ("Animation Amplitude", Range(0, 1)) = 0.1
        
        [Header(Advanced)]
        _OutlineZOffset ("Outline Z Offset", Range(-1, 1)) = 0
        [Toggle] _UseScreenSpaceOutline ("Screen Space Outline", Float) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        
        // 描边Pass
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZWrite On
            ColorMask RGB
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ _ANIMATEOUTLINE_ON
            #pragma multi_compile __ _USESCREENSPACEOUTLINE_ON
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };
            
            float _OutlineWidth;
            float4 _OutlineColor;
            float _OutlineZOffset;
            float _AnimationSpeed;
            float _AnimationAmplitude;
            
            v2f vert(appdata v)
            {
                v2f o;
                
                float3 norm = normalize(v.normal);
                float outlineWidth = _OutlineWidth;
                
                #ifdef _ANIMATEOUTLINE_ON
                // 动画描边宽度
                float time = _Time.y * _AnimationSpeed;
                outlineWidth += sin(time + v.vertex.x + v.vertex.y + v.vertex.z) * _AnimationAmplitude * _OutlineWidth;
                #endif
                
                #ifdef _USESCREENSPACEOUTLINE_ON
                // 屏幕空间描边
                float4 clipPos = UnityObjectToClipPos(v.vertex);
                float3 clipNormal = mul((float3x3)UNITY_MATRIX_VP, mul((float3x3)unity_ObjectToWorld, norm));
                float2 offset = normalize(clipNormal.xy) * outlineWidth * clipPos.w;
                clipPos.xy += offset;
                o.pos = clipPos;
                #else
                // 对象空间描边
                float4 pos = v.vertex;
                pos.xyz += norm * outlineWidth;
                o.pos = UnityObjectToClipPos(pos);
                #endif
                
                // 应用Z偏移
                o.pos.z += _OutlineZOffset * o.pos.w;
                
                // 传递顶点颜色用于颜色变化
                o.color = v.color;
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // 可以根据顶点颜色调整描边颜色
                return _OutlineColor * i.color;
            }
            ENDCG
        }
        
        // 主体Pass（如果不是只显示描边）
        Pass
        {
            Name "Main"
            Tags { "LightMode" = "ForwardBase" }
            Cull Back
            ZWrite On
            ZTest LEqual
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile __ _OUTLINEONLY_ON
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
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
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                SHADOW_COORDS(3)
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                TRANSFER_SHADOW(o);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                #ifdef _OUTLINEONLY_ON
                // 如果只显示描边，则不渲染主体
                discard;
                #endif
                
                // 纹理采样
                fixed4 tex = tex2D(_MainTex, i.uv);
                
                // 光照计算
                float3 worldNormal = normalize(i.worldNormal);
                float3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                
                // 漫反射
                float ndotl = max(0, dot(worldNormal, worldLightDir));
                fixed3 diffuse = _LightColor0.rgb * tex.rgb * _Color.rgb * ndotl;
                
                // 环境光
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * tex.rgb * _Color.rgb;
                
                // 阴影
                fixed shadow = SHADOW_ATTENUATION(i);
                
                return fixed4((diffuse + ambient) * shadow, tex.a * _Color.a);
            }
            ENDCG
        }
    }
    
    // 为2D对象优化的SubShader
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        
        // 2D描边Pass
        Pass
        {
            Name "2DOutline"
            Cull Off
            ZWrite Off
            
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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float4 _OutlineColor;
            float _OutlineWidth;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // 采样周围8个像素
                float2 texelSize = _MainTex_TexelSize.xy * _OutlineWidth;
                
                fixed4 upColor = tex2D(_MainTex, i.uv + fixed2(0, texelSize.y));
                fixed4 downColor = tex2D(_MainTex, i.uv - fixed2(0, texelSize.y));
                fixed4 rightColor = tex2D(_MainTex, i.uv + fixed2(texelSize.x, 0));
                fixed4 leftColor = tex2D(_MainTex, i.uv - fixed2(texelSize.x, 0));
                
                fixed4 centerColor = tex2D(_MainTex, i.uv);
                
                // 如果中心像素透明但周围有不透明像素，则绘制描边
                float outline = (1 - centerColor.a) * max(max(upColor.a, downColor.a), max(rightColor.a, leftColor.a));
                
                return fixed4(_OutlineColor.rgb, outline * _OutlineColor.a);
            }
            ENDCG
        }
        
        // 2D主体Pass
        Pass
        {
            Name "2DMain"
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                return tex * _Color * i.color;
            }
            ENDCG
        }
    }
    
    FallBack "Sprites/Default"
}
