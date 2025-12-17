using UnityEngine;

namespace Game.Network
{
    [System.Serializable]
    public struct FrameInput 
    {
        public Vector2 MoveDirection;
        public Vector2 LookDirection;
        public bool JumpDown;
        public bool RunHeld;
        public bool CrouchHeld;
    }
}