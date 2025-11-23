Shader "Unlit/BeneathStatic/GlitchTitle"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _GlitchIntensity ("Glitch Intensity", Range(0,1)) = 0.45
        _FlickerSpeed ("Flicker Speed", Range(0,10)) = 2.0
        _ScanlineStrength ("Scanline Strength", Range(0,1)) = 0.25
        _ChromaticOffset ("Chromatic Offset", Range(0,0.02)) = 0.006
        _SliceCount ("Glitch Slices", Range(1,10)) = 3
        _SliceAmplitude ("Slice Amplitude", Range(0,0.2)) = 0.05
        _Tint ("Tint Color", Color) = (0.6,0.95,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

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
            float _GlitchIntensity;
            float _FlickerSpeed;
            float _ScanlineStrength;
            float _ChromaticOffset;
            float _SliceCount;
            float _SliceAmplitude;
            float4 _Tint;
            float _TimeY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // simple pseudo-random by seed
            float rand(float2 co) {
                return frac(sin(dot(co, float2(12.9898,78.233))) * 43758.5453);
            }

            fixed4 sampleChromatic(sampler2D tex, float2 uv, float offset)
            {
                float2 uvR = uv + float2(offset, 0);
                float2 uvB = uv - float2(offset, 0);
                float r = tex2D(tex, uvR).r;
                float g = tex2D(tex, uv).g;
                float b = tex2D(tex, uvB).b;
                return fixed4(r,g,b,1);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // flicker multiplier (global intensity)
                float flick = 0.5 + 0.5 * sin(_TimeY * _FlickerSpeed * 6.28318 + rand(uv)*6.28);
                float global = lerp(0.7, 1.2, flick) * (1.0 + _GlitchIntensity * 0.6);

                // horizontal slice glitches
                float sliceCount = max(1.0, _SliceCount);
                float sliceIndex = floor(uv.y * sliceCount);
                float seed = rand(float2(sliceIndex, floor(_TimeY * 10.0)));
                float sliceShift = (seed - 0.5) * _SliceAmplitude * _GlitchIntensity * 2.0;
                // apply only to some slices based on intensity
                float sliceMask = step(0.6, rand(float2(sliceIndex, seed + 0.13)) + _GlitchIntensity * 0.3);
                uv.x += sliceShift * sliceMask;

                // small jitter across whole image
                uv.x += (rand(uv + _TimeY) - 0.5) * 0.004 * _GlitchIntensity;

                // chromatic sample
                float chroma = _ChromaticOffset * _GlitchIntensity * 0.8;
                fixed4 col = sampleChromatic(_MainTex, uv, chroma);

                // tint and global brightness
                col.rgb *= _Tint.rgb * global;

                // scanlines (subtle)
                float scan = sin((uv.y * 1024.0) * 3.1415) * 0.5 + 0.5;
                col.rgb = lerp(col.rgb, col.rgb * (1.0 - _ScanlineStrength), 1.0 - pow(scan, 2.0));

                // vignette + static speckle (tiny)
                float vign = smoothstep(0.2, 0.9, length(uv - 0.5) * 1.2);
                float speck = rand(uv * (_TimeY * 0.7 + 10.0));
                col.rgb = lerp(col.rgb, col.rgb + (speck - 0.5) * 0.06 * _GlitchIntensity, 0.05);

                // alpha based on brightness slightly
                col.a = 1.0;

                return col;
            }
            ENDCG
        }
    }
}
