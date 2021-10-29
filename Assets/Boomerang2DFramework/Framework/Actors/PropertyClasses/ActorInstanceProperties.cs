using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.PropertyClasses {
	/// <summary>
	/// JSON object for setting the Actor Instance Properties 
	/// </summary>
	/// <remarks>
	/// Instance Properties are the ones set on the Actor inside the Map Editor.
	/// These are properties that can potentially change often through an Actor's lifespan, but can be set to
	/// specific value on initialization.
	/// </remarks>
	[System.Serializable]
	public class ActorInstanceProperties {
		public Directions FacingDirection = Directions.Right;
		public List<string> LinkedActors = new List<string>();
	}
}