using Boomerang2DFramework.Framework.AudioManagement;
using Boomerang2DFramework.Framework.Camera;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.MapGeneration {
	/// <summary>
	/// The MonoBehavior attached to MapViews
	/// </summary>
	public class MapViewBehavior : MonoBehaviour {
		private MapViewProperties _properties;
		public float TopExtent;
		public float RightExtent;
		public float BottomExtent;
		public float LeftExtent;

		public CameraState DefaultCameraState;

		public void SetProperties(MapViewProperties mapViewProperties) {
			_properties = mapViewProperties;
		}

		private BoxCollider _boxCollider;

		private void Start() {
			_boxCollider = gameObject.GetComponent<BoxCollider>();
			_boxCollider.isTrigger = true;

			Rigidbody rigidBodyComponent = gameObject.AddComponent<Rigidbody>();
			rigidBodyComponent.isKinematic = true;
			Destroy(gameObject.GetComponent<MeshRenderer>());

			Transform gameObjectTransform = gameObject.transform;
			Vector3 position = gameObjectTransform.localPosition;
			Vector3 scale = gameObjectTransform.localScale;

			TopExtent = BoomerangUtils.RoundToPixelPerfection(position.y + scale.y / 2f);
			RightExtent = BoomerangUtils.RoundToPixelPerfection(position.x + scale.x / 2f);
			BottomExtent = BoomerangUtils.RoundToPixelPerfection(position.y - scale.y / 2f);
			LeftExtent = BoomerangUtils.RoundToPixelPerfection(position.x - scale.x / 2f);
		}

		private void Update() {
			bool shouldSetToActiveView = Boomerang2D.Player != null &&
			                             Boomerang2D.MainCameraController.CurrentView != this &&
			                             PointInsideView(Boomerang2D.Player.Transform.localPosition);

			if (shouldSetToActiveView) {
				SetToActiveView();
			}
		}
		
		/// <summary>
		/// Determines if a point is inside this View
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		private bool PointInsideView(Vector3 point) {
			Bounds bounds = _boxCollider.bounds;
			
			return point.x > bounds.min.x &&
			       point.x <= bounds.max.x &&
			       point.y > bounds.min.y &&
			       point.y <= bounds.max.y;
		}
		
		/// <summary>
		/// Set the current Active view to this View
		/// </summary>
		private void SetToActiveView() {
			Boomerang2D.MainCameraController.SetCurrentView(this);

			if (_properties.PlaysBackgroundMusic) {
				float crossFadeDuration = _properties.CrossFadeBackgroundMusic
					? _properties.CrossFadeDuration
					: 0;

				AudioManager.CrossFadeBackgroundMusicTo(_properties.BackgroundMusic, crossFadeDuration);
			} else {
				float crossFadeDuration = _properties.CrossFadeBackgroundMusic
					? _properties.CrossFadeDuration
					: 0;

				AudioManager.FadeOutBackgroundMusic(crossFadeDuration);
			}
		}

		/// <summary>
		/// Get the Percentage across the horizontal space of this MapView that is the  Main Camera's current position
		/// </summary>
		/// <returns></returns>
		public float GetXPositionPercentage() {
			float width = RightExtent - LeftExtent;
			float actorX = Boomerang2D.MainCameraController.transform.localPosition.x - LeftExtent;
			float percent = actorX / width;

			return BoomerangUtils.RoundToPixelPerfection(percent);
		}

		/// <summary>
		/// Get the Percentage across the Vertical space of this MapView that is the  Main Camera's current position
		/// </summary>
		/// <returns></returns>
		public float GetYPositionPercentage() {
			float height = TopExtent - BottomExtent;
			float actorY = Boomerang2D.MainCameraController.transform.localPosition.y - BottomExtent;
			float percent = actorY / height;

			return BoomerangUtils.RoundToPixelPerfection(percent);
		}
	}
}