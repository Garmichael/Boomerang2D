using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class DebugLog : ActorEvent {
		private DebugLogProperties MyProperties => (DebugLogProperties) Properties;

		public DebugLog(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			Debug.Log(MyProperties.Message);
			
		}
	}
}