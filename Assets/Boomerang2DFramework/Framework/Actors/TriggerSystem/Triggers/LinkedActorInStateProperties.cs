using System;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class LinkedActorInStateProperties : ActorTriggerProperties {
		public int LinkedActorSlotId;
		public string StateName = "";
	}
}