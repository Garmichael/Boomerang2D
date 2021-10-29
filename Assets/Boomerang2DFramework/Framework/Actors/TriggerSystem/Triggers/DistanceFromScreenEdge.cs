using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.StateManagement;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class DistanceFromScreenEdge : ActorTrigger {
		[SerializeField] private DistanceFromScreenEdgeProperties _properties;

		public DistanceFromScreenEdge(ActorTriggerProperties actorTriggerProperties) {
			_properties = (DistanceFromScreenEdgeProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			Dictionary<Directions, float> cameraEdges = actor.MapLayerBehavior.GetCameraEdges();
			float targetDistance = _properties.Distance.BuildValue(actor);
			float distance;

			if (_properties.Direction == Directions.Up || _properties.Direction == Directions.Down) {
				distance = Math.Abs(cameraEdges[_properties.Direction] - actor.Transform.localPosition.y);
			} else {
				distance = Math.Abs(cameraEdges[_properties.Direction] - actor.Transform.localPosition.x);
			}

			return BoomerangUtils.CompareValue(_properties.Comparison, targetDistance, distance);
		}
	}
}