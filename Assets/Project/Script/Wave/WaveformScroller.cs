using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class WaveformScroller : MonoBehaviour
{
    public float scrollSpeed = 0.2f; // uv units per second
    RawImage rawImg;
    Vector2 offset = Vector2.zero;

    void Awake()
    {
        rawImg = GetComponent<RawImage>();
    }

    void Update()
    {
        if (rawImg == null || rawImg.texture == null) return;
        offset.x += scrollSpeed * Time.deltaTime;
        // keep offset in 0..1
        if (offset.x > 1f) offset.x -= 1f;
        var uv = rawImg.uvRect;
        uv.x = offset.x;
        rawImg.uvRect = uv;
    }
}
