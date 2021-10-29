using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class DoesntOverlapColliderWithFlag : ActorTrigger {
		[SerializeField] private DoesntOverlapColliderWithFlagProperties _properties;

		public DoesntOverlapColliderWithFlag(ActorTriggerProperties actorTriggerProperties) {
			_properties = (DoesntOverlapColliderWithFlagProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			bool matchFound = false;

			if (!_properties.UseAnySelfFlag) {
				if (actor.OverlappingActorFlags.ContainsKey(_properties.SelfFlag)) {
					foreach (Actor.OverlappingCollider otherFlag in actor.OverlappingActorFlags[_properties.SelfFlag]) {
						if (otherFlag.Flag == _properties.OtherFlag) {
							matchFound = true;
							break;
						}
					}
				}
			} else {
				foreach (KeyValuePair<string, List<Actor.OverlappingCollider>> selfFlag in actor.OverlappingActorFlags) {
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

			return !matchFound;
		}
	}
}