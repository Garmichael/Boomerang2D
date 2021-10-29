using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Boomerang2DFramework.Framework.BitmapFonts;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.UiManagement.TriggerSystem;
using Boomerang2DFramework.Framework.UiManagement.UiElements.Properties;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements.Editor {
	public class ChoiceBoxCachedEditorInfo {
		public List<string> Choices;
		public int ActiveIndex;
		public string Font;
		public int RowSpacing;
		public int Width;
		public int BackgroundPaddingTop;
		public int BackgroundPaddingRight;
		public int BackgroundPaddingBottom;
		public int BackgroundPaddingLeft;
		public int ItemPaddingTop;
		public int ItemPaddingRight;
		public int ItemPaddingLeft;
		public string Tileset;
		public int BackgroundStampIndex;
		public int UnselectedStampIndex;
		public int SelectedStampIndex;
		public int PreviewChoiceCount;
		public Texture2D CachedBackgroundTexture;
		public Texture2D CachedUnSelectedItemTexture;
		public Texture2D CachedSelectedItemTexture;
	}

	public class ChoiceBoxEditor : UiElementEditor {
#if UNITY_EDITOR
		private bool _loadedData;

		private ChoiceBoxCachedEditorInfo _cachedEditorInfo;

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

		public override string RenderPropertiesForm(string propertiesJson, Type propertiesType) {
			LoadData();

			ChoiceBox element = (ChoiceBox) JsonUtility.FromJson(propertiesJson, propertiesType);

			element.IsActive = SuperForms.Checkbox("Starts Active", element.IsActive);

			SuperForms.Space();

			SuperForms.BoxSubHeader("Position");

			element.OriginCorner = (OriginCorner) SuperForms.EnumDropdown("Origin Corner", element.OriginCorner);
			element.Position = SuperForms.Vector2FieldSingleLine("Position", element.Position);

			SuperForms.Space();

			SuperForms.BoxSubHeader("Source Content");

			element.UsesContentId = SuperForms.Checkbox("Uses Content Id", element.UsesContentId);

			if (element.UsesContentId) {
				element.ContentId = SuperForms.StringField("Content Id", element.ContentId);
			} else {
				if (element.Choices.Count == 0) {
					element.Choices.Add("");
				}

				element.Choices = SuperForms.ListField("Choices", element.Choices);
			}

			element.PreviewChoiceCount = SuperForms.IntField("Preview Items Count", element.PreviewChoiceCount);
			element.PreviewChoiceCount = BoomerangUtils.MinValue(element.PreviewChoiceCount, 1);

			SuperForms.Space();


			if (AllTileSetNames.Count > 0) {
				SuperForms.Space();
				SuperForms.BoxSubHeader("Tileset");

				if (AllTileSetNames.IndexOf(element.Tileset) == -1) {
					element.Tileset = AllTileSetNames[0];
				}

				element.Tileset = AllTileSetNames[
					SuperForms.DropDown("Tileset", AllTileSetNames.IndexOf(element.Tileset), AllTileSetNames.ToArray())
				];
			} else {
				SuperForms.FullBoxLabel("Create a tileset");
				return JsonUtility.ToJson(element);
			}

			SuperForms.Space();

			List<string> stamps = new List<string>();
			foreach (KeyValuePair<string, TileSetData> tileSet in _allTileSets) {
				if (tileSet.Value.Properties.Name == element.Tileset) {
					foreach (TilesetEditorStamp stamp in tileSet.Value.Properties.Stamps) {
						stamps.Add(stamp.Name);
					}
				}
			}

			if (stamps.Count == 0) {
				SuperForms.FullBoxLabel("Create Stamps for this Tileset");
				return JsonUtility.ToJson(element);
			}

			SuperForms.BoxSubHeader("Background");

			if (element.BackgroundStampIndex < 0 || element.BackgroundStampIndex >= stamps.Count) {
				element.BackgroundStampIndex = 0;
			}

			element.BackgroundStampIndex = SuperForms.DropDown("Background Stamp", element.BackgroundStampIndex, stamps.ToArray());

			SuperForms.Region.Horizontal(() => {
				SuperForms.Label("Top/Bottom Padding");
				element.BackgroundPaddingTop = SuperForms.IntField(element.BackgroundPaddingTop);
				element.BackgroundPaddingBottom = SuperForms.IntField(element.BackgroundPaddingBottom);
			});

			SuperForms.Region.Horizontal(() => {
				SuperForms.Label("Left/Right Padding");
				element.BackgroundPaddingLeft = SuperForms.IntField(element.BackgroundPaddingLeft);
				element.BackgroundPaddingRight = SuperForms.IntField(element.BackgroundPaddingRight);
			});


			SuperForms.Space();

			SuperForms.BoxSubHeader("Items");

			element.ActiveIndex = SuperForms.IntField("Starting Active Index", element.ActiveIndex);

			if (element.UnselectedStampIndex < 0 || element.UnselectedStampIndex >= stamps.Count) {
				element.UnselectedStampIndex = 0;
			}

			element.UnselectedStampIndex = SuperForms.DropDown("Unselected Item Stamp", element.UnselectedStampIndex, stamps.ToArray());

			int indexOfUnSelectedFont = AllBitmapFontNames.IndexOf(element.UnselectedFont);
			indexOfUnSelectedFont = BoomerangUtils.MinValue(indexOfUnSelectedFont, 0);
			indexOfUnSelectedFont = SuperForms.DropDown("Unselected Font", indexOfUnSelectedFont, AllBitmapFontNames.ToArray());
			element.UnselectedFont = AllBitmapFontNames[indexOfUnSelectedFont];

			element.SelectedStampIndex = SuperForms.DropDown("Selected Item Stamp", element.SelectedStampIndex, stamps.ToArray());

			int indexOfSelectedFont = AllBitmapFontNames.IndexOf(element.SelectedFont);
			indexOfSelectedFont = BoomerangUtils.MinValue(indexOfSelectedFont, 0);
			indexOfSelectedFont = SuperForms.DropDown("Selected Font", indexOfSelectedFont, AllBitmapFontNames.ToArray());
			element.SelectedFont = AllBitmapFontNames[indexOfSelectedFont];
			
			element.ItemWidth = SuperForms.IntField("Item Width", element.ItemWidth);
			element.ItemWidth = BoomerangUtils.MinValue(element.ItemWidth, 1);

			element.ItemSpacing = SuperForms.IntField("Item Spacing", element.ItemSpacing);
			element.LetterKerning = SuperForms.IntField("Letter Kerning", element.LetterKerning);

			SuperForms.Region.Horizontal(() => {
				SuperForms.Label("Top Padding");
				element.ItemPaddingTop = SuperForms.IntField(element.ItemPaddingTop);
			});

			SuperForms.Region.Horizontal(() => {
				SuperForms.Label("Left/Right Padding");
				element.ItemPaddingLeft = SuperForms.IntField(element.ItemPaddingLeft);
				element.ItemPaddingRight = SuperForms.IntField(element.ItemPaddingRight);
			});

			SuperForms.Space();

			SuperForms.BoxSubHeader("Item Selection");

			element.AllowLooping = SuperForms.Checkbox("Allow Looping", element.AllowLooping);

			Dictionary<string, string> triggerClasses = AssemblyFinder.Assemblies.HudObjectTriggers;
			Dictionary<string, string> triggerPropertyClasses = AssemblyFinder.Assemblies.HudObjectTriggerProperties;

			HudObjectTriggerBuilder nextTriggerBuilderToDelete = null;
			HudObjectTriggerBuilder previousTriggerBuilderToDelete = null;
			HudObjectTriggerBuilder makeSelectionTriggerBuilderToDelete = null;

			SuperForms.Space();

			SuperForms.FullBoxLabel("Next Item Condition");

			SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd,
				() => { element.NextItemTriggerBuilders.Add(new HudObjectTriggerBuilder()); });

			foreach (HudObjectTriggerBuilder hudObjectTriggerBuilder in element.NextItemTriggerBuilders) {
				SuperForms.Begin.VerticalSubBox();

				if (string.IsNullOrEmpty(hudObjectTriggerBuilder.TriggerClass)) {
					hudObjectTriggerBuilder.TriggerClass = triggerClasses.First().Value;
				}

				string triggerName = "TRIGGER NOT FOUND";

				foreach (KeyValuePair<string, string> triggerClass in triggerClasses) {
					triggerName = triggerClass.Value;
					if (triggerClass.Value == hudObjectTriggerBuilder.TriggerClass) {
						triggerName = triggerClass.Key;
						break;
					}
				}

				triggerName = Regex.Replace(triggerName, "(B2D: )", "");
				triggerName = Regex.Replace(triggerName, "(Ext: )", "");
				triggerName = Regex.Replace(triggerName, "(\\B[A-Z])", " $1");

				SuperForms.BoxSubHeader(triggerName);

				hudObjectTriggerBuilder.TriggerClass = SuperForms.DictionaryDropDown(
					"Trigger Class",
					triggerClasses,
					hudObjectTriggerBuilder.TriggerClass
				);

				string selectedTriggerClass = triggerClasses.FirstOrDefault(pair => pair.Value == hudObjectTriggerBuilder.TriggerClass).Key;
				hudObjectTriggerBuilder.TriggerPropertyClass = triggerPropertyClasses[selectedTriggerClass + "Properties"];
				Type triggerPropertiesType = Type.GetType(triggerPropertyClasses[selectedTriggerClass + "Properties"]);

				object triggerProperties = JsonUtility.FromJson(hudObjectTriggerBuilder.TriggerProperties, triggerPropertiesType);

				if (triggerProperties != null) {
					FieldInfo[] fields = triggerProperties.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

					if (fields.Length > 0) {
						SuperForms.Space();

						foreach (FieldInfo field in fields) {
							field.SetValue(triggerProperties, FieldBasedForms.DrawFormField(field.Name, field.GetValue(triggerProperties)));
						}
					}

					hudObjectTriggerBuilder.TriggerProperties = JsonUtility.ToJson(triggerProperties);
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
					nextTriggerBuilderToDelete = hudObjectTriggerBuilder;
				}

				SuperForms.End.Vertical();
				SuperForms.Space();
			}

			if (nextTriggerBuilderToDelete != null) {
				element.NextItemTriggerBuilders.Remove(nextTriggerBuilderToDelete);
			}

			SuperForms.FullBoxLabel("Previous Item Condition");

			SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd,
				() => { element.PreviousItemTriggerBuilders.Add(new HudObjectTriggerBuilder()); });

			foreach (HudObjectTriggerBuilder hudObjectTriggerBuilder in element.PreviousItemTriggerBuilders) {
				SuperForms.Begin.VerticalSubBox();

				if (string.IsNullOrEmpty(hudObjectTriggerBuilder.TriggerClass)) {
					hudObjectTriggerBuilder.TriggerClass = triggerClasses.First().Value;
				}

				string triggerName = "TRIGGER NOT FOUND";

				foreach (KeyValuePair<string, string> triggerClass in triggerClasses) {
					triggerName = triggerClass.Value;
					if (triggerClass.Value == hudObjectTriggerBuilder.TriggerClass) {
						triggerName = triggerClass.Key;
						break;
					}
				}

				triggerName = Regex.Replace(triggerName, "(B2D: )", "");
				triggerName = Regex.Replace(triggerName, "(Ext: )", "");
				triggerName = Regex.Replace(triggerName, "(\\B[A-Z])", " $1");

				SuperForms.BoxSubHeader(triggerName);

				hudObjectTriggerBuilder.TriggerClass = SuperForms.DictionaryDropDown(
					"Trigger Class",
					triggerClasses,
					hudObjectTriggerBuilder.TriggerClass
				);

				string selectedTriggerClass = triggerClasses.FirstOrDefault(pair => pair.Value == hudObjectTriggerBuilder.TriggerClass).Key;
				hudObjectTriggerBuilder.TriggerPropertyClass = triggerPropertyClasses[selectedTriggerClass + "Properties"];
				Type triggerPropertiesType = Type.GetType(triggerPropertyClasses[selectedTriggerClass + "Properties"]);

				object triggerProperties = JsonUtility.FromJson(hudObjectTriggerBuilder.TriggerProperties, triggerPropertiesType);

				if (triggerProperties != null) {
					FieldInfo[] fields = triggerProperties.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

					if (fields.Length > 0) {
						SuperForms.Space();

						foreach (FieldInfo field in fields) {
							field.SetValue(triggerProperties, FieldBasedForms.DrawFormField(field.Name, field.GetValue(triggerProperties)));
						}
					}

					hudObjectTriggerBuilder.TriggerProperties = JsonUtility.ToJson(triggerProperties);
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
					previousTriggerBuilderToDelete = hudObjectTriggerBuilder;
				}

				SuperForms.End.Vertical();
				SuperForms.Space();
			}

			if (previousTriggerBuilderToDelete != null) {
				element.PreviousItemTriggerBuilders.Remove(previousTriggerBuilderToDelete);
			}

			SuperForms.FullBoxLabel("Make Selection Condition");

			SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd,
				() => { element.MakeSelectionTriggerBuilders.Add(new HudObjectTriggerBuilder()); });

			foreach (HudObjectTriggerBuilder hudObjectTriggerBuilder in element.MakeSelectionTriggerBuilders) {
				SuperForms.Begin.VerticalSubBox();

				if (string.IsNullOrEmpty(hudObjectTriggerBuilder.TriggerClass)) {
					hudObjectTriggerBuilder.TriggerClass = triggerClasses.First().Value;
				}

				string triggerName = "TRIGGER NOT FOUND";

				foreach (KeyValuePair<string, string> triggerClass in triggerClasses) {
					triggerName = triggerClass.Value;
					if (triggerClass.Value == hudObjectTriggerBuilder.TriggerClass) {
						triggerName = triggerClass.Key;
						break;
					}
				}

				triggerName = Regex.Replace(triggerName, "(B2D: )", "");
				triggerName = Regex.Replace(triggerName, "(Ext: )", "");
				triggerName = Regex.Replace(triggerName, "(\\B[A-Z])", " $1");

				SuperForms.BoxSubHeader(triggerName);

				hudObjectTriggerBuilder.TriggerClass = SuperForms.DictionaryDropDown(
					"Trigger Class",
					triggerClasses,
					hudObjectTriggerBuilder.TriggerClass
				);

				string selectedTriggerClass = triggerClasses.FirstOrDefault(pair => pair.Value == hudObjectTriggerBuilder.TriggerClass).Key;
				hudObjectTriggerBuilder.TriggerPropertyClass = triggerPropertyClasses[selectedTriggerClass + "Properties"];
				Type triggerPropertiesType = Type.GetType(triggerPropertyClasses[selectedTriggerClass + "Properties"]);

				object triggerProperties = JsonUtility.FromJson(hudObjectTriggerBuilder.TriggerProperties, triggerPropertiesType);

				if (triggerProperties != null) {
					FieldInfo[] fields = triggerProperties.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

					if (fields.Length > 0) {
						SuperForms.Space();

						foreach (FieldInfo field in fields) {
							field.SetValue(triggerProperties, FieldBasedForms.DrawFormField(field.Name, field.GetValue(triggerProperties)));
						}
					}

					hudObjectTriggerBuilder.TriggerProperties = JsonUtility.ToJson(triggerProperties);
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
					makeSelectionTriggerBuilderToDelete = hudObjectTriggerBuilder;
				}

				SuperForms.End.Vertical();
				SuperForms.Space();
			}

			if (makeSelectionTriggerBuilderToDelete != null) {
				element.MakeSelectionTriggerBuilders.Remove(makeSelectionTriggerBuilderToDelete);
			}

			SuperForms.Space();

			SuperForms.BoxSubHeader("Sound Effects");
			element.SoundEffectOnItemChange = SuperForms.StringField("On Item Change", element.SoundEffectOnItemChange);
			element.SoundEffectOnItemSelect = SuperForms.StringField("On Item Select", element.SoundEffectOnItemSelect);

			return JsonUtility.ToJson(element);
		}

		public override void RenderPreview(int renderScale, string propertiesJson, Type propertiesType) {
			LoadData();

			ChoiceBox element = (ChoiceBox) JsonUtility.FromJson(propertiesJson, propertiesType);

			if (!_allTileSets.ContainsKey(element.Tileset)) {
				return;
			}

			TileSetData tilesetData = _allTileSets[element.Tileset];
			int tileSize = tilesetData.Properties.TileSize;
			float x = element.Position.x;
			float y = element.Position.y;
			int width = GetTotalWidthInTiles(element, tileSize) * tileSize;
			int height = GetTotalHeightInTiles(element, tileSize) * tileSize;

			if (element.OriginCorner == OriginCorner.TopRight || element.OriginCorner == OriginCorner.BottomRight) {
				x = GameProperties.RenderDimensionsWidth - width - element.Position.x;
			}

			if (element.OriginCorner == OriginCorner.BottomLeft || element.OriginCorner == OriginCorner.BottomRight) {
				y = GameProperties.RenderDimensionsHeight - height - element.Position.y;
			}

			bool stampsFound = BoomerangUtils.IndexInRange(tilesetData.Properties.Stamps, element.BackgroundStampIndex) &&
			                   BoomerangUtils.IndexInRange(tilesetData.Properties.Stamps, element.UnselectedStampIndex) &&
			                   BoomerangUtils.IndexInRange(tilesetData.Properties.Stamps, element.SelectedStampIndex);

			if (!stampsFound) {
				return;
			}

			Rect backgroundDimensions = new Rect(x, y, width, height);

			if (ShouldBuildTextures(element)) {
				BuildTextures(element, tilesetData);
			}

			if (ShouldBuildFontPreviewTexture()) {
				BuildFontPreviewTexture();
			}

			DrawBackground(backgroundDimensions, renderScale);
			DrawItems(element, backgroundDimensions, tileSize, renderScale);
			DrawTextPreviews(element, backgroundDimensions, tileSize, renderScale);
		}

		private int GetTotalHeightInTiles(ChoiceBox element, int tileSize) {
			int totalChoices = element.UsesContentId
				? element.PreviewChoiceCount
				: element.Choices.Count;

			int choicesHeight = totalChoices * tileSize;
			int backgroundPaddingHeight = element.BackgroundPaddingTop + element.BackgroundPaddingBottom;
			int spacingHeight = (totalChoices - 1) * element.ItemSpacing;
			int itemPaddingHeight = element.ItemPaddingTop;

			int totalHeight = choicesHeight + backgroundPaddingHeight + spacingHeight + itemPaddingHeight;

			return Mathf.CeilToInt(totalHeight / (float) tileSize);
		}

		private int GetTotalWidthInTiles(ChoiceBox element, int tileSize) {
			int backgroundPaddingWidth = element.BackgroundPaddingLeft + element.BackgroundPaddingRight;
			int itemWidth = element.ItemWidth * tileSize;

			int totalWidth = backgroundPaddingWidth + itemWidth;

			int x = Mathf.CeilToInt(totalWidth / (float) tileSize);

			return x;
		}

		private void DrawBackground(Rect dimensions, int renderScale) {
			if (_cachedEditorInfo != null && _cachedEditorInfo.CachedBackgroundTexture != null) {
				SuperForms.Texture(
					new Rect(
						dimensions.x * renderScale,
						dimensions.y * renderScale,
						dimensions.width * renderScale,
						dimensions.height * renderScale
					), _cachedEditorInfo.CachedBackgroundTexture
				);
			}
		}

		private void DrawItems(ChoiceBox element, Rect dimensions, int tileSize, int renderScale) {
			if (_cachedEditorInfo != null && _cachedEditorInfo.CachedUnSelectedItemTexture != null && _cachedEditorInfo.CachedSelectedItemTexture != null) {
				int totalChoices = element.UsesContentId
					? element.PreviewChoiceCount
					: element.Choices.Count;

				for (int i = 0; i < totalChoices; i++) {
					Texture2D textureToUse = element.ActiveIndex == i
						? _cachedEditorInfo.CachedSelectedItemTexture
						: _cachedEditorInfo.CachedUnSelectedItemTexture;

					int x = element.BackgroundPaddingLeft + (int) dimensions.x;
					int y = element.BackgroundPaddingTop + i * tileSize + i * element.ItemSpacing + (int) dimensions.y;


					SuperForms.Texture(
						new Rect(
							x * renderScale,
							y * renderScale,
							textureToUse.width * renderScale,
							textureToUse.height * renderScale
						), textureToUse
					);
				}
			}
		}

		private void DrawTextPreviews(ChoiceBox element, Rect dimensions, int tileSize, int renderScale) {
			int totalChoices = element.UsesContentId
				? element.PreviewChoiceCount
				: element.Choices.Count;

			for (int i = 0; i < totalChoices; i++) {
				int x = element.BackgroundPaddingLeft + element.ItemPaddingLeft + (int) dimensions.x;
				int y = element.BackgroundPaddingTop + i * tileSize + i * element.ItemSpacing + element.ItemPaddingTop + (int) dimensions.y;
				int width = element.ItemWidth * tileSize - element.ItemPaddingLeft - element.ItemPaddingRight;

				SuperForms.Texture(
					new Rect(
						x * renderScale,
						y * renderScale,
						width * renderScale,
						tileSize * renderScale
					), _fontPreviewTexture
				);
			}
		}

		private bool ShouldBuildTextures(ChoiceBox element) {
			int totalChoices = element.UsesContentId
				? element.PreviewChoiceCount
				: element.Choices.Count;

			if (totalChoices == 0) {
				return false;
			}

			return _cachedEditorInfo == null ||
			       _cachedEditorInfo.CachedBackgroundTexture == null ||
			       _cachedEditorInfo.Choices.Count != element.Choices.Count ||
			       _cachedEditorInfo.ActiveIndex != element.ActiveIndex ||
			       _cachedEditorInfo.Font != element.UnselectedFont ||
			       _cachedEditorInfo.Font != element.SelectedFont ||
			       _cachedEditorInfo.RowSpacing != element.ItemSpacing ||
			       _cachedEditorInfo.Width != element.ItemWidth ||
			       _cachedEditorInfo.BackgroundPaddingTop != element.BackgroundPaddingTop ||
			       _cachedEditorInfo.BackgroundPaddingRight != element.BackgroundPaddingRight ||
			       _cachedEditorInfo.BackgroundPaddingBottom != element.BackgroundPaddingBottom ||
			       _cachedEditorInfo.BackgroundPaddingLeft != element.BackgroundPaddingLeft ||
			       _cachedEditorInfo.ItemPaddingTop != element.ItemPaddingTop ||
			       _cachedEditorInfo.ItemPaddingRight != element.ItemPaddingRight ||
			       _cachedEditorInfo.ItemPaddingLeft != element.ItemPaddingLeft ||
			       _cachedEditorInfo.Tileset != element.Tileset ||
			       _cachedEditorInfo.BackgroundStampIndex != element.BackgroundStampIndex ||
			       _cachedEditorInfo.UnselectedStampIndex != element.UnselectedStampIndex ||
			       _cachedEditorInfo.SelectedStampIndex != element.SelectedStampIndex ||
			       _cachedEditorInfo.PreviewChoiceCount != element.PreviewChoiceCount;
		}

		private void BuildTextures(ChoiceBox element, TileSetData tilesetData) {
			int tileSize = tilesetData.Properties.TileSize;
			List<TilesetEditorStamp> stamps = tilesetData.Properties.Stamps;

			Color32[] missingTileGraphicPixelData = new Color32[tileSize * tileSize];
			for (int i = 0; i < missingTileGraphicPixelData.Length; i++) {
				missingTileGraphicPixelData[i] = new Color32(255, 0, 255, 255);
			}

			Color32[] transparentTileGraphicPixelData = new Color32[tileSize * tileSize];
			for (int i = 0; i < transparentTileGraphicPixelData.Length; i++) {
				transparentTileGraphicPixelData[i] = new Color32(0, 0, 0, 0);
			}

			Texture2D backgroundTexture = BuildTexture(
				BoomerangUtils.BuildMappedObjectForPlacedStamp(
					stamps[element.BackgroundStampIndex],
					GetTotalWidthInTiles(element, tileSize),
					GetTotalHeightInTiles(element, tileSize)
				),
				tilesetData,
				missingTileGraphicPixelData,
				transparentTileGraphicPixelData
			);

			Texture2D unselectedTexture = BuildTexture(
				BoomerangUtils.BuildMappedObjectForPlacedStamp(stamps[element.UnselectedStampIndex], element.ItemWidth, 1),
				tilesetData,
				missingTileGraphicPixelData,
				transparentTileGraphicPixelData
			);

			Texture2D selectedTexture = BuildTexture(
				BoomerangUtils.BuildMappedObjectForPlacedStamp(stamps[element.SelectedStampIndex], element.ItemWidth, 1),
				tilesetData,
				missingTileGraphicPixelData,
				transparentTileGraphicPixelData
			);

			_cachedEditorInfo = new ChoiceBoxCachedEditorInfo {
				Choices = element.Choices.Select(item => (string) item.Clone()).ToList(),
				ActiveIndex = element.ActiveIndex,
				Font = element.UnselectedFont,
				Width = element.ItemWidth,
				RowSpacing = element.ItemSpacing,
				BackgroundPaddingTop = element.BackgroundPaddingTop,
				BackgroundPaddingLeft = element.BackgroundPaddingLeft,
				BackgroundPaddingBottom = element.BackgroundPaddingBottom,
				BackgroundPaddingRight = element.BackgroundPaddingRight,
				ItemPaddingTop = element.ItemPaddingTop,
				ItemPaddingLeft = element.ItemPaddingLeft,
				ItemPaddingRight = element.ItemPaddingRight,
				Tileset = element.Tileset,
				BackgroundStampIndex = element.BackgroundStampIndex,
				UnselectedStampIndex = element.UnselectedStampIndex,
				SelectedStampIndex = element.SelectedStampIndex,
				PreviewChoiceCount = element.PreviewChoiceCount,
				CachedBackgroundTexture = backgroundTexture,
				CachedUnSelectedItemTexture = unselectedTexture,
				CachedSelectedItemTexture = selectedTexture
			};
		}

		private Texture2D BuildTexture(
			List<List<int>> mappedStampObject,
			TileSetData tilesetData,
			Color32[] missingTileGraphicPixelData,
			Color32[] transparentTileGraphicPixelData
		) {
			if (mappedStampObject.Count == 0) {
				return null;
			}

			int tileSize = tilesetData.Properties.TileSize;
			Vector2Int textureDimensions = new Vector2Int(mappedStampObject[0].Count * tileSize, mappedStampObject.Count * tileSize);
			Texture2D texture2D = new Texture2D(textureDimensions.x, textureDimensions.y) {
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
						i * tileSize,
						(mappedStampObject.Count - 1 - j) * tileSize,
						tileSize,
						tileSize,
						pixelDataToUse
					);
				}
			}

			texture2D.Apply();

			return texture2D;
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

#endif
	}
}