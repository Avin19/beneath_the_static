using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TapHoldInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float holdThreshold = 0.30f; // seconds to count as dash
    private float pressTime;
    public Action<string> OnInputRegistered; // subscriber receives "dot" or "dash"

    public void OnPointerDown(PointerEventData eventData)
    {
        pressTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float duration = Time.time - pressTime;
        if (duration < holdThreshold) RegisterInput("dot");
        else RegisterInput("dash");
    }

    void RegisterInput(string s)
    {
        Debug.Log("Input: " + s);
        OnInputRegistered?.Invoke(s);
    }
}
