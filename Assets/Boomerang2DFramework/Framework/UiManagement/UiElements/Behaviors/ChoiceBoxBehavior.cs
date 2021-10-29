using System.Collections.Generic;
using Boomerang2DFramework.Framework.AudioManagement;
using Boomerang2DFramework.Framework.BitmapFonts;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.MapGeneration;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.UiManagement.TriggerSystem;
using Boomerang2DFramework.Framework.UiManagement.UiElements.Properties;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements.Behaviors {
	public class ChoiceBoxTile {
		public GameObject GameObject;
		public TileBehavior TileBehavior;
	}

	public class ChoiceBoxLetter {
		public GameObject GameObject;
		public SpriteRenderer SpriteRenderer;
	}

	public class ChoiceBoxBehavior : UiElementBehavior {
		private ChoiceBox MyProperties => (ChoiceBox) Properties;

		private Tileset _tileset;

		private readonly List<ChoiceBoxTile> _pooledBackgroundTiles = new List<ChoiceBoxTile>();
		private readonly List<ChoiceBoxTile> _pooledChoiceTiles = new List<ChoiceBoxTile>();
		private readonly List<ChoiceBoxLetter> _letterPool = new List<ChoiceBoxLetter>();

		private GameObject _backgroundContainer;
		private List<GameObject> _choiceContainers;

		private int _totalChoiceCount;
		private int _lastActiveIndex = -1;

		private int _totalChoices;
		private List<string> _choiceTexts;

		private readonly List<HudObjectTrigger> _nextItemBuiltTriggers = new List<HudObjectTrigger>();
		private readonly List<HudObjectTrigger> _previousItemBuiltTriggers = new List<HudObjectTrigger>();
		private readonly List<HudObjectTrigger> _makeSelectionBuiltTriggers = new List<HudObjectTrigger>();

		public override void Initialize(
			UiElementProperties properties,
			string hudObjectParent,
			GameObject hudObjectContainer,
			HudObjectBehavior hudObjectBehavior,
			Rect hudDimensions,
			string contentId = ""
		) {
			base.Initialize(properties, hudObjectParent, hudObjectContainer, hudObjectBehavior, hudDimensions, contentId);

			_tileset = new Tileset(MyProperties.Tileset);

			if (contentId != "") {
				MyProperties.ContentId = contentId;
			}

			if (MyProperties.UsesContentId) {
				if (UiManager.DialogContentStore.ContainsKey(MyProperties.ContentId)) {
					_choiceTexts = UiManager.DialogContentStore[MyProperties.ContentId].Choices;
				} else {
					_choiceTexts = new List<string> {
						"Invalid", "ContentId", "Selected"
					};
				}
			} else {
				_choiceTexts = MyProperties.Choices;
			}

			_totalChoices = _choiceTexts.Count;

			BuildItemSelectionTriggers();
			SetUpContainers();
		}

		private void BuildItemSelectionTriggers() {
			foreach (HudObjectTriggerBuilder hudObjectTriggerBuilder in MyProperties.NextItemTriggerBuilders) {
				_nextItemBuiltTriggers.Add(hudObjectTriggerBuilder.BuildTrigger());
			}

			foreach (HudObjectTriggerBuilder hudObjectTriggerBuilder in MyProperties.PreviousItemTriggerBuilders) {
				_previousItemBuiltTriggers.Add(hudObjectTriggerBuilder.BuildTrigger());
			}

			foreach (HudObjectTriggerBuilder hudObjectTriggerBuilder in MyProperties.MakeSelectionTriggerBuilders) {
				_makeSelectionBuiltTriggers.Add(hudObjectTriggerBuilder.BuildTrigger());
			}
		}

		private void SetUpContainers() {
			_backgroundContainer = new GameObject("Container");
			_backgroundContainer.transform.parent = HudObjectContainer.transform;
			_backgroundContainer.transform.localPosition = Vector3.zero;

			_choiceContainers = new List<GameObject>();

			for (int i = 0; i < _totalChoices; i++) {
				GameObject choiceContainer = new GameObject("Choice Container");
				choiceContainer.transform.parent = _backgroundContainer.transform;
				_backgroundContainer.transform.localPosition = Vector3.zero;

				_choiceContainers.Add(choiceContainer);
			}
		}

		public void Update() {
			ProcessItemSelectionTriggers();

			if (_lastActiveIndex != MyProperties.ActiveIndex) {
				ResetPooledObjects();
				Render();
				_lastActiveIndex = MyProperties.ActiveIndex;
			}
		}

		private void ProcessItemSelectionTriggers() {
			bool nextItemSelectionTriggersMet = true;
			int previousActiveIndex = MyProperties.ActiveIndex;

			foreach (HudObjectTrigger uiElementTrigger in _nextItemBuiltTriggers) {
				if (!uiElementTrigger.IsTriggered(HudObjectBehavior)) {
					nextItemSelectionTriggersMet = false;
				}
			}

			if (_nextItemBuiltTriggers.Count > 0 && nextItemSelectionTriggersMet) {
				MyProperties.ActiveIndex++;
			}

			bool previousItemSelectionTriggersMet = true;
			foreach (HudObjectTrigger uiElementTrigger in _previousItemBuiltTriggers) {
				if (!uiElementTrigger.IsTriggered(HudObjectBehavior)) {
					previousItemSelectionTriggersMet = false;
				}
			}

			if (_previousItemBuiltTriggers.Count > 0 && previousItemSelectionTriggersMet) {
				MyProperties.ActiveIndex--;
			}

			if (MyProperties.AllowLooping) {
				if (MyProperties.ActiveIndex >= _totalChoices) {
					MyProperties.ActiveIndex = 0;
				} else if (MyProperties.ActiveIndex < 0) {
					MyProperties.ActiveIndex = _totalChoices - 1;
				}
			} else {
				MyProperties.ActiveIndex = BoomerangUtils.ClampValue(MyProperties.ActiveIndex, 0, _totalChoices - 1);
			}

			if (previousActiveIndex != MyProperties.ActiveIndex) {
				if (MyProperties.SoundEffectOnItemChange != "") {
					AudioManager.PlayOnce(MyProperties.SoundEffectOnItemChange);
				}
			}

			bool makeSelectionTriggersMet = true;
			foreach (HudObjectTrigger uiElementTrigger in _makeSelectionBuiltTriggers) {
				if (!uiElementTrigger.IsTriggered(HudObjectBehavior)) {
					makeSelectionTriggersMet = false;
				}
			}

			if (_makeSelectionBuiltTriggers.Count > 0 && makeSelectionTriggersMet) {
				if (MyProperties.SoundEffectOnItemSelect != "") {
					AudioManager.PlayOnce(MyProperties.SoundEffectOnItemSelect);
				}

				UiManager.RemoveHudObject(HudObjectParent, MyProperties.ActiveIndex);
			}
		}

		private void ResetPooledObjects() {
			foreach (ChoiceBoxTile tile in _pooledBackgroundTiles) {
				tile.TileBehavior.SetProperties(null, _tileset.TileSprites);
			}

			foreach (ChoiceBoxTile tile in _pooledChoiceTiles) {
				tile.TileBehavior.SetProperties(null, _tileset.TileSprites);
			}

			foreach (ChoiceBoxLetter letter in _letterPool) {
				letter.SpriteRenderer.sprite = null;
			}
		}

		private void Render() {
			DrawBackground();
			DrawChoiceItems();
			DrawText();
			PositionElement();
		}

		private void DrawBackground() {
			int tileSize = _tileset.TilesetProperties.TileSize;
			Vector2Int dimensions = new Vector2Int(
				GetTotalWidthInTiles(),
				GetTotalHeightInTiles()
			);

			TilesetEditorStamp stamp = _tileset.TilesetProperties.Stamps[MyProperties.BackgroundStampIndex];
			List<List<int>> parsedStamp = BoomerangUtils.BuildMappedObjectForPlacedStamp(stamp, dimensions.x, dimensions.y);

			FillTilePool(dimensions.x * dimensions.y, _pooledBackgroundTiles);

			int poolIndex = 0;
			for (int j = 0; j < parsedStamp.Count; j++) {
				List<int> row = parsedStamp[j];

				for (int i = 0; i < row.Count; i++) {
					int tileId = row[i];

					if (tileId > 0 && _tileset.TilesetProperties.TilesLookup.ContainsKey(tileId)) {
						float xOffset = BoomerangUtils.RoundToPixelPerfection(i * (float) tileSize / GameProperties.PixelsPerUnit);
						float yOffset = -BoomerangUtils.RoundToPixelPerfection((j + 1) * (float) tileSize / GameProperties.PixelsPerUnit);

						_pooledBackgroundTiles[poolIndex].TileBehavior.SetProperties(
							_tileset.TilesetProperties.TilesLookup[tileId],
							_tileset.TileSprites
						);

						_pooledBackgroundTiles[poolIndex].GameObject.transform.parent = _backgroundContainer.transform;
						_pooledBackgroundTiles[poolIndex].GameObject.transform.localPosition = new Vector3(xOffset, yOffset, 0);
					}

					poolIndex++;
				}
			}
		}

		private void DrawChoiceItems() {
			FillTilePool(_totalChoices * MyProperties.ItemWidth, _pooledChoiceTiles);

			int tileSize = _tileset.TilesetProperties.TileSize;
			int poolIndex = 0;

			TilesetEditorStamp selectedStamp = _tileset.TilesetProperties.Stamps[MyProperties.SelectedStampIndex];
			List<List<int>> parsedSelectedStamp = BoomerangUtils.BuildMappedObjectForPlacedStamp(selectedStamp, MyProperties.ItemWidth, 1);

			TilesetEditorStamp unselectedStamp = _tileset.TilesetProperties.Stamps[MyProperties.UnselectedStampIndex];
			List<List<int>> parsedUnselectedStamp = BoomerangUtils.BuildMappedObjectForPlacedStamp(unselectedStamp, MyProperties.ItemWidth, 1);

			for (int choiceIndex = 0; choiceIndex < _totalChoices; choiceIndex++) {
				List<List<int>> mapToUse = MyProperties.ActiveIndex == choiceIndex
					? parsedSelectedStamp
					: parsedUnselectedStamp;

				for (int j = 0; j < mapToUse.Count; j++) {
					List<int> row = mapToUse[j];

					for (int i = 0; i < row.Count; i++) {
						int tileId = row[i];

						if (tileId > 0 && _tileset.TilesetProperties.TilesLookup.ContainsKey(tileId)) {
							float xOffset = BoomerangUtils.RoundToPixelPerfection(i * (float) tileSize / GameProperties.PixelsPerUnit);
							float yOffset = -BoomerangUtils.RoundToPixelPerfection((j + 1) * (float) tileSize / GameProperties.PixelsPerUnit);

							_pooledChoiceTiles[poolIndex].TileBehavior.SetProperties(
								_tileset.TilesetProperties.TilesLookup[tileId],
								_tileset.TileSprites
							);

							_pooledChoiceTiles[poolIndex].GameObject.transform.parent = _choiceContainers[choiceIndex].transform;
							_pooledChoiceTiles[poolIndex].GameObject.transform.localPosition = new Vector3(xOffset, yOffset, 0);
						}

						poolIndex++;
					}
				}

				float y = tileSize * choiceIndex;
				y += choiceIndex * MyProperties.ItemSpacing;
				y += MyProperties.BackgroundPaddingTop;

				_choiceContainers[choiceIndex].transform.localPosition = new Vector3(
					MyProperties.BackgroundPaddingLeft * GameProperties.PixelSize,
					-y * GameProperties.PixelSize,
					-0.1f
				);
			}
		}

		private void FillTilePool(int size, List<ChoiceBoxTile> collection) {
			while (size >= collection.Count) {
				GameObject tileGameObject = new GameObject {name = "PooledTile"};
				tileGameObject.transform.parent = HudObjectContainer.transform;
				TileBehavior tileBehavior = tileGameObject.AddComponent<TileBehavior>();
				tileBehavior.SetProperties(null, _tileset.TileSprites);

				GameObject sprite = new GameObject {name = "Sprite"};
				sprite.transform.parent = tileGameObject.transform;
				sprite.transform.localPosition = new Vector3(
					_tileset.TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2,
					_tileset.TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2,
					0f
				);

				sprite.AddComponent<SpriteRenderer>();

				collection.Add(new ChoiceBoxTile {
					GameObject = tileGameObject,
					TileBehavior = tileBehavior
				});
			}
		}

		private void DrawText() {
			float runningWidth = 0;
			int poolIndex = 0;
			int totalGlyphs = 0;

			foreach (string text in _choiceTexts) {
				totalGlyphs += text.Length;
			}

			FillTextPool(totalGlyphs);

			for (int choiceIndex = 0; choiceIndex < _choiceTexts.Count; choiceIndex++) {
				string text = _choiceTexts[choiceIndex];

				string fontToUse = choiceIndex == MyProperties.ActiveIndex
					? MyProperties.SelectedFont
					: MyProperties.UnselectedFont;

				UiManager.BitmapFontDetails bitmapFontDetails = UiManager.Glyphs[fontToUse];
				BitmapFontProperties bitmapFontProperties = bitmapFontDetails.BitmapFontProperties;
				int glyphHeight = bitmapFontProperties.GlyphHeight;

				foreach (char character in text) {
					if (!UiManager.Glyphs[fontToUse].GlyphLookup.ContainsKey(character)) {
						Debug.Log("Character not found: " + character + " | " + (int) character);
						continue;
					}

					int spriteIndex = bitmapFontDetails.GlyphLookup[character].SpriteIndex;
					int width = bitmapFontDetails.GlyphLookup[character].Width;

					DisplayLetter(
						spriteIndex,
						runningWidth + width / 2f * GameProperties.PixelSize + MyProperties.ItemPaddingLeft * GameProperties.PixelSize,
						-(glyphHeight / 2f * GameProperties.PixelSize + MyProperties.ItemPaddingTop * GameProperties.PixelSize),
						poolIndex,
						choiceIndex,
						fontToUse
					);

					runningWidth += (width + MyProperties.LetterKerning) * GameProperties.PixelSize;
					poolIndex++;
				}

				runningWidth = 0;
			}
		}

		private void DisplayLetter(int spriteIndex, float xOffset, float yOffset, int poolIndex, int parentChoiceIndex, string font) {
			GameObject letter = _letterPool[poolIndex].GameObject;
			SpriteRenderer letterSpriteRenderer = _letterPool[poolIndex].SpriteRenderer;
			letter.transform.parent = _choiceContainers[parentChoiceIndex].transform;
			letter.transform.localPosition = new Vector3(xOffset, yOffset, -0.1f);
			Sprite sprite = BoomerangDatabase.BitmapFontSpriteEntries[font][spriteIndex];
			letterSpriteRenderer.sprite = sprite;
		}

		private void FillTextPool(int size) {
			while (size >= _letterPool.Count) {
				GameObject glyphGameObject = new GameObject("GlyphCharacter");
				SpriteRenderer spriteRenderer = glyphGameObject.AddComponent<SpriteRenderer>();
				glyphGameObject.transform.parent = HudObjectContainer.transform;

				_letterPool.Add(new ChoiceBoxLetter {
					GameObject = glyphGameObject,
					SpriteRenderer = spriteRenderer
				});
			}
		}

		private void PositionElement() {
			float x = HudDimensions.x + MyProperties.Position.x * GameProperties.PixelSize;
			float y = HudDimensions.y - MyProperties.Position.y * GameProperties.PixelSize;

			if (MyProperties.OriginCorner == OriginCorner.TopRight || MyProperties.OriginCorner == OriginCorner.BottomRight) {
				x = HudDimensions.width / 2 -
				    MyProperties.Position.x * GameProperties.PixelSize -
				    GetTotalWidthInTiles() * _tileset.TilesetProperties.TileSize * GameProperties.PixelSize;
			}

			if (MyProperties.OriginCorner == OriginCorner.BottomLeft || MyProperties.OriginCorner == OriginCorner.BottomRight) {
				y = -HudDimensions.height / 2 +
				    MyProperties.Position.y * GameProperties.PixelSize +
				    GetTotalHeightInTiles() * _tileset.TilesetProperties.TileSize * GameProperties.PixelSize;
			}

			_backgroundContainer.transform.localPosition = new Vector3(x, y, _backgroundContainer.transform.localPosition.z);
		}

		private int GetTotalHeightInTiles() {
			int tileSize = _tileset.TilesetProperties.TileSize;
			int choicesHeight = _totalChoices * tileSize;
			int backgroundPaddingHeight = MyProperties.BackgroundPaddingTop + MyProperties.BackgroundPaddingBottom;
			int spacingHeight = (_totalChoices - 1) * MyProperties.ItemSpacing;
			int itemPaddingHeight = MyProperties.ItemPaddingTop;

			int totalHeight = choicesHeight + backgroundPaddingHeight + spacingHeight + itemPaddingHeight;

			return Mathf.CeilToInt(totalHeight / (float) tileSize);
		}

		private int GetTotalWidthInTiles() {
			int tileSize = _tileset.TilesetProperties.TileSize;
			int backgroundPaddingWidth = MyProperties.BackgroundPaddingLeft + MyProperties.BackgroundPaddingRight;
			int itemWidth = MyProperties.ItemWidth * tileSize;

			int totalWidth = backgroundPaddingWidth + itemWidth;

			int x = Mathf.CeilToInt(totalWidth / (float) tileSize);

			return x;
		}
	}
}