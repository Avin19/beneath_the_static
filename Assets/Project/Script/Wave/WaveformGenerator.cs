using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveformGenerator : MonoBehaviour
{
    [Header("Timing units (ms)")]
    public int dotMs = 200;
    public int dashMs = 600;
    public int gapMs = 200;

    [Header("Texture settings")]
    public int textureWidth = 1024;
    public int textureHeight = 128;
    public Color backgroundColor = new Color(0.02f, 0.04f, 0.04f);
    public Color waveformColor = new Color(0.0f, 0.8f, 0.9f, 1f);

    [Header("Output (uGUI)")]
    public RawImage targetRawImage;

    // Build a Texture2D from a symbol pattern (dot/dash/glitch).
    public Texture2D BuildWaveformTexture(List<string> patternSymbols)
    {
        // create texture
        Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;

        // clear with background
        Color[] bg = new Color[textureWidth * textureHeight];
        for (int i = 0; i < bg.Length; i++) bg[i] = backgroundColor;
        tex.SetPixels(bg);

        // compute total length in ms
        int totalMs = 0;
        for (int i = 0; i < patternSymbols.Count; i++)
        {
            string s = patternSymbols[i];
            if (s == "dot") totalMs += dotMs;
            else if (s == "dash") totalMs += dashMs;
            else if (s == "glitch") totalMs += dotMs; // treat glitch as short for layout
            if (i < patternSymbols.Count - 1) totalMs += gapMs;
        }

        // horizontal scaling: ms -> pixels
        float pxPerMs = (float)textureWidth / Mathf.Max(1, totalMs);

        // vertical center
        int midY = textureHeight / 2;
        int peakHeight = Mathf.FloorToInt(textureHeight * 0.38f); // max peak half-height

        int cursorPx = 0;
        for (int i = 0; i < patternSymbols.Count; i++)
        {
            string s = patternSymbols[i];

            int pulseMs = (s == "dash") ? dashMs : dotMs;
            int pulsePx = Mathf.Max(1, Mathf.RoundToInt(pulseMs * pxPerMs));
            // draw the pulse: a tapered rectangle / simple triangular peak
            DrawPulse(tex, cursorPx, pulsePx, midY, peakHeight, waveformColor);
            cursorPx += pulsePx;

            // draw gap
            if (i < patternSymbols.Count - 1)
            {
                int gapPx = Mathf.Max(1, Mathf.RoundToInt(gapMs * pxPerMs));
                cursorPx += gapPx;
            }
        }

        tex.Apply();
        // assign to RawImage if provided
        if (targetRawImage != null)
        {
            targetRawImage.texture = tex;
            // set uvRect so the texture loops nicely; default size = 1 (full texture)
            targetRawImage.uvRect = new Rect(0, 0, 1, 1);
        }

        return tex;
    }

    void DrawPulse(Texture2D tex, int startX, int width, int midY, int peakH, Color col)
    {
        int endX = Mathf.Min(tex.width, startX + width);
        for (int x = startX; x < endX; x++)
        {
            float pxT = (float)(x - startX) / Mathf.Max(1, (endX - startX - 1));
            float tri = 1f - Mathf.Abs(pxT - 0.5f) * 2f; // peak in middle
            int h = Mathf.RoundToInt(tri * peakH);

            for (int y = midY - h; y <= midY + h; y++)
            {
                if (y < 0 || y >= tex.height) continue;
                float dist = Mathf.Abs(y - midY) / (float)(h + 1);
                float alpha = Mathf.Clamp01(1f - dist * 1.6f);
                Color existing = tex.GetPixel(x, y);
                Color blended = Color.Lerp(existing, col, alpha);
                tex.SetPixel(x, y, blended);
            }
        }
    }
}
