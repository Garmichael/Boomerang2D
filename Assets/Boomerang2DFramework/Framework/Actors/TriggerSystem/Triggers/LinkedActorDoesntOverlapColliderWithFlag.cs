using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.MapGeneration;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class LinkedActorDoesntOverlapColliderWithFlag : ActorTrigger {
		[SerializeField] private LinkedActorDoesntOverlapColliderWithFlagProperties _properties;

		public LinkedActorDoesntOverlapColliderWithFlag(ActorTriggerProperties actorTriggerProperties) {
			_properties = (LinkedActorDoesntOverlapColliderWithFlagProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			List<Actor> otherActors = MapManager.GetActorsFromCatalogByMapId(
				actor.ActorInstanceProperties.LinkedActors[_properties.LinkedActorSlotId]
			);

			bool matchFound = false;

			foreach (Actor otherActor in otherActors) {
				if (!_properties.UseAnySelfFlag) {
					if (otherActor.OverlappingActorFlags.ContainsKey(_properties.SelfFlag)) {
						foreach (Actor.OverlappingCollider otherFlag in otherActor.OverlappingActorFlags[_properties.SelfFlag]) {
							if (otherFlag.Flag == _properties.OtherFlag) {
								matchFound = true;
								break;
							}
						}
					}
				} else {
					foreach (KeyValuePair<string, List<Actor.OverlappingCollider>> selfFlag in otherActor.OverlappingActorFlags) {
						foreach (Actor.OverlappingCollider otherFlag in selfFlag.Value) {
							if (otherFlag.Flag == _properties.OtherFlag) {
								matchFound = true;
								break;
							}
						}

						if (matchFound) {
							break;
						}
					}
				}
			}

			return !matchFound;
		}
	}
}