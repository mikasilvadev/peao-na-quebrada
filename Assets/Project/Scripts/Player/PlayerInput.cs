using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player.Input
{
    public class PlayerInput : MonoBehaviour
    {
        public event Action<Vector2> OnMoveInput;
        public event Action<Vector2> OnLookInput;
        public event Action OnJumpInput;
        public event Action<bool> OnRunInput;
        public event Action<bool> OnCrouchInput;

        public event Action OnInteractInput;

        public void OnMove(InputValue value)
        {
            OnMoveInput?.Invoke(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            OnLookInput?.Invoke(value.Get<Vector2>());
        }

        public void OnJump(InputValue value)
        {
            if (value.isPressed)
            {
                OnJumpInput?.Invoke();
            }
        }

        public void OnRun(InputValue value)
        {
            OnRunInput?.Invoke(value.isPressed);
        }

        public void OnCrouch(InputValue value)
        {
            OnCrouchInput?.Invoke(value.isPressed);
        }

        public void OnInteract(InputValue value)
        {
            if (value.isPressed) OnInteractInput?.Invoke();
        }
    }
}