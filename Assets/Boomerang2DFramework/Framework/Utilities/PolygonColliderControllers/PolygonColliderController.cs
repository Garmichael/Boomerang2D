using UnityEngine;

namespace Boomerang2DFramework.Framework.Utilities.PolygonColliderControllers {
	public abstract class PolygonColliderController {
		public virtual Vector2[] GeneratePoints() {
			return new Vector2[0];
		}
	}
}