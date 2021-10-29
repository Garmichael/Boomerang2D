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
	public class SingleGraphicCachedEditorInfo {
		public bool UsesPortraitContent;

		public string Tileset = "";
		public int StampIndex;

		public Vector2Int StampSize;

		public string PreviewTileset = "";
		public int PreviewStampIndex;

		public Texture2D CachedTexture;
	}

	public class SingleGraphicEditor : UiElementEditor {
#if UNITY_EDITOR
		private bool _loadedData;

		private struct TileSetData {
			public TilesetProperties Properties;
			public List<Texture2D> Textures;
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

		private SingleGraphicCachedEditorInfo _cachedEditorInfo;

		private void LoadData() {
			if (_loadedData) {
				return;
			}

			_loadedData = true;
			_allTileSets.Clear();

			foreach (KeyValuePair<string, TextAsset> tilesetJson in BoomerangDatabase.TilesetJsonDatabaseEntries) {
				TilesetProperties tilesetProperties = JsonUtility.FromJson<TilesetProperties>(tilesetJson.Value.text);
				tilesetProperties.PopulateLookupTable();

				List<Texture2D> tilesetTextures = LoadTilesetTextures(tilesetProperties);
				List<Color32[]> tilesetPixelData = LoadTilesetPixelDatas(tilesetTextures);

				_allTileSets.Add(tilesetProperties.Name, new TileSetData {
					Properties = tilesetProperties,
					Textures = tilesetTextures,
					PixelDatas = tilesetPixelData
				});
			}
		}

		public override string RenderPropertiesForm(string propertiesJson, Type propertiesType) {
			LoadData();

			SingleGraphic element = (SingleGraphic) JsonUtility.FromJson(propertiesJson, propertiesType);

			element.IsActive = SuperForms.Checkbox("Starts Active", element.IsActive);

			SuperForms.Space();

			SuperForms.BoxSubHeader("Position");

			element.OriginCorner = (OriginCorner) SuperForms.EnumDropdown("Origin Corner", element.OriginCorner);
			element.Position = SuperForms.Vector2FieldSingleLine("Position", element.Position);

			SuperForms.Space();


			SuperForms.BoxSubHeader("Graphic");

			if (_allTileSets.Count == 0) {
				SuperForms.FullBoxLabel("Add a Tileset");
			} else {
				element.UsesPortraitContent = SuperForms.Checkbox("Uses Content Portrait", element.UsesPortraitContent);

				if (element.UsesPortraitContent) {
					element.ContentId = SuperForms.StringField("Content Id", element.ContentId);
					
					if (AllTileSetNames.IndexOf(element.PreviewTileset) == -1) {
						element.PreviewTileset = AllTileSetNames[0];
					}

					element.PreviewTileset = AllTileSetNames[
						SuperForms.DropDown("Preview Tileset", AllTileSetNames.IndexOf(element.PreviewTileset), AllTileSetNames.ToArray())
					];

					List<string> stamps = new List<string>();
					foreach (KeyValuePair<string, TileSetData> tileSet in _allTileSets) {
						if (tileSet.Value.Properties.Name == element.Tileset) {
							foreach (TilesetEditorStamp stamp in tileSet.Value.Properties.Stamps) {
								stamps.Add(stamp.Name);
							}
						}
					}

					if (element.PreviewStampIndex < 0 || element.PreviewStampIndex >= stamps.Count) {
						element.PreviewStampIndex = 0;
					}

					if (stamps.Count > 0) {
						element.PreviewStampIndex = SuperForms.DropDown("Preview Stamp", element.PreviewStampIndex, stamps.ToArray());
					} else {
						SuperForms.FullBoxLabel("Add a Stamp to the Tileset");
					}
				} else {
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

					if (element.StampIndex < 0 || element.StampIndex >= stamps.Count) {
						element.StampIndex = 0;
					}

					if (stamps.Count > 0) {
						element.StampIndex = SuperForms.DropDown("Stamp", element.StampIndex, stamps.ToArray());
					} else {
						SuperForms.FullBoxLabel("Add a Stamp to the Tileset");
					}
				}

				element.StampSize = SuperForms.Vector2FieldSingleLine("Stamp Size", element.StampSize);
				element.StampSize.x = BoomerangUtils.MinValue(element.StampSize.x, 1);
				element.StampSize.y = BoomerangUtils.MinValue(element.StampSize.y, 1);
			}

			return JsonUtility.ToJson(element);
		}

		private bool ShouldBuildTexture(SingleGraphic element) {
			return _cachedEditorInfo == null ||
			       _cachedEditorInfo.CachedTexture == null ||
			       element.StampSize != _cachedEditorInfo.StampSize ||
			       element.UsesPortraitContent != _cachedEditorInfo.UsesPortraitContent ||
			       element.Tileset != _cachedEditorInfo.Tileset ||
			       element.StampIndex != _cachedEditorInfo.StampIndex ||
			       element.PreviewTileset != _cachedEditorInfo.PreviewTileset ||
			       element.PreviewStampIndex != _cachedEditorInfo.PreviewStampIndex;
		}

		private void BuildStampTexture(SingleGraphic element, TileSetData tilesetData, int stampIndex) {
			int width = element.StampSize.x * tilesetData.Properties.TileSize;
			int height = element.StampSize.y * tilesetData.Properties.TileSize;
			Color32[] missingTileGraphicPixelData = SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays].GetPixels32();
			Color32[] transparentTileGraphicPixelData = new Color32[(int) GameProperties.PixelsPerUnit * (int) GameProperties.PixelsPerUnit];
			for (int i = 0; i < transparentTileGraphicPixelData.Length; i++) {
				transparentTileGraphicPixelData[i] = new Color32(0, 0, 0, 0);
			}

			Texture2D texture2D = new Texture2D(width, height) {
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};

			if (!BoomerangUtils.IndexInRange(tilesetData.Properties.Stamps, stampIndex)) {
				return;
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

			_cachedEditorInfo = new SingleGraphicCachedEditorInfo {
				CachedTexture = texture2D,
				Tileset = element.Tileset,
				StampIndex = element.StampIndex,
				PreviewTileset = element.PreviewTileset,
				PreviewStampIndex = element.PreviewStampIndex,
				StampSize = element.StampSize,
				UsesPortraitContent = element.UsesPortraitContent
			};
		}

		public override void RenderPreview(int renderScale, string propertiesJson, Type propertiesType) {
			LoadData();

			SingleGraphic element = (SingleGraphic) JsonUtility.FromJson(propertiesJson, propertiesType);

			if (element?.Tileset == null || !_allTileSets.ContainsKey(element.Tileset)) {
				return;
			}

			if (ShouldBuildTexture(element)) {
				BuildStampTexture(
					element,
					element.UsesPortraitContent
						? _allTileSets[element.PreviewTileset]
						: _allTileSets[element.Tileset],
					element.UsesPortraitContent
						? element.PreviewStampIndex
						: element.StampIndex
				);
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


#endif
	}
}