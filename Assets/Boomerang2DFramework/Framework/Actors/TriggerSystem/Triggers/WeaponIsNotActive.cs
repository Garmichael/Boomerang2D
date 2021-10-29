using System;
using Boomerang2DFramework.Framework.Actors.Weapons;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class WeaponIsNotActive : ActorTrigger {
		[SerializeField] private WeaponIsNotActiveProperties _properties;

		public WeaponIsNotActive(ActorTriggerProperties actorTriggerProperties) {
			_properties = (WeaponIsNotActiveProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			WeaponBehavior weapon = actor.GetWeaponBehavior(_properties.WeaponName);

			return !weapon.IsActing;
		}
	}
}