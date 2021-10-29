using System;
using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class StatBoolValue : ActorTrigger {
		[SerializeField] private StatBoolValueProperties _properties;

		public StatBoolValue(ActorTriggerProperties actorTriggerProperties) {
			_properties = (StatBoolValueProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			Dictionary<string, BoolStatProperties> hashedStats = actor.ActorProperties.StatsBools.ToDictionary(stat => stat.Name);

			if (hashedStats.ContainsKey(_properties.StatName)) {
				bool statValue = hashedStats[_properties.StatName].Value;

				return statValue == _properties.StatValue;
			}

			return false;
		}
	}
}