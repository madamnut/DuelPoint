using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // 레거시 UI용

public class TitleManager : MonoBehaviour
{
    [SerializeField] private Button startButton; // Start 버튼
    [SerializeField] private Button exitButton;  // Exit 버튼

    private void Start()
    {
        // 버튼 이벤트 연결
        startButton.onClick.AddListener(OnStartClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지 - 이벤트 제거
        startButton.onClick.RemoveListener(OnStartClicked);
        exitButton.onClick.RemoveListener(OnExitClicked);
    }

    private void OnStartClicked()
    {
        // Lobby 씬으로 이동
        SceneManager.LoadScene("Lobby");
    }

    private void OnExitClicked()
    {
        // 애디터에서는 Stop, 빌드 시에는 종료
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
