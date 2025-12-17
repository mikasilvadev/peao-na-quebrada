using UnityEngine;

namespace Game.Player.Movement
{
    public class PlayerCrouchState : PlayerBaseState
    {
        public override PlayerMovementState StateEnum => PlayerMovementState.Crouching;

        public override void EnterState(PlayerMovement player)
        {
            player.SetCrouchState(true);
        }
        public override void UpdateState(PlayerMovement player)
        {
            var input = player.CurrentInput;

            if (!player.isGrounded)
            {
                player.InputManager.ResetCrouchLock();
                player.ChangeState(player.InAirState);
            }
            else if (input.JumpDown)
            {
                player.InputManager.ResetCrouchLock();
                player.ChangeState(player.InAirState);
            }
            else if (input.RunHeld && player.MoveMagnitude > 0.1f)
            {
                player.ChangeState(player.RunningState);
            }
            else if (!input.CrouchHeld)
            {
                if (player.MoveMagnitude > 0.1f)
                {
                    player.ChangeState(player.WalkingState);
                }
                else
                {
                    player.ChangeState(player.IdleState);
                }
            }
        }

        public override void FixedUpdateState(PlayerMovement player)
        {
            player.HandleMovement();
            player.HandleRotation();
        }

        public override void ExitState(PlayerMovement player)
        {
            player.SetCrouchState(false);
        }
    }
}