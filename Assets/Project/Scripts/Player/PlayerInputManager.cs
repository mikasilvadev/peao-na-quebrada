using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Network;
using Game.Player.Input;

namespace Game.Player.Movement
{
    public class PlayerInputManager : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private MovementSettings settings;
        [SerializeField] private Game.Player.Input.PlayerInput playerInput;
        public bool IsRunning { get; private set; }
        public bool IsCrouching { get; private set; }
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpRequest { get; private set; }

        private class ComplexButtonLogic
        {
            public bool IsActive;
            public bool IsLocked;
            public bool IsPressed;
            public float PressTimer;
            public float LastPressTime;

            public void UpdateLogic(bool isPressed, InputMode mode, float autoLockTime)
            {
                IsPressed = isPressed;
                if (!isPressed) PressTimer = 0f;

                switch (mode)
                {
                    case InputMode.Hold:
                        IsActive = IsPressed;
                        IsLocked = false;
                        break;

                    case InputMode.Toggle:
                        IsActive = IsLocked;
                        break;

                    case InputMode.AutoLock:
                        if (IsPressed && !IsLocked)
                        {
                            PressTimer += Time.deltaTime;
                            if (PressTimer >= autoLockTime) IsLocked = true;
                            IsActive = true;
                        }
                        else
                        {
                            IsActive = IsLocked;
                        }
                        break;
                }
            }

            public void OnPress(InputMode mode)
            {
                LastPressTime = Time.time;

                if (mode == InputMode.Toggle)
                {
                    IsLocked = !IsLocked;
                }
                else if (mode == InputMode.AutoLock && IsLocked)
                {
                    IsLocked = false;
                }
            }
        }

        private ComplexButtonLogic runLogic = new ComplexButtonLogic();
        private ComplexButtonLogic crouchLogic = new ComplexButtonLogic();

        void OnEnable()
        {
            if (playerInput == null) playerInput = GetComponent<Game.Player.Input.PlayerInput>();
            playerInput.OnMoveInput += (v) => MoveInput = v;
            playerInput.OnLookInput += (v) => LookInput = v;
            playerInput.OnJumpInput += () => JumpRequest = true;
            playerInput.OnRunInput += HandleRunPress;
            playerInput.OnCrouchInput += HandleCrouchPress;
        }

        void OnDisable()
        {
            playerInput.OnRunInput -= HandleRunPress;
            playerInput.OnCrouchInput -= HandleCrouchPress;
        }

        private void HandleRunPress(bool pressed)
        {
            runLogic.IsPressed = pressed;
            if (pressed) runLogic.OnPress(settings.runMode);
        }

        private void HandleCrouchPress(bool pressed)
        {
            crouchLogic.IsPressed = pressed;
            if (pressed) crouchLogic.OnPress(settings.crouchMode);
        }

        public void ConsumeJumpRequest() => JumpRequest = false;

        public void ResetCrouchLock()
        {
            crouchLogic.IsLocked = false;
            IsCrouching = false;
        }

        void Update()
        {
            runLogic.UpdateLogic(runLogic.IsPressed, settings.runMode, settings.autoLockTime);
            crouchLogic.UpdateLogic(crouchLogic.IsPressed, settings.crouchMode, settings.autoLockTime);
            bool wantRun = runLogic.IsActive;
            bool wantCrouch = crouchLogic.IsActive;
            if (wantRun && wantCrouch)
            {
                if (runLogic.LastPressTime > crouchLogic.LastPressTime)
                {
                    IsRunning = true;
                    IsCrouching = false;
                    if (settings.crouchMode != InputMode.Hold) crouchLogic.IsLocked = false;
                }
                else
                {
                    IsRunning = false;
                    IsCrouching = true;
                    if (settings.runMode != InputMode.Hold) runLogic.IsLocked = false;
                }
            }
            else
            {
                IsRunning = wantRun;
                IsCrouching = wantCrouch;
            }
            if (settings.runMode == InputMode.Hold && !runLogic.IsPressed)
            {
                IsRunning = false;
            }
            if (settings.crouchMode == InputMode.Hold && !crouchLogic.IsPressed)
            {
                IsCrouching = false;
            }
        }
    }
}