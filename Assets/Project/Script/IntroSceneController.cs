using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroSceneController : MonoBehaviour
{
    [Header("UI References")]
    public Image dialImage; // DialUI (Image - Filled)
    public TypewriterEffect typewriter;
    public TextMeshProUGUI tapText;
    public string introText = "You found the radio buried under dust. When you turned it on… everything changed.";
    public float dialDuration = 2f;
    public float delayAfterTyping = 0.5f;

    void Start()
    {
        // initial states
        if (dialImage != null) dialImage.fillAmount = 0f;
        tapText.gameObject.SetActive(false);

        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        // Fade in canvas or radio if desired (add CanvasGroup fade here)
        // 1) Animate dial fill 0 -> 0.5
        float t = 0f;
        while (t < dialDuration)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 0.5f, t / dialDuration);
            if (dialImage != null) dialImage.fillAmount = f;
            yield return null;
        }

        // 2) Start typewriter
        typewriter.StartTyping(introText);

        // Wait until typing finished (approx time)
        yield return new WaitForSeconds(introText.Length * typewriter.charDelay + delayAfterTyping);

        // 3) Show tap-to-continue
        tapText.gameObject.SetActive(true);

        // Wait for tap/click
        bool tapped = false;
        while (!tapped)
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                tapped = true;
            }
            yield return null;
        }

        // Proceed to next scene (Scene3) or show level select
        SceneManager.LoadScene(2);
        Debug.Log("Intro tapped -> continue");
    }
}
