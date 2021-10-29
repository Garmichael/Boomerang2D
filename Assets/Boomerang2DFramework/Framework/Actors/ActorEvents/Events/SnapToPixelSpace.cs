using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SnapToPixelSpace : ActorEvent {
		public SnapToPixelSpace(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			Vector3 offset = sourceActor.RealPosition - targetActor.RealPosition;
			offset = BoomerangUtils.RoundToPixelPerfection(offset);
			targetActor.SetRealPosition(sourceActor.RealPosition - offset);
		}
	}
}