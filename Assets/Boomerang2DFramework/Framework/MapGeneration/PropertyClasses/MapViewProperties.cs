using UnityEngine;

namespace Boomerang2DFramework.Framework.MapGeneration.PropertyClasses {
	/// <summary>
	/// JSON definition for a placed Map View
	/// </summary>
	[System.Serializable]
	public class MapViewProperties {
		public Vector2Int Position;
		public Vector2Int Dimensions;

		public string CameraBehaviorClass;
		public string CameraBehaviorPropertiesClass;
		public string CameraBehaviorProperties;
		public string CameraTransitionMode;

		public bool PlaysBackgroundMusic;
		public string BackgroundMusic = "";
		public bool CrossFadeBackgroundMusic;
		public float CrossFadeDuration = 0.25f;
	}
}