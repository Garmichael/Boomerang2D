using Boomerang2DFramework.Framework.MapGeneration;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SetPausedState : ActorEvent {
		private SetPausedStateProperties MyProperties => (SetPausedStateProperties) Properties;

		public SetPausedState(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			if (MyProperties.AffectAllActors) {
				Actor[] actors = MapManager.GetActorCatalog().ToArray();
				foreach (Actor actor in actors) {
					actor.SetPaused(MyProperties.IsPaused);
				}
			} else {
				targetActor.SetPaused(MyProperties.IsPaused);
			}
		}
	}
}