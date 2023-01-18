using Boomerang2DFramework.Framework.MapGeneration;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class ReloadLevel : GameEvent {
		public ReloadLevel(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			MapManager.LoadMap(MapManager.ActiveMapProperties.Name);
		}
	}
}