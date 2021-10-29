using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.StateManagement;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers {
	[Serializable]
	public class IsOffScreen : ActorTrigger {
		public IsOffScreen(ActorTriggerProperties actorTriggerProperties) { }

		public override bool IsTriggered(Actor actor, State state) {
			Dictionary<Directions, float> cameraEdges = actor.MapLayerBehavior.GetCameraEdges();
			float halfSpriteWidth = actor.SpriteRenderer.size.x / 2;
			float halfSpriteHeight = actor.SpriteRenderer.size.y / 2;

			return
				!(actor.Transform.localPosition.x > cameraEdges[Directions.Left] + halfSpriteWidth &&
				  actor.Transform.localPosition.x < cameraEdges[Directions.Right] - halfSpriteWidth &&
				  actor.Transform.localPosition.y > cameraEdges[Directions.Down] + halfSpriteHeight &&
				  actor.Transform.localPosition.y < cameraEdges[Directions.Up] - halfSpriteHeight);
		}
	}
}