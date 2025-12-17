using UnityEngine;

namespace Game.Player.Movement
{
    public class PlayerInAirState : PlayerBaseState
    {
        public override PlayerMovementState StateEnum => PlayerMovementState.InAir;

        public override void EnterState(PlayerMovement player)
        {
            if (player.CurrentInput.JumpDown)
            {
                player.HandleJump();
            }

            player.InputManager.ConsumeJumpRequest();
        }

        public override void UpdateState(PlayerMovement player)
        {
            player.InputManager.ConsumeJumpRequest();

            if (player.isGrounded)
            {
                if (player.MoveMagnitude > 0.1f)
                    player.ChangeState(player.WalkingState);
                else
                    player.ChangeState(player.IdleState);
            }
        }

        public override void FixedUpdateState(PlayerMovement player)
        {
            player.HandleMovement();
            player.HandleRotation();
        }
    }
}