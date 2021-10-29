using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SnapPositionX : ActorEvent {
		private SnapPositionXProperties MyProperties => (SnapPositionXProperties) Properties;

		public SnapPositionX(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null || sourceActor?.ActorProperties == null) {
				return;
			}
			
			float snapValue = MyProperties.SnapValue.BuildValue(sourceActor);
			float newX = BoomerangUtils.RoundToPixelPerfection(
				snapValue * Mathf.Round(targetActor.RealPosition.x / snapValue)
			);

			targetActor.SetRealPositionX(newX);
		}
	}
}