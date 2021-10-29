using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Camera {
	/// <summary>
	/// Base State for Camera States
	/// </summary>
	public class CameraState : State {
		public override void OnEnterState() {
			base.OnEnterState();

			if (StateProperties != null) {
				Boomerang2D.MainCameraController.HorizontalLerpSpeed = ((CameraStateProperties) StateProperties).HorizontalLerpSpeed;
				Boomerang2D.MainCameraController.VerticalLerpSpeed = ((CameraStateProperties) StateProperties).VerticalLerpSpeed;
			}
		}
		
		/// <summary>
		/// Set to lock the Camera within the boundaries of the CurrentView
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		protected Vector3 BoundToExtentsOfView(Vector3 position) {
			float verticalExtent = Boomerang2D.MainCameraController.OrthographicSize;
			float horizontalExtent = verticalExtent * Boomerang2D.MainCameraController.Aspect;

			if (position.x - horizontalExtent < Boomerang2D.MainCameraController.CurrentView.LeftExtent) {
				position.x = Boomerang2D.MainCameraController.CurrentView.LeftExtent + horizontalExtent;
			}

			if (position.x + horizontalExtent > Boomerang2D.MainCameraController.CurrentView.RightExtent) {
				position.x = Boomerang2D.MainCameraController.CurrentView.RightExtent - horizontalExtent;
			}

			if (position.y - verticalExtent < Boomerang2D.MainCameraController.CurrentView.BottomExtent) {
				position.y = Boomerang2D.MainCameraController.CurrentView.BottomExtent + verticalExtent;
			}

			if (position.y + verticalExtent > Boomerang2D.MainCameraController.CurrentView.TopExtent) {
				position.y = Boomerang2D.MainCameraController.CurrentView.TopExtent - verticalExtent;
			}

			return position;
		}
	}
}