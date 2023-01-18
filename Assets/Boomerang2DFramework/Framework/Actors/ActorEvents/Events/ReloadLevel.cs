using Boomerang2DFramework.Framework.MapGeneration;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class ReloadLevel : ActorEvent {
		public ReloadLevel(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			MapManager.LoadMap(MapManager.ActiveMapProperties.Name);
		}
	}
}