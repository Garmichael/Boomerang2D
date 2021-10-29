using System.Collections.Generic;
using System.Globalization;
using Boomerang2DFramework.Framework.BitmapFonts;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.GameFlagManagement;
using Boomerang2DFramework.Framework.MapGeneration;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.UiManagement.UiElements.Properties;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements.Behaviors {
	public class TextBoxLetter {
		public GameObject GameObject;
		public SpriteRenderer SpriteRenderer;
	}

	public class TextBoxTile {
		public GameObject GameObject;
		public TileBehavior TileBehavior;
	}

	public class TextBoxBehavior : UiElementBehavior {
		private TextBox MyProperties => (TextBox) Properties;
		private GameObject _backgroundContainer;
		private GameObject _textContainer;
		private GameObject _buttonPromptContainer;

		private Tileset _backgroundTileset;
		private Tileset _buttonPromptTileset;

		private readonly List<TextBoxLetter> _letterPool = new List<TextBoxLetter>();
		private readonly List<TextBoxTile> _backgroundPool = new List<TextBoxTile>();
		private readonly List<TextBoxTile> _buttonPromptPool = new List<TextBoxTile>();

		private string _lastTextContent;
		private bool _lastIsActiveState;

		public override void Initialize(
			UiElementProperties properties,
			string hudObjectParent,
			GameObject hudObjectContainer,
			HudObjectBehavior hudObjectBehavior,
			Rect hudDimensions,
			string contentId = ""
		) {
			base.Initialize(properties, hudObjectParent, hudObjectContainer, hudObjectBehavior, hudDimensions, contentId);

			_backgroundTileset = new Tileset(MyProperties.Tileset);

			if (MyProperties.UsesButtonPrompt) {
				_buttonPromptTileset = new Tileset(MyProperties.ButtonPromptTileset);
			}

			_lastIsActiveState = MyProperties.IsActive;

			SetupContainers();
		}

		public void Update() {
			string text = GetText();

			if (text != _lastTextContent || MyProperties.IsActive != _lastIsActiveState) {
				ResetPooledObjects();
				Render(HudDimensions, text);
				_lastTextContent = text;
				_lastIsActiveState = MyProperties.IsActive;
			}
		}

		private void ResetPooledObjects() {
			foreach (TextBoxLetter letter in _letterPool) {
				letter.SpriteRenderer.sprite = null;
			}

			foreach (TextBoxTile backgroundTile in _backgroundPool) {
				backgroundTile.TileBehavior.SetProperties(null, _backgroundTileset.TileSprites);
			}

			foreach (TextBoxTile buttonPromptTile in _buttonPromptPool) {
				buttonPromptTile.TileBehavior.SetProperties(null, _backgroundTileset.TileSprites);
			}
		}

		private void Render(Rect hudDimensions, string text) {
			Vector2Int elementDimensions = new Vector2Int(
				MyProperties.ScalesHorizontally
					? GetBackgroundMaxWidthInTiles(text)
					: MyProperties.MaxWidth,
				MyProperties.ScalesVertically
					? GetMaxHeightInTiles(text)
					: MyProperties.MaxHeight
			);

			Vector2 topLeft = GetTopLeft(elementDimensions, hudDimensions);

			DrawBackground(topLeft, elementDimensions);

			DisplayText(topLeft, text);

			if (MyProperties.UsesButtonPrompt) {
				DisplayButtonPrompt(topLeft, elementDimensions);
			}
		}

		private void SetupContainers() {
			_textContainer = new GameObject("Text");
			_textContainer.transform.parent = HudObjectContainer.transform;
			_textContainer.transform.localPosition = new Vector3(0, 0, 0.02f);

			_backgroundContainer = new GameObject("Background");
			_backgroundContainer.transform.parent = HudObjectContainer.transform;
			_backgroundContainer.transform.localPosition = new Vector3(0, 0, 0.03f);

			_buttonPromptContainer = new GameObject("Button Prompt");
			_buttonPromptContainer.transform.parent = HudObjectContainer.transform;
			_buttonPromptContainer.transform.localPosition = new Vector3(0, 0, 0.01f);
		}

		private string GetText() {
			string text = "";

			if (MyProperties.TextSource == TextSource.ContentDatabase) {
				string contentIdToUse = ContentId == ""
					? MyProperties.ContentId
					: ContentId;

				if (!UiManager.DialogContentStore.ContainsKey(contentIdToUse)) {
					MyProperties.TextSource = TextSource.StaticText;
				} else {
					if (MyProperties.MappedContentProperty == MappedProperty.Text) {
						text = UiManager.DialogContentStore[contentIdToUse].Text;
					} else if (MyProperties.MappedContentProperty == MappedProperty.SpeakerName) {
						text = UiManager.DialogContentStore[contentIdToUse].SpeakerName;
					} else if (MyProperties.MappedContentProperty == MappedProperty.DialogContentId) {
						text = UiManager.DialogContentStore[contentIdToUse].DialogContentId;
					}
				}
			}

			if (MyProperties.TextSource == TextSource.StaticText) {
				text = MyProperties.StaticText;
			} else if (MyProperties.TextSource == TextSource.GameFlagFloat) {
				text = GameFlags.GetFloatFlag(MyProperties.GameFlag).ToString(CultureInfo.InvariantCulture);
			} else if (MyProperties.TextSource == TextSource.GameFlagString) {
				text = GameFlags.GetStringFlag(MyProperties.GameFlag);
			} else if (MyProperties.TextSource == TextSource.GameFlagBool) {
				text = GameFlags.GetBoolFlag(MyProperties.GameFlag).ToString();
			}

			text = FormatTextWithBreaks(text);

			if (MyProperties.PrefixWithZeroes) {
				if (text.Length > MyProperties.FixedLengthOfText) {
					text = text.Substring(0, MyProperties.FixedLengthOfText);
				} else {
					while (text.Length < MyProperties.FixedLengthOfText) {
						text = "0" + text;
					}
				}
			}

			return text;
		}

		private string FormatTextWithBreaks(string text) {
			if (MyProperties.WordBreak == WordBreak.None || text == "") {
				return text;
			}

			string[] rows = text.Split((char) 10);
			int rowCount = rows.Length;

			for (int row = 0; row < rowCount; row++) {
				int breakPoint = GetCharacterCountThatFitsOnThisRow(rows[row]);

				if (breakPoint < rows[row].Length) {
					if (MyProperties.WordBreak == WordBreak.OnSpaces || MyProperties.WordBreak == WordBreak.OnSpecialCharacter) {
						int currentIndex;
						for (currentIndex = breakPoint; currentIndex >= 0; currentIndex--) {
							if (rows[row][currentIndex].ToString() == " ") {
								rows[row] = rows[row].Remove(currentIndex, 1);
								break;
							}

							if (MyProperties.WordBreak == WordBreak.OnSpecialCharacter &&
							    !char.IsLetterOrDigit(rows[row][currentIndex]) &&
							    currentIndex != breakPoint
							) {
								currentIndex++;
								break;
							}
						}

						breakPoint = currentIndex;
					}

					if (breakPoint > 0) {
						rows[row] = rows[row].Substring(0, breakPoint) + "\n" + rows[row].Substring(breakPoint);
						text = string.Join("\n", rows);
						rows = text.Split((char) 10);
						rowCount = rows.Length;
					}
				}
			}

			return string.Join("\n", rows);
		}

		private int GetCharacterCountThatFitsOnThisRow(string text) {
			int tileSize = _backgroundTileset.TilesetProperties.TileSize;
			string fontToUse = MyProperties.IsActive ? MyProperties.ActiveFont : MyProperties.Font;
			UiManager.BitmapFontDetails bitmapFontDetails = UiManager.Glyphs[fontToUse];
			int maxWidth = MyProperties.MaxWidth * tileSize - MyProperties.PaddingLeft - MyProperties.PaddingRight;
			int runningWidth = 0;
			int lastCharacterIndex = 0;
			char[] characters = text.ToCharArray();

			while (runningWidth < maxWidth && lastCharacterIndex < characters.Length) {
				char character = characters[lastCharacterIndex];
				lastCharacterIndex++;
				if (bitmapFontDetails.GlyphLookup.ContainsKey(character)) {
					int width = bitmapFontDetails.GlyphLookup[character].Width;
					runningWidth += width + MyProperties.LetterKerning;

					if (runningWidth > maxWidth) {
						lastCharacterIndex--;
					}
				}
			}

			return lastCharacterIndex;
		}

		private int GetBackgroundMaxWidthInTiles(string text) {
			int tileSize = _backgroundTileset.TilesetProperties.TileSize;
			int widthInPixels = GetTextRowLengthInPixels(text) + MyProperties.PaddingLeft + MyProperties.PaddingRight;
			int totalTiles = Mathf.CeilToInt(widthInPixels / (float) tileSize);

			return BoomerangUtils.ClampValue(totalTiles, MyProperties.MinWidth, MyProperties.MaxWidth);
		}

		private int GetTextRowLengthInPixels(string text) {
			string fontToUse = MyProperties.IsActive ? MyProperties.ActiveFont : MyProperties.Font;
			UiManager.BitmapFontDetails bitmapFontDetails = UiManager.Glyphs[fontToUse];
			string[] rows = text.Split((char) 10);
			int maxLength = 0;

			foreach (string row in rows) {
				int runningWidth = 0;

				foreach (char character in row) {
					if (bitmapFontDetails.GlyphLookup.ContainsKey(character)) {
						int width = bitmapFontDetails.GlyphLookup[character].Width;
						runningWidth += width + MyProperties.LetterKerning;
					}
				}

				if (runningWidth > maxLength) {
					maxLength = runningWidth;
				}
			}

			return maxLength;
		}

		private int GetMaxHeightInTiles(string text) {
			int numberOfRows = text.Split((char) 10).Length;
			string fontToUse = MyProperties.IsActive ? MyProperties.ActiveFont : MyProperties.Font;
			int fontHeight = UiManager.Glyphs[fontToUse].BitmapFontProperties.GlyphHeight;
			int totalHeightInCharacters = numberOfRows * fontHeight;
			int totalHeightInRowSpacing = (numberOfRows - 1) * MyProperties.RowSpacing;
			int heightInPixels = totalHeightInCharacters + totalHeightInRowSpacing + MyProperties.PaddingTop + MyProperties.PaddingBottom;
			int tileSize = _backgroundTileset.TilesetProperties.TileSize;
			int totalTiles = Mathf.CeilToInt(heightInPixels / (float) tileSize);
			return BoomerangUtils.ClampValue(totalTiles, MyProperties.MinHeight, MyProperties.MaxHeight);
		}

		private Vector2 GetTopLeft(Vector2Int elementDimensions, Rect hudDimensions) {
			int tileSize = _backgroundTileset.TilesetProperties.TileSize;

			float pixelDistanceHorizontal = BoomerangUtils.RoundToPixelPerfection(MyProperties.Position.x / GameProperties.PixelsPerUnit);
			float pixelDistanceVertical = BoomerangUtils.RoundToPixelPerfection(MyProperties.Position.y / GameProperties.PixelsPerUnit);

			float elementLeft = -hudDimensions.x - hudDimensions.width + pixelDistanceHorizontal;
			float elementTop = hudDimensions.height - hudDimensions.y - pixelDistanceVertical;

			bool isFromRight = MyProperties.OriginCorner == OriginCorner.TopRight || MyProperties.OriginCorner == OriginCorner.BottomRight;
			bool isFromBottom = MyProperties.OriginCorner == OriginCorner.BottomLeft || MyProperties.OriginCorner == OriginCorner.BottomRight;

			float elementWidth = elementDimensions.x * tileSize / GameProperties.PixelsPerUnit;
			float elementHeight = elementDimensions.y * tileSize / GameProperties.PixelsPerUnit;

			Vector2 topLeft = new Vector2(
				isFromRight
					? hudDimensions.width / 2 - pixelDistanceHorizontal - elementWidth
					: elementLeft,
				isFromBottom
					? -(hudDimensions.height / 2) + pixelDistanceVertical + elementHeight
					: elementTop
			);

			return topLeft;
		}

		private void DrawBackground(Vector2 origin, Vector2Int dimensions) {
			int tileSize = _backgroundTileset.TilesetProperties.TileSize;
			int stampToUse = MyProperties.IsActive ? MyProperties.ActiveBackgroundStampIndex : MyProperties.BackgroundStampIndex;
			TilesetEditorStamp stamp = _backgroundTileset.TilesetProperties.Stamps[stampToUse];
			List<List<int>> parsedStamp = BoomerangUtils.BuildMappedObjectForPlacedStamp(stamp, dimensions.x, dimensions.y);

			FillBackgroundPool(dimensions.x * dimensions.y);

			int index = 0;
			for (int j = 0; j < parsedStamp.Count; j++) {
				List<int> row = parsedStamp[j];

				for (int i = 0; i < row.Count; i++) {
					int tileId = row[i];

					if (tileId > 0 && _backgroundTileset.TilesetProperties.TilesLookup.ContainsKey(tileId)) {
						float xOffset = BoomerangUtils.RoundToPixelPerfection(i * (float) tileSize / GameProperties.PixelsPerUnit);
						float yOffset = BoomerangUtils.RoundToPixelPerfection((j + 1) * (float) tileSize / GameProperties.PixelsPerUnit);

						_backgroundPool[index].TileBehavior.SetProperties(
							_backgroundTileset.TilesetProperties.TilesLookup[tileId],
							_backgroundTileset.TileSprites
						);

						_backgroundPool[index].GameObject.transform.localPosition = new Vector3(origin.x + xOffset, origin.y - yOffset, 0);
					}

					index++;
				}
			}
		}

		private void FillBackgroundPool(int size) {
			while (size >= _backgroundPool.Count) {
				GameObject tileGameObject = new GameObject {name = "BackgroundTile"};
				tileGameObject.transform.parent = _backgroundContainer.transform;
				TileBehavior tileBehavior = tileGameObject.AddComponent<TileBehavior>();
				tileBehavior.SetProperties(null, _backgroundTileset.TileSprites);

				GameObject sprite = new GameObject {name = "Sprite"};
				sprite.transform.parent = tileGameObject.transform;
				sprite.transform.localPosition = new Vector3(
					_backgroundTileset.TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2,
					_backgroundTileset.TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2,
					0f
				);

				sprite.AddComponent<SpriteRenderer>();

				_backgroundPool.Add(new TextBoxTile {
					GameObject = tileGameObject,
					TileBehavior = tileBehavior
				});
			}
		}

		private void DisplayText(Vector2 origin, string text) {
			bool isFirstCharacter = true;
			double runningWidth = 0;
			double runningTop = 0;
			double pixelSize = GameProperties.PixelSize;
			string fontToUse = MyProperties.IsActive ? MyProperties.ActiveFont : MyProperties.Font;
			UiManager.BitmapFontDetails bitmapFontDetails = UiManager.Glyphs[fontToUse];
			BitmapFontProperties bitmapFontProperties = bitmapFontDetails.BitmapFontProperties;
			int glyphHeight = bitmapFontProperties.GlyphHeight;

			origin.x += MyProperties.PaddingLeft * (float) pixelSize;
			origin.y -= MyProperties.PaddingTop * (float) pixelSize;

			FillTextPool(text.Length);

			for (int index = 0; index < text.Length; index++) {
				char character = text[index];

				if (character != 10 && !UiManager.Glyphs[fontToUse].GlyphLookup.ContainsKey(character)) {
					Debug.Log("Character not found: " + character + " | " + (int) character);
					continue;
				}

				if (character == 10) {
					runningTop -= (glyphHeight + MyProperties.RowSpacing) * pixelSize;
					runningWidth = 0;
					continue;
				}

				int spriteIndex = bitmapFontDetails.GlyphLookup[character].SpriteIndex;
				int width = bitmapFontDetails.GlyphLookup[character].Width;

				if (isFirstCharacter) {
					origin.x += width * (float) pixelSize / 2;
					origin.y -= glyphHeight * (float) pixelSize / 2;
					isFirstCharacter = false;
				}

				DisplayLetter(spriteIndex, origin, runningWidth, runningTop, index);

				runningWidth += pixelSize * (width + MyProperties.LetterKerning);
			}
		}

		private void FillTextPool(int size) {
			while (size >= _letterPool.Count) {
				GameObject glyphGameObject = new GameObject("GlyphCharacter");
				SpriteRenderer spriteRenderer = glyphGameObject.AddComponent<SpriteRenderer>();
				glyphGameObject.transform.parent = _textContainer.transform;

				_letterPool.Add(new TextBoxLetter {
					GameObject = glyphGameObject,
					SpriteRenderer = spriteRenderer
				});
			}
		}

		private void DisplayLetter(int spriteIndex, Vector2 origin, double width, double top, int letterIndex) {
			if (_textContainer == null) {
				return;
			}

			GameObject letter = _letterPool[letterIndex].GameObject;
			SpriteRenderer letterSpriteRenderer = _letterPool[letterIndex].SpriteRenderer;
			letter.transform.localPosition = new Vector3(origin.x + (float) width, origin.y + (float) top);
			string fontToUse = MyProperties.IsActive ? MyProperties.ActiveFont : MyProperties.Font;
			Sprite sprite = BoomerangDatabase.BitmapFontSpriteEntries[fontToUse][spriteIndex];
			letterSpriteRenderer.sprite = sprite;
		}

		private void FillButtonPromptPool(int size) {
			while (size >= _buttonPromptPool.Count) {
				GameObject buttonTileGameObject = new GameObject {name = "ButtonPromptTile"};
				buttonTileGameObject.transform.parent = _buttonPromptContainer.transform;
				TileBehavior tileBehavior = buttonTileGameObject.AddComponent<TileBehavior>();
				tileBehavior.SetProperties(null, _backgroundTileset.TileSprites);

				GameObject sprite = new GameObject {name = "Sprite"};
				sprite.transform.parent = buttonTileGameObject.transform;
				sprite.transform.localPosition = new Vector3(
					_backgroundTileset.TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2,
					_backgroundTileset.TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2,
					0f
				);

				sprite.AddComponent<SpriteRenderer>();

				_buttonPromptPool.Add(new TextBoxTile {
					GameObject = buttonTileGameObject,
					TileBehavior = tileBehavior
				});
			}
		}

		private void DisplayButtonPrompt(Vector2 origin, Vector2 containerDimensions) {
			if (_buttonPromptContainer == null) {
				return;
			}

			int tileSize = _buttonPromptTileset.TilesetProperties.TileSize;
			int containerTileSize = _backgroundTileset.TilesetProperties.TileSize;

			List<List<int>> parsedStamp = BoomerangUtils.BuildMappedObjectForPlacedStamp(
				_buttonPromptTileset.TilesetProperties.Stamps[MyProperties.ButtonPromptStampIndex],
				MyProperties.ButtonPromptSize.x, MyProperties.ButtonPromptSize.y
			);

			FillButtonPromptPool(MyProperties.ButtonPromptSize.x * MyProperties.ButtonPromptSize.y);

			if (MyProperties.HudElementContainer == HudElementContainer.Screen) {
				origin = new Vector2(-GameProperties.UnitsWide / 2, GameProperties.UnitsHigh / 2);
				containerDimensions = new Vector2(GameProperties.UnitsWide, GameProperties.UnitsHigh);
				containerTileSize = (int) GameProperties.PixelsPerUnit;
			}

			Vector2 buttonPosition = origin;

			float pixelDistanceHorizontal = BoomerangUtils.RoundToPixelPerfection(MyProperties.ButtonPromptPosition.x / GameProperties.PixelsPerUnit);
			float pixelDistanceVertical = BoomerangUtils.RoundToPixelPerfection(MyProperties.ButtonPromptPosition.y / GameProperties.PixelsPerUnit);

			float containerWidth = BoomerangUtils.RoundToPixelPerfection(containerDimensions.x * containerTileSize / GameProperties.PixelsPerUnit);
			float containerHeight = BoomerangUtils.RoundToPixelPerfection(containerDimensions.y * containerTileSize / GameProperties.PixelsPerUnit);

			float buttonWidth = BoomerangUtils.RoundToPixelPerfection(MyProperties.ButtonPromptSize.x * tileSize / GameProperties.PixelsPerUnit);
			float buttonHeight = BoomerangUtils.RoundToPixelPerfection(MyProperties.ButtonPromptSize.y * tileSize / GameProperties.PixelsPerUnit);

			bool isFromRight = MyProperties.ButtonPromptOriginCorner == OriginCorner.TopRight ||
			                   MyProperties.ButtonPromptOriginCorner == OriginCorner.BottomRight;

			bool isFromLeft = MyProperties.ButtonPromptOriginCorner == OriginCorner.BottomLeft ||
			                  MyProperties.ButtonPromptOriginCorner == OriginCorner.BottomRight;

			buttonPosition.x = isFromRight
				? origin.x + containerWidth - buttonWidth - pixelDistanceHorizontal
				: origin.x + pixelDistanceHorizontal;

			buttonPosition.y = isFromLeft
				? origin.y - containerHeight + buttonHeight + pixelDistanceVertical
				: origin.y - pixelDistanceVertical;

			float tileOffset = BoomerangUtils.RoundToPixelPerfection(tileSize / GameProperties.PixelsPerUnit);

			int index = 0;
			for (int j = 0; j < parsedStamp.Count; j++) {
				List<int> row = parsedStamp[j];

				for (int i = 0; i < row.Count; i++) {
					int tileId = row[i];

					_buttonPromptPool[index].TileBehavior.SetProperties(
						_buttonPromptTileset.TilesetProperties.TilesLookup[tileId],
						_buttonPromptTileset.TileSprites
					);

					Vector3 position = new Vector3(buttonPosition.x + i * tileOffset, buttonPosition.y - (j + 1) * tileOffset, 0f);
					_buttonPromptPool[index].GameObject.transform.parent = _buttonPromptContainer.transform;
					_buttonPromptPool[index].GameObject.transform.localPosition = position;

					index++;
				}
			}
		}
	}
}