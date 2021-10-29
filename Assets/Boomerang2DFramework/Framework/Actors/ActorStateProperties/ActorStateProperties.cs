using Boomerang2DFramework.Framework.StateManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorStateProperties {
	[System.Serializable]
	public class ActorStateProperties : StateProperties {
		/// <summary>
		/// Allows this Actor to be Grounded through the IsGrounded property
		/// </summary>
		public bool CanBeGrounded = true;

		/// <summary>
		/// Allows this Actor to collide with Solid tiles. 
		/// </summary>
		/// <remarks>
		/// When Rays are cast in GenerateRayData(), this property sets the DistanceToSolid values from Tiles
		/// </remarks>
		public bool CollidesWithGeometry = true;

		/// <summary>
		/// Allows this Actor to collide with Solid Bounding Boxes on other Actors
		/// </summary>
		/// <remarks>
		/// When Rays are cast in GenerateRayData(), this property sets the DistanceToSolid values from Actors
		/// </remarks>
		public bool CollidesWithActors = true;

		/// <summary>
		///  Allows this Actor to Overlap Geometry
		/// </summary>
		/// <remarks>
		/// When Overlaps are checked in GetOverlappingCollisions(), this property collects flags from Tiles
		/// </remarks>
		public bool OverlapsGeometry = true;

		/// <summary>
		///  Allows this Actor to Overlap other Actors
		/// </summary>
		/// <remarks>
		/// When Overlaps are checked in GetOverlappingCollisions(), this property collects flags from Actors
		/// </remarks>
		public bool OverlapsOtherActors = true;

		/// <summary>
		///  Allows this Actor to Overlap Weapons
		/// </summary>
		/// <remarks>
		/// When Overlaps are checked in GetOverlappingCollisions(), this property collects Weapon names
		/// </remarks>
		public bool OverlapsWeapons = true;
	}
}