using Boomerang2DFramework.Framework.Actors;
using Boomerang2DFramework.Framework.MapGeneration;
using UnityEngine;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class SetPauseStateOnAllActors : GameEvent {
		private SetPauseStateOnAllActorsProperties MyProperties => (SetPauseStateOnAllActorsProperties) Properties;

		public SetPauseStateOnAllActors(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			Actor[] actors = MapManager.GetActorCatalog().ToArray();
			foreach (Actor actor in actors) {
				actor.SetPaused(MyProperties.IsPaused);
			}
		}
	}
}