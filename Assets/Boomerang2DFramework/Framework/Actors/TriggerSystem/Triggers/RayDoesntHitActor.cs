using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class RayDoesntHitActor : ActorTrigger {
		[SerializeField] private RayDoesntHitActorProperties _properties;

		public RayDoesntHitActor(ActorTriggerProperties actorTriggerProperties) {
			_properties = (RayDoesntHitActorProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			bool meetsCriteria = false;

			List<Actor.ActorBoundingBoxes> rayHitsCollectionToUse;

			switch (_properties.Direction) {
				case Directions.Up:
					rayHitsCollectionToUse = actor.ActorBoundingBoxesHitByRaysUp;
					break;
				case Directions.Right:
					rayHitsCollectionToUse = actor.ActorBoundingBoxesHitByRaysRight;
					break;
				case Directions.Down:
					rayHitsCollectionToUse = actor.ActorBoundingBoxesHitByRaysDown;
					break;
				default:
					rayHitsCollectionToUse = actor.ActorBoundingBoxesHitByRaysLeft;
					break;
			}

			foreach (Actor.ActorBoundingBoxes boundingBox in rayHitsCollectionToUse) {
				if (boundingBox.BoundingBoxProperties.Flags.IndexOf(_properties.Flag) > -1) {
					meetsCriteria = true;
				}
			}

			return !meetsCriteria;
		}
	}
}