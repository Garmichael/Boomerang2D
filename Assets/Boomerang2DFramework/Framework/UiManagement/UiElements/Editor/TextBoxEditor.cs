using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.BitmapFonts;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.UiManagement.UiElements.Properties;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements.Editor {
	public class TextBoxCachedEditorInfo {
		public int MaxWidth;
		public int MaxHeight;
		public string Font;
		public int PaddingTop;
		public int PaddingRight;
		public int PaddingBottom;
		public int PaddingLeft;
		public string Tileset;
		public int BackgroundStampIndex;
		public int PreviewTextRowsToShow;
		public int RowSpacing;
		public int LetterKerning;
		public WordBreak WordBreak;
		public Texture2D CachedBackgroundTexture;
	}

	public class CachedButtonPromptEditorInfo {
		public int ButtonPromptWidth;
		public int ButtonPromptHeight;
		public string ButtonPromptTileset;
		public int ButtonPromptStampIndex;
		public Texture2D CachedButtonTexture;
	}

	public class TextBoxEditor : UiElementEditor {
#if UNITY_EDITOR
		private bool _loadedData;

		private struct TileSetData {
			public TilesetProperties Properties;
			public List<Color32[]> PixelDatas;
		}

		private readonly Dictionary<string, TileSetData> _allTileSets = new Dictionary<string, TileSetData>();
		private readonly List<BitmapFontProperties> _allBitmapFonts = new List<BitmapFontProperties>();

		private List<string> AllTileSetNames {
			get {
				List<string> tileSetNames = new List<string>();
				foreach (KeyValuePair<string, TileSetData> tileSet in _allTileSets) {
					tileSetNames.Add(tileSet.Key);
				}

				return tileSetNames;
			}
		}

		private List<string> AllBitmapFontNames {
			get {
				List<string> names = new List<string>();

				foreach (BitmapFontProperties bitmapFont in _allBitmapFonts) {
					names.Add(bitmapFont.Name);
				}

				return names;
			}
		}

		private Texture2D _fontPreviewTexture;
		private TextBoxCachedEditorInfo _cachedEditorInfo;
		private CachedButtonPromptEditorInfo _cachedButtonPromptEditorInfo;

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
					PixelDatas = tilesetPixelData
				});
			}

			_allBitmapFonts.Clear();

			foreach (KeyValuePair<string, TextAsset> bitmapFontJson in BoomerangDatabase.BitmapFontJsonEntries) {
				BitmapFontProperties bitmapFontProperties = JsonUtility.FromJson<BitmapFontProperties>(bitmapFontJson.Value.text);
				_allBitmapFonts.Add(bitmapFontProperties);
			}
		}

		private bool ShouldBuildBackgroundTexture(TextBox layoutElement) {
			return _cachedEditorInfo == null ||
			       _cachedEditorInfo.CachedBackgroundTexture == null ||
			       _cachedEditorInfo.MaxWidth != layoutElement.MaxWidth ||
			       _cachedEditorInfo.MaxHeight != layoutElement.MaxHeight ||
			       _cachedEditorInfo.Font != layoutElement.Font ||
			       Math.Abs(_cachedEditorInfo.PaddingTop - layoutElement.PaddingTop) > 0.1 ||
			       Math.Abs(_cachedEditorInfo.PaddingRight - layoutElement.PaddingRight) > 0.1 ||
			       Math.Abs(_cachedEditorInfo.PaddingBottom - layoutElement.PaddingBottom) > 0.1 ||
			       Math.Abs(_cachedEditorInfo.PaddingLeft - layoutElement.PaddingLeft) > 0.1 ||
			       _cachedEditorInfo.Tileset != layoutElement.Tileset ||
			       _cachedEditorInfo.BackgroundStampIndex != layoutElement.BackgroundStampIndex ||
			       _cachedEditorInfo.PreviewTextRowsToShow != layoutElement.PreviewTextRowsToShow ||
			       _cachedEditorInfo.RowSpacing != layoutElement.RowSpacing ||
			       _cachedEditorInfo.LetterKerning != layoutElement.LetterKerning ||
			       _cachedEditorInfo.WordBreak != layoutElement.WordBreak;
		}

		private void BuildBackgroundTexture(TextBox element) {
			Color32[] missingTileGraphicPixelData = SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays].GetPixels32();
			Color32[] transparentTileGraphicPixelData = new Color32[(int) GameProperties.PixelsPerUnit * (int) GameProperties.PixelsPerUnit];
			for (int i = 0; i < transparentTileGraphicPixelData.Length; i++) {
				transparentTileGraphicPixelData[i] = new Color32(0, 0, 0, 0);
			}

			TileSetData tilesetData = _allTileSets[element.Tileset];
			TilesetEditorStamp tilesetEditorStamp = tilesetData.Properties.Stamps[element.BackgroundStampIndex];
			List<List<int>> mappedStampObject = BoomerangUtils.BuildMappedObjectForPlacedStamp(
				tilesetEditorStamp,
				element.MaxWidth,
				element.MaxHeight
			);

			int width = element.MaxWidth * tilesetData.Properties.TileSize;
			int height = element.MaxHeight * tilesetData.Properties.TileSize;

			Texture2D texture2D = new Texture2D(width, height) {
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};

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
						(element.MaxHeight - 1 - j) * tilesetData.Properties.TileSize,
						tilesetData.Properties.TileSize,
						tilesetData.Properties.TileSize,
						pixelDataToUse
					);
				}
			}

			texture2D.Apply();

			_cachedEditorInfo = new TextBoxCachedEditorInfo {
				MaxWidth = element.MaxWidth,
				MaxHeight = element.MaxHeight,
				Font = element.Font,
				PaddingTop = element.PaddingTop,
				PaddingRight = element.PaddingRight,
				PaddingBottom = element.PaddingBottom,
				PaddingLeft = element.PaddingLeft,
				Tileset = element.Tileset,
				RowSpacing = element.RowSpacing,
				LetterKerning = element.LetterKerning,
				WordBreak = element.WordBreak,
				PreviewTextRowsToShow = element.PreviewTextRowsToShow,
				BackgroundStampIndex = element.BackgroundStampIndex,
				CachedBackgroundTexture = texture2D
			};
		}

		private bool ShouldBuildFontPreviewTexture() {
			return _fontPreviewTexture == null;
		}

		private void BuildFontPreviewTexture() {
			_fontPreviewTexture = new Texture2D(1, 1) {
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};

			_fontPreviewTexture.SetPixels32(new[] {new Color32(200, 200, 200, 200)});
			_fontPreviewTexture.Apply();
		}

		private bool ShouldBuildButtonPromptTexture(TextBox element) {
			return _cachedButtonPromptEditorInfo == null ||
			       _cachedButtonPromptEditorInfo.CachedButtonTexture == null ||
			       _cachedButtonPromptEditorInfo.ButtonPromptWidth != element.ButtonPromptPosition.x ||
			       _cachedButtonPromptEditorInfo.ButtonPromptHeight != element.ButtonPromptPosition.y ||
			       _cachedButtonPromptEditorInfo.ButtonPromptTileset != element.ButtonPromptTileset ||
			       _cachedButtonPromptEditorInfo.ButtonPromptStampIndex != element.ButtonPromptStampIndex;
		}

		private void BuildButtonPromptTexture(TextBox element) {
			Color32[] missingTileGraphicPixelData = SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays].GetPixels32();
			Color32[] transparentTileGraphicPixelData = new Color32[(int) GameProperties.PixelsPerUnit * (int) GameProperties.PixelsPerUnit];
			for (int i = 0; i < transparentTileGraphicPixelData.Length; i++) {
				transparentTileGraphicPixelData[i] = new Color32(0, 0, 0, 0);
			}

			TileSetData tilesetData = _allTileSets[element.ButtonPromptTileset];
			TilesetEditorStamp tilesetEditorStamp = tilesetData.Properties.Stamps[element.ButtonPromptStampIndex];
			List<List<int>> mappedStampObject = BoomerangUtils.BuildMappedObjectForPlacedStamp(
				tilesetEditorStamp,
				element.ButtonPromptSize.x,
				element.ButtonPromptSize.y
			);

			int width = element.ButtonPromptSize.x * tilesetData.Properties.TileSize;
			int height = element.ButtonPromptSize.y * tilesetData.Properties.TileSize;

			Texture2D texture2D = new Texture2D(width, height) {
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp
			};

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
						(element.ButtonPromptSize.y - 1 - j) * tilesetData.Properties.TileSize,
						tilesetData.Properties.TileSize,
						tilesetData.Properties.TileSize,
						pixelDataToUse
					);
				}
			}

			texture2D.Apply();

			_cachedButtonPromptEditorInfo = new CachedButtonPromptEditorInfo {
				ButtonPromptWidth = element.ButtonPromptSize.x,
				ButtonPromptHeight = element.ButtonPromptSize.y,
				ButtonPromptTileset = element.ButtonPromptTileset,
				ButtonPromptStampIndex = element.ButtonPromptStampIndex,
				CachedButtonTexture = texture2D
			};
		}

		public override string RenderPropertiesForm(string propertiesJson, Type propertiesType) {
			LoadData();

			TextBox element = (TextBox) JsonUtility.FromJson(propertiesJson, propertiesType);

			element.IsActive = SuperForms.Checkbox("Starts Active", element.IsActive);

			SuperForms.Space();

			SuperForms.BoxSubHeader("Position");

			element.OriginCorner = (OriginCorner) SuperForms.EnumDropdown("Origin Corner", element.OriginCorner);
			element.Position = SuperForms.Vector2FieldSingleLine("Position", element.Position);

			SuperForms.Space();
			SuperForms.BoxSubHeader("Text Properties");

			if (AllBitmapFontNames.Count == 0) {
				SuperForms.FullBoxLabel("No Fonts Available");
				SuperForms.FullBoxLabel("Create a Font with the Bitmap Font Editor");
				return JsonUtility.ToJson(element);
			}

			element.TextSource = (TextSource) SuperForms.EnumDropdown("Text Source", element.TextSource);

			if (element.TextSource == TextSource.ContentDatabase) {
				element.ContentId = SuperForms.StringField("Content Id", element.ContentId);
				element.MappedContentProperty = (MappedProperty) SuperForms.EnumDropdown("Content Property", element.MappedContentProperty);
			} else if (element.TextSource == TextSource.StaticText) {
				element.StaticText = SuperForms.StringField("Text", element.StaticText);
			} else {
				element.GameFlag = SuperForms.StringField("Game Flag", element.GameFlag);
			}

			element.PrefixWithZeroes = SuperForms.Checkbox("Prefix with Zeroes", element.PrefixWithZeroes);
			if (element.PrefixWithZeroes) {
				element.FixedLengthOfText = SuperForms.IntField("Length of Text", element.FixedLengthOfText);
			}

			int indexOfSelectedFont = AllBitmapFontNames.IndexOf(element.Font);
			indexOfSelectedFont = BoomerangUtils.MinValue(indexOfSelectedFont, 0);
			indexOfSelectedFont = SuperForms.DropDown("Font", indexOfSelectedFont, AllBitmapFontNames.ToArray());
			element.Font = AllBitmapFontNames[indexOfSelectedFont];

			int indexOfSelectedActiveFont = AllBitmapFontNames.IndexOf(element.ActiveFont);
			indexOfSelectedActiveFont = BoomerangUtils.MinValue(indexOfSelectedActiveFont, 0);
			indexOfSelectedActiveFont = SuperForms.DropDown("Active Font", indexOfSelectedActiveFont, AllBitmapFontNames.ToArray());
			element.ActiveFont = AllBitmapFontNames[indexOfSelectedActiveFont];

			element.WordBreak = (WordBreak) SuperForms.EnumDropdown("Word Break", element.WordBreak);
			element.RowSpacing = SuperForms.IntField("Row Spacing", element.RowSpacing);
			element.LetterKerning = SuperForms.IntField("Letter Kerning", element.LetterKerning);

			SuperForms.Space();
			SuperForms.BoxSubHeader("Dimensions");

			element.ScalesHorizontally = SuperForms.Checkbox("Scale Width", element.ScalesHorizontally);

			if (element.ScalesHorizontally) {
				element.MinWidth = SuperForms.IntField("Min Width", element.MinWidth);
				element.MaxWidth = SuperForms.IntField("Max Width", element.MaxWidth);
			} else {
				element.MaxWidth = SuperForms.IntField("Width", element.MaxWidth);
			}

			SuperForms.Space();

			element.ScalesVertically = SuperForms.Checkbox("Scale Height", element.ScalesVertically);

			if (element.ScalesVertically) {
				element.MinHeight = SuperForms.IntField("Min Height", element.MinHeight);
				element.MaxHeight = SuperForms.IntField("Max Height", element.MaxHeight);
			} else {
				element.MaxHeight = SuperForms.IntField("Height", element.MaxHeight);
			}

			element.MaxWidth = BoomerangUtils.MinValue(element.MaxWidth, 1);
			element.MaxHeight = BoomerangUtils.MinValue(element.MaxHeight, 1);
			element.MinWidth = BoomerangUtils.ClampValue(element.MinWidth, 1, element.MaxWidth);
			element.MinHeight = BoomerangUtils.ClampValue(element.MinHeight, 1, element.MaxHeight);

			SuperForms.Space();

			SuperForms.BoxSubHeader("Padding");
			SuperForms.Region.Horizontal(() => {
				SuperForms.Label("Top/Bottom");
				element.PaddingTop = SuperForms.IntField(element.PaddingTop);
				element.PaddingBottom = SuperForms.IntField(element.PaddingBottom);
			});

			SuperForms.Region.Horizontal(() => {
				SuperForms.Label("Left/Right");
				element.PaddingLeft = SuperForms.IntField(element.PaddingLeft);
				element.PaddingRight = SuperForms.IntField(element.PaddingRight);
			});

			List<string> stamps = new List<string>();
			foreach (KeyValuePair<string, TileSetData> tileSet in _allTileSets) {
				if (tileSet.Value.Properties.Name == element.Tileset) {
					foreach (TilesetEditorStamp stamp in tileSet.Value.Properties.Stamps) {
						stamps.Add(stamp.Name);
					}
				}
			}

			if (AllTileSetNames.Count > 0) {
				SuperForms.Space();
				SuperForms.BoxSubHeader("Background Image");

				if (AllTileSetNames.IndexOf(element.Tileset) == -1) {
					element.Tileset = AllTileSetNames[0];
				}

				element.Tileset = AllTileSetNames[
					SuperForms.DropDown("Tileset", AllTileSetNames.IndexOf(element.Tileset), AllTileSetNames.ToArray())
				];

				if (element.BackgroundStampIndex < 0 || element.BackgroundStampIndex >= stamps.Count) {
					element.BackgroundStampIndex = 0;
				}

				if (stamps.Count > 0) {
					element.BackgroundStampIndex = SuperForms.DropDown("Stamp", element.BackgroundStampIndex, stamps.ToArray());
					element.ActiveBackgroundStampIndex = SuperForms.DropDown("Active Stamp", element.ActiveBackgroundStampIndex, stamps.ToArray());
				} else {
					SuperForms.FullBoxLabel("Add a Stamp to the Tileset");
				}
			} else {
				SuperForms.FullBoxLabel("Create a Tileset to add Graphics");
			}

			if (AllTileSetNames.Count > 0) {
				SuperForms.Space();
				SuperForms.BoxSubHeader("Button Prompt");

				element.UsesButtonPrompt = SuperForms.Checkbox("Uses Button Prompt", element.UsesButtonPrompt);

				if (element.UsesButtonPrompt) {
					element.OnlyShowButtonPromptOnActive = SuperForms.Checkbox("Only when Active", element.OnlyShowButtonPromptOnActive);
					element.ButtonPromptPosition = SuperForms.Vector2FieldSingleLine("Position", element.ButtonPromptPosition);
					element.ButtonPromptSize = SuperForms.Vector2FieldSingleLine("Size", element.ButtonPromptSize);
					element.ButtonPromptSize.x = BoomerangUtils.MinValue(element.ButtonPromptSize.x, 1);
					element.ButtonPromptSize.y = BoomerangUtils.MinValue(element.ButtonPromptSize.y, 1);

					element.ButtonPromptOriginCorner =
						(OriginCorner) SuperForms.EnumDropdown("Origin Corner", element.ButtonPromptOriginCorner);

					element.HudElementContainer =
						(HudElementContainer) SuperForms.EnumDropdown("Origin Container", element.HudElementContainer);

					if (AllTileSetNames.IndexOf(element.ButtonPromptTileset) == -1) {
						element.ButtonPromptTileset = AllTileSetNames[0];
					}

					element.ButtonPromptTileset = AllTileSetNames[
						SuperForms.DropDown("Tileset", AllTileSetNames.IndexOf(element.ButtonPromptTileset), AllTileSetNames.ToArray())
					];

					element.ButtonPromptStampIndex =
						SuperForms.DropDown("Button Stamp", element.ButtonPromptStampIndex, stamps.ToArray());
				}
			}

			SuperForms.Space();

			SuperForms.BoxSubHeader("Preview Options");
			element.PreviewTextRowsToShow = SuperForms.IntField("Preview Rows Shown", element.PreviewTextRowsToShow);

			SuperForms.Space();

			return JsonUtility.ToJson(element);
		}

		public override void RenderPreview(int renderScale, string propertiesJson, Type propertiesType) {
			LoadData();

			TextBox element = (TextBox) JsonUtility.FromJson(propertiesJson, propertiesType);

			if (!_allTileSets.ContainsKey(element.Tileset)) {
				return;
			}

			TileSetData tilesetData = _allTileSets[element.Tileset];
			float x = element.Position.x;
			float y = element.Position.y;
			int width = element.MaxWidth * tilesetData.Properties.TileSize;
			int height = element.MaxHeight * tilesetData.Properties.TileSize;

			if (element.OriginCorner == OriginCorner.TopRight || element.OriginCorner == OriginCorner.BottomRight) {
				x = GameProperties.RenderDimensionsWidth
				    - element.MaxWidth * tilesetData.Properties.TileSize
				    - element.Position.x;
			}

			if (element.OriginCorner == OriginCorner.BottomLeft || element.OriginCorner == OriginCorner.BottomRight) {
				y = GameProperties.RenderDimensionsHeight
				    - element.MaxHeight * tilesetData.Properties.TileSize
				    - element.Position.y;
			}

			if (!BoomerangUtils.IndexInRange(tilesetData.Properties.Stamps, element.BackgroundStampIndex)) {
				return;
			}

			Rect backgroundDimensions = new Rect(x, y, width, height);

			DrawBackground(element, backgroundDimensions, renderScale);

			DrawTextPreview(element, x, y, width, renderScale);

			if (element.UsesButtonPrompt) {
				DrawButtonPrompt(element, backgroundDimensions, renderScale);
			}
		}

		private void DrawBackground(TextBox element, Rect dimensions, int renderScale) {
			if (ShouldBuildBackgroundTexture(element)) {
				BuildBackgroundTexture(element);
			}

			SuperForms.Texture(
				new Rect(
					dimensions.x * renderScale,
					dimensions.y * renderScale,
					dimensions.width * renderScale,
					dimensions.height * renderScale
				), _cachedEditorInfo.CachedBackgroundTexture
			);
		}

		private void DrawTextPreview(TextBox element, float x, float y, int width, int renderScale) {
			if (ShouldBuildFontPreviewTexture()) {
				BuildFontPreviewTexture();
			}

			if (!AllBitmapFontNames.Contains(element.Font)) {
				return;
			}

			for (int i = 0; i < element.PreviewTextRowsToShow; i++) {
				float thisRowWidth = (width - element.PaddingLeft - element.PaddingRight) * renderScale;
				float thisRowHeight = _allBitmapFonts[AllBitmapFontNames.IndexOf(element.Font)].GlyphHeight * renderScale;
				float thisRowX = (x + element.PaddingLeft) * renderScale;
				float thisRowY = (y + element.PaddingTop) * renderScale
				                 + i * thisRowHeight
				                 + i * element.RowSpacing * renderScale;

				SuperForms.Texture(new Rect(thisRowX, thisRowY, thisRowWidth, thisRowHeight), _fontPreviewTexture);
			}
		}

		private void DrawButtonPrompt(TextBox element, Rect backgroundDimensions, int renderScale) {
			if (!_allTileSets.ContainsKey(element.ButtonPromptTileset)) {
				return;
			}

			TileSetData tilesetData = _allTileSets[element.ButtonPromptTileset];

			float xOffset = element.HudElementContainer == HudElementContainer.Screen
				? 0
				: backgroundDimensions.x;

			float yOffset = element.HudElementContainer == HudElementContainer.Screen
				? 0
				: backgroundDimensions.y;

			float x = (xOffset + element.ButtonPromptPosition.x) * renderScale;
			float y = (yOffset + element.ButtonPromptPosition.y) * renderScale;
			int width = element.ButtonPromptSize.x * tilesetData.Properties.TileSize;
			int height = element.ButtonPromptSize.y * tilesetData.Properties.TileSize;

			if (element.ButtonPromptOriginCorner == OriginCorner.TopRight || element.ButtonPromptOriginCorner == OriginCorner.BottomRight) {
				float containerWidth = element.HudElementContainer == HudElementContainer.Screen
					? GameProperties.RenderDimensionsWidth
					: backgroundDimensions.width;

				x = (containerWidth
				     + xOffset
				     - element.ButtonPromptSize.x * tilesetData.Properties.TileSize
				     - element.ButtonPromptPosition.x
				    ) * renderScale;
			}

			if (element.ButtonPromptOriginCorner == OriginCorner.BottomLeft || element.ButtonPromptOriginCorner == OriginCorner.BottomRight) {
				float containerHeight = element.HudElementContainer == HudElementContainer.Screen
					? GameProperties.RenderDimensionsHeight
					: backgroundDimensions.height;

				y = (containerHeight
				     + yOffset
				     - element.ButtonPromptSize.y * tilesetData.Properties.TileSize
				     - element.ButtonPromptPosition.y
				    ) * renderScale;
			}

			if (!BoomerangUtils.IndexInRange(tilesetData.Properties.Stamps, element.ButtonPromptStampIndex)) {
				return;
			}

			if (ShouldBuildButtonPromptTexture(element)) {
				BuildButtonPromptTexture(element);
			}

			SuperForms.Texture(
				new Rect(x, y, width * renderScale, height * renderScale),
				_cachedButtonPromptEditorInfo.CachedButtonTexture
			);
		}
#endif
	}
}