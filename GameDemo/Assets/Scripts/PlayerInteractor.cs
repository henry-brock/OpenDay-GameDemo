using UnityEngine;
using UnityEngine.InputSystem;

namespace OpenDay
{
    /// <summary>
    /// Lets the player activate the nearest <see cref="IInteractable"/> with
    /// the Interact action (hold E, or the gamepad's North/Y button).
    ///
    /// Setup (in the Unity Editor):
    ///   1. Add this component to the Player GameObject (alongside PlayerInput).
    /// The PlayerInput component calls OnInteract below.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [Tooltip("How close a blueprint needs to be to activate.")]
        [SerializeField] private float interactRange = 3f;

        /// <summary>Called by PlayerInput (Send Messages) for the "Interact" action.</summary>
        private void OnInteract(InputValue value)
        {
            if (!value.isPressed)
            {
                return;
            }

            IInteractable nearest = null;
            var nearestDistance = interactRange;

            foreach (var candidate in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (candidate is not IInteractable interactable)
                {
                    continue;
                }

                var distance = Vector2.Distance(transform.position, candidate.transform.position);
                if (distance <= nearestDistance)
                {
                    nearest = interactable;
                    nearestDistance = distance;
                }
            }

            nearest?.Interact();
        }
    }
}
