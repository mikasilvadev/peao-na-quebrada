using UnityEngine;

namespace Game.Player.Movement
{
    public enum InputMode { Hold, Toggle, AutoLock }

    [CreateAssetMenu(fileName = "Player_MovementSettings", menuName = "Settings/Movement Settings")]
    public class MovementSettings : ScriptableObject
    {
        [Header("Velocidades")]
        public float walkingSpeed = 5.0f;
        public float runningSpeed = 10.0f;
        public float crouchingSpeed = 2.5f;

        [Header("Pulo e Gravidade")]
        public float jumpForce = 4f;
        public float gravityScale = 1.0f;
        public float fallGravityScale = 2.0f;
        public float groundStickForce = 20f;
        [Range(0, 1)] public float airControlPercentage = 0.5f;

        [Header("Colisão (Agachamento)")]
        [Range(0.1f, 1f)] public float crouchHeightRatio = 0.5f;
        [Range(0.1f, 1f)] public float crouchCenterRatio = 0.5f;

        [Header("Rotação e Câmera")]
        public float mouseSensitivityX = 15f;
        public float mouseSensitivityY = 15f;
        public float rotationSmoothing = 10f;

        [Header("Comportamento de Input")]
        public InputMode runMode = InputMode.Hold;
        public InputMode crouchMode = InputMode.Hold;
        public float autoLockTime = 1.5f;
    }
}