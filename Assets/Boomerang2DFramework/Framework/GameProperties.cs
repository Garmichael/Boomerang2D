using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework {
	public static class GameProperties {
		// Change these to suit your Game Project
		public const float PixelsPerUnit = 16; // Should be a power of 2
		public const float AspectWidth = 16f; // Aspect Ratio Width
		public const float AspectHeight = 9f; // Aspect Ration Height
		public const float UnitsWide = 20f; // How many tiles fit on the screen horizontally

		// Viewport Settings
		// Should not be altered
		public const float AspectRatio = AspectWidth / AspectHeight;
		public const float DefaultCameraSize = (float) ((double) AspectHeight / AspectWidth * (UnitsWide / 2));
		public const int RenderDimensionsWidth = (int) (PixelsPerUnit * UnitsWide);
		public const int RenderDimensionsHeight = (int) (RenderDimensionsWidth / AspectRatio);
		public const float UnitsHigh = UnitsWide / AspectRatio;
		public static float PixelSize => BoomerangUtils.RoundToPixelPerfection(1 / PixelsPerUnit);
		public const int MapLayerDepth = 10;
		public const RenderDisplayMode RenderDisplayMode = Framework.RenderDisplayMode.PixelPerfect;

		// Folder and Namespace References
		// Should not be altered
		public const string InitializingGameObjectName = "Boomerang2D";

		public const string ActorContentDirectory = "Assets/Boomerang2DFramework/Content/Actors";
		public const string AudioContentDirectory = "Assets/Boomerang2DFramework/Content/Audio";
		public const string TextureBuiltInDirectory = "Assets/Boomerang2DFramework/Framework/Textures";
		public const string TextureCustomDirectory = "Assets/Boomerang2DFramework/Content/Textures";
		public const string WeaponContentDirectory = "Assets/Boomerang2DFramework/Content/Weapons";
		public const string MapContentDirectory = "Assets/Boomerang2DFramework/Content/Maps";
		public const string TilesetContentDirectory = "Assets/Boomerang2DFramework/Content/Tilesets";
		public const string TileCollidersBuiltInDirectory = "Assets/Boomerang2DFramework/Framework/MapGeneration/TileColliders";
		public const string TileCollidersCustomContentDirectory = "Assets/Boomerang2DFramework/Content/TileColliders";
		public const string SpriteShadersBuiltInDirectory = "Assets/Boomerang2DFramework/Framework/SpriteShaders";
		public const string SpriteShadersCustomDirectory = "Assets/Boomerang2DFramework/Content/SpriteShaders";
		public const string ParticleEffectContentDirectory = "Assets/Boomerang2DFramework/Content/ParticleEffects";
		public const string BitmapFontsContentDirectory = "Assets/Boomerang2DFramework/Content/BitmapFonts";
		public const string HudObjectsContentDirectory = "Assets/Boomerang2DFramework/Content/HudObjects";
		public const string MapPrefabsContentDirectory = "Assets/Boomerang2DFramework/Content/MapPrefabs";
		public const string DialogContentContentDirectory = "Assets/Boomerang2DFramework/Content/DialogContent";

		public const string GameFlagContentFile = "Assets/Boomerang2DFramework/Framework/GameFlagManagement/GameFlags.json";
		public const string DialogContentContentFile = "Assets/Boomerang2DFramework/Content/DialogContent/DialogContent.json";
		public const string AudioMixerAssetFile = "Assets/Boomerang2DFramework/Framework/AudioManagement/MasterMixer.mixer";

		public const string EngineActorStateClassNs = "Boomerang2DFramework.Framework.Actors.States";
		public const string EngineCameraStateClassNs = "Boomerang2DFramework.Framework.Camera.States";
		public const string EngineActorTriggerClassNs = "Boomerang2DFramework.Framework.Actors.TriggerSystem.Triggers";
		public const string EngineActorFilterClassNs = "Boomerang2DFramework.Framework.Actors.ActorFinderFilters.Filters";
		public const string EngineActorEventsClassNs = "Boomerang2DFramework.Framework.Actors.ActorEvents.Events";
		public const string EngineGameEventsClassNs = "Boomerang2DFramework.Framework.GameEvents.Events";
		public const string EngineWeaponStrikeClassNs = "Boomerang2DFramework.Framework.Actors.Weapons.MeleeStrikeTypes.MeleeStrikes";
		public const string EngineUiElementsPropertiesClassNs = "Boomerang2DFramework.Framework.UiManagement.UiElements.Properties";
		public const string EngineUiElementsBehaviorClassNs = "Boomerang2DFramework.Framework.UiManagement.UiElements.Behaviors";
		public const string EngineUiElementsEditorClassNs = "Boomerang2DFramework.Framework.UiManagement.UiElements.Editor";
		public const string EngineUiElementsTriggerClassNs = "Boomerang2DFramework.Framework.UiManagement.TriggerSystem.Triggers";

		public const string CustomActorStateClassNs = "Boomerang2DFramework.CustomScripts.ActorStates";
		public const string CustomCameraStateClassNs = "Boomerang2DFramework.CustomScripts.CameraStates";
		public const string CustomActorTriggerClassNs = "Boomerang2DFramework.CustomScripts.ActorTriggers";
		public const string CustomActorFilterClassNs = "Boomerang2DFramework.CustomScripts.ActorFinderFilters";
		public const string CustomActorEventsClassNs = "Boomerang2DFramework.CustomScripts.ActorEvents";
		public const string CustomGameEventsClassNs = "Boomerang2DFramework.CustomScripts.GameEvents";
		public const string CustomWeaponStrikeClassNs = "Boomerang2DFramework.CustomScripts.MeleeStrikeTypes";
		public const string CustomUiElementsPropertiesClassNs = "Boomerang2DFramework.CustomScripts.UiElements.Properties";
		public const string CustomUiElementsBehaviorClassNs = "Boomerang2DFramework.CustomScripts.UiElements.Behaviors";
		public const string CustomUiElementsEditorClassNs = "Boomerang2DFramework.CustomScripts.UiElements.Editor";
		public const string CustomHudObjectTriggerClassNs = "Boomerang2DFramework.CustomScripts.HudObjectTriggers";

		static GameProperties() {
			Debug.Log("Engine Properties Initialized. Render Dimensions are " + RenderDimensionsWidth + "x" + RenderDimensionsHeight);
		}
	}
}