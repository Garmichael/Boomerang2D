using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.UiManagement.UiElements.Properties;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements.Editor {
	public class GraphicGridCachedEditorInfo {
		public ValueSource MaxValueSource;
		public int StaticMaxValue;

		public ValueSource CurrentValueSource;
		public int StaticCurrentValue;

		public int ItemsPerRow = 1;

		public string Tileset;
		public int FilledStampIndex;
		public bool UsesEmptyStamp;
		public int EmptyStampIndex;

		public Vector2Int StampSize;
		public Vector2Int Spacing;

		public int PreviewMaxCount;
		public int PreviewCurrentCount;
		public Texture2D CachedTexture;
	}

	public class GraphicGridEditor : UiElementEditor {
#if UNITY_EDITOR
		private bool _loadedData;

		private struct TileSetData {
			public TilesetProperties Properties;
			public List<Color32[]> PixelDatas;
		}

		private readonly Dictionary<string, TileSetData> _allTileSets = new Dictionary<string, TileSetData>();

		private List<string> AllTileSetNames {
			get {
				List<string> tileSetNames = new List<string>();
				foreach (KeyValuePair<string, TileSetData> tileSet in _allTileSets) {
					tileSetNames.Add(tileSet.Key);
				}

				return tileSetNames;
			}
		}

		private GraphicGridCachedEditorInfo _cachedEditorInfo;

		private void LoadData() {
			if (_loadedData) {
				return;
			}

			_loadedData = true;
			_allTileSets.Clear();

			foreach (KeyValuePair<string, TextAsset> actorJson in BoomerangDatabase.TilesetJsonDatabaseEntries) {
				TilesetProperties tilesetProperties = JsonUtility.FromJson<TilesetProperties>(actorJson.Value.text);
				tilesetProperties.PopulateLookupTable();

				List<Texture2D> tilesetTextures = LoadTilesetTextures(tilesetProperties);
				List<Color32[]> tilesetPixelData = LoadTilesetPixelDatas(tilesetTextures);

				_allTileSets.Add(tilesetProperties.Name, new TileSetData {
					Properties = tilesetProperties,
					PixelDatas = tilesetPixelData
				});
			}
		}


		private bool ShouldBuildTexture(GraphicGrid element) {
			return _cachedEditorInfo == null ||
			       _cachedEditorInfo.CachedTexture == null ||
			       element.MaxValueSource != _cachedEditorInfo.MaxValueSource ||
			       element.StaticMaxValue != _cachedEditorInfo.StaticMaxValue ||
			       element.CurrentValueSource != _cachedEditorInfo.CurrentValueSource ||
			       element.StaticCurrentValue != _cachedEditorInfo.StaticCurrentValue ||
			       element.ItemsPerRow != _cachedEditorInfo.ItemsPerRow ||
			       element.Tileset != _cachedEditorInfo.Tileset ||
			       element.FilledStampIndex != _cachedEditorInfo.FilledStampIndex ||
			       element.UsesEmptyStamp != _cachedEditorInfo.UsesEmptyStamp ||
			       element.EmptyStampIndex != _cachedEditorInfo.EmptyStampIndex ||
			       element.StampSize.x != _cachedEditorInfo.StampSize.x ||
			       element.StampSize.y != _cachedEditorInfo.StampSize.y ||
			       element.Spacing != _cachedEditorInfo.Spacing ||
			       element.PreviewCurrentCount != _cachedEditorInfo.PreviewCurrentCount ||
			       element.PreviewMaxCount != _cachedEditorInfo.PreviewMaxCount;
		}

		private void BuildTexture(GraphicGrid element) {
			Color32[] missingTileGraphicPixelData = SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays].GetPixels32();
			Color32[] transparentTileGraphicPixelData = new Color32[(int) GameProperties.PixelsPerUnit * (int) GameProperties.PixelsPerUnit];
			for (int i = 0; i < transparentTileGraphicPixelData.Length; i++) {
				transparentTileGraphicPixelData[i] = new Color32(0, 0, 0, 0);
			}

			TileSetData tilesetData = _allTileSets[element.Tileset];

			Texture2D filledItemTexture = BuildStampTexture(
				element,
				tilesetData,
				transparentTileGraphicPixelData,
				missingTileGraphicPixelData,
				element.FilledStampIndex
			);

			Texture2D emptyItemTexture = BuildStampTexture(
				element,
				tilesetData,
				transparentTileGraphicPixelData,
				missingTileGraphicPixelData,
				element.EmptyStampIndex
			);

			Color32[] filledItemPixels = filledItemTexture.GetPixels32();
			Color32[] emptyItemPixels = emptyItemTexture.GetPixels32();

			int singleIconWidth = element.StampSize.x * tilesetData.Properties.TileSize;
			int singleIconHeight = element.StampSize.y * tilesetData.Properties.TileSize;

			int filledElementCount = element.CurrentValueSource == ValueSource.GameFlagFloat
				? element.PreviewCurrentCount
				: element.StaticCurrentValue;

			int maxElementCount = element.MaxValueSource == ValueSource.GameFlagFloat
				? element.PreviewMaxCount
				: element.StaticMaxValue;

			if (element.UsesEmptyStamp) {
				filledElementCount = BoomerangUtils.MaxValue(filledElementCount, maxElementCount);
			}

			int totalElements = element.UsesEmptyStamp
				? maxElementCount
				: filledElementCount;

			int totalElementsTall = Mathf.CeilToInt((float) totalElements / element.ItemsPerRow);
			int fullElementWidth = singleIconWidth * element.ItemsPerRow + element.Spacing.x * (element.ItemsPerRow - 1);
			int fullElementHeight = singleIconHeight * totalElementsTall + element.Spacing.y * (totalElementsTall - 1);
			fullElementHeight = BoomerangUtils.MinValue(fullElementHeight, 1);
			fullElementWidth = BoomerangUtils.MinValue(fullElementWidth, 1);

			Texture2D completeTexture = new Texture2D(fullElementWidth, fullElementHeight) {
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};

			Color32[] transparency = new Color32[fullElementWidth * fullElementHeight];
			for (int i = 0; i < transparency.Length; i++) {
				transparency[i] = new Color32(0, 0, 0, 0);
			}

			completeTexture.SetPixels32(0, 0, fullElementWidth, fullElementHeight, transparency);

			int placementRow = totalElementsTall - 1;
			int placementColumn = 0;

			for (int itemIndex = 0; itemIndex < filledElementCount; itemIndex++) {
				int x = placementColumn * singleIconWidth;
				int y = placementRow * singleIconHeight;
				int totalSpaceOffsetX = placementColumn * element.Spacing.x;
				int totalSpaceOffsetY = placementRow * element.Spacing.y;

				completeTexture.SetPixels32(
					x + totalSpaceOffsetX,
					y + totalSpaceOffsetY,
					singleIconWidth,
					singleIconHeight,
					filledItemPixels
				);

				placementColumn++;

				if (placementColumn == element.ItemsPerRow) {
					placementColumn = 0;
					placementRow--;
				}
			}

			for (int itemIndex = 0; itemIndex < totalElements - filledElementCount; itemIndex++) {
				int x = placementColumn * singleIconWidth;
				int y = placementRow * singleIconHeight;
				int totalSpaceOffsetX = placementColumn * element.Spacing.x;
				int totalSpaceOffsetY = placementRow * element.Spacing.y;

				completeTexture.SetPixels32(
					x + totalSpaceOffsetX,
					y + totalSpaceOffsetY,
					singleIconWidth,
					singleIconHeight,
					emptyItemPixels
				);

				placementColumn++;

				if (placementColumn == element.ItemsPerRow) {
					placementColumn = 0;
					placementRow--;
				}
			}

			completeTexture.Apply();

			_cachedEditorInfo = new GraphicGridCachedEditorInfo {
				CachedTexture = completeTexture,
				MaxValueSource = element.MaxValueSource,
				StaticMaxValue = element.StaticMaxValue,
				CurrentValueSource = element.CurrentValueSource,
				StaticCurrentValue = element.StaticCurrentValue,
				ItemsPerRow = element.ItemsPerRow,
				Tileset = element.Tileset,
				FilledStampIndex = element.FilledStampIndex,
				UsesEmptyStamp = element.UsesEmptyStamp,
				EmptyStampIndex = element.EmptyStampIndex,
				StampSize = element.StampSize,
				Spacing = element.Spacing,
				PreviewCurrentCount = element.PreviewCurrentCount,
				PreviewMaxCount = element.PreviewMaxCount
			};
		}

		private Texture2D BuildStampTexture(
			GraphicGrid element,
			TileSetData tilesetData,
			Color32[] transparentTileGraphicPixelData,
			Color32[] missingTileGraphicPixelData,
			int stampIndex
		) {
			int width = element.StampSize.x * tilesetData.Properties.TileSize;
			int height = element.StampSize.y * tilesetData.Properties.TileSize;
			Texture2D texture2D = new Texture2D(width, height) {
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};

			if (!BoomerangUtils.IndexInRange(tilesetData.Properties.Stamps, stampIndex)) {
				return texture2D;
			}

			TilesetEditorStamp filledStamp = tilesetData.Properties.Stamps[stampIndex];
			List<List<int>> mappedStampObject = BoomerangUtils.BuildMappedObjectForPlacedStamp(
				filledStamp,
				element.StampSize.x,
				element.StampSize.y
			);

			for (int j = 0; j < mappedStampObject.Count; j++) {
				for (int i = 0; i < mappedStampObject[j].Count; i++) {
					int value = mappedStampObject[j][i];
					bool tileTextureExists = tilesetData.Properties.TilesLookup.ContainsKey(value);

					Color32[] pixelDataToUse = value == 0
						? transparentTileGraphicPixelData
						: tileTextureExists
							? tilesetData.PixelDatas[tilesetData.Properties.TilesLookup[value].AnimationFrames[0]]
							: missingTileGraphicPixelData;

					texture2D.SetPixels32(
						i * tilesetData.Properties.TileSize,
						(element.StampSize.y - 1 - j) * tilesetData.Properties.TileSize,
						tilesetData.Properties.TileSize,
						tilesetData.Properties.TileSize,
						pixelDataToUse
					);
				}
			}

			texture2D.Apply();

			return texture2D;
		}

		public override void RenderPreview(int renderScale, string propertiesJson, Type propertiesType) {
			LoadData();

			GraphicGrid element = (GraphicGrid) JsonUtility.FromJson(propertiesJson, propertiesType);

			if (element?.Tileset == null || !_allTileSets.ContainsKey(element.Tileset)) {
				return;
			}

			if (ShouldBuildTexture(element)) {
				BuildTexture(element);
			}

			int x = element.Position.x * renderScale;
			int y = element.Position.y * renderScale;

			if (element.OriginCorner == OriginCorner.TopRight || element.OriginCorner == OriginCorner.BottomRight) {
				x = GameProperties.RenderDimensionsWidth * renderScale - _cachedEditorInfo.CachedTexture.width * renderScale - element.Position.x * renderScale;
			}

			if (element.OriginCorner == OriginCorner.BottomLeft || element.OriginCorner == OriginCorner.BottomRight) {
				y = GameProperties.RenderDimensionsHeight * renderScale - _cachedEditorInfo.CachedTexture.height * renderScale -
				    element.Position.y * renderScale;
			}

			SuperForms.Texture(
				new Rect(x, y,
					_cachedEditorInfo.CachedTexture.width * renderScale,
					_cachedEditorInfo.CachedTexture.height * renderScale
				),
				_cachedEditorInfo.CachedTexture
			);
		}

		public override string RenderPropertiesForm(string propertiesJson, Type propertiesType) {
			LoadData();

			GraphicGrid element = (GraphicGrid) JsonUtility.FromJson(propertiesJson, propertiesType);

			element.IsActive = SuperForms.Checkbox("Starts Active", element.IsActive);

			SuperForms.Space();

			SuperForms.BoxSubHeader("Position");

			element.OriginCorner = (OriginCorner) SuperForms.EnumDropdown("Origin Corner", element.OriginCorner);
			element.Position = SuperForms.Vector2FieldSingleLine("Position", element.Position);

			SuperForms.Space();

			SuperForms.BoxSubHeader("Grid Properties");

			element.ItemsPerRow = SuperForms.IntField("Items Per Row", element.ItemsPerRow);
			element.Spacing = SuperForms.Vector2FieldSingleLine("Grid Spacing", element.Spacing);
			element.Spacing.x = BoomerangUtils.MinValue(element.Spacing.x, 0);
			element.Spacing.y = BoomerangUtils.MinValue(element.Spacing.y, 0);

			SuperForms.Space();

			element.CurrentValueSource = (ValueSource) SuperForms.EnumDropdown("Current Value Source", element.CurrentValueSource);
			if (element.CurrentValueSource == ValueSource.StaticNumber) {
				element.StaticCurrentValue = SuperForms.IntField("Current Value", element.StaticCurrentValue);
			} else {
				element.CurrentValueGameFlag = SuperForms.StringField("Game Flag", element.CurrentValueGameFlag);
			}

			SuperForms.Space();

			SuperForms.BoxSubHeader("Graphics");

			if (_allTileSets.Count == 0) {
				SuperForms.FullBoxLabel("Add a Tileset");
			} else {
				element.StampSize = SuperForms.Vector2FieldSingleLine("Stamp Size", element.StampSize);
				element.StampSize.x = BoomerangUtils.MinValue(element.StampSize.x, 1);
				element.StampSize.y = BoomerangUtils.MinValue(element.StampSize.y, 1);

				if (AllTileSetNames.IndexOf(element.Tileset) == -1) {
					element.Tileset = AllTileSetNames[0];
				}

				element.Tileset = AllTileSetNames[
					SuperForms.DropDown("Tileset", AllTileSetNames.IndexOf(element.Tileset), AllTileSetNames.ToArray())
				];

				List<string> stamps = new List<string>();
				foreach (KeyValuePair<string, TileSetData> tileSet in _allTileSets) {
					if (tileSet.Value.Properties.Name == element.Tileset) {
						foreach (TilesetEditorStamp stamp in tileSet.Value.Properties.Stamps) {
							stamps.Add(stamp.Name);
						}
					}
				}

				if (element.EmptyStampIndex < 0 || element.EmptyStampIndex >= stamps.Count) {
					element.EmptyStampIndex = 0;
				}

				if (stamps.Count > 0) {
					element.FilledStampIndex = SuperForms.DropDown("Filled Item Stamp", element.FilledStampIndex, stamps.ToArray());
					element.FilledActiveStampIndex = SuperForms.DropDown("Active Filled Item Stamp", element.FilledActiveStampIndex, stamps.ToArray());

					if (!BoomerangUtils.IndexInRange(stamps, element.FilledStampIndex)) {
						element.FilledStampIndex = 0;
					}

					if (!BoomerangUtils.IndexInRange(stamps, element.FilledActiveStampIndex)) {
						element.FilledActiveStampIndex = 0;
					}

					SuperForms.Space();

					element.UsesEmptyStamp = SuperForms.Checkbox("Uses Empty Stamp", element.UsesEmptyStamp);

					if (element.UsesEmptyStamp) {
						element.EmptyStampIndex = SuperForms.DropDown("Empty Item Stamp", element.EmptyStampIndex, stamps.ToArray());

						if (!BoomerangUtils.IndexInRange(stamps, element.EmptyStampIndex)) {
							element.EmptyStampIndex = 0;
						}

						element.EmptyActiveStampIndex = SuperForms.DropDown("Active Empty Item Stamp", element.EmptyActiveStampIndex, stamps.ToArray());

						if (!BoomerangUtils.IndexInRange(stamps, element.EmptyActiveStampIndex)) {
							element.EmptyActiveStampIndex = 0;
						}

						element.MaxValueSource = (ValueSource) SuperForms.EnumDropdown("Max Value Source", element.MaxValueSource);
						if (element.MaxValueSource == ValueSource.StaticNumber) {
							element.StaticMaxValue = SuperForms.IntField("Value", element.StaticMaxValue);
						} else {
							element.MaxValueGameFlag = SuperForms.StringField("Game Flag", element.MaxValueGameFlag);
						}
					}
				} else {
					SuperForms.FullBoxLabel("Add a Stamp to the Tileset");
				}
			}

			SuperForms.Space();

			bool showPreviewSettings = element.MaxValueSource == ValueSource.GameFlagFloat && element.UsesEmptyStamp ||
			                           element.CurrentValueSource == ValueSource.GameFlagFloat;

			if (showPreviewSettings) {
				SuperForms.BoxSubHeader("Preview");

				if (element.CurrentValueSource == ValueSource.GameFlagFloat) {
					element.PreviewCurrentCount = SuperForms.IntField("Current Value", element.PreviewCurrentCount);
				}

				if (element.MaxValueSource == ValueSource.GameFlagFloat && element.UsesEmptyStamp) {
					element.PreviewMaxCount = SuperForms.IntField("Max Value", element.PreviewMaxCount);
				}
			}


			SuperForms.Space();

			return JsonUtility.ToJson(element);
		}
#endif
	}
}