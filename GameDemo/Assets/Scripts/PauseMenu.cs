using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OpenDay
{
    /// <summary>
    /// In-level pause menu: Resume / Save / Return to Title Screen. Opened
    /// with Escape (keyboard) or the gamepad Start button.
    ///
    /// Setup (in the Unity Editor):
    ///   1. Add this component to any GameObject in the level scene.
    ///   2. Assign menuRoot to the pause panel (a UI object, inactive by default).
    ///   3. Assign the three Button fields to your Resume/Save/Return buttons.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button returnToTitleButton;
        [SerializeField] private string titleScreenSceneName = "TitleScreen";

        private PlayerInventory _inventory;
        private PlayerInput _playerInput;
        private bool _isPaused;

        private void Awake()
        {
            _inventory = FindFirstObjectByType<PlayerInventory>();
            _playerInput = _inventory != null ? _inventory.GetComponent<PlayerInput>() : null;

            resumeButton.onClick.AddListener(Resume);
            saveButton.onClick.AddListener(Save);
            returnToTitleButton.onClick.AddListener(ReturnToTitle);

            SetPaused(false);
        }

        private void Update()
        {
            var pausePressed = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

            if (pausePressed)
            {
                SetPaused(!_isPaused);
            }
        }

        private void Resume()
        {
            SetPaused(false);
        }

        private void Save()
        {
            var collected = _inventory != null ? _inventory.Collected : Array.Empty<CSModule>();
            SaveSystem.Save(SceneManager.GetActiveScene().buildIndex, collected);
        }

        private void ReturnToTitle()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(titleScreenSceneName);
        }

        private void SetPaused(bool paused)
        {
            _isPaused = paused;
            menuRoot.SetActive(paused);
            Time.timeScale = paused ? 0f : 1f;

            // Stops gameplay input (e.g. the gamepad's A button also being
            // Jump) from leaking through while a menu button is being pressed.
            if (_playerInput != null)
            {
                _playerInput.enabled = !paused;
            }

            if (paused)
            {
                EventSystem.current?.SetSelectedGameObject(resumeButton.gameObject);
            }
        }
    }
}
