using UnityEngine;

namespace Game.Player.Movement
{
    public class PlayerRunningState : PlayerBaseState
    {
        public override PlayerMovementState StateEnum => PlayerMovementState.Running;
        public override void EnterState(PlayerMovement player) {}

        public override void UpdateState(PlayerMovement player)
        {
            var input = player.CurrentInput;

            if (!player.isGrounded) 
            { 
                player.ChangeState(player.InAirState); 
            }
            else if (input.JumpDown)
            { 
                player.ChangeState(player.InAirState);
            }
            else if (input.CrouchHeld)
            {
                player.ChangeState(player.CrouchState);
            }
            else if (!input.RunHeld || player.MoveMagnitude < 0.1f)
            {
                player.ChangeState(player.WalkingState);
            }
        }

        public override void FixedUpdateState(PlayerMovement player)
        {
            player.HandleMovement();
            player.HandleRotation();
        }
    }
}