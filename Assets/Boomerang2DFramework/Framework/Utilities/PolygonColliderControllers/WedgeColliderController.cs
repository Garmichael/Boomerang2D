using System.Collections.Generic;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Utilities.PolygonColliderControllers {
	public class WedgeColliderController : PolygonColliderController {
		private int _chunkiness = 20;
		private float _radius = 0.5f;
		private float _arcWidth = 45;
		private float _rotation;
		private float _offsetX;
		private float _offsetY;

		public void SetProperties(PolygonCollider2D collider2D, int chunkiness, float radius, float arcWidth, float rotation, float offsetX, float offsetY) {
			chunkiness = BoomerangUtils.ClampValue(chunkiness, 5, 120);
			arcWidth = BoomerangUtils.ClampValue(arcWidth, 0, 360);

			if (radius <= 0.01f) {
				radius = 0.01f;
			}

			_chunkiness = chunkiness;
			_radius = radius;
			_arcWidth = arcWidth;
			_rotation = rotation;
			_offsetX = offsetX;
			_offsetY = offsetY;

			if (collider2D) {
				collider2D.points = GeneratePoints();
			}
		}

		public override Vector2[] GeneratePoints() {
			List<Vector2> points = new List<Vector2>();

			float pointAngle = 90 - _rotation - _arcWidth / 2;

			if ((int) (_arcWidth % 360) != 0) {
				points.Add(new Vector3(0, 0, 0));
			}

			int nodes = (int) Mathf.Ceil(_arcWidth / _chunkiness);

			for (int i = 0; i <= nodes; i++) {
				float x = _radius * Mathf.Cos(pointAngle * Mathf.Deg2Rad) + _offsetX;
				float y = _radius * Mathf.Sin(pointAngle * Mathf.Deg2Rad) + _offsetY;

				points.Add(new Vector2(x, y));
				pointAngle += _arcWidth / nodes;
			}

			return points.ToArray();
		}
	}
}