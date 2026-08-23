using UnityEngine;

namespace OpenDay
{
    /// <summary>
    /// One stage of the software lifecycle (Requirements → Build → Test) that
    /// the player activates with Interact. Stages must be run in order: each
    /// gates the next (the Software Engineering idea), and activating one out
    /// of turn wastes the work done so far (the Software Project Management
    /// idea about sequencing along a critical path).
    ///
    /// Setup (in the Unity Editor):
    ///   1. Add this component to a GameObject marking the stage's terminal.
    ///   2. Set Order (0-based) and assign the shared Gate.
    /// </summary>
    public class PipelineStage : MonoBehaviour, IInteractable
    {
        [Tooltip("Position in the pipeline; stage 0 must be run first.")]
        [SerializeField] private int order;

        [Tooltip("The gate this stage reports to.")]
        [SerializeField] private PipelineGate gate;

        [SerializeField] private Color pendingColor = new Color(0.45f, 0.45f, 0.5f);
        [SerializeField] private Color doneColor = new Color(0.25f, 0.75f, 0.35f);

        // Shared across the level's stages: how many have run correctly so far.
        private static int _nextExpectedOrder;

        private SpriteRenderer _renderer;
        private bool _isDone;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();

            // The first stage to wake resets the run (e.g. on scene reload).
            if (order == 0)
            {
                _nextExpectedOrder = 0;
            }

            Refresh();
        }

        public void Interact()
        {
            if (_isDone)
            {
                return;
            }

            if (order != _nextExpectedOrder)
            {
                // Out of order: the pipeline restarts, so stages must be redone.
                _nextExpectedOrder = 0;
                gate.ResetProgress();
                ResetAllStages();
                return;
            }

            _isDone = true;
            _nextExpectedOrder++;
            gate.StageCompleted();
            Refresh();
        }

        private void ResetAllStages()
        {
            foreach (var stage in FindObjectsByType<PipelineStage>(FindObjectsSortMode.None))
            {
                stage._isDone = false;
                stage.Refresh();
            }
        }

        private void Refresh()
        {
            if (_renderer != null)
            {
                _renderer.color = _isDone ? doneColor : pendingColor;
            }
        }
    }
}
