using UnityEngine;
using UnityEngine.InputSystem;

namespace OpenDay
{
    /// <summary>
    /// Lets the player activate the nearest <see cref="ObjectBlueprint"/> with
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

            ObjectBlueprint nearest = null;
            var nearestDistance = interactRange;

            foreach (var blueprint in FindObjectsByType<ObjectBlueprint>(FindObjectsSortMode.None))
            {
                var distance = Vector2.Distance(transform.position, blueprint.transform.position);
                if (distance <= nearestDistance)
                {
                    nearest = blueprint;
                    nearestDistance = distance;
                }
            }

            nearest?.Spawn();
        }
    }
}
