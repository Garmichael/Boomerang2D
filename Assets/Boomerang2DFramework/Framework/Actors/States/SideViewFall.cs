using Boomerang2DFramework.Framework.Actors.ActorStateProperties;
using Boomerang2DFramework.Framework.StateManagement;
using Boomerang2DFramework.Framework.Utilities;

namespace Boomerang2DFramework.Framework.Actors.States {
	public class SideViewFall : ActorState {
		private SideViewFallProperties MyStateProperties => (SideViewFallProperties) StateProperties;

		public SideViewFall(Actor actor, StateMachine stateMachine, StateProperties stateProperties) {
			Actor = actor;
			MyStateMachine = stateMachine;
			StateProperties = stateProperties;
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

			if (MyStateProperties.AutoForward) {
				if (Actor.FacingDirection == Directions.Right) {
					isMovingRight = true;
					isMovingLeft = false;
				} else {
					isMovingRight = false;
					isMovingLeft = true;
				}
			} else if (MyStateProperties.AutoBackward) {
				if (Actor.FacingDirection == Directions.Right) {
					isMovingRight = false;
					isMovingLeft = true;
				} else {
					isMovingRight = true;
					isMovingLeft = false;
				}
			}

			float accelerationForward = MyStateProperties.AccelerationForward.BuildValue(Actor);
			float accelerationBackward = MyStateProperties.AccelerationBackward.BuildValue(Actor);
			float maxForwardSpeed = MyStateProperties.MaxForwardSpeed.BuildValue(Actor);
			float maxBackwardSpeed = MyStateProperties.MaxBackwardSpeed.BuildValue(Actor);

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
				if (MyStateProperties.MidAirChangeFacing) {
					Actor.FacingDirection = Directions.Right;
				}

				if (Actor.FacingDirection == Directions.Right) {
					Actor.OffsetVelocityX(accelerationForward);

					if (Actor.Velocity.x > maxForwardSpeed) {
						Actor.SetVelocityX(maxForwardSpeed);
					}
				} else {
					Actor.OffsetVelocityX(accelerationBackward);

					if (Actor.Velocity.x > maxBackwardSpeed) {
						Actor.SetVelocityX(maxBackwardSpeed);
					}
				}
			} else {
				if (MyStateProperties.MidAirChangeFacing) {
					Actor.FacingDirection = Directions.Left;
				}

				if (Actor.FacingDirection == Directions.Left) {
					Actor.OffsetVelocityX(-accelerationForward);

					if (Actor.Velocity.x < -maxForwardSpeed) {
						Actor.SetVelocityX(-maxForwardSpeed);
					}
				} else {
					Actor.OffsetVelocityX(-accelerationBackward);

					if (Actor.Velocity.x < -maxBackwardSpeed) {
						Actor.SetVelocityX(-maxBackwardSpeed);
					}
				}
			}

			if (Actor.Velocity.y > -MyStateProperties.TerminalVelocity.BuildValue(Actor)) {
				Actor.OffsetVelocityY(-MyStateProperties.FallAcceleration.BuildValue(Actor));
			}
		}

		public override void ProcessPostFrameState() {
			if (MyStateProperties.GlueDistance > 0 &&
			    (!MyStateProperties.GlueOnlyOnDescent || MyStateProperties.GlueOnlyOnDescent && Actor.Velocity.y < 0)
			) {
				Actor.GenerateRayData(Directions.Down);
				if (Actor.DistanceToSolidDown < MyStateProperties.GlueDistance) {
					Actor.OffsetRealPositionY(-Actor.DistanceToSolidDown);
				}
			}
		}
	}
}