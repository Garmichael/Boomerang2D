using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class TimeInState : ActorTrigger {
		[SerializeField] private TimeInStateProperties _properties;

		public TimeInState(ActorTriggerProperties actorTriggerProperties) {
			_properties = (TimeInStateProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			ValueComparison comparison = _properties.Comparison;
			float lengthInState = _properties.LengthInState.BuildValue(actor);

			switch (comparison) {
				case ValueComparison.Equal:
					return Math.Abs(lengthInState - state.TimeInState) < 0.001;
				case ValueComparison.GreaterThan:
					return state.TimeInState > lengthInState;
				case ValueComparison.GreaterThanOrEqual:
					return state.TimeInState >= lengthInState;
				case ValueComparison.LessThan:
					return state.TimeInState < lengthInState;
				case ValueComparison.LessThanOrEqual:
					return state.TimeInState <= lengthInState;
				default:
					return false;
			}
		}
	}
}