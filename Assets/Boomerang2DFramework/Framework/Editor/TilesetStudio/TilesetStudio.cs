using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.TilesetStudio {
	public partial class TilesetStudio : EditorWindow {
		[MenuItem("Tools/Boomerang2D/Tileset Studio", false, 151)]
		public static void ShowWindow() {
			_window = (TilesetStudio) GetWindow(typeof(TilesetStudio));
			_window.minSize = new Vector2(600, 400);
			_window.Show();
			_window.autoRepaintOnSceneChange = true;

			const string textureForTabIcon = "Assets/Boomerang2DFramework/Framework/EditorAssets/editorWindowIconTilesetStudio.png";
			_window.titleContent = new GUIContent("Tileset Studio", AssetDatabase.LoadAssetAtPath<Texture>(textureForTabIcon));
		}

		private void Update() {
			_totalTime = Time.realtimeSinceStartup;

			const float fps = 30f;

			if (_repaintTime > _totalTime + 1f) {
				_repaintTime = 0f;
			}

			if (_repaintTime + 1.0f / fps < _totalTime || _repaintNext) {
				_repaintTime = _totalTime;
				_repaintNext = false;
				Repaint();
				UpdateCheckSaveTime();
			}
		}

		private void UpdateCheckSaveTime() {
			if (_totalTime > _timeToCheckSave + 1) {
				_timeToCheckSave = _totalTime;

				if (ActiveTileset != null && BoomerangDatabase.TilesetJsonDatabaseEntries.ContainsKey(ActiveTileset.Name)) {
					string originalJson = BoomerangDatabase.TilesetJsonDatabaseEntries[ActiveTileset.Name].text;
					_fileHasChanged = originalJson != JsonUtility.ToJson(ActiveTileset, true);
				}
			}
		}

		private void OnEnable() {
			_timeToCheckSave = 0;

			LoadData();
			LoadAtlasTextures();
		}

		private void LoadData() {
			_allTilesets.Clear();
			_allTilesetTextures.Clear();

			foreach (KeyValuePair<string, TextAsset> actorJson in BoomerangDatabase.TilesetJsonDatabaseEntries) {
				TilesetProperties tilesetProperties = JsonUtility.FromJson<TilesetProperties>(actorJson.Value.text);

				List<Texture2D> tilesetTextures = LoadTilesetTextures(tilesetProperties);

				tilesetProperties.PopulateLookupTable();
				_allTilesets.Add(tilesetProperties);
				_allTilesetTextures.Add(tilesetTextures);
			}

			_allColliders.Clear();

			foreach (KeyValuePair<string, GameObject> tileColliderGameObject in BoomerangDatabase.TileColliderDatabaseEntries) {
				Collider2D[] colliderSet = tileColliderGameObject.Value.GetComponents<Collider2D>();
				_allColliders.Add(tileColliderGameObject.Key, colliderSet);
			}
		}

		private void SaveTileset() {
			ActiveTileset.Name = AllTilesetNames[_selectedTilesetIndex];

			File.WriteAllText(
				GameProperties.TilesetContentDirectory + "/" + AllTilesetNames[_selectedTilesetIndex] + ".json",
				JsonUtility.ToJson(ActiveTileset, true)
			);

			AssetDatabase.Refresh();
			BoomerangDatabase.PopulateDatabase();
			OnEnable();
		}

		private void LoadAtlasTextures() {
			_brushModeAtlasTextures.Clear();

			Texture2D atlas = SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.BrushModeAtlas];

			const int atlasTileSize = 16;
			int width = Mathf.FloorToInt((float) atlas.width / atlasTileSize);
			int height = Mathf.FloorToInt((float) atlas.height / atlasTileSize);

			for (int y = height - 1; y >= 0; y--) {
				for (int x = 0; x < width; x++) {
					Texture2D croppedTexture = new Texture2D(atlasTileSize, atlasTileSize) {
						wrapMode = TextureWrapMode.Clamp,
						filterMode = FilterMode.Point
					};

					Color[] pixels = atlas.GetPixels(x * atlasTileSize, y * atlasTileSize, atlasTileSize, atlasTileSize);
					croppedTexture.SetPixels(pixels, 0);
					croppedTexture.Apply();
					_brushModeAtlasTextures.Add(croppedTexture);
				}
			}
		}

		private List<Texture2D> LoadTilesetTextures(TilesetProperties tilesetProperties) {
			List<Texture2D> tilesetTextures = new List<Texture2D>();
			Sprite[] tileSprites = BoomerangDatabase.TilesetSpriteDatabaseEntries[tilesetProperties.Name];

			if (tileSprites == null) {
				return new List<Texture2D>();
			}

			foreach (Sprite sprite in tileSprites) {
				Texture2D croppedTexture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
				Color[] pixels = sprite.texture.GetPixels((int) sprite.rect.x,
					(int) sprite.rect.y,
					(int) sprite.rect.width,
					(int) sprite.rect.height, 0);

				croppedTexture.SetPixels(pixels, 0);
				croppedTexture.wrapMode = TextureWrapMode.Clamp;
				croppedTexture.filterMode = FilterMode.Point;
				croppedTexture.Apply();
				tilesetTextures.Add(croppedTexture);
			}

			return tilesetTextures;
		}

		private bool ShouldReloadData() {
			bool shouldReload = false;

			foreach (List<Texture2D> texture2Ds in _allTilesetTextures) {
				foreach (Texture2D texture2D in texture2Ds) {
					if (texture2D == null) {
						shouldReload = true;
						break;
					}
				}

				if (shouldReload) {
					break;
				}
			}

			return shouldReload;
		}

		private void OnTilesetChange() {
			_previousSelectedTilesetIndex = _selectedTilesetIndex;
		}

		private void ImportImage(string importedImagePath) {
			string fileExtension = Path.GetExtension(importedImagePath);
			string fileName = ActiveTileset.Name + fileExtension;
			string assetPath = GameProperties.TilesetContentDirectory + "/" + fileName;

			if (File.Exists(assetPath)) {
				FileUtil.DeleteFileOrDirectory(assetPath);
				AssetDatabase.Refresh();
			}

			FileUtil.ReplaceFile(importedImagePath, assetPath);
			AssetDatabase.Refresh();

			SpriteSlicer.SliceSprite(assetPath, ActiveTileset.TileSize, ActiveTileset.TileSize);
			AssetDatabase.Refresh();

			BoomerangDatabase.PopulateDatabase();
			BoomerangDatabase.IndexContent();

			SaveTileset();
			OnEnable();
		}

		private void AutoGenerateTiles() {
			for (int i = 0; i < BoomerangDatabase.TilesetSpriteDatabaseEntries[ActiveTileset.Name].Length; i++) {
				TileProperties newTileProperties = new TileProperties {
					Slot = ActiveTileset.Tiles.Count == 0
						? 1
						: ActiveTileset.Tiles.Last().Slot + 1,
					AnimationFrames = new List<int> {i},
					AnimationFramesSpeeds = new List<float> {0.0f},
					UsesCollider = false,
					Flags = new List<string>()
				};

				ActiveTileset.Tiles.Add(newTileProperties);
			}
		}

		private void OnGUI() {
			EditorGUIUtility.labelWidth = 45.0f;
			_windowWidth = position.width;
			_windowHeight = position.height;

			if (ShouldReloadData()) {
				OnEnable();
			}

			SuperForms.Title("Tileset Studio");

			DrawTilesetSelectorBar();
			DrawMainArea();
		}

		private void DrawTilesetSelectorBar() {
			SuperForms.Region.MainOptionBarInline(() => {
				if (_mainMode != Mode.Normal) {
					return;
				}

				if (_selectedTilesetIndex > AllTilesetNames.Count) {
					_selectedTilesetIndex = AllTilesetNames.Count - 1;
				}

				_selectedTilesetIndex = SuperForms.DropDownLarge(
					_selectedTilesetIndex,
					AllTilesetNames.ToArray(),
					GUILayout.Width(200)
				);

				if (_selectedTilesetIndex != _previousSelectedTilesetIndex) {
					OnTilesetChange();
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAddLarge)) {
					_inputName = "";
					_mainMode = Mode.Create;
				}

				if (ActiveTileset != null) {
					SuperForms.IconButtons saveIcon = _fileHasChanged
						? SuperForms.IconButtons.ButtonSaveAlertLarge
						: SuperForms.IconButtons.ButtonSaveLarge;

					if (SuperForms.IconButton(saveIcon)) {
						SaveTileset();
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonRenameLarge)) {
						_inputName = ActiveTileset.Name;
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
				if (_mainMode == Mode.Normal && ActiveTileset != null) {
					DrawTilesetProperties();

					if (_tileEditingMode == TileEditingMode.Tiles) {
						DrawTileSelector();
						if (ActiveTile != null) {
							DrawTileProperties();
						}
					}

					if (_tileEditingMode == TileEditingMode.Stamps || _tileEditingMode == TileEditingMode.StampsSelectingTile) {
						DrawStampEditor();
						DrawStampEditorPanel();
					}

					if (_tileEditingMode == TileEditingMode.Brushes || _tileEditingMode == TileEditingMode.BrushesSelectingTile) {
						DrawBrushEditor();
						DrawBrushPropertiesPanel();
					}
				}

				if (_mainMode == Mode.Create) {
					DrawCreateTilesetForm();
				}

				if (_mainMode == Mode.Rename) {
					DrawRenameTilesetForm();
				}

				if (_mainMode == Mode.Delete) {
					DrawDeleteTilesetForm();
				}

				if (_mainMode == Mode.SelectFrame) {
					DrawSelectFrameForm();
				}
			});
		}

		private void DrawCreateTilesetForm() {
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Create a new Tileset");
				_inputName = SuperForms.StringField("Name", _inputName);

				StringValidation.StringValidationInfo stringValidationInfo = StringValidation.GetValidity(_inputName, AllTilesetNames);

				if (stringValidationInfo.IsValid) {
					SuperForms.Button("Create new Tileset", () => {
						TilesetProperties newTilesetProperties = new TilesetProperties {
							Name = _inputName,
							EditorTilePerRow = 10
						};

						File.WriteAllText(
							GameProperties.TilesetContentDirectory + "/" + _inputName + ".json",
							JsonUtility.ToJson(newTilesetProperties, true)
						);

						AssetDatabase.Refresh();
						BoomerangDatabase.PopulateDatabase();
						OnEnable();
						_mainMode = Mode.Normal;
					});
				} else {
					SuperForms.FullBoxLabel("Invalid Name: " + stringValidationInfo.ValidationMessage);
				}

				SuperForms.Button("Cancel", () => { _mainMode = Mode.Normal; });
			}, GUILayout.Width(260));
		}

		private void DrawRenameTilesetForm() {
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Rename this Tileset");
				_inputName = SuperForms.StringField("New Name", _inputName);

				List<string> otherNames = new List<string>();

				foreach (string otherName in AllTilesetNames) {
					if (otherName != ActiveTileset.Name) {
						otherNames.Add(otherName);
					}
				}

				StringValidation.StringValidationInfo stringValidationInfo = StringValidation.GetValidity(_inputName, otherNames);

				if (stringValidationInfo.IsValid) {
					SuperForms.Button("Rename Tileset", () => {
						if (_inputName != ActiveTileset.Name) {
							string oldName = ActiveTileset.Name;

							AssetDatabase.DeleteAsset(GameProperties.TilesetContentDirectory + "/" + oldName + ".json");
							AssetDatabase.DeleteAsset(GameProperties.TilesetContentDirectory + "/" + oldName + ".png");
							ActiveTileset.Name = _inputName;
							SaveTileset();
						}

						_mainMode = Mode.Normal;
					});
				} else {
					SuperForms.FullBoxLabel("Invalid Name: " + stringValidationInfo.ValidationMessage);
				}

				SuperForms.Button("Cancel", () => { _mainMode = Mode.Normal; });
			}, GUILayout.Width(260));
		}

		private void DrawDeleteTilesetForm() {
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Delete this Tileset?");

				SuperForms.Button("NO!", () => { _mainMode = Mode.Normal; });

				SuperForms.Button("Yes, Delete", () => {
					AssetDatabase.DeleteAsset(GameProperties.TilesetContentDirectory + "/" + ActiveTileset.Name + ".json");
					AssetDatabase.DeleteAsset(GameProperties.TilesetContentDirectory + "/" + ActiveTileset.Name + ".png");
					AssetDatabase.Refresh();
					OnEnable();
					_mainMode = Mode.Normal;
				});
			}, GUILayout.Width(260));
		}

		private void DrawTilesetProperties() {
			SuperForms.Region.Area(new Rect(10, 20, 260, _windowHeight - 110), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Tileset Properties");
					List<string> tileSizeOptions = new List<string>();
					int power = 0;
					while (tileSizeOptions.Count < 12) {
						string size = Mathf.Pow(2, power).ToString(CultureInfo.InvariantCulture);
						tileSizeOptions.Add(size + "x" + size);
						power++;
					}

					int dropdownIndex = (int) Mathf.Log(ActiveTileset.TileSize, 2);
					dropdownIndex = SuperForms.DropDown("Tile Size", dropdownIndex, tileSizeOptions.ToArray());
					ActiveTileset.TileSize = (int) Mathf.Pow(2, dropdownIndex);

					_autoGenerateTilesOnImport = SuperForms.Checkbox("Auto-Generate Tiles", _autoGenerateTilesOnImport);

					ActiveTileset.EditorTilePerRow = SuperForms.IntField("Display Width", ActiveTileset.EditorTilePerRow);

					ActiveTileset.EditorTilePerRow = BoomerangUtils.MinValue(ActiveTileset.EditorTilePerRow, 3);
					ActiveTileset.EditorTilePerRow = BoomerangUtils.MaxValue(ActiveTileset.EditorTilePerRow, 30);

					SuperForms.Button("Import TileSheet Image", () => {
						string importedImagePath = EditorUtility.OpenFilePanel(
							"Import TileSheet Image",
							"",
							"png"
						);

						if (importedImagePath.Length != 0) {
							ImportImage(importedImagePath);
							if (_autoGenerateTilesOnImport) {
								AutoGenerateTiles();
							}
						}
					});
				});

				SuperForms.Space();

				SuperForms.Region.VerticalBox(() => {
					SuperForms.Region.Horizontal(() => {
						SuperForms.Button("Tiles", _tileEditingMode == TileEditingMode.Tiles, () => { _tileEditingMode = TileEditingMode.Tiles; });

						SuperForms.Button("Stamps",
							_tileEditingMode == TileEditingMode.Stamps || _tileEditingMode == TileEditingMode.StampsSelectingTile,
							() => { _tileEditingMode = TileEditingMode.Stamps; }
						);

						SuperForms.Button("Brushes",
							_tileEditingMode == TileEditingMode.Brushes ||
							_tileEditingMode == TileEditingMode.BrushesSelectingTile,
							() => { _tileEditingMode = TileEditingMode.Brushes; }
						);
					});

					if (_tileEditingMode == TileEditingMode.Tiles) {
						SuperForms.Button("Add a Tile", () => {
							TileProperties newTileProperties = new TileProperties {
								Slot = ActiveTileset.Tiles.Count == 0
									? 1
									: ActiveTileset.Tiles.Last().Slot + 1,
								AnimationFrames = new List<int> {0},
								UsesCollider = false
							};

							ActiveTileset.Tiles.Add(newTileProperties);
						});

						SuperForms.Space();

						SuperForms.Button("Copy Selected Tile Collider", () => { _colliderPasteSource = ActiveTile; });

						SuperForms.Button("Paste Copied Tile Collider", () => {
							ActiveTile.UsesCollider = _colliderPasteSource.UsesCollider;
							ActiveTile.SolidOnTop = _colliderPasteSource.SolidOnTop;
							ActiveTile.SolidOnRight = _colliderPasteSource.SolidOnRight;
							ActiveTile.SolidOnBottom = _colliderPasteSource.SolidOnBottom;
							ActiveTile.SolidOnLeft = _colliderPasteSource.SolidOnLeft;
							ActiveTile.CollisionShape = _colliderPasteSource.CollisionShape;
							ActiveTile.CollisionFlippedX = _colliderPasteSource.CollisionFlippedX;
							ActiveTile.CollisionFlippedY = _colliderPasteSource.CollisionFlippedY;
							ActiveTile.CollisionRotation = _colliderPasteSource.CollisionRotation;
							ActiveTile.CollisionOffset = _colliderPasteSource.CollisionOffset;
						});

						if (_colliderPasteSource != null) {
							_autoPasteColliderInfo = SuperForms.Checkbox("Auto-Paste on Selection", _autoPasteColliderInfo);
						}
					}

					if (_tileEditingMode == TileEditingMode.Stamps || _tileEditingMode == TileEditingMode.StampsSelectingTile) {
						SuperForms.Button("Add a Stamp", () => {
							List<int> emptyRow = new List<int> {0, 0, 0, 0, 0};
							List<List<int>> fullGrid = new List<List<int>> {
								new List<int>(emptyRow),
								new List<int>(emptyRow),
								new List<int>(emptyRow),
								new List<int>(emptyRow),
								new List<int>(emptyRow)
							};

							TilesetEditorStamp newStamp = new TilesetEditorStamp {
								Name = "New Stamp",
								Height = 5,
								Width = 5,
								ParsedTileIds = fullGrid
							};

							ActiveTileset.Stamps.Add(newStamp);
						});
					}

					if (_tileEditingMode == TileEditingMode.Brushes || _tileEditingMode == TileEditingMode.BrushesSelectingTile) {
						SuperForms.Button("Add a Brush", () => {
							ActiveTileset.Brushes.Add(new TilesetEditorBrush {
								Name = "Unnamed Brush"
							});
						});
					}

					if (_tileEditingMode == TileEditingMode.Brushes) { }
				});
			});
		}

		private void DrawTileSelector() {
			SuperForms.Region.Area(new Rect(280, 20, _windowWidth - 550, _windowHeight - 110), () => {
				SuperForms.Region.Vertical(() => {
					SuperForms.Region.Scroll("tileSelectorScrollPosition", () => {
						SuperForms.Region.VerticalBox(() => {
							int tilesPerRow = ActiveTileset.EditorTilePerRow;
							int currentRow = 0;

							GUIStyle tileButton = new GUIStyle {
								fixedWidth = 50,
								fixedHeight = 50
							};

							for (int i = 0; i < ActiveTileset.Tiles.Count; i++) {
								int j;
								SuperForms.Begin.Horizontal();

								for (j = 0; j < tilesPerRow; j++) {
									if (i + j >= ActiveTileset.Tiles.Count) {
										break;
									}

									TileProperties tileProperties = ActiveTileset.Tiles[i + j];

									if (SuperForms.Button("", tileButton)) {
										_selectedTileIndex = i + j;
										GUI.FocusControl("");

										if (_colliderPasteSource != null && _autoPasteColliderInfo) {
											ActiveTile.UsesCollider = _colliderPasteSource.UsesCollider;
											ActiveTile.SolidOnTop = _colliderPasteSource.SolidOnTop;
											ActiveTile.SolidOnRight = _colliderPasteSource.SolidOnRight;
											ActiveTile.SolidOnBottom = _colliderPasteSource.SolidOnBottom;
											ActiveTile.SolidOnLeft = _colliderPasteSource.SolidOnLeft;
											ActiveTile.CollisionShape = _colliderPasteSource.CollisionShape;
											ActiveTile.CollisionFlippedX = _colliderPasteSource.CollisionFlippedX;
											ActiveTile.CollisionFlippedY = _colliderPasteSource.CollisionFlippedY;
											ActiveTile.CollisionRotation = _colliderPasteSource.CollisionRotation;
											ActiveTile.CollisionOffset = _colliderPasteSource.CollisionOffset;
										}
									}

									Rect textureSpace = new Rect(j * 50, currentRow * 50, 50, 50);

									GUI.DrawTexture(
										textureSpace,
										SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.TitleBackground]
									);

									Rect tileSpace = new Rect(
										textureSpace.x + 2,
										textureSpace.y + 2,
										textureSpace.width - 4,
										textureSpace.height - 4
									);

									int frameToDraw = Drawers.GetAnimationFrame(tileProperties, _totalTime);
									if (tileProperties.AnimationFrames[frameToDraw] >= ActiveTilesetTextures.Count) {
										continue;
									}

									GUI.DrawTexture(tileSpace, ActiveTilesetTextures[tileProperties.AnimationFrames[frameToDraw]]);

									if (ActiveTile == tileProperties && Event.current.type == EventType.Repaint) {
										if (tileProperties.UsesCollider && _allColliders.ContainsKey(ActiveTile.CollisionShape)) {
											Collider2D[] tileColliders = _allColliders[ActiveTile.CollisionShape];
											Drawers.DrawTileColliders(tileSpace, tileProperties, tileColliders);
										}

										Drawers.DrawTileBorder(
											2,
											SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays], textureSpace
										);
									}
								}

								SuperForms.End.Horizontal();

								i += j - 1;
								currentRow++;
							}
						}, GUILayout.ExpandHeight(true));
					});
				}, GUILayout.ExpandHeight(true));
			});
		}

		private void DrawTileProperties() {
			SuperForms.Region.Area(new Rect(_windowWidth - 260, 20, 260, _windowHeight - 110), () => {
				SuperForms.Region.Scroll("scrollForTileProperties", () => {
					SuperForms.Region.VerticalBox(() => {
						SuperForms.BoxHeader("TileMap Slot " + ActiveTile.Slot);
						SuperForms.Space();

						if (SuperForms.Button("Delete Tile")) {
							ActiveTileset.Tiles.Remove(ActiveTile);
							_selectedTileIndex = -1;
							return;
						}

						SuperForms.Space();
						SuperForms.BoxHeader("Appearance");
						SuperForms.BoxSubHeader("Frames");

						int differentInListCount = ActiveTile.AnimationFrames.Count - ActiveTile.AnimationFramesSpeeds.Count;

						if (differentInListCount != 0 || ActiveTile.AnimationFrames.Count == 0) {
							ActiveTile.AnimationFrames = new List<int> {0};
							ActiveTile.AnimationFramesSpeeds = new List<float> {0};
						}

						for (int i = 0; i < ActiveTile.AnimationFrames.Count; i++) {
							if (i > ActiveTile.AnimationFrames.Count - 1) {
								break;
							}

							int index = i;
							SuperForms.Region.Horizontal(() => {
								SuperForms.TinyButton("O", () => {
									_selectFrameId = index;
									_mainMode = Mode.SelectFrame;
								});
								ActiveTile.AnimationFrames[index] = SuperForms.IntField(ActiveTile.AnimationFrames[index]);

								if (ActiveTile.AnimationFrames.Count > 1) {
									ActiveTile.AnimationFramesSpeeds[index] = SuperForms.FloatField(ActiveTile.AnimationFramesSpeeds[index]);

									if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
										ActiveTile.AnimationFrames.RemoveAt(index);
										ActiveTile.AnimationFramesSpeeds.RemoveAt(index);
									}
								}
							});
						}

						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
							ActiveTile.AnimationFrames.Add(0);
							ActiveTile.AnimationFramesSpeeds.Add(0);
						}

						SuperForms.Space();

						SuperForms.BoxHeader("Flags");
						ActiveTile.Flags = SuperForms.ListField("Flags", ActiveTile.Flags);

						SuperForms.Space();

						SuperForms.BoxHeader("Collider");

						ActiveTile.UsesCollider = SuperForms.Checkbox("Uses Collider", ActiveTile.UsesCollider);

						if (ActiveTile.UsesCollider) {
							_selectedColliderShape = BoomerangUtils.ClampValue(
								AllColliderNames.IndexOf(ActiveTile.CollisionShape), 0, AllColliderNames.Count
							);

							SuperForms.Region.Horizontal(() => {
								SuperForms.Label("Shape ", GUILayout.Width(50));
								_selectedColliderShape = SuperForms.DropDown(
									_selectedColliderShape,
									AllColliderNames.ToArray()
								);
							});

							ActiveTile.CollisionShape = AllColliderNames[_selectedColliderShape];

							SuperForms.Space();

							ActiveTile.CollisionFlippedX = SuperForms.Checkbox("Flip Horizontally", ActiveTile.CollisionFlippedX);
							ActiveTile.CollisionFlippedY = SuperForms.Checkbox("Flip Veritcally", ActiveTile.CollisionFlippedY);

							SuperForms.Label("Rotation");
							SuperForms.Region.Horizontal(() => {
								ActiveTile.CollisionRotation = SuperForms.HorizontalSlider(ActiveTile.CollisionRotation, 0, 360);
								ActiveTile.CollisionRotation = SuperForms.FloatField(ActiveTile.CollisionRotation, GUILayout.Width(50));
								ActiveTile.CollisionRotation = Mathf.Floor(ActiveTile.CollisionRotation * 10) / 10;
							});

							ActiveTile.CollisionOffset = SuperForms.Vector2Field("Offset", ActiveTile.CollisionOffset);

							SuperForms.Space();

							SuperForms.BoxHeader("Collision");

							ActiveTile.SolidOnTop = SuperForms.Checkbox("Solid on Top", ActiveTile.SolidOnTop);
							ActiveTile.SolidOnBottom = SuperForms.Checkbox("Solid on Bottom", ActiveTile.SolidOnBottom);
							ActiveTile.SolidOnLeft = SuperForms.Checkbox("Solid on Left", ActiveTile.SolidOnLeft);
							ActiveTile.SolidOnRight = SuperForms.Checkbox("Solid on Right", ActiveTile.SolidOnRight);

							SuperForms.Region.Horizontal(() => {
								SuperForms.Button("All Solid", () => {
									ActiveTile.SolidOnTop = ActiveTile.SolidOnBottom = true;
									ActiveTile.SolidOnLeft = ActiveTile.SolidOnRight = true;
								});

								SuperForms.Button("None Solid", () => {
									ActiveTile.SolidOnTop = ActiveTile.SolidOnBottom = false;
									ActiveTile.SolidOnLeft = ActiveTile.SolidOnRight = false;
								});
							});
						}
					}, GUILayout.ExpandHeight(true));
				});
			});
		}

		private void DrawSelectFrameForm() {
			SuperForms.Region.Area(new Rect(20, 20, _windowWidth - 40, _windowHeight - 110), () => {
				SuperForms.Space();

				SuperForms.Region.HorizontalBox(() => {
					SuperForms.BoxSubHeader("Select a Tile for this Frame");
					SuperForms.Button("Cancel", () => { _mainMode = Mode.Normal; }, GUILayout.ExpandWidth(false));
				});

				SuperForms.Space();

				SuperForms.Region.VerticalBox(() => {
					SuperForms.Region.Scroll("scrollForTileFrameSelector", () => {
						int tilesPerRow = ActiveTileset.EditorTilePerRow;

						int currentRow = 0;

						GUIStyle tileButton = new GUIStyle {
							fixedWidth = 50,
							fixedHeight = 50
						};

						for (int i = 0; i < ActiveTilesetTextures.Count; i++) {
							int j;

							SuperForms.Begin.Horizontal();

							for (j = 0; j < tilesPerRow; j++) {
								if (i + j >= ActiveTilesetTextures.Count) {
									break;
								}

								int selectedIndex = i + j;

								SuperForms.Button("", () => {
									ActiveTile.AnimationFrames[_selectFrameId] = selectedIndex;
									GUI.FocusControl("");
									_mainMode = Mode.Normal;
								}, tileButton);

								Rect textureSpace = new Rect(j * 50, currentRow * 50, 50, 50);

								GUI.DrawTexture(
									textureSpace,
									SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.TitleBackground]
								);

								Rect tileSpace = new Rect(
									textureSpace.x + 2,
									textureSpace.y + 2,
									textureSpace.width - 4,
									textureSpace.height - 4
								);

								GUI.DrawTexture(tileSpace, ActiveTilesetTextures[i + j]);
							}

							SuperForms.End.Horizontal();

							i += j - 1;
							currentRow++;
						}
					});
				}, GUILayout.ExpandHeight(true));
			}, GUILayout.ExpandHeight(true));
		}

		private void DrawStampEditor() {
			SuperForms.Region.Area(new Rect(280, 20, _windowWidth - 550, _windowHeight - 110), () => {
				SuperForms.Region.VerticalBox(() => {
					if (ActiveStamp == null) {
						SuperForms.FullBoxLabel(ActiveTileset.Stamps.Count == 0
							? "Add a Stamp on the left"
							: "Select a Stamp on the Right"
						);

						return;
					}

					if (ActiveStamp.ParsedTileIds.Count == 0) {
						ParseStampGrid();
					}

					ResizeStampGrid();

					SuperForms.Region.Scroll("stampEditorScrollPosition", () => {
						int i;
						int j;
						const int tileSize = 32;
						const int tilePadding = 1;
						Vector2Int topLeft = new Vector2Int(tileSize + tilePadding, tileSize + tilePadding);

						GUIStyle tileButton = new GUIStyle {
							fixedWidth = tileSize,
							fixedHeight = tileSize,
							margin = new RectOffset(0, tilePadding, 0, tilePadding)
						};

						SuperForms.Region.Horizontal(() => {
							SuperForms.Button("", tileButton);

							for (i = 0; i < ActiveStamp.Width; i++) {
								bool isRepeatable = ActiveStamp.RepeatableColumns.Contains(i);
								if (SuperForms.Button("", tileButton)) {
									if (isRepeatable) {
										ActiveStamp.RepeatableColumns.Remove(i);
									} else {
										ActiveStamp.RepeatableColumns.Add(i);
									}

									GUI.FocusControl("");
								}

								isRepeatable = ActiveStamp.RepeatableColumns.Contains(i);

								GUI.DrawTexture(
									new Rect((i + 1) * (tileSize + tilePadding), 0, tileSize, tileSize),
									SuperFormsStyles.GuiTextures[isRepeatable
										? SuperFormsStyles.B2DEditorTextures.IconRepeatHover
										: SuperFormsStyles.B2DEditorTextures.IconRepeat
									]
								);
							}
						});

						for (i = 0; i < ActiveStamp.Height; i++) {
							SuperForms.Region.Horizontal(() => {
								bool isRepeatable = ActiveStamp.RepeatableRows.Contains(i);

								if (SuperForms.Button("", tileButton)) {
									if (isRepeatable) {
										ActiveStamp.RepeatableRows.Remove(i);
									} else {
										ActiveStamp.RepeatableRows.Add(i);
									}

									GUI.FocusControl("");
								}

								isRepeatable = ActiveStamp.RepeatableRows.Contains(i);

								GUI.DrawTexture(
									new Rect(0, (i + 1) * (tileSize + tilePadding), tileSize, tileSize),
									SuperFormsStyles.GuiTextures[isRepeatable
										? SuperFormsStyles.B2DEditorTextures.IconRepeatHover
										: SuperFormsStyles.B2DEditorTextures.IconRepeat
									]
								);

								for (j = 0; j < ActiveStamp.Width; j++) {
									if (SuperForms.Button("", tileButton)) {
										if (_tileEditingMode == TileEditingMode.StampsSelectingTile && Event.current.button == 0) {
											ActiveStamp.ParsedTileIds[i][j] = _selectTilePanelTileId;
										} else if (Event.current.button == 1) {
											ActiveStamp.ParsedTileIds[i][j] = 0;
										}

										GUI.FocusControl("");
									}

									int idOfTile = ActiveStamp.ParsedTileIds[i][j];

									Texture2D tileGraphic = SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.TransparentTile];

									if (idOfTile > 0) {
										TileProperties tilePropertiesOfMatchingSlot = ActiveTileset.Tiles.FirstOrDefault(x => x.Slot == idOfTile);

										tileGraphic = tilePropertiesOfMatchingSlot == null
											? SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]
											: ActiveTilesetTextures[tilePropertiesOfMatchingSlot.AnimationFrames[0]];
									}

									GUI.DrawTexture(
										new Rect(j * (tileSize + tilePadding) + topLeft.x, i * (tileSize + tilePadding) + topLeft.y, tileSize, tileSize),
										tileGraphic
									);
								}
							});
						}
					});

					UnParseStampGrid();
				});
			});
		}

		private void ParseStampGrid() {
			foreach (string tileRow in ActiveStamp.TileRows) {
				List<string> tiles = tileRow.Split(',').ToList();
				List<int> tileInts = new List<int>();

				foreach (string tile in tiles) {
					int parsedInt;
					tileInts.Add(int.TryParse(tile, out parsedInt) ? parsedInt : 0);
				}

				ActiveStamp.ParsedTileIds.Add(tileInts);
			}
		}

		private void UnParseStampGrid() {
			ActiveStamp.TileRows = new List<string>();
			foreach (List<int> row in ActiveStamp.ParsedTileIds) {
				ActiveStamp.TileRows.Add(string.Join(",", row));
			}
		}

		private void ResizeStampGrid() {
			foreach (List<int> row in ActiveStamp.ParsedTileIds) {
				while (ActiveStamp.Width > row.Count) {
					row.Add(0);
				}

				while (row.Count > ActiveStamp.Width) {
					row.RemoveAt(row.Count - 1);
				}
			}

			while (ActiveStamp.Height > ActiveStamp.ParsedTileIds.Count) {
				List<int> row = new List<int>();
				for (int i = 0; i < ActiveStamp.Width; i++) {
					row.Add(0);
				}

				ActiveStamp.ParsedTileIds.Add(row);
			}

			while (ActiveStamp.ParsedTileIds.Count > ActiveStamp.Height) {
				ActiveStamp.ParsedTileIds.RemoveAt(ActiveStamp.ParsedTileIds.Count - 1);
			}
		}

		private void DrawStampEditorPanel() {
			SuperForms.Region.Area(new Rect(_windowWidth - 260, 20, 260, _windowHeight - 110), () => {
				if (ActiveStamp != null) {
					SuperForms.Region.VerticalBox(() => {
						ActiveStamp.Name = SuperForms.StringField("Name", ActiveStamp.Name);
						ActiveStamp.Width = SuperForms.IntField("Width", ActiveStamp.Width);
						ActiveStamp.Height = SuperForms.IntField("Height", ActiveStamp.Height);

						ActiveStamp.Width = BoomerangUtils.MinValue(ActiveStamp.Width, 1);
						ActiveStamp.Height = BoomerangUtils.MinValue(ActiveStamp.Height, 1);

						ActiveStamp.RepeatableColumns.RemoveAll(item => item > ActiveStamp.Width - 1);
						ActiveStamp.RepeatableRows.RemoveAll(item => item > ActiveStamp.Height - 1);
					});
				}

				SuperForms.Space();

				SuperForms.Region.VerticalBox(() => {
					SuperForms.Region.Scroll("stampPropertiesScrollPosition", () => {
						TilesetEditorStamp toDelete = null;

						for (int i = 0; i < ActiveTileset.Stamps.Count; i++) {
							TilesetEditorStamp tilesetEditorStamp = ActiveTileset.Stamps[i];
							SuperForms.Begin.Horizontal();

							if (SuperForms.Button(tilesetEditorStamp.Name, _selectedStampIndex == i)) {
								_selectedStampIndex = i;
							}

							SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete, () => { toDelete = tilesetEditorStamp; });
							SuperForms.End.Horizontal();
						}

						if (toDelete != null) {
							ActiveTileset.Stamps.Remove(toDelete);
						}
					});
				}, GUILayout.Height(200));

				if (ActiveStamp != null) {
					SuperForms.Space();
					DrawTilesetSelectorPanel();
				}
			});
		}

		private void DrawBrushEditor() {
			SuperForms.Region.Area(new Rect(280, 20, _windowWidth - 550, _windowHeight - 110), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.Region.Scroll("TileEditorBrushEditor", () => {
						if (ActiveBrush == null) {
							SuperForms.FullBoxLabel(ActiveTileset.Brushes.Count == 0
								? "Add a Brush on the left"
								: "Select a Brush on the Right"
							);

							return;
						}

						if (ActiveBrush.ParsedDefinitions == null || ActiveBrush.ParsedDefinitions.Count == 0) {
							ParseActiveBrush();
						}

						DrawBrushEditorAtlasTiles();

						UnParseActiveBrush();
					});
				});
			});
		}

		private void DrawBrushEditorAtlasTiles() {
			SuperForms.BoxHeader("9-Slice Tiles");

			SuperForms.Region.Horizontal(() => {
				BrushAtlasButton(0);
				BrushAtlasButton(1);
				BrushAtlasButton(2);
				BrushAtlasButton(9);
			});

			SuperForms.Region.Horizontal(() => {
				BrushAtlasButton(3);
				BrushAtlasButton(4);
				BrushAtlasButton(5);
			});

			SuperForms.Region.Horizontal(() => {
				BrushAtlasButton(6);
				BrushAtlasButton(7);
				BrushAtlasButton(8);
			});

			SuperForms.BoxHeader("Narrow Path Tiles");

			SuperForms.Region.Horizontal(() => {
				BrushAtlasButton(19);
				BrushAtlasButton(10);
				BrushAtlasButton(11);
			});

			SuperForms.Region.Horizontal(() => {
				BrushAtlasButton(14);
				BrushAtlasButton(12);
				BrushAtlasButton(13);
			});

			SuperForms.Region.Horizontal(() => {
				BrushAtlasButton(18);
				BrushAtlasButton(16);
				BrushAtlasButton(15);
				BrushAtlasButton(17);
			});

			SuperForms.BoxHeader("Three-way Intersection Tiles");

			SuperForms.Region.Horizontal(() => {
				BrushAtlasButton(20);
				BrushAtlasButton(21);
				BrushAtlasButton(22);
				SuperForms.Space();
				BrushAtlasButton(23);
				BrushAtlasButton(24);
				BrushAtlasButton(25);
			});

			SuperForms.Space();

			SuperForms.Region.Horizontal(() => {
				BrushAtlasButton(26);
				BrushAtlasButton(27);
				BrushAtlasButton(28);
				SuperForms.Space();
				BrushAtlasButton(29);
				BrushAtlasButton(30);
				BrushAtlasButton(31);
			});

			SuperForms.BoxHeader("Four-way Intersection Tiles");

			SuperForms.Region.Horizontal(() => {
				BrushAtlasButton(32);
				BrushAtlasButton(33);
				BrushAtlasButton(34);
				BrushAtlasButton(35);
			});

			SuperForms.Space();

			SuperForms.Region.Horizontal(() => {
				BrushAtlasButton(36);
				BrushAtlasButton(37);
				BrushAtlasButton(38);
				BrushAtlasButton(39);
				SuperForms.Space();
				BrushAtlasButton(40);
				BrushAtlasButton(41);
			});

			SuperForms.Space();

			SuperForms.Region.Horizontal(() => {
				BrushAtlasButton(42);
				BrushAtlasButton(43);
				BrushAtlasButton(44);
				BrushAtlasButton(45);
			});

			SuperForms.Space();
			SuperForms.Region.Horizontal(() => { BrushAtlasButton(46); });
		}

		private void ParseActiveBrush() {
			ActiveBrush.ParsedDefinitions = new Dictionary<int, int>();

			if (string.IsNullOrEmpty(ActiveBrush.Definitions)) {
				for (int i = 0; i < 48; i++) {
					ActiveBrush.ParsedDefinitions.Add(i, 0);
				}
			} else {
				string[] definitions = ActiveBrush.Definitions.Split(';');

				foreach (string definition in definitions) {
					string[] parsedDefinition = definition.Split(':');

					if (parsedDefinition.Length == 2) {
						int key, value;

						ActiveBrush.ParsedDefinitions.Add(
							int.TryParse(parsedDefinition[0], out key) ? key : 0,
							int.TryParse(parsedDefinition[1], out value) ? value : 0
						);
					}
				}
			}
		}

		private void UnParseActiveBrush() {
			ActiveBrush.Definitions = "";
			foreach (KeyValuePair<int, int> definition in ActiveBrush.ParsedDefinitions) {
				ActiveBrush.Definitions += definition.Key + ":" + definition.Value + ";";
			}
		}

		private void BrushAtlasButton(int index) {
			int parsedDefinition = ActiveBrush.ParsedDefinitions[index];
			Texture2D textureToUse = _brushModeAtlasTextures[index];

			if (parsedDefinition > 0) {
				if (!ActiveTileset.TilesLookup.ContainsKey(parsedDefinition)) {
					ActiveBrush.ParsedDefinitions[index] = 0;
				} else {
					TileProperties tileProperties = ActiveTileset.TilesLookup[parsedDefinition];
					textureToUse = ActiveTilesetTextures[tileProperties.AnimationFrames[0]];
				}
			}

			GUIStyle guiTile = new GUIStyle {
				fixedWidth = 32,
				fixedHeight = 32,
				margin = new RectOffset(10, 10, 10, 10),
				normal = {
					background = textureToUse
				}
			};

			if (SuperForms.Button(guiTile)) {
				if (Event.current.button == 0 && _tileEditingMode == TileEditingMode.BrushesSelectingTile) {
					_tileEditingMode = TileEditingMode.Brushes;
					parsedDefinition = _selectTilePanelTileId;
				} else if (Event.current.button == 1) {
					parsedDefinition = 0;
				}
			}

			ActiveBrush.ParsedDefinitions[index] = parsedDefinition;
		}

		private void DrawBrushPropertiesPanel() {
			SuperForms.Region.Area(new Rect(_windowWidth - 260, 20, 260, _windowHeight - 110), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.Region.Scroll("TilesetEditorBrushList", () => {
						int toDelete = -1;
						foreach (TilesetEditorBrush tilesetEditorBrush in ActiveTileset.Brushes) {
							int i = ActiveTileset.Brushes.IndexOf(tilesetEditorBrush);

							SuperForms.Region.Horizontal(() => {
								SuperForms.Button(tilesetEditorBrush.Name, _selectedBrushIndex == i,
									() => { _selectedBrushIndex = ActiveTileset.Brushes.IndexOf(tilesetEditorBrush); });
								SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete, () => { toDelete = i; });
							});
						}

						if (toDelete >= 0) {
							ActiveTileset.Brushes.RemoveAt(toDelete);
						}
					});
				});

				if (ActiveBrush != null) {
					ActiveBrush.Name = SuperForms.StringField("Name", ActiveBrush.Name);
					SuperForms.Space();
					DrawTilesetSelectorPanel();
				}
			});
		}

		private float _tileSelectorScale = 1;

		private void DrawTilesetSelectorPanel() {
			SuperForms.Region.Horizontal(() => {
				SuperForms.Button("0.25x", Math.Abs(_tileSelectorScale - 0.25f) < 0.01, () => { _tileSelectorScale = 0.25f; });
				SuperForms.Button("0.5x", Math.Abs(_tileSelectorScale - 0.5f) < 0.01, () => { _tileSelectorScale = 0.5f; });
				SuperForms.Button("1x", Math.Abs(_tileSelectorScale - 1f) < 0.01, () => { _tileSelectorScale = 1f; });
				SuperForms.Button("2x", Math.Abs(_tileSelectorScale - 2f) < 0.01, () => { _tileSelectorScale = 2f; });
				SuperForms.Button("4x", Math.Abs(_tileSelectorScale - 4f) < 0.01, () => { _tileSelectorScale = 4f; });
				SuperForms.Button("8x", Math.Abs(_tileSelectorScale - 8f) < 0.01, () => { _tileSelectorScale = 8f; });
			});

			SuperForms.Region.Scroll("MapEditorTilePicker", () => {
				float tileSize = ActiveTileset.TileSize * _tileSelectorScale;
				float totalTileRows = Mathf.CeilToInt((float) ActiveTileset.Tiles.Count / ActiveTileset.EditorTilePerRow);

				SuperForms.BlockedArea(
					GUILayout.Width(ActiveTileset.EditorTilePerRow * tileSize + 20),
					GUILayout.Height(totalTileRows * tileSize)
				);

				if (Event.current.type == EventType.Repaint) {
					_tileSelectorContainer = GUILayoutUtility.GetLastRect();
				}

				SuperForms.Region.Area(_tileSelectorContainer, () => {
					for (int i = 0; i < totalTileRows; i++) {
						SuperForms.Begin.Horizontal();
						for (int j = 0; j < ActiveTileset.EditorTilePerRow; j++) {
							int tileIndex = i * ActiveTileset.EditorTilePerRow + j;

							if (tileIndex >= ActiveTileset.Tiles.Count) {
								continue;
							}

							SuperForms.Button("", () => {
								if (_tileEditingMode == TileEditingMode.Stamps) {
									_tileEditingMode = TileEditingMode.StampsSelectingTile;
								} else if (_tileEditingMode == TileEditingMode.Brushes) {
									_tileEditingMode = TileEditingMode.BrushesSelectingTile;
								}

								_selectTilePanelTileId = ActiveTileset.Tiles[tileIndex].Slot;
								GUI.FocusControl("");
							}, new GUIStyle {
								fixedWidth = tileSize,
								fixedHeight = tileSize
							});

							SuperForms.Texture(
								new Rect(j * tileSize, i * tileSize, tileSize, tileSize),
								ActiveTilesetTextures[ActiveTileset.Tiles[tileIndex].AnimationFrames[0]]
							);

							bool shouldHighlight = _selectTilePanelTileId == ActiveTileset.Tiles[tileIndex].Slot &&
							                       (_tileEditingMode == TileEditingMode.StampsSelectingTile ||
							                        _tileEditingMode == TileEditingMode.BrushesSelectingTile);

							if (shouldHighlight) {
								SuperForms.Texture(
									new Rect(j * tileSize, i * tileSize, tileSize, 2),
									SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.BoundingBox]
								);
								SuperForms.Texture(
									new Rect(j * tileSize, i * tileSize + tileSize - 2, tileSize, 2),
									SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.BoundingBox]
								);
								SuperForms.Texture(
									new Rect(j * tileSize, i * tileSize, 2, tileSize - 2),
									SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.BoundingBox]
								);
								SuperForms.Texture(
									new Rect(j * tileSize + tileSize - 2, i * tileSize, 2, tileSize - 2),
									SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.BoundingBox]
								);
							}
						}

						SuperForms.End.Horizontal();
					}
				});
			});
		}
	}
}