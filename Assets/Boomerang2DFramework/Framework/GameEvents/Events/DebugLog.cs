using UnityEngine;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class DebugLog : GameEvent {
		private DebugLogProperties MyProperties => (DebugLogProperties) Properties;

		public DebugLog(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			Debug.Log(MyProperties.Log);
		}
	}
}