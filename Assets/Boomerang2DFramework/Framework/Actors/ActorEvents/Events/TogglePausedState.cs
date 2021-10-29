using Boomerang2DFramework.Framework.MapGeneration;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class TogglePausedState : ActorEvent {
		private TogglePausedStateProperties MyProperties => (TogglePausedStateProperties) Properties;

		public TogglePausedState(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null || sourceActor?.ActorProperties == null) {
				return;
			}
			
			if (MyProperties.AffectAllActors) {
				Actor[] actors = MapManager.GetActorCatalog().ToArray();
				foreach (Actor actor in actors) {
					actor.SetPaused(!actor.IsPaused);
				}
			} else {
				sourceActor.SetPaused(!targetActor.IsPaused);
			}
		}
	}
}