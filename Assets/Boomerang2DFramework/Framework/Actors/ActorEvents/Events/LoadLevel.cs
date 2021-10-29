using Boomerang2DFramework.Framework.MapGeneration;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class LoadLevel : ActorEvent {
		private LoadLevelProperties MyProperties => (LoadLevelProperties) Properties;

		public LoadLevel(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			MapManager.LoadMap(MyProperties.LevelName);
		}
	}
}