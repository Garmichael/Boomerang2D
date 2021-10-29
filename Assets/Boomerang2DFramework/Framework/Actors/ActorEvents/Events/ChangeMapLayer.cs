using Boomerang2DFramework.Framework.MapGeneration;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class ChangeMapLayer : ActorEvent {
		private ChangeMapLayerProperties MyProperties => (ChangeMapLayerProperties) Properties;

		public ChangeMapLayer(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			MapManager.MoveActorToMapLayer(targetActor, MyProperties.NewMapLayerName);
		}
	}
}