Shader "UI/Unlit/GlitchBackground"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture (tile)", 2D) = "white" {}
        _Tint ("Tint Color", Color) = (0.02,0.06,0.08,1)
        _NoiseIntensity ("Noise Intensity", Range(0,1)) = 0.18
        _ScrollSpeed ("Noise Scroll Speed", Range(0,2)) = 0.05
        _FlickerIntensity ("Flicker Intensity", Range(0,1)) = 0.25
        _GlitchChance ("Glitch Chance", Range(0,1)) = 0.02
        _GlitchMaxOffset ("Max Glitch Offset", Range(0,0.2)) = 0.06
        _ScanlineStrength ("Scanline Strength", Range(0,1)) = 0.12
        _Vignette ("Vignette Strength", Range(0,1)) = 0.6
        _Chromatic ("Chromatic Offset", Range(0,0.02)) = 0.006
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _Tint;
            float _NoiseIntensity;
            float _ScrollSpeed;
            float _FlickerIntensity;
            float _GlitchChance;
            float _GlitchMaxOffset;
            float _ScanlineStrength;
            float _Vignette;
            float _Chromatic;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float rand(float2 co) {
                return frac(sin(dot(co, float2(12.9898,78.233))) * 43758.5453);
            }

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                float2 uv = i.uv;

                // base color from main texture
                float4 baseCol = tex2D(_MainTex, uv) * _Tint;

                // noise: tiled noise texture, scrolling
                float2 noiseUV = uv * float2(4.0, 8.0); // tile noise; tune if needed
                noiseUV.x += _Time.y * _ScrollSpeed;
                noiseUV.y += _Time.y * (_ScrollSpeed * 0.2);
                float noise = tex2D(_NoiseTex, noiseUV).r;

                // flicker (slow) modulated by noise
                float flick = 1.0 + (sin(_Time.y * 3.0 + noise * 6.28) * 0.5) * _FlickerIntensity;
                baseCol.rgb *= flick;

                // scanlines (subtle)
                float scan = sin((uv.y * 1080.0) * 3.14159) * 0.5 + 0.5;
                baseCol.rgb = lerp(baseCol.rgb, baseCol.rgb * (1.0 - _ScanlineStrength), 1.0 - pow(scan, 2.0));

                // vignette
                float2 centered = uv - 0.5;
                float vign = smoothstep(0.9, 0.2, length(centered) * (1.0 + _Vignette));
                baseCol.rgb *= vign;

                // apply noise overlay as subtle grain
                baseCol.rgb = lerp(baseCol.rgb, baseCol.rgb + (noise - 0.5) * 0.35, _NoiseIntensity);

                // small random glitch slices (horizontal offsets)
                float glitch = 0.0;
                float2 uvR = uv, uvG = uv, uvB = uv;
                // choose occasional glitch seed
                float seed = rand(float2(floor(uv.y * 64.0), floor(_Time.y * 4.0)));
                if (seed < _GlitchChance) {
                    // compute a slice offset based on seed
                    float sliceOffset = (rand(float2(_Time.y, uv.y)) - 0.5) * _GlitchMaxOffset;
                    uvR.x += sliceOffset * 1.2;
                    uvG.x += sliceOffset * 0.5;
                    uvB.x -= sliceOffset * 0.8;
                    glitch = 1.0;
                } else {
                    // chromatic micro shift even when no full glitch
                    uvR.x += _Chromatic * 0.7 * (noise - 0.5);
                    uvB.x -= _Chromatic * 0.7 * (noise - 0.5);
                }

                // sample with chroma separation
                float r = tex2D(_MainTex, uvR).r;
                float g = tex2D(_MainTex, uvG).g;
                float b = tex2D(_MainTex, uvB).b;

                float4 finalCol;
                finalCol.r = r;
                finalCol.g = g;
                finalCol.b = b;
                finalCol.a = baseCol.a; // keep alpha from base

                // boost/attenuate slightly when glitch occurs
                finalCol.rgb = lerp(finalCol.rgb, finalCol.rgb * (1.0 + glitch * 0.25), glitch);

                return finalCol;
            }
            ENDCG
        }
    }
}
