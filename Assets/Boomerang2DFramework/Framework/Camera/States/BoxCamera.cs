using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Camera.States {
	public class BoxCamera : CameraState {
		private BoxCameraProperties MyStateProperties => (BoxCameraProperties) StateProperties;

		public BoxCamera(StateProperties stateProperties) {
			MyStateMachine = Boomerang2D.MainCameraController.StateMachine;
			StateProperties = stateProperties;
		}

		public override void OnEnterState() {
			base.OnEnterState();
			Vector3 destinationPosition = Boomerang2D.MainCameraController.Target.RealPosition;
			destinationPosition = BoundToExtentsOfView(destinationPosition);
			Boomerang2D.MainCameraController.RealPosition = destinationPosition;
		}

		public override void ProcessState() {
			base.ProcessState();

			if (Boomerang2D.MainCameraController.CurrentView == null) {
				return;
			}

			Vector3 destinationPosition = Boomerang2D.MainCameraController.Target.RealPosition;

			float horizontalDistanceFromCenter = destinationPosition.x - Boomerang2D.MainCameraController.RealPosition.x;
			bool isPastRightPusher = horizontalDistanceFromCenter > MyStateProperties.RightPushEdge;
			bool isPastLeftPusher = horizontalDistanceFromCenter < -MyStateProperties.LeftPushEdge;

			if (!isPastRightPusher && !isPastLeftPusher) {
				destinationPosition.x = Boomerang2D.MainCameraController.RealPosition.x;
			} else if (isPastRightPusher) {
				destinationPosition.x = Boomerang2D.MainCameraController.Target.RealPosition.x - MyStateProperties.RightPushEdge;
			} else {
				destinationPosition.x = Boomerang2D.MainCameraController.Target.RealPosition.x + MyStateProperties.LeftPushEdge;
			}

			float verticalDistanceFromCenter = destinationPosition.y - Boomerang2D.MainCameraController.RealPosition.y;
			bool isPastTopPusher = verticalDistanceFromCenter > MyStateProperties.TopPushEdge;
			bool isPastBottomPusher = verticalDistanceFromCenter < -MyStateProperties.BottomPushEdge;

			if (!isPastTopPusher && !isPastBottomPusher) {
				destinationPosition.y = Boomerang2D.MainCameraController.RealPosition.y;
			} else if (isPastTopPusher) {
				destinationPosition.y = Boomerang2D.MainCameraController.Target.RealPosition.y - MyStateProperties.TopPushEdge;
			} else {
				destinationPosition.y = Boomerang2D.MainCameraController.Target.RealPosition.y + MyStateProperties.BottomPushEdge;
			}

			if (MyStateProperties.SticksToBottom) {
				float distanceFromBottomOfView = destinationPosition.y - Boomerang2D.MainCameraController.CurrentView.BottomExtent;

				if (distanceFromBottomOfView < MyStateProperties.StickinessAmount) {
					float verticalExtent = Boomerang2D.MainCameraController.OrthographicSize;

					destinationPosition.y = Boomerang2D.MainCameraController.CurrentView.BottomExtent + verticalExtent;
				}
			}

			destinationPosition = BoundToExtentsOfView(destinationPosition);

			Boomerang2D.MainCameraController.DestinationPosition = destinationPosition;
		}
	}
}