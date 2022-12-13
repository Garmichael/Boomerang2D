using Boomerang2DFramework.Framework.Actors.ActorStateProperties;
using Boomerang2DFramework.Framework.StateManagement;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.States {
	public class Swoop : ActorState {
		private SwoopProperties MyStateProperties => (SwoopProperties) StateProperties;
		private Vector2 _startPoint;
		private Vector2 _midPoint;
		private Vector2 _endPoint;
		private Vector2 _controlA;
		private Vector2 _controlB;

		private float _velocity = 1 / GameProperties.PixelsPerUnit;

		private float _currentDistance;

		public Swoop(Actor actor, StateMachine stateMachine, StateProperties stateProperties) {
			Actor = actor;
			MyStateMachine = stateMachine;
			StateProperties = stateProperties;
		}

		public override void OnEnterState() {
			base.OnEnterState();

			_startPoint = new Vector2(
				Actor.RealPosition.x,
				Actor.RealPosition.y
			);

			_midPoint = new Vector2(
				MyStateProperties.X.BuildValue(Actor),
				MyStateProperties.Y.BuildValue(Actor)
			);

			_midPoint += new Vector2(0, (_midPoint.y - _startPoint.y) * .5f);

			float width = MyStateProperties.WidthPercent.BuildValue(Actor) / 100;
			_controlA = _midPoint + new Vector2((_startPoint.x - _midPoint.x) * width, 0);
			_controlB = _midPoint + new Vector2((_startPoint.x - _midPoint.x) * -width, 0);

			_endPoint = new Vector2(
				Actor.RealPosition.x - 2 * (Actor.RealPosition.x - _midPoint.x),
				Actor.RealPosition.y
			);

			Actor.FacingDirection = _midPoint.x < _startPoint.x ? Directions.Left : Directions.Right;

			_currentDistance = 0;
			_velocity = MyStateProperties.SpeedInPixels.BuildValue(Actor);
		}

		public override void ProcessState() {
			base.ProcessState();

			if (CompletedAction) {
				return;
			}

			_currentDistance += _velocity;

			Vector2 position = BoomerangUtils.GetPointOnBezier(
				BoomerangUtils.EaseLerp(_currentDistance, 1, 0, 1, MyStateProperties.Easing),
				_startPoint,
				_controlA,
				_controlB,
				_endPoint
			);

			Actor.SetRealPosition(position.x, position.y);

			if (_currentDistance >= 1f) {
				CompletedAction = true;
			}
		}
	}
}