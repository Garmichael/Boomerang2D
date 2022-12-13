using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;

namespace Boomerang2DFramework.Framework.Actors.ActorStateProperties.AnimationProperties {
	/// <summary>
	/// JSON definition for a single Frame of a State's Animation 
	/// </summary>
	[System.Serializable]
	public class StatePropertiesAnimationFrame {
		public int SpriteFrame;
		public float Duration;
		public bool FlipHorizontal;
		public bool FlipVertical;
		public bool Rotate;
		public List<BoundingBoxProperties> BoundingBoxProperties = new List<BoundingBoxProperties>();
		public List<ActorParticleEffectProperties> ParticleEffectProperties = new List<ActorParticleEffectProperties>();
	}
}