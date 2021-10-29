using Boomerang2DFramework.Framework.Actors.ActorStateProperties;
using Boomerang2DFramework.Framework.StateManagement;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.States {
	public class PointToPoint : ActorState {
		private Vector2 _startPosition;
		private Vector2 _endPosition;

		private PointToPointProperties MyStateProperties => (PointToPointProperties) StateProperties;

		public PointToPoint(Actor actor, StateMachine stateMachine, StateProperties stateProperties) {
			Actor = actor;
			MyStateMachine = stateMachine;
			StateProperties = stateProperties;
		}

		public override void OnEnterState() {
			base.OnEnterState();
			_startPosition = Actor.RealPosition;
			_endPosition = MyStateProperties.EndPosition +
			               (MyStateProperties.EndPointRelative ? _startPosition : Vector2.zero);
		}

		public override void ProcessState() {
			Vector3 newLerpPosition = new Vector2(
				BoomerangUtils.EaseLerp(
					TimeInState,
					MyStateProperties.TimeToComplete,
					_startPosition.x,
					_endPosition.x,
					MyStateProperties.EasingMode
				),
				BoomerangUtils.EaseLerp(
					TimeInState,
					MyStateProperties.TimeToComplete,
					_startPosition.y,
					_endPosition.y,
					MyStateProperties.EasingMode
				)
			);

			Actor.SetVelocity(
				(newLerpPosition.x - Actor.RealPosition.x) / GameProperties.PixelSize,
				(newLerpPosition.y - Actor.RealPosition.y) / GameProperties.PixelSize
			);
		}
	}
}