// MenuManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button playButton;

    void Start()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        // or use playButton.SetOnClick(() => OnPlayClicked());
    }

    void OnPlayClicked()
    {
        Debug.Log("Play pressed — load Level Select");
        SceneManager.LoadScene(1);
    }
}
