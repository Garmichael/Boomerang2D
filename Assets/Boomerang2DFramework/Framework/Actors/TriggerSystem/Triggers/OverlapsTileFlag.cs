using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class OverlapsTileFlag : ActorTrigger {
		[SerializeField] private OverlapsTileFlagProperties _properties;

		public OverlapsTileFlag(ActorTriggerProperties actorTriggerProperties) {
			_properties = (OverlapsTileFlagProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			bool matchFound = false;
			
			if (!_properties.UseAnySelfFlag) {
				if (actor.OverlappingTileFlags.ContainsKey(_properties.SelfFlag)) {
					if (!actor.OverlappingTileFlags.ContainsKey(_properties.SelfFlag)) {
						return false;
					}

					foreach (string overlappingCollider in actor.OverlappingTileFlags[_properties.SelfFlag]) {
						if (overlappingCollider == _properties.OtherFlag) {
							matchFound = true;
							break;
						}
					}
				}
			} else {
				foreach (KeyValuePair<string, List<string>> selfFlag in actor.OverlappingTileFlags) {
					foreach (string overlappingCollider in selfFlag.Value) {
						if (overlappingCollider == _properties.OtherFlag) {
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