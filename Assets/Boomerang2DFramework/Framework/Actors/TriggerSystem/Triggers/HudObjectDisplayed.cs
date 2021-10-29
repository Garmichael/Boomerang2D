using System;
using Boomerang2DFramework.Framework.StateManagement;
using Boomerang2DFramework.Framework.UiManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class HudObjectDisplayed : ActorTrigger {
		[SerializeField] private HudObjectDisplayedProperties _properties;

		public HudObjectDisplayed(ActorTriggerProperties actorTriggerProperties) {
			_properties = (HudObjectDisplayedProperties) actorTriggerProperties;
		}

		public override bool IsTriggered(Actor actor, State state) {
			bool isOpen = UiManager.HudObjectIsOpen(_properties.HudObjectName);

			if (_properties.IsOpened) {
				return isOpen;
			}

			return !isOpen;
		}
	}
}