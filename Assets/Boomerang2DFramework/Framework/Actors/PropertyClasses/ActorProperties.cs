using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.InteractionEvents;

namespace Boomerang2DFramework.Framework.Actors.PropertyClasses {
	/// <summary>
	/// JSON object for setting the Actor Properties 
	/// </summary>
	[System.Serializable]
	public class ActorProperties {
		public string Name;
		public bool IsPlayer;
		public float SpriteWidth = 16;
		public float SpriteHeight = 16;
		public bool ObeysChunking = true;

		public List<ActorStateProperties> States;
		public List<string> Weapons = new List<string>();
		public List<BoundingBoxProperties> BoundingBoxes = new List<BoundingBoxProperties>();
		public List<FloatStatProperties> StatsFloats = new List<FloatStatProperties>();
		public List<StringStatProperties> StatsStrings = new List<StringStatProperties>();
		public List<BoolStatProperties> StatsBools = new List<BoolStatProperties>();
		public List<ActorInteractionEvent> InteractionEvents = new List<ActorInteractionEvent>();
		public List<string> ChildrenActors = new List<string>();

		public List<ActorParticleEffectProperties> ParticleEffects = new List<ActorParticleEffectProperties>();
	}
}