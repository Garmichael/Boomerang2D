using Boomerang2DFramework.Framework.Actors.ActorStateProperties;
using Boomerang2DFramework.Framework.StateManagement;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.States {
	public class SideViewJump : ActorState {
		private SideViewJumpProperties MyStateProperties => (SideViewJumpProperties) StateProperties;

		public SideViewJump(Actor actor, StateMachine stateMachine, StateProperties stateProperties) {
			Actor = actor;
			MyStateMachine = stateMachine;
			StateProperties = stateProperties;
		}

		public override void OnEnterState() {
			base.OnEnterState();

			if (MyStateProperties.ForceFacingDirectionOnEntry) {
				Actor.FacingDirection = MyStateProperties.FacingDirectionOnEntry;
			}

			Vector2 newVelocity = new Vector2(Actor.Velocity.x, MyStateProperties.JumpStrength.BuildValue(Actor));

			if (MyStateProperties.SetInitialHorizontalJumpStrength) {
				newVelocity.x = Actor.FacingDirection == Directions.Right
					? MyStateProperties.HorizontalJumpStrength.BuildValue(Actor)
					: -MyStateProperties.HorizontalJumpStrength.BuildValue(Actor);
			}

			Actor.SetVelocity(newVelocity);
		}

		public override void ProcessState() {
			base.ProcessState();

			bool isMovingRight = BoomerangUtils.GetInputResult(
				MyStateProperties.MoveRightInputType,
				MyStateProperties.MoveRightInputValue
			);

			bool isMovingLeft = BoomerangUtils.GetInputResult(
				MyStateProperties.MoveLeftInputType,
				MyStateProperties.MoveLeftInputValue
			);

			if (MyStateProperties.AutoJumpForward) {
				if (Actor.FacingDirection == Directions.Right) {
					isMovingRight = true;
					isMovingLeft = false;
				} else {
					isMovingRight = false;
					isMovingLeft = true;
				}
			} else if (MyStateProperties.AutoJumpBackward) {
				if (Actor.FacingDirection == Directions.Right) {
					isMovingRight = false;
					isMovingLeft = true;
				} else {
					isMovingRight = true;
					isMovingLeft = false;
				}
			}

			if (!isMovingRight && !isMovingLeft) {
				float decelerationForward = MyStateProperties.DecelerationForward.BuildValue(Actor);
				float decelerationBackward = MyStateProperties.DecelerationBackward.BuildValue(Actor);

				if (Actor.FacingDirection == Directions.Right) {
					if (Actor.Velocity.x > 0f) {
						Actor.OffsetVelocityX(-decelerationForward);

						if (Actor.Velocity.x <= 0f) {
							Actor.SetVelocityX(0);
						}
					} else {
						Actor.OffsetVelocityX(decelerationBackward);

						if (Actor.Velocity.x >= 0f) {
							Actor.SetVelocityX(0);
						}
					}
				} else if (Actor.FacingDirection == Directions.Left) {
					if (Actor.Velocity.x < 0f) {
						Actor.OffsetVelocityX(decelerationForward);
						if (Actor.Velocity.x >= 0f) {
							Actor.SetVelocityX(0);
						}
					} else {
						Actor.OffsetVelocityX(-decelerationBackward);
						if (Actor.Velocity.x <= 0f) {
							Actor.SetVelocityX(0);
						}
					}
				}
			} else if (isMovingRight) {
				float accelerationForward = MyStateProperties.AccelerationForward.BuildValue(Actor);
				float accelerationBackward = MyStateProperties.AccelerationBackward.BuildValue(Actor);
				float maxSpeedForward = MyStateProperties.MaxForwardSpeed.BuildValue(Actor);
				float maxSpeedBackward = MyStateProperties.MaxBackwardSpeed.BuildValue(Actor);

				if (MyStateProperties.MidAirChangeFacing) {
					Actor.FacingDirection = Directions.Right;
				}

				if (Actor.FacingDirection == Directions.Right) {
					Actor.OffsetVelocityX(accelerationForward);

					if (Actor.Velocity.x > maxSpeedForward) {
						Actor.SetVelocityX(maxSpeedForward);
					}
				} else {
					Actor.OffsetVelocityX(accelerationBackward);

					if (Actor.Velocity.x > maxSpeedBackward) {
						Actor.SetVelocityX(maxSpeedBackward);
					}
				}
			} else {
				float accelerationForward = MyStateProperties.AccelerationForward.BuildValue(Actor);
				float accelerationBackward = MyStateProperties.AccelerationBackward.BuildValue(Actor);
				float maxSpeedForward = MyStateProperties.MaxForwardSpeed.BuildValue(Actor);
				float maxSpeedBackward = MyStateProperties.MaxBackwardSpeed.BuildValue(Actor);

				if (MyStateProperties.MidAirChangeFacing) {
					Actor.FacingDirection = Directions.Left;
				}

				if (Actor.FacingDirection == Directions.Left) {
					Actor.OffsetVelocityX(-accelerationForward);

					if (Actor.Velocity.x < -maxSpeedForward) {
						Actor.SetVelocityX(-maxSpeedForward);
					}
				} else {
					Actor.OffsetVelocityX(-accelerationBackward);

					if (Actor.Velocity.x < -maxSpeedBackward) {
						Actor.SetVelocityX(-maxSpeedBackward);
					}
				}
			}

			Actor.OffsetVelocityY(-MyStateProperties.Gravity.BuildValue(Actor));
		}

		[CanBeCalledByModificationTrigger]
		public void DeadStop() {
			Actor.SetVelocityY(0);
		}
	}
}