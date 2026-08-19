using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OpenDay
{
    /// <summary>
    /// Title screen menu: Start New Game / Resume / Quit. Button highlighting
    /// and confirm (gamepad stick + A, or mouse/keyboard) is handled
    /// automatically by Unity's UI system once an EventSystem is present in
    /// the scene — no custom input polling needed here.
    ///
    /// Setup (in the Unity Editor):
    ///   1. Add this component to any GameObject in the title screen scene.
    ///   2. Assign the three Button fields below to your Start/Resume/Quit
    ///      buttons.
    ///   3. Make sure the scene has an EventSystem with an
    ///      Input System UI Input Module (needed for any UI button to work).
    /// </summary>
    public class TitleScreenController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private string firstLevelSceneName = "Level1_ObjectOrientedProgramming";

        private void Awake()
        {
            startButton.onClick.AddListener(StartNewGame);
            resumeButton.onClick.AddListener(ResumeGame);
            quitButton.onClick.AddListener(QuitGame);

            // Nothing to resume yet? Grey the button out rather than hide it,
            // so first-time players can see it's an option for later.
            resumeButton.interactable = SaveSystem.HasSave;
        }

        private void Start()
        {
            // So a gamepad player can press A immediately, no stick nudge needed.
            EventSystem.current?.SetSelectedGameObject(startButton.gameObject);
        }

        private void StartNewGame()
        {
            SceneManager.LoadScene(firstLevelSceneName);
        }

        private void ResumeGame()
        {
            if (!SaveSystem.TryLoad(out var levelBuildIndex, out var collectedModules))
            {
                return;
            }

            SaveSystem.SetPendingResume(collectedModules);
            SceneManager.LoadScene(levelBuildIndex);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
