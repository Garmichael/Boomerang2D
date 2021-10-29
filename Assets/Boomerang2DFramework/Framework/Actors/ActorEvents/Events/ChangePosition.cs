using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class ChangePosition : ActorEvent {
		private ChangePositionProperties MyProperties => (ChangePositionProperties) Properties;

		public ChangePosition(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null || sourceActor?.ActorProperties == null) {
				return;
			}
			
			float x;
			float y;

			if (MyProperties.UseThisVelocity) {
				x = MyProperties.UpdateX ? sourceActor.Velocity.x : 0;
				y = MyProperties.UpdateY ? sourceActor.Velocity.y : 0;

				Vector3 offset = new Vector3(x, y, 0) * GameProperties.PixelSize;
				targetActor.OffsetRealPosition(offset);
				targetActor.SnapToPositioning();
				return;
			}

			x = MyProperties.UpdateX
				? MyProperties.OffsetX.BuildValue(sourceActor)
				: MyProperties.IsRelativeChange
					? 0
					: targetActor.RealPosition.x;

			y = MyProperties.UpdateY
				? MyProperties.OffsetY.BuildValue(sourceActor)
				: MyProperties.IsRelativeChange
					? 0
					: targetActor.RealPosition.y;

			if (MyProperties.IsRelativeChange) {
				targetActor.OffsetRealPosition(x, y);
			} else {
				targetActor.SetRealPosition(x, y);
			}
		}
	}
}