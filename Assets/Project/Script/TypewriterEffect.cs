using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    public float charDelay = 0.035f;
    Coroutine running;

    public void StartTyping(string content)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(TypeRoutine(content));
    }

    IEnumerator TypeRoutine(string content)
    {
        textUI.text = "";
        foreach (char c in content)
        {
            textUI.text += c;
            yield return new WaitForSeconds(charDelay);
        }
        running = null;
    }
}

