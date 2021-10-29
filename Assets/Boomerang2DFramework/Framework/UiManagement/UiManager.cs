using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.BitmapFonts;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.Camera;
using Boomerang2DFramework.Framework.DialogBoxContent;
using Boomerang2DFramework.Framework.UiManagement.UiElements;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement {
	public static class UiManager {
		private const float InitialZ = -20;
		private static readonly Dictionary<string, BuiltHud> BuiltHuds = new Dictionary<string, BuiltHud>();

		public static readonly Dictionary<string, DialogContentItem> DialogContentStore =
			new Dictionary<string, DialogContentItem>();

		public static readonly Dictionary<string, BitmapFontDetails> Glyphs =
			new Dictionary<string, BitmapFontDetails>();

		public struct BitmapFontDetails {
			public BitmapFontProperties BitmapFontProperties;
			public Dictionary<int, BitmapFontGlyphProperties> GlyphLookup;
		}

		private struct BuiltHud {
			public GameObject Container;
			public List<BuiltElement> BuiltElements;
			public Action[] Callbacks;
			public LayerCameraController LayerCameraController;
		}

		private struct BuiltElement {
			public GameObject Container;
			public UiElementBehavior Behavior;
		}

		static UiManager() {
			BuildDialogContentStore();
			BuildFontGlyphs();
		}

		private static void BuildDialogContentStore() {
			DialogContentItems unIndexedContentStore =
				JsonUtility.FromJson<DialogContentItems>(BoomerangDatabase.DialogContentStore.text);

			DialogContentStore.Clear();
			foreach (DialogContentItem contentItem in unIndexedContentStore.ContentItems) {
				if (DialogContentStore.ContainsKey(contentItem.DialogContentId)) {
					DialogContentStore[contentItem.DialogContentId] = contentItem;
				} else {
					DialogContentStore.Add(contentItem.DialogContentId, contentItem);
				}
			}
		}

		private static void BuildFontGlyphs() {
			Glyphs.Clear();

			foreach (KeyValuePair<string, TextAsset> bitmapFontJson in BoomerangDatabase.BitmapFontJsonEntries) {
				BitmapFontProperties bitmapFontProperties =
					JsonUtility.FromJson<BitmapFontProperties>(bitmapFontJson.Value.text);

				BitmapFontDetails bitmapFontDetails = new BitmapFontDetails {
					BitmapFontProperties = bitmapFontProperties,
					GlyphLookup = new Dictionary<int, BitmapFontGlyphProperties>()
				};

				foreach (BitmapFontGlyphProperties bitmapFontGlyphProperties in bitmapFontProperties.Glyphs) {
					bitmapFontDetails.GlyphLookup.Add(bitmapFontGlyphProperties.CharacterCode,
						bitmapFontGlyphProperties);
				}

				Glyphs.Add(bitmapFontProperties.Name, bitmapFontDetails);
			}
		}

		/// <summary>
		/// Spawns a HudObject into the game space
		/// </summary>
		/// <param name="hudObjectName">The name of the HudObject as defined in the HudObject Editor</param>
		/// <param name="contentId">An optional parameter for the Content Id to use, as defined in the Dialog Content
		/// Editor</param>
		/// <param name="onCloseCallbacks">An Array of actions to execute when this Hud Object Closes</param>
		public static void DisplayHudObject(
			string hudObjectName,
			string contentId = "",
			params Action[] onCloseCallbacks
		) {
			if (!BoomerangDatabase.HudObjectEntries.ContainsKey(hudObjectName)) {
				Debug.LogWarning("No Hud Found by the name of " + hudObjectName);
				return;
			}

			if (BuiltHuds.ContainsKey(hudObjectName)) {
				return;
			}

			HudObjectProperties hudObjectProperties =
				JsonUtility.FromJson<HudObjectProperties>(BoomerangDatabase.HudObjectEntries[hudObjectName].text);
			
			GameObject mainContainer = new GameObject("HudContainer:" + hudObjectName);

			Vector3 z = new Vector3(0, 0, -(InitialZ - BuiltHuds.Count * 10 + BuiltHuds.Count)); 
			mainContainer.transform.localPosition = z;

			LayerCameraController layerCameraController = mainContainer.AddComponent<LayerCameraController>();
			layerCameraController.RegisterUiLayerCamera(hudObjectName);
			layerCameraController.SetShader(hudObjectProperties.Shader);

			GameObject hudContainer = new GameObject(hudObjectName);
			hudContainer.transform.parent = mainContainer.transform;
			hudContainer.transform.localPosition = new Vector3(0, 0, -BuiltHuds.Count);

			BuiltHuds.Add(hudObjectName, new BuiltHud {
				Container = mainContainer,
				BuiltElements = new List<BuiltElement>(),
				Callbacks = onCloseCallbacks,
				LayerCameraController = layerCameraController
			});

			HudObjectBehavior hudObjectBehavior = hudContainer.AddComponent<HudObjectBehavior>();

			BuildElementsForHudObject(hudObjectName, hudContainer, hudObjectBehavior, hudObjectProperties, contentId);
			hudObjectBehavior.Initialize(hudObjectProperties);
		}

		/// <summary>
		/// Checks to see if a HudObject is open
		/// </summary>
		/// <param name="hudObjectName">The HudObject name, as defined in the HudObject Editor</param>
		/// <returns></returns>
		public static bool HudObjectIsOpen(string hudObjectName) {
			return BuiltHuds.ContainsKey(hudObjectName);
		}

		/// <summary>
		/// Sets a shader for an opened HudObject
		/// </summary>
		/// <param name="hudObjectName">The HudObject name, as defined in the HudObject Editor</param>
		/// <param name="shader">The Shader Name. Must be in the B2D Database</param>
		/// <param name="floats">A Collection of values and properties for Shader Floats</param>
		/// <param name="ints">A Collection of values and properties for Shader Ints</param>
		/// <param name="colors">A Collection of values and properties for Shader Colors</param>
		/// <param name="textures">A Collection of values and properties for Shader Textures. Must be in the
		/// B2D Database</param>
		public static void SetShader(
			string hudObjectName,
			string shader,
			Dictionary<string, float> floats = null,
			Dictionary<string, int> ints = null,
			Dictionary<string, Color> colors = null,
			Dictionary<string, Texture> textures = null
		) {
			if (BuiltHuds.ContainsKey(hudObjectName)) {
				BuiltHuds[hudObjectName].LayerCameraController.SetShader(shader, floats, ints, colors, textures);
			}
		}

		private static void BuildElementsForHudObject(
			string hudObjectName,
			GameObject hudContainer,
			HudObjectBehavior hudObjectBehavior,
			HudObjectProperties hudObjectProperties,
			string contentId
		) {
			for (int index = 0; index < hudObjectProperties.Elements.Count; index++) {
				HudElementProperties hudElementProperties = hudObjectProperties.Elements[index];
				Type behaviorType = Type.GetType(hudElementProperties.ElementBehaviorClass);
				Type propertiesType = Type.GetType(hudElementProperties.ElementPropertiesClass);

				if (behaviorType == null || propertiesType == null) {
					Debug.LogWarning("Behavior Class " + behaviorType + " or Properties not found");
					continue;
				}

				GameObject elementContainer = new GameObject(hudElementProperties.ElementName);
				elementContainer.transform.parent = hudContainer.transform;
				elementContainer.transform.localPosition =
					new Vector3(0, 0, GameProperties.MapLayerDepth - (index + 1) * 0.1f);

				UiElementBehavior elementBehavior = (UiElementBehavior) elementContainer.AddComponent(behaviorType);

				BuiltHuds[hudObjectName].BuiltElements.Add(new BuiltElement {
					Behavior = elementBehavior,
					Container = elementContainer
				});

				Rect hudDimensions = new Rect(
					-GameProperties.UnitsWide / 2,
					GameProperties.UnitsHigh / 2,
					GameProperties.UnitsWide,
					GameProperties.UnitsHigh
				);

				UiElementProperties elementProperties =
					(UiElementProperties) JsonUtility.FromJson(hudElementProperties.Properties, propertiesType);
				elementBehavior.Initialize(elementProperties, hudObjectName, elementContainer, hudObjectBehavior,
					hudDimensions, contentId);
			}
		}

		/// <summary>
		/// Removes a HudObject from the game space
		/// </summary>
		/// <param name="hudObjectName">The HudObject name, as defined in the HudObject Editor</param>
		/// <param name="callbackIndex">If set, will execute the Callback found at the index of  its OnCloseCallbacks
		/// List</param>
		public static void RemoveHudObject(string hudObjectName, int callbackIndex = -1) {
			if (!BuiltHuds.ContainsKey(hudObjectName)) {
				return;
			}

			Action[] callbacks = BuiltHuds[hudObjectName].Callbacks;

			foreach (BuiltElement builtElement in BuiltHuds[hudObjectName].BuiltElements) {
				builtElement.Behavior.CleanUp();
			}

			Boomerang2D.MainCameraController.ClearUiLayer(hudObjectName);
			GameObject.DestroyImmediate(BuiltHuds[hudObjectName].Container);
			BuiltHuds.Remove(hudObjectName);

			if (BoomerangUtils.IndexInRange(callbacks, callbackIndex)) {
				callbacks[callbackIndex]?.Invoke();
			} else {
				foreach (Action callback in callbacks) {
					callback?.Invoke();
				}
			}
		}

		/// <summary>
		/// Set the Active status an Element in an opened HudObject 
		/// </summary>
		/// <param name="hudObjectName">The HudObject name, as defined in the HudObject Editor</param>
		/// <param name="elementIndex">The Index of the Callback to Execute</param>
		/// <param name="isActive">The value to set the Active status to</param>
		public static void SetElementActiveStatus(string hudObjectName, int elementIndex, bool isActive) {
			if (!BuiltHuds.ContainsKey(hudObjectName)) {
				return;
			}

			BuiltHuds[hudObjectName].BuiltElements[elementIndex].Behavior.SetActive(isActive);
		}
	}
}