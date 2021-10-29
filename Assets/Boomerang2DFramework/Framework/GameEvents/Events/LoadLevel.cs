using Boomerang2DFramework.Framework.MapGeneration;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class LoadLevel : GameEvent {
		private LoadLevelProperties MyProperties => (LoadLevelProperties) Properties;

		public LoadLevel(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			MapManager.LoadMap(MyProperties.LevelName);
		}
	}
}