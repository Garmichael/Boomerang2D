using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class OverlapsWeaponCollider : ActorTrigger {
		[SerializeField] private OverlapsWeaponColliderProperties _properties;

		public OverlapsWeaponCollider(ActorTriggerProperties actorTriggerProperties) {
			_properties = (OverlapsWeaponColliderProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			bool matchFound = false;

			if (!_properties.UseAnySelfFlag) {
				if (actor.OverlappingWeapons.ContainsKey(_properties.SelfFlag)) {
					if (!actor.OverlappingWeapons.ContainsKey(_properties.SelfFlag)) {
						return false;
					}

					foreach (string weaponName in actor.OverlappingWeapons[_properties.SelfFlag]) {
						if (_properties.WeaponNames.IndexOf(weaponName) > -1) {
							matchFound = true;
							break;
						}
					}
				}
			} else {
				foreach (KeyValuePair<string, List<string>> selfFlag in actor.OverlappingWeapons) {
					foreach (string weaponName in selfFlag.Value) {
						if (_properties.WeaponNames.IndexOf(weaponName) > -1) {
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