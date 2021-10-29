using Boomerang2DFramework.Framework.Actors;
using Boomerang2DFramework.Framework.MapGeneration;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class TogglePausedStateOnAllActors : GameEvent {
		public TogglePausedStateOnAllActors(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			Actor[] actors = MapManager.GetActorCatalog().ToArray();
			foreach (Actor actor in actors) {
				actor.SetPaused(!actor.IsPaused);
			}
		}
	}
}