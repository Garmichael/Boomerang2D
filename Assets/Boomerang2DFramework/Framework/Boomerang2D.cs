using Boomerang2DFramework.Framework.Actors;
using Boomerang2DFramework.Framework.Camera;
using Boomerang2DFramework.Framework.GlobalTimeManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework {
	public static class Boomerang2D {
		public static CameraController MainCameraController { get; private set; }
		public static ActorManager ActorManager { get; private set; }

		public static Actor Player { get; set; }

		/// <summary>
		/// Initializes the Boomerang 2D Framework. This must be called in some sort of a StartGame script before
		/// doing anything else with the Framework
		/// </summary>
		public static void InitializeFramework() {
			Application.targetFrameRate = 60;
			QualitySettings.vSyncCount = 1;

			BuildCamera();
			AttachRequiredBehaviors();
		}

		/// <summary>
		/// Builds the Main Camera Controller for the Game
		/// </summary>
		private static void BuildCamera() {
			GameObject mainCamera = new GameObject("MainCamera");
			mainCamera.AddComponent<AudioListener>();
			MainCameraController = mainCamera.AddComponent<CameraController>();
		}

		/// <summary>
		/// Attaches the required behavior classes for the Framework to function properly
		/// </summary>
		private static void AttachRequiredBehaviors() {
			GameObject frameworkManagerGameObject = GameObject.Find(GameProperties.InitializingGameObjectName);
			frameworkManagerGameObject.AddComponent<GlobalTimeManager>();
			ActorManager = frameworkManagerGameObject.AddComponent<ActorManager>();
			ActorManager.Initialize();
		}
	}
}