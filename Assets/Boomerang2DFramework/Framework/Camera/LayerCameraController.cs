using System;
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
		private Material _material;

		public void RegisterMapLayerCamera(string mapName, MapLayerBehavior mapLayerBehavior) {
			RenderTexture renderTexture = BuildRenderTexture();
			Boomerang2D.MainCameraController.AddMapLayerRender(mapName, mapLayerBehavior, renderTexture);
		}

		public void RegisterUiLayerCamera(string mapName) {
			RenderTexture renderTexture = BuildRenderTexture();

			Boomerang2D.MainCameraController.AddUiLayerRender(mapName, renderTexture);
		}

		private RenderTexture BuildRenderTexture() {
			RenderTexture = new RenderTexture(
				GameProperties.RenderDimensionsWidth,
				GameProperties.RenderDimensionsHeight,
				24,
				RenderTextureFormat.Default
			) {filterMode = FilterMode.Point};
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

			return RenderTexture;
		}

		public void SetShader(
			string shader,
			Dictionary<string, float> floats = null,
			Dictionary<string, int> ints = null,
			Dictionary<string, Color> colors = null,
			Dictionary<string, Texture> textures = null
		) {
			BuildRenderMaterial();

			_material.shader = Shader.Find(shader);

			if (floats != null) {
				foreach (KeyValuePair<string, float> property in floats) {
					_material.SetFloat(property.Key, property.Value);
				}
			}

			if (ints != null) {
				foreach (KeyValuePair<string, int> property in ints) {
					_material.SetInt(property.Key, property.Value);
				}
			}

			if (colors != null) {
				foreach (KeyValuePair<string, Color> property in colors) {
					_material.SetColor(property.Key, property.Value);
				}
			}

			if (textures != null) {
				foreach (KeyValuePair<string, Texture> property in textures) {
					_material.SetTexture(property.Key, property.Value);
				}
			}

			_material.SetFloat(Shader.PropertyToID("_StartTime"), Time.time);
		}

		private void OnPostRender() {
			if (_material == null) {
				BuildRenderMaterial();
			}

			Graphics.Blit(RenderTexture, RenderTexture, _material);
		}

		private void BuildRenderMaterial() {
			Shader shader = Shader.Find("Unlit/Transparent");
			_material = new Material(shader);
		}
	}
}