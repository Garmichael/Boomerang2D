using System;
using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class OverlapsWeaponColliderProperties : ActorTriggerProperties {
		public bool UseAnySelfFlag;
		public string SelfFlag = "";
		public List<string> WeaponNames = new List<string>();
	}
}