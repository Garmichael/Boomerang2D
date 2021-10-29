using System;
using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class StatStringValue : ActorTrigger {
		[SerializeField] private StatStringValueProperties _properties;

		public StatStringValue(ActorTriggerProperties actorTriggerProperties) {
			_properties = (StatStringValueProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			Dictionary<string, StringStatProperties> hashedStats = actor.ActorProperties.StatsStrings.ToDictionary(stat => stat.Name);

			if (hashedStats.ContainsKey(_properties.StatName)) {
				string statValue = hashedStats[_properties.StatName].Value;

				return statValue == _properties.StatValue;
			}

			return false;
		}
	}
}