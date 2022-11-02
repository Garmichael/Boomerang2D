using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.MapEditor {
	public partial class MapEditor : EditorWindow {
		private void DrawMapEditingAreaTileEditorObject(MapLayerProperties mapLayer, TileEditorObject tileEditorEditorObject) {
			int tileSize = _allTileSets[mapLayer.Tileset].Properties.TileSize;

			if (ShouldBuildTextureForTileEditorObject(tileEditorEditorObject, mapLayer.Tileset)) {
				switch (tileEditorEditorObject.TileEditorType) {
					case TileEditorObjectType.Tile:
						BuildTextureForTileEditorObject(tileEditorEditorObject, mapLayer.Tileset);
						break;
					case TileEditorObjectType.Stamp:
						BuildTextureForStampObject(tileEditorEditorObject, mapLayer.Tileset);
						break;
					case TileEditorObjectType.Brush:
						BuildTextureForBrushObject(tileEditorEditorObject, mapLayer.Tileset);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			SuperForms.Texture(new Rect(
					tileEditorEditorObject.X * tileSize * MapEditingScale,
					tileEditorEditorObject.Y * tileSize * MapEditingScale,
					tileEditorEditorObject.Width * tileSize * MapEditingScale,
					tileEditorEditorObject.Height * tileSize * MapEditingScale)
				, tileEditorEditorObject.CachedEditorTexture);

			bool isSelected = _selectedTileEditorObjects.Contains(tileEditorEditorObject);
			bool isInCorrectMode = _editingMode == EditingMode.TilesTileSheet ||
			                       _editingMode == EditingMode.TilesStamps ||
			                       _editingMode == EditingMode.TilesBrushes;

			if (isSelected && isInCorrectMode) {
				float x = tileEditorEditorObject.X * tileSize * MapEditingScale;
				float y = tileEditorEditorObject.Y * tileSize * MapEditingScale;
				float width = tileEditorEditorObject.Width * tileSize * MapEditingScale;
				float height = tileEditorEditorObject.Height * tileSize * MapEditingScale;

				SuperForms.Texture(
					new Rect(x, y, width, 2),
					_viewBorderTexture
				);
				SuperForms.Texture(
					new Rect(x, y, 2, height),
					_viewBorderTexture
				);
				SuperForms.Texture(
					new Rect(x, y + height - 2, width, 2),
					_viewBorderTexture
				);
				SuperForms.Texture(
					new Rect(x + width - 2, y, 2, height),
					_viewBorderTexture
				);
			}
		}

		private void DrawMapEditingAreaActiveEditableBrush() {
			int tileSize = ActiveTileset.Properties.TileSize;
			float width = _brushEditorObjectBeingEdited.Width * tileSize * MapEditingScale;
			float height = _brushEditorObjectBeingEdited.Height * tileSize * MapEditingScale;

			SuperForms.Texture(
				new Rect(0, 0, width, height),
				_brushEditorObjectBeingEdited.CachedEditorTexture
			);
		}

		private void DrawMapEditingPanelTiles() {
			DrawMapEditingPanelLayers();

			if (ActiveMapLayer == null) {
				return;
			}

			if (ActiveMapLayer.MapLayerType == MapLayerType.DepthLayer) {
				SuperForms.ParagraphLabel("Cannot Add Tiles to a Depth Layer");
				return;
			}

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Tileset");

				SuperForms.Region.Horizontal(() => {
					SuperForms.Button(
						"Tiles",
						_editingMode == EditingMode.TilesTileSheet,
						() => {
							_editingMode = EditingMode.TilesTileSheet;
							_tileEditingMode = TileEditingMode.NoneSelected;
						}
					);

					SuperForms.Button("Stamps", _editingMode == EditingMode.TilesStamps, () => {
						_editingMode = EditingMode.TilesStamps;
						_tileEditingMode = TileEditingMode.NoneSelected;
					});

					SuperForms.Button("Brushes", _editingMode == EditingMode.TilesBrushes, () => {
						_editingMode = EditingMode.TilesBrushes;
						_tileEditingMode = TileEditingMode.NoneSelected;
					});
				});
			});

			if (_editingMode == EditingMode.TilesTileSheet) {
				DrawMapEditingPanelTilesTileSheet();
			} else if (_editingMode == EditingMode.TilesStamps) {
				DrawMapEditingPanelTilesStamps();
			} else if (_editingMode == EditingMode.TilesBrushes) {
				DrawMapEditingPanelTilesBrushes();
			}
		}

		private void DrawMapEditingPanelTilesTileSheet() {
			SuperForms.Region.Horizontal(() => {
				SuperForms.Label("Scale");
				_tileSelectorScale = SuperForms.DropDown(_tileSelectorScale, _tileSelectorScales);
			});

			float displayScale = _tileSelectorScales[_tileSelectorScale];

			SuperForms.Region.Vertical(() => {
				SuperForms.Region.Scroll("MapEditorTilePicker", () => {
					float tileSize = ActiveTileset.Properties.TileSize * displayScale;
					float tilesPerRow = ActiveTileset.Properties.EditorTilePerRow;
					float totalTileRows = Mathf.CeilToInt(ActiveTileset.Properties.Tiles.Count / tilesPerRow);

					SuperForms.BlockedArea(GUILayout.Width(tilesPerRow * tileSize), GUILayout.Height(totalTileRows * tileSize));

					if (Event.current.type == EventType.Repaint) {
						_tileSelectorContainer = GUILayoutUtility.GetLastRect();
					}

					SuperForms.Region.Area(_tileSelectorContainer, () => {
						for (int i = 0; i < totalTileRows; i++) {
							SuperForms.Begin.Horizontal();
							for (int j = 0; j < ActiveTileset.Properties.EditorTilePerRow; j++) {
								int tileIndex = i * ActiveTileset.Properties.EditorTilePerRow + j;

								if (tileIndex >= ActiveTileset.Properties.Tiles.Count) {
									continue;
								}

								SuperForms.Button("", () => {
									_tileEditingMode = TileEditingMode.PlacingTile;
									_selectedTileIndex = ActiveTileset.Properties.Tiles[tileIndex].Slot;
									_selectedTileEditorObjects.Clear();
									GUI.FocusControl("");
								}, new GUIStyle {
									fixedWidth = tileSize,
									fixedHeight = tileSize
								});

								SuperForms.Texture(
									new Rect(j * tileSize, i * tileSize, tileSize, tileSize),
									ActiveTileset.Textures[ActiveTileset.Properties.Tiles[tileIndex].AnimationFrames[0]]
								);

								if (_tileEditingMode == TileEditingMode.PlacingTile && _selectedTileIndex == ActiveTileset.Properties.Tiles[tileIndex].Slot) {
									SuperForms.Texture(
										new Rect(j * tileSize, i * tileSize, tileSize, 2),
										_viewBorderTexture
									);
									SuperForms.Texture(
										new Rect(j * tileSize, i * tileSize + tileSize - 2, tileSize, 2),
										_viewBorderTexture
									);
									SuperForms.Texture(
										new Rect(j * tileSize, i * tileSize, 2, tileSize - 2),
										_viewBorderTexture
									);
									SuperForms.Texture(
										new Rect(j * tileSize + tileSize - 2, i * tileSize, 2, tileSize - 2),
										_viewBorderTexture
									);
								}
							}

							SuperForms.End.Horizontal();
						}
					});
				});

				if (_selectedTileEditorObjects.Count == 1) {
					DrawSelectedTileProperties();
				}
			});
		}

		private void DrawSelectedTileProperties() {
			TileEditorObject selectedTileEditorEditorObject = _selectedTileEditorObjects[0];

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Selected Tile Object");
				if (selectedTileEditorEditorObject.TileEditorType == TileEditorObjectType.Brush) {
					SuperForms.Button("Edit", () => {
						_editingMode = EditingMode.TilesBrushes;
						_tileEditingMode = TileEditingMode.PaintingBrush;
						_brushEditorObjectBeingEdited = selectedTileEditorEditorObject;
						_brushEditorObjectBeingEdited.BrushTreatEdgeLikeSolid = selectedTileEditorEditorObject.BrushTreatEdgeLikeSolid;
						_selectedTileEditorObjects.Clear();

						MapLayerProperties brushMapLayer = null;

						foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
							if (mapLayer.TileEditorObjects.Contains(_brushEditorObjectBeingEdited)) {
								mapLayer.TileEditorObjects.Remove(_brushEditorObjectBeingEdited);
								brushMapLayer = mapLayer;
								break;
							}
						}

						if (brushMapLayer == null) {
							return;
						}

						_brushEditorObjectBeingEdited.X = BoomerangUtils.ClampValue(
							_brushEditorObjectBeingEdited.X, 0, ActiveMap.Width - _brushEditorObjectBeingEdited.Width);

						_brushEditorObjectBeingEdited.Y = BoomerangUtils.ClampValue(
							_brushEditorObjectBeingEdited.Y, 0, ActiveMap.Height - _brushEditorObjectBeingEdited.Height);

						if (_brushEditorObjectBeingEdited.ParsedBrushMapDefinition == null) {
							_brushEditorObjectBeingEdited.ParsedBrushMapDefinition = ParseBrushEditorObjectDefinition(_brushEditorObjectBeingEdited);
						}

						foreach (List<int> row in _brushEditorObjectBeingEdited.ParsedBrushMapDefinition) {
							int extraCellsNeeded = ActiveMap.Width - (_brushEditorObjectBeingEdited.X + _brushEditorObjectBeingEdited.Width);
							for (int i = 0; i < _brushEditorObjectBeingEdited.X; i++) {
								row.Insert(0, 0);
							}

							for (int i = 0; i < extraCellsNeeded; i++) {
								row.Add(0);
							}
						}

						List<int> newRowData = new List<int>();
						int extraRowsNeeded = ActiveMap.Height - (_brushEditorObjectBeingEdited.Y + _brushEditorObjectBeingEdited.Height);
						for (int j = 0; j < ActiveMap.Width; j++) {
							newRowData.Add(0);
						}

						for (int i = 0; i < _brushEditorObjectBeingEdited.Y; i++) {
							_brushEditorObjectBeingEdited.ParsedBrushMapDefinition.Insert(0, new List<int>(newRowData));
						}

						for (int i = 0; i < extraRowsNeeded; i++) {
							_brushEditorObjectBeingEdited.ParsedBrushMapDefinition.Add(new List<int>(newRowData));
						}

						_brushEditorObjectBeingEdited.X = 0;
						_brushEditorObjectBeingEdited.Y = 0;
						_brushEditorObjectBeingEdited.Width = _brushEditorObjectBeingEdited.ParsedBrushMapDefinition[0].Count;
						_brushEditorObjectBeingEdited.Height = _brushEditorObjectBeingEdited.ParsedBrushMapDefinition.Count;
						_brushEditorObjectBeingEdited.CachedEditorTexture = BuildPaintbrushMatteTexture(_brushEditorObjectBeingEdited, brushMapLayer.Tileset);
					});
				}

				selectedTileEditorEditorObject.Id = SuperForms.IntField("Id", selectedTileEditorEditorObject.Id);
				selectedTileEditorEditorObject.X = SuperForms.IntField("X", selectedTileEditorEditorObject.X);
				selectedTileEditorEditorObject.Y = SuperForms.IntField("Y", selectedTileEditorEditorObject.Y);

				if (selectedTileEditorEditorObject.TileEditorType == TileEditorObjectType.Brush) {
					SuperForms.Region.Horizontal(() => {
						SuperForms.Label("Width");
						SuperForms.Label(selectedTileEditorEditorObject.Width.ToString());
					});
					SuperForms.Region.Horizontal(() => {
						SuperForms.Label("Height");
						SuperForms.Label(selectedTileEditorEditorObject.Height.ToString());
					});
				} else {
					selectedTileEditorEditorObject.Width = SuperForms.IntField("Width", selectedTileEditorEditorObject.Width);
					selectedTileEditorEditorObject.Height = SuperForms.IntField("Height", selectedTileEditorEditorObject.Height);
				}

				selectedTileEditorEditorObject.Width = BoomerangUtils.MinValue(selectedTileEditorEditorObject.Width, 1);
				selectedTileEditorEditorObject.Height = BoomerangUtils.MinValue(selectedTileEditorEditorObject.Height, 1);

				SuperForms.Region.Horizontal(() => {
					SuperForms.Button("To Bottom", () => {
						ActiveMapLayer.TileEditorObjects.Remove(selectedTileEditorEditorObject);
						ActiveMapLayer.TileEditorObjects.Insert(0, selectedTileEditorEditorObject);
					});

					SuperForms.Button("Down", () => {
						int index = ActiveMapLayer.TileEditorObjects.IndexOf(selectedTileEditorEditorObject);
						if (index > 0) {
							BoomerangUtils.Swap(ActiveMapLayer.TileEditorObjects, index, index - 1);
						}
					});

					SuperForms.Button("Up", () => {
						int index = ActiveMapLayer.TileEditorObjects.IndexOf(selectedTileEditorEditorObject);
						if (index < ActiveMapLayer.TileEditorObjects.Count - 1) {
							BoomerangUtils.Swap(ActiveMapLayer.TileEditorObjects, index, index + 1);
						}
					});

					SuperForms.Button("To Top", () => {
						ActiveMapLayer.TileEditorObjects.Remove(selectedTileEditorEditorObject);
						ActiveMapLayer.TileEditorObjects.Add(selectedTileEditorEditorObject);
					});
				});
			});
		}

		private void DrawMapEditingPanelTilesStamps() {
			SuperForms.Region.Horizontal(() => {
				SuperForms.Label("Scale");
				_tileSelectorScale = SuperForms.DropDown(_tileSelectorScale, _tileSelectorScales);
			});

			float displayScale = _tileSelectorScales[_tileSelectorScale];

			SuperForms.Region.Scroll("MapEditorStampList", () => {
				float originalTileSize = ActiveTileset.Properties.TileSize;
				float tileSize = originalTileSize * displayScale;

				for (int i = 0; i < ActiveTileset.Properties.Stamps.Count; i++) {
					TilesetEditorStamp tilesetEditorStamp = ActiveTileset.Properties.Stamps[i];

					Texture2D textureToUse = ActiveTileset.StampTextures[i];

					if (_tileEditingMode == TileEditingMode.PlacingStamp && _selectedTilesetStampIndex == i) {
						bool shouldBuildNewTexture = _cachedSelectedStampBorderTexture == null ||
						                             _cachedSelectedStampBorderTextureDimensions.x != tilesetEditorStamp.Width ||
						                             _cachedSelectedStampBorderTextureDimensions.y != tilesetEditorStamp.Height;

						if (shouldBuildNewTexture) {
							Vector2Int textureSize = new Vector2Int(
								(int) (tilesetEditorStamp.Width * originalTileSize),
								(int) (tilesetEditorStamp.Height * originalTileSize)
							);

							_cachedSelectedStampBorderTexture = new Texture2D(textureSize.x, textureSize.y) {
								wrapMode = TextureWrapMode.Clamp,
								filterMode = FilterMode.Point
							};

							Graphics.CopyTexture(textureToUse, _cachedSelectedStampBorderTexture);
							Color32[] pixels = _cachedSelectedStampBorderTexture.GetPixels32();

							for (int c = 0; c < pixels.Length; c++) {
								int row = Mathf.CeilToInt((float) (c + 1) / textureSize.x);
								int rowStart = row * textureSize.x - textureSize.x;
								int rowEnd = row * textureSize.x;

								bool isInTopRow = row <= 2;
								bool isInBottomRow = row >= textureSize.y - 1;
								bool isInLeftCol = c >= rowStart && c <= rowStart + 1;
								bool isInRightCol = c <= rowEnd && c >= rowEnd - 2;

								if (isInTopRow || isInBottomRow || isInLeftCol || isInRightCol) {
									pixels[c] = new Color32(0, 255, 0, 200);
								}
							}

							_cachedSelectedStampBorderTexture.SetPixels32(pixels);
							_cachedSelectedStampBorderTexture.Apply();

							_cachedSelectedStampBorderTextureDimensions = new Vector2Int(tilesetEditorStamp.Width, tilesetEditorStamp.Height);
						}

						textureToUse = _cachedSelectedStampBorderTexture;
					}

					GUIStyle stampButton = new GUIStyle {
						fixedWidth = tilesetEditorStamp.Width * tileSize,
						fixedHeight = tilesetEditorStamp.Height * tileSize,
						margin = new RectOffset(10, 10, 10, 10),
						normal = {
							background = textureToUse
						}
					};

					if (SuperForms.Button(stampButton)) {
						_tileEditingMode = TileEditingMode.PlacingStamp;
						_selectedTilesetStampIndex = i;
						_selectedTileEditorObjects.Clear();
					}
				}
			});

			if (_selectedTileEditorObjects.Count == 1) {
				DrawSelectedTileProperties();
			}
		}

		private void DrawMapEditingPanelTilesBrushes() {
			SuperForms.Region.Scroll("MapEditorBrushList", () => {
				if (_tileEditingMode == TileEditingMode.NoneSelected) {
					for (int index = 0; index < ActiveTileset.Properties.Brushes.Count; index++) {
						if (SuperForms.Button(ActiveTileset.Properties.Brushes[index].Name)) {
							_selectedTileEditorObjects.Clear();
							_tileEditingMode = TileEditingMode.PaintingBrush;

							int tileSize = ActiveTileset.Properties.TileSize;
							float scale = GameProperties.PixelsPerUnit / tileSize;

							int width = Mathf.CeilToInt(ActiveMap.Width * scale);
							int height = Mathf.CeilToInt(ActiveMap.Height * scale);

							List<List<int>> parsedDefinition = new List<List<int>>();

							List<int> rowData = new List<int>();
							for (int i = 0; i < width; i++) {
								rowData.Add(0);
							}

							for (int j = 0; j < height; j++) {
								parsedDefinition.Add(new List<int>(rowData));
							}

							_brushEditorObjectBeingEdited = new TileEditorObject {
								TileEditorType = TileEditorObjectType.Brush,
								Id = index,
								X = 0,
								Y = 0,
								Width = width,
								Height = height,
								CachedEditorTexture = null,
								CachedEditorTextureInfo = null,
								BrushMapDefinition = "",
								ParsedBrushMapDefinition = parsedDefinition
							};
						}
					}
				}

				if (_tileEditingMode == TileEditingMode.PaintingBrush) {
					SuperForms.BoxSubHeader("Painting Controls");
					SuperForms.Space();

					SuperForms.Region.Horizontal(() => {
						SuperForms.Button("Brush", _brushMode == BrushModes.Brush, () => { _brushMode = BrushModes.Brush; });
						SuperForms.Button("Area", _brushMode == BrushModes.Area, () => { _brushMode = BrushModes.Area; });
					});

					_brushEditorObjectBeingEdited.BrushTreatEdgeLikeSolid = SuperForms.Checkbox(
						"Treat Edge as Solid",
						_brushEditorObjectBeingEdited.BrushTreatEdgeLikeSolid
					);
					
					SuperForms.Space();
					SuperForms.Space();
					SuperForms.Space();

					SuperForms.Button("Apply Brush", () => {
						_tileEditingMode = TileEditingMode.NoneSelected;
						ApplyBrushMatteAsEditorObject(_brushEditorObjectBeingEdited);
					});

					SuperForms.Button("Cancel", () => { _tileEditingMode = TileEditingMode.NoneSelected; });
				}
			});

			if (_selectedTileEditorObjects.Count == 1) {
				DrawSelectedTileProperties();
			}
		}
	}
}