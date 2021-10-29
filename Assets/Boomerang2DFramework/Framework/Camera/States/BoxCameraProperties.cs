namespace Boomerang2DFramework.Framework.Camera.States {
	[System.Serializable]
	public class BoxCameraProperties : CameraStateProperties {
		public float RightPushEdge;
		public float LeftPushEdge;
		public float TopPushEdge;
		public float BottomPushEdge;
		public bool SticksToBottom;
		public float StickinessAmount;
	}
}