using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.PropertyClasses {
	/// <summary>
	/// JSON definition for an Actor's Particle Effects
	/// </summary>
	[System.Serializable]
	public class ActorParticleEffectProperties {
		public string Name;
		public bool Enabled;
		public Vector2 DefaultOffsetPosition;
	}
}