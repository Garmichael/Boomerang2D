using System.Collections.Generic;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Utilities.PolygonColliderControllers {
	public class BetterBoxColliderController : PolygonColliderController {
		private float _width = 0.5f;
		private float _height = 0.5f;
		private float _rotation;
		private float _originX;
		private float _originY;
		private float _offsetX;
		private float _offsetY;

		public void SetProperties(
			PolygonCollider2D collider2D,
			float width,
			float height,
			float rotation,
			float originX,
			float originY,
			float offsetX,
			float offsetY
		) {
			originX = BoomerangUtils.ClampValue(originX, 0, 100);
			originY = BoomerangUtils.ClampValue(originY, 0, 100);

			_width = width;
			_height = height;
			_rotation = 90 - rotation;
			_originX = originX;
			_originY = originY;
			_offsetX = offsetX;
			_offsetY = offsetY;

			if (collider2D) {
				collider2D.points = GeneratePoints();
			}
		}

		public override Vector2[] GeneratePoints() {
			List<Vector2> points = new List<Vector2>();

			Vector2 topLeftCorner = new Vector2(-(_width * _originX / 100), _height * (100 - _originY) / 100) + new Vector2(_offsetX, _offsetY);
			Vector2 topRightCorner = new Vector2(_width * (100 - _originX) / 100, _height * (100 - _originY) / 100) + new Vector2(_offsetX, _offsetY);
			Vector2 bottomRightCorner = new Vector2(_width * (100 - _originX) / 100, -(_height * _originY / 100)) + new Vector2(_offsetX, _offsetY);
			Vector2 bottomLeftCorner = new Vector2(-(_width * _originX / 100), -(_height * _originY / 100)) + new Vector2(_offsetX, _offsetY);

			topLeftCorner = RotateAroundOrigin(topLeftCorner);
			topRightCorner = RotateAroundOrigin(topRightCorner);
			bottomRightCorner = RotateAroundOrigin(bottomRightCorner);
			bottomLeftCorner = RotateAroundOrigin(bottomLeftCorner);

			points.Add(topLeftCorner);
			points.Add(topRightCorner);
			points.Add(bottomRightCorner);
			points.Add(bottomLeftCorner);

			return points.ToArray();
		}

		private Vector2 RotateAroundOrigin(Vector2 point) {
			return new Vector2(
				point.x * Mathf.Cos(_rotation * Mathf.Deg2Rad) + point.y * Mathf.Sin(_rotation * Mathf.Deg2Rad),
				point.y * Mathf.Cos(_rotation * Mathf.Deg2Rad) - point.x * Mathf.Sin(_rotation * Mathf.Deg2Rad)
			);
		}
	}
}