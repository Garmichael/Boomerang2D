using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.MapGeneration;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class LinkedActorInState : ActorTrigger {
		[SerializeField] private LinkedActorInStateProperties _properties;

		public LinkedActorInState(ActorTriggerProperties actorTriggerProperties) {
			_properties = (LinkedActorInStateProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			if (_properties.LinkedActorSlotId >= actor.ActorInstanceProperties.LinkedActors.Count) {
				return false;
			}

			List<Actor> otherActors = MapManager.GetActorsFromCatalogByMapId(
				actor.ActorInstanceProperties.LinkedActors[_properties.LinkedActorSlotId]
			);

			bool matchFound = false;

			foreach (Actor otherActor in otherActors) {
				if (otherActor.StateMachine.GetCurrentState().Name == _properties.StateName) {
					matchFound = true;
					break;
				}
			}

			return matchFound;
		}
	}
}