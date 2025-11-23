using UnityEngine;
using TMPro;

public class TMPGlitchAnimator : MonoBehaviour
{
    TextMeshProUGUI tmp;
    Material mat;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        mat = Instantiate(tmp.fontMaterial);
        tmp.fontMaterial = mat;
    }

    void Update()
    {
        float t = Time.time;

        mat.SetFloat("_GlitchIntensity", 0.2f + Mathf.Abs(Mathf.Sin(t * 2.1f)) * 0.4f);
        mat.SetFloat("_ChromaticOffset", 0.003f + Mathf.Abs(Mathf.Sin(t * 1.3f)) * 0.003f);
    }
}
