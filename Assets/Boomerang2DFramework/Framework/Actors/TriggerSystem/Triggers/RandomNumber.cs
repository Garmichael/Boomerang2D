using System;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class RandomNumber : ActorTrigger {
		[SerializeField] private RandomNumberProperties _properties;

		public RandomNumber(ActorTriggerProperties actorTriggerProperties) {
			_properties = (RandomNumberProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			int min = (int) _properties.MinValue.BuildValue(actor);
			int max = (int) _properties.MaxValue.BuildValue(actor);
			int target = (int) _properties.TargetValue.BuildValue(actor);
			
			int randomizedNumber = Random.Range(min, max);

			return
				_properties.Comparison == ValueComparison.LessThan && randomizedNumber < target ||
				_properties.Comparison == ValueComparison.LessThanOrEqual && randomizedNumber <= target ||
				_properties.Comparison == ValueComparison.Equal && randomizedNumber == target ||
				_properties.Comparison == ValueComparison.GreaterThanOrEqual && randomizedNumber >= target ||
				_properties.Comparison == ValueComparison.GreaterThan && randomizedNumber > target;
		}
	}
}