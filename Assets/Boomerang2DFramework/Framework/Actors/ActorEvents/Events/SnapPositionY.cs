using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SnapPositionY : ActorEvent {
		private SnapPositionYProperties MyProperties => (SnapPositionYProperties) Properties;

		public SnapPositionY(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null || sourceActor?.ActorProperties == null) {
				return;
			}
			
			float snapValue = MyProperties.SnapValue.BuildValue(sourceActor);
			float newY = BoomerangUtils.RoundToPixelPerfection(
				snapValue * Mathf.Round(targetActor.RealPosition.y / snapValue)
			);

			targetActor.SetRealPositionY(newY);
		}
	}
}