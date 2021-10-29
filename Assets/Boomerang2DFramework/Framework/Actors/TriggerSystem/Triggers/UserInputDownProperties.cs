using System;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class UserInputDownProperties : ActorTriggerProperties {
		public InputType InputType;
		public string InputValue;
	}
}