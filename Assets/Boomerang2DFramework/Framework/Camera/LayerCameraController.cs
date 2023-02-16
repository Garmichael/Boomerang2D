using System.Collections.Generic;
using Boomerang2DFramework.Framework.MapGeneration;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Camera {
	/// <summary>
	/// The Camera Controller for Map and UI Layers
	/// </summary>
	/// <remarks>
	/// Each Map Layer and UI Layer has a local Camera assigned to it. The camera renders to a RenderTexture which
	/// is catalogued with the CameraController, which renders each one to the screen.
	/// </remarks>
	public class LayerCameraController : MonoBehaviour {
		public UnityEngine.Camera Camera;
		public RenderTexture RenderTexture;
		public Vector2 VisibleTileCount;
		public Material Material;
		
		public void RegisterMapLayerCamera(string mapName, MapLayerBehavior mapLayerBehavior) {
			BuildRenderMaterial();
			BuildRenderTexture();
			Boomerang2D.MainCameraController.AddMapLayerRender(mapName, mapLayerBehavior, this);
		}

		public void RegisterUiLayerCamera(string mapName) {
			BuildRenderMaterial();
			BuildRenderTexture();
			Boomerang2D.MainCameraController.AddUiLayerRender(mapName, this);
		}

		private void BuildRenderTexture() {
			RenderTexture = new RenderTexture(
				GameProperties.RenderDimensionsWidth,
				GameProperties.RenderDimensionsHeight,
				24,
				RenderTextureFormat.Default
			) { filterMode = FilterMode.Point };
			RenderTexture.Create();

			Camera = gameObject.AddComponent<UnityEngine.Camera>();
			Camera.clearFlags = CameraClearFlags.Color;
			Camera.backgroundColor = new Color(0, 0, 0, 0);
			Camera.cullingMask = -1;
			Camera.orthographic = true;
			Camera.nearClipPlane = 0f;
			Camera.farClipPlane = GameProperties.MapLayerDepth;
			Camera.depth = 1f;
			Camera.targetDisplay = 0;
			Camera.aspect = GameProperties.AspectRatio;
			Camera.orthographicSize = GameProperties.DefaultCameraSize;
			Camera.targetTexture = RenderTexture;

			VisibleTileCount = new Vector2(
				Camera.orthographicSize * 2f * Camera.aspect,
				Camera.orthographicSize * 2f
			);
		}

		public void SetShader(
			string shader,
			Dictionary<string, float> floats = null,
			Dictionary<string, int> ints = null,
			Dictionary<string, Color> colors = null,
			Dictionary<string, Texture> textures = null
		) {
			Material.shader = Shader.Find(shader);
			
			if (floats != null) {
				foreach (KeyValuePair<string, float> property in floats) {
					Material.SetFloat(property.Key, property.Value);
				}
			}

			if (ints != null) {
				foreach (KeyValuePair<string, int> property in ints) {
					Material.SetInt(property.Key, property.Value);
				}
			}

			if (colors != null) {
				foreach (KeyValuePair<string, Color> property in colors) {
					Material.SetColor(property.Key, property.Value);
				}
			}

			if (textures != null) {
				foreach (KeyValuePair<string, Texture> property in textures) {
					Material.SetTexture(property.Key, property.Value);
				}
			}

			Material.SetFloat(Shader.PropertyToID("_StartTime"), Time.time);
		}

		private void BuildRenderMaterial(string shader = "Unlit/Transparent") {
			Material = new Material(Shader.Find(shader));
		}
	}
}