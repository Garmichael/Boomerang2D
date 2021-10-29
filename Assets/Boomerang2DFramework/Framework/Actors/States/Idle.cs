using Boomerang2DFramework.Framework.Actors.ActorStateProperties;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.States {
	public class Idle : ActorState {
		private IdleProperties MyStateProperties => (IdleProperties) StateProperties;

		public Idle(Actor actor, StateMachine stateMachine, StateProperties stateProperties) {
			Actor = actor;
			MyStateMachine = stateMachine;
			StateProperties = stateProperties;
		}

		[CanBeCalledByModificationTrigger]
		public void Flip() {
			Actor.FacingDirection = Actor.FacingDirection == Directions.Right
				? Directions.Left
				: Directions.Right;
		}

		public override void ProcessState() {
			base.ProcessState();

			float deceleration = MyStateProperties.Deceleration.BuildValue(Actor);
			float factoredDeceleration = deceleration;

			if (Actor.Velocity.x > 0) {
				Actor.OffsetVelocityX(-factoredDeceleration);

				if (Actor.Velocity.x <= 0f) {
					Actor.SetVelocityX(0);
				}
			} else if (Actor.Velocity.x < 0) {
				Actor.OffsetVelocityX(factoredDeceleration);

				if (Actor.Velocity.x >= 0f) {
					Actor.SetVelocityX(0);
				}
			}

			if (Actor.Velocity.y > 0) {
				Actor.OffsetVelocityY(-factoredDeceleration);

				if (Actor.Velocity.y <= 0f) {
					Actor.SetVelocityY(0);
				}
			} else if (Actor.Velocity.y < 0) {
				Actor.OffsetVelocityY(factoredDeceleration);

				if (Actor.Velocity.y >= 0f) {
					Actor.SetVelocityY(0);
				}
			}
		}

		public override void ProcessPostFrameState() {
			if (!MyStateProperties.CanBeGrounded) {
				return;
			}

			Actor.GenerateRayData(Directions.Down);

			if (Mathf.Abs(Actor.DistanceToSolidDown) <= MyStateProperties.GlueHeight.BuildValue(Actor)) {
				Actor.OffsetRealPositionY(-Actor.DistanceToSolidDown);
			}
		}
	}
}