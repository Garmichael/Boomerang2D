using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class ChangeVelocity : ActorEvent {
		private ChangeVelocityProperties MyProperties => (ChangeVelocityProperties) Properties;

		public ChangeVelocity(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null || sourceActor?.ActorProperties == null) {
				return;
			}
			
			float xVelocity = MyProperties.VelocityX.BuildValue(sourceActor);
			float yVelocity = MyProperties.VelocityY.BuildValue(sourceActor);
			
			if (MyProperties.IsRelativeChange) {
				targetActor.OffsetRealPosition(xVelocity, yVelocity);
			} else {
				targetActor.SetRealPosition(xVelocity, yVelocity);
			}
		}
	}
}