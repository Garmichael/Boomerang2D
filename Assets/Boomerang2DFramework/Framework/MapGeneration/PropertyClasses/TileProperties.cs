using System.Collections.Generic;
using UnityEngine;

namespace Boomerang2DFramework.Framework.MapGeneration.PropertyClasses {
	/// <summary>
	/// JSON definition for a Tile
	/// </summary>
	[System.Serializable]
	public class TileProperties {
		public int Slot;
		public bool UsesCollider = true;
		public bool SolidOnTop = true;
		public bool SolidOnBottom = true;
		public bool SolidOnLeft = true;
		public bool SolidOnRight = true;
		public string CollisionShape = "Square";
		public bool CollisionFlippedX;
		public bool CollisionFlippedY;
		public float CollisionRotation;
		public Vector2 CollisionOffset = Vector2.zero;
		public List<string> Flags = new List<string>();
		public List<int> AnimationFrames = new List<int>();
		public List<float> AnimationFramesSpeeds = new List<float>();
	}
}