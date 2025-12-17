using UnityEngine;

namespace Game.Player.Movement
{
    public abstract class PlayerBaseState
    {
        public abstract PlayerMovementState StateEnum { get; }
        public abstract void EnterState(PlayerMovement player);
        public abstract void UpdateState(PlayerMovement player);
        public abstract void FixedUpdateState(PlayerMovement player);
        public virtual void ExitState(PlayerMovement player) { }
    }
}