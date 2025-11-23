Shader "TextMeshPro/Unlit/TMP_Glitch"
{
    Properties
    {
        _FaceColor ("Face Color", Color) = (0.8,0.95,1,1)
        _MainTex ("Font Atlas", 2D) = "white" {}
        _GlitchIntensity ("Glitch Intensity", Range(0,1)) = 0.35
        _FlickerSpeed ("Flicker Speed", Range(0,10)) = 2.0
        _ChromaticOffset ("Chromatic Offset", Range(0,0.02)) = 0.004
        _SliceCount ("Slices", Range(1,10)) = 3
        _SliceAmplitude ("Slice Amp", Range(0,0.2)) = 0.03
        _ScanlineStrength ("Scanline", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True"
            "RenderType"="Transparent" 
        }

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
            float4 _FaceColor;
            float _GlitchIntensity;
            float _FlickerSpeed;
            float _ChromaticOffset;
            float _SliceCount;
            float _SliceAmplitude;
            float _ScanlineStrength;
            float4 _MainTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898,78.233))) * 43758.5453);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // --- Flicker ---
                float flick = 0.5 + 0.5 * sin(_Time.y * _FlickerSpeed * 6.283 + rand(uv)*6.28);
                float global = lerp(0.7, 1.2, flick);

                // --- Slice Glitch ---
                float sliceCount = max(1.0, _SliceCount);
                float sliceIndex = floor(uv.y * sliceCount);
                float seed = rand(float2(sliceIndex, floor(_Time.y * 8.0)));
                float sliceShift = (seed - 0.5) * _SliceAmplitude * 2.0 * _GlitchIntensity;

                float sliceMask = step(0.6, rand(float2(sliceIndex, seed + 0.13)) + _GlitchIntensity * 0.3);
                uv.x += sliceShift * sliceMask;

                // --- Chromatic Offset ---
                float chroma = _ChromaticOffset * _GlitchIntensity;
                float r = tex2D(_MainTex, uv + float2(chroma,0)).a;
                float g = tex2D(_MainTex, uv).a;
                float b = tex2D(_MainTex, uv - float2(chroma,0)).a;

                float sdf = g; // SDF stored in alpha channel
                float w = fwidth(sdf) * 0.5;
                float alpha = smoothstep(0.5 - w, 0.5 + w, sdf);

                float3 col = float3(r,g,b) * _FaceColor.rgb * global;

                // --- Scanlines ---
                float scan = sin((uv.y * 900.0)) * 0.5 + 0.5;
                col = lerp(col, col * (1.0 - _ScanlineStrength), scan);

                return float4(col, alpha * _FaceColor.a);
            }
            ENDCG
        }
    }
}
