using System;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.States {
	public class SineWave : ActorState {
		private SineWaveProperties MyStateProperties => (SineWaveProperties) StateProperties;
		private Vector2 _originalPosition;

		public SineWave(Actor actor, StateMachine stateMachine, StateProperties stateProperties) {
			Actor = actor;
			MyStateMachine = stateMachine;
			StateProperties = stateProperties;
		}

		public override void OnEnterState() {
			base.OnEnterState();

			_originalPosition = Actor.RealPosition;
			if (MyStateProperties.ForceFacingDirectionOnEntry) {
				Actor.FacingDirection = MyStateProperties.FacingDirectionOnEntry;
			}
		}

		public override void ProcessState() {
			base.ProcessState();
			bool isHorizontal = Actor.FacingDirection == Directions.Left || Actor.FacingDirection == Directions.Right;
			bool isPositive = Actor.FacingDirection == Directions.Right || Actor.FacingDirection == Directions.Up;

			float amplitude = MyStateProperties.Amplitude.BuildValue(Actor);
			float period = MyStateProperties.Period.BuildValue(Actor);
			float frequency = (float) Math.PI / period;

			if (isHorizontal) {
				float x = TimeInState * MyStateProperties.Speed.BuildValue(Actor);
				float y = amplitude * Mathf.Sin(x * frequency);

				if (isPositive) {
					x += _originalPosition.x;
					y += _originalPosition.y;
				} else {
					x = _originalPosition.x - x;
					y += _originalPosition.y;
				}

				Actor.SetRealPosition(x, y);
			} else {
				float y = TimeInState * MyStateProperties.Speed.BuildValue(Actor);
				float x = amplitude * Mathf.Sin(y * frequency);

				if (isPositive) {
					x += _originalPosition.x;
					y += _originalPosition.y;
				} else {
					x += _originalPosition.x;
					y = _originalPosition.y - y;
				}

				Actor.SetRealPosition(x, y);
			}
		}
	}
}