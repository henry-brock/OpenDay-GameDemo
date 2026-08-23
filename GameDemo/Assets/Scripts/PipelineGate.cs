using UnityEngine;

namespace OpenDay
{
    /// <summary>
    /// The "deploy" gate blocking the Software Engineering key. Stays solid
    /// until every <see cref="PipelineStage"/> has been completed in order,
    /// then opens — you can't ship a build that hasn't passed the pipeline.
    ///
    /// Setup (in the Unity Editor):
    ///   1. Add this component to a solid platform-like GameObject.
    ///   2. Set Required Stages to the number of pipeline stages in the level.
    /// </summary>
    public class PipelineGate : MonoBehaviour
    {
        [Tooltip("How many pipeline stages must be completed before this opens.")]
        [SerializeField] private int requiredStages = 3;

        [SerializeField] private Color lockedColor = new Color(0.75f, 0.2f, 0.25f);
        [SerializeField] private Color openColor = new Color(0.25f, 0.75f, 0.35f);

        private SpriteRenderer _renderer;
        private Collider2D _collider;
        private int _stagesComplete;

        public bool IsOpen => _stagesComplete >= requiredStages;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            Refresh();
        }

        /// <summary>Called by a <see cref="PipelineStage"/> when it completes.</summary>
        public void StageCompleted()
        {
            _stagesComplete++;
            Refresh();

            if (IsOpen)
            {
                Debug.Log("Build passed the pipeline — deploy gate open.");
            }
        }

        /// <summary>Called when the player activates a stage out of order.</summary>
        public void ResetProgress()
        {
            if (_stagesComplete == 0)
            {
                return;
            }

            Debug.Log("Stage run out of order — pipeline reset to the start.");
            _stagesComplete = 0;
            Refresh();
        }

        private void Refresh()
        {
            // An open gate stops blocking the way and visibly turns green.
            if (_collider != null)
            {
                _collider.enabled = !IsOpen;
            }

            if (_renderer != null)
            {
                _renderer.color = IsOpen ? openColor : lockedColor;
            }
        }
    }
}
