using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class OverlapsColliderWithFlag : ActorTrigger {
		[SerializeField] private OverlapsColliderWithFlagProperties _properties;

		public OverlapsColliderWithFlag(ActorTriggerProperties actorTriggerProperties) {
			_properties = (OverlapsColliderWithFlagProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			bool matchFound = false;

			if (!_properties.UseAnySelfFlag) {
				if (actor.OverlappingActorFlags.ContainsKey(_properties.SelfFlag)) {
					if (!actor.OverlappingActorFlags.ContainsKey(_properties.SelfFlag)) {
						return false;
					}

					foreach (Actor.OverlappingCollider overlappingCollider in actor.OverlappingActorFlags[_properties.SelfFlag]) {
						if (overlappingCollider.Flag == _properties.OtherFlag) {
							matchFound = true;
							break;
						}
					}
				}
			} else {
				foreach (KeyValuePair<string, List<Actor.OverlappingCollider>> selfFlag in actor.OverlappingActorFlags) {
					foreach (Actor.OverlappingCollider overlappingCollider in selfFlag.Value) {
						if (overlappingCollider.Flag == _properties.OtherFlag) {
							matchFound = true;
							break;
						}
					}

					if (matchFound) {
						break;
					}
				}
			}

			return matchFound;
		}
	}
}