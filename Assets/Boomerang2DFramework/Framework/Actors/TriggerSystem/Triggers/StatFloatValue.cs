using System;
using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class StatFloatValue : ActorTrigger {
		[SerializeField] private StatFloatValueProperties _properties;

		public StatFloatValue(ActorTriggerProperties actorTriggerProperties) {
			_properties = (StatFloatValueProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			Dictionary<string, FloatStatProperties> hashedStats =
				actor.ActorProperties.StatsFloats.ToDictionary(actorPropertiesStat => actorPropertiesStat.Name);
			float target = _properties.StatValue.BuildValue(actor);

			if (hashedStats.ContainsKey(_properties.StatName)) {
				float statValue = hashedStats[_properties.StatName].Value;

				switch (_properties.Comparison) {
					case ValueComparison.Equal:
						return Math.Abs(statValue - target) < 0.001;
					case ValueComparison.LessThanOrEqual:
						return statValue <= target;
					case ValueComparison.LessThan:
						return statValue < target;
					case ValueComparison.GreaterThanOrEqual:
						return statValue >= target;
					case ValueComparison.GreaterThan:
						return statValue > target;
					default:
						return false;
				}
			}

			return false;
		}
	}
}