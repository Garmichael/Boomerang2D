using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters {
	public class IsOnScreen : ActorFinderFilter {
		public IsOnScreen(ActorFinderFilterProperties properties) {
			Properties = properties;
		}

		public override List<Actor> FetchMatchingActors(Actor originActor, List<Actor> actorCatalog) {
			List<Actor> matchingActors = new List<Actor>();

			foreach (Actor actor in actorCatalog) {
				Dictionary<Directions, float> cameraEdges = actor.MapLayerBehavior.GetCameraEdges();
				float halfSpriteWidth = actor.SpriteRenderer.size.x / 2;
				float halfSpriteHeight = actor.SpriteRenderer.size.y / 2;

				if (actor.Transform.localPosition.x > cameraEdges[Directions.Left] + halfSpriteWidth &&
				    actor.Transform.localPosition.x < cameraEdges[Directions.Right] - halfSpriteWidth &&
				    actor.Transform.localPosition.y > cameraEdges[Directions.Down] + halfSpriteHeight &&
				    actor.Transform.localPosition.y < cameraEdges[Directions.Up] - halfSpriteHeight) {
					matchingActors.Add(actor);
				}
			}

			return matchingActors;
		}
	}
}