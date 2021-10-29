using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Boomerang2DFramework.Framework.BitmapFonts;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.BitmapFontEditor {
	public class BitmapFontEditor : EditorWindow {
		private static BitmapFontEditor _window;

		private static float _windowWidth;
		private static float _windowHeight;

		private float _repaintTime;
		private bool _repaintNext;
		private float _totalTime;
		private float _timeToCheckSave;
		private bool _fileHasChanged;

		private enum Mode {
			Normal,
			Create,
			Rename,
			Delete
		}

		private Mode _mainMode;
		private string _inputName;
		private int _atlasScale = 1;
		private Texture2D _bitmapFontAtlas;

		private int _selectedBitmapFontIndex;
		private int _previousBitmapFontIndex;
		private readonly List<BitmapFontProperties> _allBitmapFonts = new List<BitmapFontProperties>();

		private BitmapFontProperties ActiveBitmapFont => BoomerangUtils.IndexInRange(_allBitmapFonts, _selectedBitmapFontIndex)
			? _allBitmapFonts[_selectedBitmapFontIndex]
			: null;

		private List<string> AllBitmapFontNames {
			get {
				List<string> tileSetNames = new List<string>();
				foreach (BitmapFontProperties bitmapFontProperties in _allBitmapFonts) {
					tileSetNames.Add(bitmapFontProperties.Name);
				}

				return tileSetNames;
			}
		}

		private readonly Dictionary<int, string> _characterMap = new Dictionary<int, string>();

		private List<string> AllCharacters {
			get {
				List<string> allCharacters = new List<string>();
				foreach (KeyValuePair<int, string> character in _characterMap) {
					allCharacters.Add(character.Value);
				}

				return allCharacters;
			}
		}

		private int _defaultWidth = 16;
		private bool _predictNextPosition = true;
		private bool _predictNextCharacter = true;
		private int _selectedGlyphIndex;

		private BitmapFontGlyphProperties ActiveGlyph =>
			ActiveBitmapFont.Glyphs != null &&
			BoomerangUtils.IndexInRange(ActiveBitmapFont.Glyphs, _selectedGlyphIndex)
				? ActiveBitmapFont.Glyphs[_selectedGlyphIndex]
				: null;

		private bool _selectingGlyphCharacterMode;

		[MenuItem("Tools/Boomerang2D/Bitmap Font Editor", false, 251)]
		public static void ShowWindow() {
			_window = (BitmapFontEditor) GetWindow(typeof(BitmapFontEditor), false, "Bitmap Font Editor");
			_window.minSize = new Vector2(600, 400);
			_window.Show();
			_window.autoRepaintOnSceneChange = true;

			_window.titleContent = new GUIContent(
				"Bitmap Font Editor",
				AssetDatabase.LoadAssetAtPath<Texture>("Assets/Boomerang2DFramework/Framework/EditorAssets/editorWindowIconBitmapFontStudio.png")
			);
		}

		private void Update() {
			_totalTime = Time.realtimeSinceStartup;
			UpdateRepaint();
			UpdateCheckSaveTime();
		}

		private void UpdateCheckSaveTime() {
			if (ActiveBitmapFont == null) {
				return;
			}

			if (_totalTime > _timeToCheckSave + 1) {
				_timeToCheckSave = _totalTime;
				string originalJson = BoomerangDatabase.BitmapFontJsonEntries[ActiveBitmapFont.Name].text;
				_fileHasChanged = originalJson != JsonUtility.ToJson(ActiveBitmapFont, true);
			}
		}

		private void UpdateRepaint() {
			const float fps = 60f;

			if (_repaintTime > _totalTime + 1f) {
				_repaintTime = 0f;
			}

			if (_repaintTime + 1.0f / fps < _totalTime || _repaintNext) {
				_repaintTime = _totalTime;
				_repaintNext = false;
				Repaint();
			}
		}

		private void OnEnable() {
			_selectedGlyphIndex = -1;
			_selectingGlyphCharacterMode = false;

			LoadData();
		}

		private void LoadData() {
			_allBitmapFonts.Clear();

			foreach (KeyValuePair<string, TextAsset> bitmapFontJson in BoomerangDatabase.BitmapFontJsonEntries) {
				BitmapFontProperties bitmapFontProperties = JsonUtility.FromJson<BitmapFontProperties>(bitmapFontJson.Value.text);
				_allBitmapFonts.Add(bitmapFontProperties);
			}

			_characterMap.Clear();

			for (int i = 32; i <= 255; i++) {
				if (i < 128 || i > 160) {
					string characterToAdd = ((char) i).ToString();
					_characterMap.Add(i, characterToAdd);
				}
			}
		}

		private void SaveBitmapFont() {
			if (ActiveBitmapFont == null) {
				return;
			}

			_fileHasChanged = false;

			int spriteIndex = 0;
			List<Rect> regions = new List<Rect>();
			foreach (BitmapFontGlyphProperties glyph in ActiveBitmapFont.Glyphs) {
				regions.Add(new Rect(
					glyph.X,
					_bitmapFontAtlas.height - glyph.Row * ActiveBitmapFont.GlyphHeight - ActiveBitmapFont.GlyphHeight,
					glyph.Width,
					ActiveBitmapFont.GlyphHeight
				));

				glyph.SpriteIndex = spriteIndex;
				spriteIndex++;
			}

			if (regions.Count > 0) {
				SpriteSlicer.SliceRegions(_bitmapFontAtlas, regions);
			}

			File.WriteAllText(GameProperties.BitmapFontsContentDirectory + "/" + ActiveBitmapFont.Name + ".json", JsonUtility.ToJson(ActiveBitmapFont, true));

			AssetDatabase.Refresh();
			BoomerangDatabase.PopulateDatabase();

			OnEnable();
		}

		private void ImportImage(string importedImagePath) {
			string fileExtension = "." + importedImagePath.Split('.')[importedImagePath.Split('.').Length - 1];
			string fileName = ActiveBitmapFont.Name + fileExtension;
			string assetPath = GameProperties.BitmapFontsContentDirectory + "/" + fileName;

			if (File.Exists(assetPath)) {
				FileUtil.DeleteFileOrDirectory(assetPath);
				AssetDatabase.Refresh();
			}

			FileUtil.ReplaceFile(importedImagePath, assetPath);
			AssetDatabase.Refresh();

			BuildEditorTextures();
			OnEnable();
		}

		private bool ShouldBuildEditorTextures() {
			return _bitmapFontAtlas == null;
		}

		private void BuildEditorTextures() {
			if (ActiveBitmapFont == null) {
				return;
			}

			_bitmapFontAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(GameProperties.BitmapFontsContentDirectory + "/" + ActiveBitmapFont.Name + ".png");

			if (_bitmapFontAtlas != null) {
				_bitmapFontAtlas.filterMode = FilterMode.Point;
				_bitmapFontAtlas.wrapMode = TextureWrapMode.Clamp;
			}
		}

		private void OnBitmapFontChange() {
			_previousBitmapFontIndex = _selectedBitmapFontIndex;
			BuildEditorTextures();
		}

		private void OnGUI() {
			_windowWidth = position.width;
			_windowHeight = position.height;

			if (ShouldBuildEditorTextures()) {
				BuildEditorTextures();
			}

			EditorGUIUtility.labelWidth = 45.0f;
			SuperForms.Title("Bitmap Font Editor");
			DrawSelectorBar();
			DrawMainArea();
		}

		private void DrawSelectorBar() {
			SuperForms.Region.MainOptionBarInline(() => {
				if (_mainMode != Mode.Normal) {
					return;
				}

				if (_selectedBitmapFontIndex > _allBitmapFonts.Count) {
					_selectedBitmapFontIndex = _allBitmapFonts.Count - 1;
				}

				if (_allBitmapFonts.Count > 0) {
					_selectedBitmapFontIndex = SuperForms.DropDownLarge(
						_selectedBitmapFontIndex,
						AllBitmapFontNames.ToArray(),
						GUILayout.Width(200)
					);
				}

				if (_selectedBitmapFontIndex != _previousBitmapFontIndex) {
					OnBitmapFontChange();
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAddLarge)) {
					_inputName = "";
					_mainMode = Mode.Create;
				}

				if (ActiveBitmapFont != null) {
					SuperForms.IconButtons saveIcon = _fileHasChanged
						? SuperForms.IconButtons.ButtonSaveAlertLarge
						: SuperForms.IconButtons.ButtonSaveLarge;

					if (SuperForms.IconButton(saveIcon)) {
						SaveBitmapFont();
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonRenameLarge)) {
						_inputName = ActiveBitmapFont.Name;
						_mainMode = Mode.Rename;
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDeleteLarge)) {
						_mainMode = Mode.Delete;
					}
				}

				SuperForms.IconButton(SuperForms.IconButtons.ButtonRefreshLarge, OnEnable);
			});
		}

		private void DrawMainArea() {
			SuperForms.Region.MainArea(new Rect(0, 80, _windowWidth, _windowHeight - 80), () => {
				switch (_mainMode) {
					case Mode.Normal:
						if (ActiveBitmapFont != null) {
							DrawBitmapEditor();
							DrawOptionsPanel();
						}

						break;
					case Mode.Create:
						DrawAddNewBitmapFontForm();
						break;
					case Mode.Rename:
						DrawRenameBitmapFontForm();
						break;
					case Mode.Delete:
						DrawDeleteBitmapFontForm();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			});
		}

		private void DrawAddNewBitmapFontForm() {
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Create New Bitmap Font");
				_inputName = SuperForms.StringField("Bitmap Font Name", _inputName);


				List<string> otherNames = _allBitmapFonts.Select(bitmapFontProperties => bitmapFontProperties.Name).ToList();
				StringValidation.StringValidationInfo stringValidationInfo = StringValidation.GetValidity(_inputName, otherNames);

				if (stringValidationInfo.IsValid) {
					SuperForms.Button("Create Bitmap Font", () => {
						BitmapFontProperties newBitmapFont = new BitmapFontProperties {
							Name = _inputName
						};

						_allBitmapFonts.Add(newBitmapFont);
						_mainMode = Mode.Normal;
						_selectedBitmapFontIndex = _allBitmapFonts.IndexOf(newBitmapFont);
						SaveBitmapFont();
					});
				} else {
					SuperForms.FullBoxLabel("Invalid Name: " + stringValidationInfo.ValidationMessage);
				}

				SuperForms.Button("Cancel", () => { _mainMode = Mode.Normal; });
			}, GUILayout.Width(260));
		}

		private void DrawRenameBitmapFontForm() {
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Rename Bitmap Font");
				_inputName = SuperForms.StringField("New Name", _inputName);

				List<string> otherNames = new List<string>();
				foreach (BitmapFontProperties bitmapFontProperties in _allBitmapFonts) {
					if (bitmapFontProperties != ActiveBitmapFont) {
						otherNames.Add(bitmapFontProperties.Name);
					}
				}

				StringValidation.StringValidationInfo stringValidationInfo = StringValidation.GetValidity(_inputName, otherNames);

				if (stringValidationInfo.IsValid) {
					SuperForms.Button("Rename Bitmap Font", () => {
						if (_inputName != ActiveBitmapFont.Name) {
							string oldName = ActiveBitmapFont.Name;

							AssetDatabase.DeleteAsset(GameProperties.BitmapFontsContentDirectory + "/" + oldName + ".json");
							AssetDatabase.DeleteAsset(GameProperties.BitmapFontsContentDirectory + "/" + oldName + ".png");

							ActiveBitmapFont.Name = _inputName;
							SaveBitmapFont();
						}

						_mainMode = Mode.Normal;
					});
				} else {
					SuperForms.FullBoxLabel("Invalid Name: " + stringValidationInfo.ValidationMessage);
				}

				SuperForms.Button("Cancel", () => { _mainMode = Mode.Normal; });
			}, GUILayout.Width(260));
		}

		private void DrawDeleteBitmapFontForm() {
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Delete Bitmap Font");
				SuperForms.Region.Horizontal(() => {
					SuperForms.Button("NO!", () => { _mainMode = Mode.Normal; });
					SuperForms.Button("DELETE", () => {
						AssetDatabase.DeleteAsset(GameProperties.BitmapFontsContentDirectory + "/" + ActiveBitmapFont.Name + ".json");
						AssetDatabase.Refresh();
						BoomerangDatabase.PopulateDatabase();
						_mainMode = Mode.Normal;
						OnEnable();
					});
				});
			}, GUILayout.Width(260));
		}

		private void DrawBitmapEditor() {
			SuperForms.Region.Horizontal(() => {
				SuperForms.Button("1x", _atlasScale == 1, () => { _atlasScale = 1; }, GUILayout.ExpandWidth(false));
				SuperForms.Button("2x", _atlasScale == 2, () => { _atlasScale = 2; }, GUILayout.ExpandWidth(false));
				SuperForms.Button("3x", _atlasScale == 3, () => { _atlasScale = 3; }, GUILayout.ExpandWidth(false));
				SuperForms.Button("4x", _atlasScale == 4, () => { _atlasScale = 4; }, GUILayout.ExpandWidth(false));
				SuperForms.Button("5x", _atlasScale == 5, () => { _atlasScale = 5; }, GUILayout.ExpandWidth(false));
			});

			SuperForms.Region.Area(new Rect(0, 20, _windowWidth - 320, _windowHeight - 110), () => {
				SuperForms.Space();

				SuperForms.Region.Scroll("BitmapFontEditorAtlasView", () => {
					SuperForms.Region.VerticalBox(() => {
						if (_bitmapFontAtlas != null) {
							int width = _bitmapFontAtlas.width * _atlasScale;
							int height = _bitmapFontAtlas.height * _atlasScale;
							SuperForms.BlockedArea(width, height);
							SuperForms.Texture(new Rect(0, 0, width, height), _bitmapFontAtlas);
							DrawGlyphOutlines();
						} else {
							SuperForms.FullBoxLabel("To begin, Import a Bitmap Atlas");
						}
					});
				});
			});
		}

		private void DrawGlyphOutlines() {
			foreach (BitmapFontGlyphProperties glyph in ActiveBitmapFont.Glyphs) {
				Texture2D borderTexture = ActiveGlyph == glyph
					? SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]
					: SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.BoundingBox];

				int x = glyph.X * _atlasScale;
				int y = glyph.Row * ActiveBitmapFont.GlyphHeight * _atlasScale;
				int width = glyph.Width * _atlasScale;
				int height = ActiveBitmapFont.GlyphHeight * _atlasScale;


				SuperForms.Texture(
					new Rect(x, y, width, 2),
					borderTexture
				);
				SuperForms.Texture(
					new Rect(x, y, 2, height),
					borderTexture
				);
				SuperForms.Texture(
					new Rect(x, y + height - 2, width, 2),
					borderTexture
				);
				SuperForms.Texture(
					new Rect(x + width - 2, y, 2, height),
					borderTexture
				);
			}
		}

		private void DrawOptionsPanel() {
			SuperForms.Region.Area(new Rect(_windowWidth - 300, 20, 300, _windowHeight - 110), () => {
				DrawBitmapFontPropertiesForm();
				if (_bitmapFontAtlas != null) {
					SuperForms.Space();
					DrawNewGlyphOptionsForm();
					SuperForms.Space();
					DrawActiveGlyphPropertiesForm();
					SuperForms.Space();
					DrawGlyphCollectionList();
				}
			});
		}

		private void DrawBitmapFontPropertiesForm() {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Bitmap Font Properties");
				ActiveBitmapFont.GlyphHeight = SuperForms.IntField("Glyph Height", ActiveBitmapFont.GlyphHeight);

				SuperForms.Button("Import Bitmap Font Atlas", () => {
					string importedImagePath = EditorUtility.OpenFilePanel(
						"Import TileSheet Image",
						"",
						"png"
					);

					if (importedImagePath.Length != 0) {
						ImportImage(importedImagePath);
					}
				});
			});
		}

		private void DrawNewGlyphOptionsForm() {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.Space();
				SuperForms.BoxHeader("Add a Glyph");
				_defaultWidth = SuperForms.IntField("Default Width", _defaultWidth);
				_predictNextPosition = SuperForms.Checkbox("Predict Next Position", _predictNextPosition);
				_predictNextCharacter = SuperForms.Checkbox("Predict Next Character", _predictNextCharacter);

				SuperForms.Button("Add Glyph", () => {
					int newGlyphRow = 0;
					int newGlyphX = 0;
					int newGlyphCharacterCode = 32;

					BitmapFontGlyphProperties lastGlyphProperties = ActiveBitmapFont.Glyphs.Count > 0
						? ActiveBitmapFont.Glyphs.Last()
						: null;

					if (lastGlyphProperties != null) {
						if (_predictNextPosition) {
							newGlyphRow = lastGlyphProperties.Row;
							newGlyphX = lastGlyphProperties.X + lastGlyphProperties.Width;

							if (newGlyphX >= _bitmapFontAtlas.width) {
								newGlyphX = 0;
								newGlyphRow++;
							}
						}

						if (_predictNextCharacter) {
							newGlyphCharacterCode = lastGlyphProperties.CharacterCode + 1;
							if (newGlyphCharacterCode >= 128 && newGlyphCharacterCode <= 160) {
								newGlyphCharacterCode = 161;
							}

							if (!_characterMap.ContainsKey(newGlyphCharacterCode)) {
								newGlyphCharacterCode = 32;
							}
						}
					}

					ActiveBitmapFont.Glyphs.Add(new BitmapFontGlyphProperties {
						X = newGlyphX,
						Row = newGlyphRow,
						Width = _defaultWidth,
						CharacterCode = newGlyphCharacterCode
					});

					SuperForms.ScrollRegionToBottom("BitmapFontEditorCharacterList");
					_selectedGlyphIndex = ActiveBitmapFont.Glyphs.Count - 1;
				}, GUILayout.ExpandWidth(false));
			});
		}

		private void DrawActiveGlyphPropertiesForm() {
			if (ActiveGlyph == null) {
				return;
			}

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Selected Glyph Properties");

				if (_selectingGlyphCharacterMode) {
					const int perRow = 11;
					int itemRenderedIndex = 0;

					foreach (KeyValuePair<int, string> character in _characterMap) {
						bool isFirstButtonOnRow = itemRenderedIndex % perRow == 0;
						bool isLastButtonOnRow = itemRenderedIndex % perRow == perRow - 1;
						bool isLastIndex = character.Key == 255;

						if (isFirstButtonOnRow) {
							SuperForms.Begin.Horizontal();
						}

						if (SuperForms.Button(character.Value, character.Key == ActiveGlyph.CharacterCode, GUILayout.Width(20))) {
							ActiveGlyph.CharacterCode = character.Key;
							_selectingGlyphCharacterMode = false;
						}

						if (isLastButtonOnRow || isLastIndex) {
							SuperForms.End.Horizontal();
						}

						itemRenderedIndex++;
					}

					SuperForms.Button("Cancel", () => { _selectingGlyphCharacterMode = false; });
				} else {
					int selectedIndex = AllCharacters.IndexOf(_characterMap[ActiveGlyph.CharacterCode]);
					string selectedCharacter = AllCharacters[selectedIndex];

					SuperForms.Region.Horizontal(() => {
						SuperForms.Label("Character");

						SuperForms.Button(selectedCharacter, () => { _selectingGlyphCharacterMode = true; });
					});

					foreach (KeyValuePair<int, string> character in _characterMap) {
						if (character.Value == selectedCharacter) {
							ActiveGlyph.CharacterCode = character.Key;
						}
					}

					ActiveGlyph.Row = SuperForms.IntField("Row", ActiveGlyph.Row);
					ActiveGlyph.X = SuperForms.IntField("X", ActiveGlyph.X);
					ActiveGlyph.Width = SuperForms.IntField("Width", ActiveGlyph.Width);
				}
			});
		}

		private void DrawGlyphCollectionList() {
			if (ActiveBitmapFont.Glyphs.Count == 0) {
				return;
			}

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Characters");

				SuperForms.Region.Horizontal(() => {
					SuperForms.Label("", GUILayout.Width(80));
					SuperForms.Label("Row", GUILayout.Width(50));
					SuperForms.Label("X", GUILayout.Width(50));
					SuperForms.Label("Width", GUILayout.Width(50));
				});

				SuperForms.Region.Scroll("BitmapFontEditorCharacterList", () => {
					BitmapFontGlyphProperties toDelete = null;

					for (int index = 0; index < ActiveBitmapFont.Glyphs.Count; index++) {
						BitmapFontGlyphProperties glyph = ActiveBitmapFont.Glyphs[index];
						SuperForms.Begin.Horizontal();

						if (SuperForms.Button(_characterMap[glyph.CharacterCode], ActiveGlyph == glyph, GUILayout.Width(80))) {
							_selectedGlyphIndex = index;
						}

						SuperForms.Label(glyph.Row.ToString(), GUILayout.Width(50));
						SuperForms.Label(glyph.X.ToString(), GUILayout.Width(50));
						SuperForms.Label(glyph.Width.ToString(), GUILayout.Width(50));

						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
							toDelete = glyph;
						}

						SuperForms.End.Horizontal();
					}

					if (toDelete != null) {
						ActiveBitmapFont.Glyphs.Remove(toDelete);
					}
				});
			});
		}
	}
}