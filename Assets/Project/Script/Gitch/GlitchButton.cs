using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

[RequireComponent(typeof(Button))]
public class GlitchButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Button button;
    public Image backgroundImage;       // assign BTN root Image
    public TextMeshProUGUI labelTMP;    // assign child TextMeshProUGUI
    public Material tmpGlitchMaterial;  // assign the MAT_TMP_Glitch material (optional)
    public float hoverScale = 1.06f;
    public float pressScale = 0.94f;
    public float animSpeed = 10f;

    // runtime copies so we don't edit shared material
    Material runtimeTMPMaterial;
    Vector3 targetScale = Vector3.one;

    void Awake()
    {
        button = GetComponent<Button>();
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
        if (labelTMP == null) labelTMP = GetComponentInChildren<TextMeshProUGUI>();

        // create instance material so tweaks per-button are safe
        if (tmpGlitchMaterial != null && labelTMP != null)
        {
            runtimeTMPMaterial = new Material(tmpGlitchMaterial);
            labelTMP.fontMaterial = runtimeTMPMaterial;
            labelTMP.UpdateMeshPadding();
        }
    }

    void Update()
    {
        // smooth scaling
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animSpeed);
    }

    // Pointer handlers (hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale;
        // brighten label a bit
        if (runtimeTMPMaterial != null) runtimeTMPMaterial.SetFloat("_GlitchIntensity", 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = Vector3.one;
        if (runtimeTMPMaterial != null) runtimeTMPMaterial.SetFloat("_GlitchIntensity", 0.32f);
    }

    // Press handlers
    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = Vector3.one * pressScale;
        // quick chroma burst for press
        if (runtimeTMPMaterial != null) runtimeTMPMaterial.SetFloat("_ChromaticOffset", 0.01f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = Vector3.one * hoverScale;
        if (runtimeTMPMaterial != null) runtimeTMPMaterial.SetFloat("_ChromaticOffset", 0.003f);
    }

    // Helper to assign click handler in code (optional)
    public void SetOnClick(Action onClick)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());
    }
}
