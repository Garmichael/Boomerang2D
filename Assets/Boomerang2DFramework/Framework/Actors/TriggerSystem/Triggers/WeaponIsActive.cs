using System;
using Boomerang2DFramework.Framework.Actors.Weapons;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class WeaponIsActive : ActorTrigger {
		[SerializeField] private WeaponIsActiveProperties _properties;

		public WeaponIsActive(ActorTriggerProperties actorTriggerProperties) {
			_properties = (WeaponIsActiveProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			WeaponBehavior weapon = actor.GetWeaponBehavior(_properties.WeaponName);

			return weapon.IsActing;
		}
	}
}