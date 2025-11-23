using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class MenuBackgroundController : MonoBehaviour
{
    [Header("Textures (prefer assigning in Inspector)")]
    public Texture2D mainTexture;       // main static image (background)
    public Texture2D noiseTexture;      // noise tile texture (small, tileable)

    [Header("Material & shader")]
    public Shader glitchShader;         // assign UI/Unlit/GlitchBackground or leave null to find by name
    Material matInstance;

    [Header("Effect settings")]
    [Range(0f, 1f)] public float noiseIntensity = 0.18f;
    public float scrollSpeed = 0.06f;
    [Range(0f, 1f)] public float flickerIntensity = 0.26f;
    [Range(0f, 1f)] public float glitchChance = 0.02f;
    public float glitchMaxOffset = 0.06f;
    [Range(0f, 1f)] public float scanlineStrength = 0.12f;
    [Range(0f, 1f)] public float vignette = 0.6f;
    [Range(0f, 0.02f)] public float chroma = 0.006f;

    [Header("Auto glitch burst (optional)")]
    public float burstIntervalMin = 6f;
    public float burstIntervalMax = 18f;
    public float burstDuration = 0.6f;
    public float burstGlitchChance = 0.25f;
    public float burstMaxOffset = 0.18f;

    // example local path from this session (editor only); change if needed
    public string fallbackLocalPath = "/mnt/data/A_digital_image_depicts_a_textured,_static_noise_p.png";

    RawImage rawImg;

    void Awake()
    {
        rawImg = GetComponent<RawImage>();

        if (glitchShader == null)
        {
            glitchShader = Shader.Find("UI/Unlit/GlitchBackground");
        }

        matInstance = new Material(glitchShader);

        // Try to auto-load main texture if not assigned
        if (mainTexture == null)
        {
            // Try load from local path (Editor/Standalone only)
            if (File.Exists(fallbackLocalPath))
            {
                byte[] bytes = File.ReadAllBytes(fallbackLocalPath);
                Texture2D t = new Texture2D(2, 2);
                if (t.LoadImage(bytes))
                {
                    mainTexture = t;
                }
            }
        }

        // If still null, create a simple 2x2 color texture as fallback
        if (mainTexture == null)
        {
            mainTexture = new Texture2D(2, 2);
            mainTexture.SetPixels(new Color[] { Color.black, Color.black, Color.black, Color.black });
            mainTexture.Apply();
        }

        // noise texture fallback (small built-in noise)
        if (noiseTexture == null)
        {
            noiseTexture = GenerateNoiseTexture(128, 128);
            noiseTexture.wrapMode = TextureWrapMode.Repeat;
            noiseTexture.filterMode = FilterMode.Bilinear;
        }

        // assign to material
        matInstance.SetTexture("_MainTex", mainTexture);
        matInstance.SetTexture("_NoiseTex", noiseTexture);

        // set initial values
        ApplyInspectorValues();

        // assign to RawImage
        rawImg.material = matInstance;

        // start bursts
        StartCoroutine(BurstLoop());
    }

    void ApplyInspectorValues()
    {
        matInstance.SetFloat("_NoiseIntensity", noiseIntensity);
        matInstance.SetFloat("_ScrollSpeed", scrollSpeed);
        matInstance.SetFloat("_FlickerIntensity", flickerIntensity);
        matInstance.SetFloat("_GlitchChance", glitchChance);
        matInstance.SetFloat("_GlitchMaxOffset", glitchMaxOffset);
        matInstance.SetFloat("_ScanlineStrength", scanlineStrength);
        matInstance.SetFloat("_Vignette", vignette);
        matInstance.SetFloat("_Chromatic", chroma);
    }

    void Update()
    {
        // allow runtime tweaking
        ApplyInspectorValues();

        // small subtle variation to flicker over time for life
        float flickVariation = 0.6f + 0.4f * Mathf.Sin(Time.time * 0.8f);
        matInstance.SetFloat("_FlickerIntensity", flickerIntensity * flickVariation);

        // optionally we can animate noise tiling/offset via material UV, but shader handles scroll using _Time
    }

    IEnumerator BurstLoop()
    {
        while (true)
        {
            float wait = Random.Range(burstIntervalMin, burstIntervalMax);
            yield return new WaitForSeconds(wait);

            // do a glitch burst
            float prevChance = matInstance.GetFloat("_GlitchChance");
            float prevOffset = matInstance.GetFloat("_GlitchMaxOffset");

            matInstance.SetFloat("_GlitchChance", burstGlitchChance);
            matInstance.SetFloat("_GlitchMaxOffset", burstMaxOffset);

            yield return new WaitForSeconds(burstDuration);

            // restore
            matInstance.SetFloat("_GlitchChance", prevChance);
            matInstance.SetFloat("_GlitchMaxOffset", prevOffset);
        }
    }

    // cheap procedural noise fallback when no noise texture assigned
    Texture2D GenerateNoiseTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.R8, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        Color[] cols = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n = Random.value;
                cols[y * w + x] = new Color(n, n, n);
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }
}
