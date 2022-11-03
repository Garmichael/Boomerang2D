using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.MapEditor {
	public partial class MapEditor : EditorWindow {
		private void HandleInput() {
			EditorInputManager.InputValues inputValues = EditorInputManager.GetInput();
			_mapMousePosition = inputValues.MouseLocation + _mapEditingAreaScrollPosition + new Vector2(_mapEditingAreaRect.x, -_mapEditingAreaRect.y);
			Vector2Int mapMousePosition = new Vector2Int(
				Mathf.FloorToInt(_mapMousePosition.x / MapEditingScale),
				Mathf.FloorToInt(_mapMousePosition.y / MapEditingScale)
			);

			bool mouseIsInMapEditingArea = inputValues.MouseLocation.x > _mapEditingAreaRect.x &&
			                               inputValues.MouseLocation.y > _mapEditingAreaRect.y &&
			                               inputValues.MouseLocation.x < _mapEditingAreaRect.x + _mapEditingAreaRect.width &&
			                               inputValues.MouseLocation.y < _mapEditingAreaRect.y + _mapEditingAreaRect.height;

			InputSelectionOrPanKeyboardControls(inputValues);
			InputHandleScrolling(inputValues, mapMousePosition);
			InputMiddleMousePanning(inputValues);

			bool isInTileSelectionMode = _editingMode == EditingMode.TilesTileSheet ||
			                             _editingMode == EditingMode.TilesStamps ||
			                             _editingMode == EditingMode.TilesBrushes;

			if (isInTileSelectionMode && _inputMode == InputMode.Selection) {
				if (_tileEditingMode == TileEditingMode.NoneSelected) {
					if (!inputValues.ControlHeld) {
						_selectionObjectJustCloned = false;
					}

					if (inputValues.MouseLeftDown && mouseIsInMapEditingArea) {
						InputVerifyTileSelection(mapMousePosition);
					}

					if (inputValues.MouseLeftHeld && mouseIsInMapEditingArea) {
						if (_inputSelectionBoxStarted) {
							InputDrawSelectionBox(mapMousePosition);
						} else {
							bool isBrushMode = _selectedTileEditorObjects.Count == 1 &&
							                   _selectedTileEditorObjects[0].TileEditorType == TileEditorObjectType.Brush;

							if (inputValues.ShiftHeld && _selectedTileEditorObjects.Count == 1 && !isBrushMode) {
								InputScaleTileEditorObject(mapMousePosition);
							} else if (inputValues.ControlHeld && !_selectionObjectJustCloned && _selectedTileEditorObjects.Count > 0) {
								InputCloneTileEditorObjects();
							} else {
								InputDragTileEditorObjects(mapMousePosition);
								_inputHasStartedScaling = false;
							}
						}
					}

					if (inputValues.MouseLeftUp) {
						if (_inputSelectionBoxStarted) {
							InputSelectTilesUnderSelectionBox(mapMousePosition);
							_inputSelectionBoxStarted = false;
						} else {
							_inputHoveredTileEditorOnMouseDown = null;
							_inputHasStartedScaling = false;
						}
					}

					if (inputValues.MouseRightDown && mouseIsInMapEditingArea) {
						InputRemoveTileEditorObject(mapMousePosition);
					}
				}

				if (_tileEditingMode == TileEditingMode.PlacingTile) {
					if (mouseIsInMapEditingArea) {
						InputShowSelectedTileUnderMouse(mapMousePosition);
					}

					if (inputValues.MouseLeftDown && mouseIsInMapEditingArea) {
						InputPlaceTileEditorObject(mapMousePosition);
					}

					if (inputValues.MouseRightDown) {
						InputDeselectSelectedTile();
					}
				}

				if (_tileEditingMode == TileEditingMode.PlacingStamp) {
					if (mouseIsInMapEditingArea) {
						InputShowSelectedStampUnderMouse(mapMousePosition);
					}

					if (inputValues.MouseLeftDown && mouseIsInMapEditingArea) {
						InputPlaceStampEditorObject(mapMousePosition);
					}

					if (inputValues.MouseRightDown) {
						InputDeselectSelectedTile();
					}
				}

				if (_tileEditingMode == TileEditingMode.PaintingBrush && mouseIsInMapEditingArea) {
					if (_brushMode == BrushModes.Brush) {
						int tileSize = _allTileSets[ActiveMapLayer.Tileset].Properties.TileSize;
						Vector2Int clickedPosition = new Vector2Int(
							Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
							Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
						);

						bool wasModified = false;

						if (inputValues.MouseLeftHeld &&
						    _brushEditorObjectBeingEdited.ParsedBrushMapDefinition[clickedPosition.y][clickedPosition.x] == 0) {
							InputPaintBrushMatte(mapMousePosition);
							wasModified = true;
						}

						if (inputValues.MouseRightHeld &&
						    _brushEditorObjectBeingEdited.ParsedBrushMapDefinition[clickedPosition.y][clickedPosition.x] == 1) {
							InputEraseBrushMatte(mapMousePosition);
							wasModified = true;
						}

						if (wasModified) {
							_brushEditorObjectBeingEdited.CachedEditorTexture = BuildPaintbrushMatteTexture(
								_brushEditorObjectBeingEdited, ActiveTileset.Properties.Name
							);
						}
					}

					if (_brushMode == BrushModes.Area) {
						if (inputValues.MouseLeftDown && _brushAreaMode == 0) {
							_brushAreaStartLocation = mapMousePosition;
							_brushAreaMode = 1;
						}

						if (inputValues.MouseLeftUp && _brushAreaMode == 1) {
							InputPaintAreaMatte(mapMousePosition);
							_brushEditorObjectBeingEdited.CachedEditorTexture = BuildPaintbrushMatteTexture(
								_brushEditorObjectBeingEdited, ActiveTileset.Properties.Name
							);
							_brushAreaMode = 0;
						}

						if (inputValues.MouseRightDown && _brushAreaMode == 0) {
							_brushAreaStartLocation = mapMousePosition;
							_brushAreaMode = 2;
						}

						if (inputValues.MouseRightUp && _brushAreaMode == 2) {
							InputEraseAreaMatte(mapMousePosition);
							_brushEditorObjectBeingEdited.CachedEditorTexture = BuildPaintbrushMatteTexture(
								_brushEditorObjectBeingEdited, ActiveTileset.Properties.Name
							);
							_brushAreaMode = 0;
						}
					}
				}
			}

			if (_inputMode == InputMode.Selection) {
				if (_editingMode == EditingMode.Actors) {
					if (_actorEditingMode == ActorEditingMode.NoneSelected) {
						if (!inputValues.ControlHeld) {
							_selectionObjectJustCloned = false;
						}

						if (inputValues.MouseLeftDown && mouseIsInMapEditingArea) {
							InputVerifyActorSelection(mapMousePosition);
						}

						if (inputValues.MouseLeftHeld && mouseIsInMapEditingArea) {
							if (_inputSelectionBoxStarted) {
								InputDrawSelectionBox(mapMousePosition);
							} else {
								if (inputValues.ControlHeld && !_selectionObjectJustCloned && _selectedActorObjects.Count > 0) {
									InputCloneActorObjects();
								} else {
									InputDragActorObjects(mapMousePosition);
								}
							}
						}

						if (inputValues.MouseLeftUp) {
							if (_inputSelectionBoxStarted) {
								InputSelectActorsUnderSelectionBox(mapMousePosition);
								_inputSelectionBoxStarted = false;
							} else {
								_inputHoveredActorOnMouseDown = null;
							}
						}

						if (inputValues.MouseRightDown && mouseIsInMapEditingArea) {
							InputRemoveActorObject(mapMousePosition);
						}
					}

					if (_actorEditingMode == ActorEditingMode.PlacingActor) {
						if (mouseIsInMapEditingArea) {
							InputShowSelectedActorUnderMouse(mapMousePosition);
						}

						if (inputValues.MouseLeftDown && mouseIsInMapEditingArea) {
							InputPlaceSelectedActor(mapMousePosition);
						}

						if (inputValues.MouseRightDown) {
							InputDeselectSelectedActor();
						}
					}
				}

				if (_editingMode == EditingMode.Prefabs) {
					if (_prefabEditingMode == PrefabEditingMode.NoneSelected) {
						if (!inputValues.ControlHeld) {
							_selectionObjectJustCloned = false;
						}

						if (inputValues.MouseLeftDown && mouseIsInMapEditingArea) {
							InputVerifyPrefabSelection(mapMousePosition);
						}

						if (inputValues.MouseLeftHeld && mouseIsInMapEditingArea) {
							if (_inputSelectionBoxStarted) {
								InputDrawSelectionBox(mapMousePosition);
							} else {
								if (inputValues.ControlHeld && !_selectionObjectJustCloned && _selectedPrefabObjects.Count > 0) {
									InputClonePrefabObjects();
								} else {
									InputDragPrefabObjects(mapMousePosition);
								}
							}
						}

						if (inputValues.MouseLeftUp) {
							if (_inputSelectionBoxStarted) {
								InputSelectPrefabsUnderSelectionBox(mapMousePosition);
								_inputSelectionBoxStarted = false;
							} else {
								_inputHoveredPrefabOnMouseDown = null;
							}
						}

						if (inputValues.MouseRightDown && mouseIsInMapEditingArea) {
							InputRemovePrefabObject(mapMousePosition);
						}
					}

					if (_prefabEditingMode == PrefabEditingMode.PlacingPrefab) {
						if (mouseIsInMapEditingArea) {
							InputShowSelectedPrefabUnderMouse(mapMousePosition);
						}

						if (inputValues.MouseLeftDown && mouseIsInMapEditingArea) {
							InputPlaceSelectedPrefab(mapMousePosition);
						}

						if (inputValues.MouseRightDown) {
							InputDeselectSelectedPrefab();
						}
					}
				}

				if (_editingMode == EditingMode.Views) {
					if (!inputValues.ControlHeld) {
						_selectionObjectJustCloned = false;
					}

					if (inputValues.MouseLeftDown && mouseIsInMapEditingArea) {
						InputVerifyViewSelection(mapMousePosition);
					}

					if (inputValues.MouseLeftHeld && mouseIsInMapEditingArea) {
						if (_inputSelectionBoxStarted) {
							InputDrawSelectionBox(mapMousePosition);
						} else {
							if (inputValues.ShiftHeld && _selectedMapViews.Count == 1) {
								InputScaleView(mapMousePosition);
							} else if (inputValues.ControlHeld && !_selectionObjectJustCloned && _selectedMapViews.Count > 0) {
								InputCloneViews();
							} else {
								InputDragViews(mapMousePosition);
								_inputHasStartedScaling = false;
							}
						}
					}

					if (inputValues.MouseLeftUp) {
						if (_inputSelectionBoxStarted) {
							InputSelectViewsUnderSelectionBox(mapMousePosition);
							_inputSelectionBoxStarted = false;
						} else {
							_inputHoveredViewOnMouseDown = null;
							_inputHasStartedScaling = false;
						}
					}

					if (inputValues.MouseRightDown && mouseIsInMapEditingArea) {
						InputRemoveViewObject(mapMousePosition);
					}
				}

				if (_editingMode == EditingMode.Regions) {
					if (!inputValues.ControlHeld) {
						_selectionObjectJustCloned = false;
					}

					if (inputValues.MouseLeftDown && mouseIsInMapEditingArea) {
						InputVerifyRegionSelection(mapMousePosition);
					}

					if (inputValues.MouseLeftHeld && mouseIsInMapEditingArea) {
						if (_inputSelectionBoxStarted) {
							InputDrawSelectionBox(mapMousePosition);
						} else {
							if (inputValues.ShiftHeld && _selectedMapRegions.Count == 1) {
								InputScaleRegion(mapMousePosition);
							} else if (inputValues.ControlHeld && !_selectionObjectJustCloned && _selectedMapRegions.Count > 0) {
								InputCloneRegions();
							} else {
								InputDragRegions(mapMousePosition);
								_inputHasStartedScaling = false;
							}
						}
					}

					if (inputValues.MouseLeftUp) {
						if (_inputSelectionBoxStarted) {
							InputSelectRegionsUnderSelectionBox(mapMousePosition);
							_inputSelectionBoxStarted = false;
						} else {
							_inputHoveredRegionOnMouseDown = null;
							_inputHasStartedScaling = false;
						}
					}

					if (inputValues.MouseRightDown && mouseIsInMapEditingArea) {
						InputRemoveRegionObject(mapMousePosition);
					}
				}
			}
		}

		private void InputSelectionOrPanKeyboardControls(EditorInputManager.InputValues inputValues) {
			if (inputValues.KeyPressed == KeyCode.Q) {
				_inputMode = InputMode.Selection;
			}

			if (inputValues.KeyPressed == KeyCode.W) {
				_inputMode = InputMode.Pan;
			}
		}

		private void InputMiddleMousePanning(EditorInputManager.InputValues inputValues) {
			if (inputValues.MouseMiddleHeld || _inputMode == InputMode.Pan && inputValues.MouseLeftHeld) {
				_mapEditingAreaScrollPosition -= inputValues.MouseMovementDistance;
			}
		}

		private void InputHandleScrolling(EditorInputManager.InputValues inputValues, Vector2Int mapMousePositionInTiles) {
			bool canZoom = _totalTime - _lastZoomTime > ZoomDelay;

			if (inputValues.ScrolledUp && canZoom && _selectedMapScaleIndex < _mapScales.Count - 1) {
				InputScrollZoomIn(mapMousePositionInTiles);
			}

			if (inputValues.ScrolledDown && canZoom && _selectedMapScaleIndex > 0) {
				InputScrollZoomOut(mapMousePositionInTiles);
			}
		}

		private void InputScrollZoomIn(Vector2Int mapMousePositionInTiles) {
			_selectedMapScaleIndex += 1;
			_selectedMapScaleIndex = BoomerangUtils.MaxValue(_selectedMapScaleIndex, _mapScales.Count - 1);
			_lastZoomTime = _totalTime;

			float tilesWide = Mathf.Floor(_mapEditingAreaRect.width / MapEditingScale);
			float tilesHigh = Mathf.Floor(_mapEditingAreaRect.height / MapEditingScale);
			float leftTilePosition = mapMousePositionInTiles.x - tilesWide / 2;
			float leftEdge = leftTilePosition * MapEditingScale;

			float topTilePosition = mapMousePositionInTiles.y - tilesHigh / 2;
			float topEdge = topTilePosition * MapEditingScale;

			_mapEditingAreaScrollPosition = new Vector2(leftEdge, topEdge);
		}

		private void InputScrollZoomOut(Vector2Int mapMousePositionInTiles) {
			_selectedMapScaleIndex -= 1;
			_selectedMapScaleIndex = BoomerangUtils.MinValue(_selectedMapScaleIndex, 0);
			_lastZoomTime = _totalTime;

			float tilesWide = Mathf.Floor(_mapEditingAreaRect.width / MapEditingScale);
			float tilesHigh = Mathf.Floor(_mapEditingAreaRect.height / MapEditingScale);
			float leftTilePosition = mapMousePositionInTiles.x - tilesWide / 2;
			float leftEdge = leftTilePosition * MapEditingScale;

			float topTilePosition = mapMousePositionInTiles.y - tilesHigh / 2;
			float topEdge = topTilePosition * MapEditingScale;

			_mapEditingAreaScrollPosition = new Vector2(leftEdge, topEdge);
		}


		private void InputVerifyTileSelection(Vector2Int mapMousePosition) {
			_inputMouseDownPosition = mapMousePosition;
			_inputHoveredTileEditorOnMouseDown = InputMouseIsHoveringATileObject(mapMousePosition);

			if (_inputHoveredTileEditorOnMouseDown == null) {
				_inputSelectionBoxStarted = true;
				return;
			}

			if (!_selectedTileEditorObjects.Contains(_inputHoveredTileEditorOnMouseDown)) {
				_selectedTileEditorObjects.Clear();
				_selectedTileEditorObjects.Add(_inputHoveredTileEditorOnMouseDown);
			}

			_selectedObjectOffsets.Clear();

			foreach (TileEditorObject tileEditorObject in _selectedTileEditorObjects) {
				MapLayerProperties parentLayer = GetTileEditorObjectParentLayer(tileEditorObject);
				int tileSize = _allTileSets[parentLayer.Tileset].Properties.TileSize;
				Vector2Int clickedPosition = new Vector2Int(
					Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
					Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
				);

				_selectedObjectOffsets.Add(new Vector2Int(
					clickedPosition.x - tileEditorObject.X,
					clickedPosition.y - tileEditorObject.Y
				));
			}

			if (_selectedTileEditorObjects.Count == 1) {
				foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
					if (mapLayer.TileEditorObjects.Contains(_selectedTileEditorObjects[0])) {
						_selectedMapLayerIndex = ActiveMap.Layers.IndexOf(mapLayer);
						break;
					}
				}
			}
		}

		private TileEditorObject InputMouseIsHoveringATileObject(Vector2Int mapMousePosition) {
			TileEditorObject hoveredTileEditorEditorObject = null;
			bool isHovered = false;

			for (int i = ActiveMap.Layers.Count - 1; i >= 0; i--) {
				MapLayerProperties mapLayer = ActiveMap.Layers[i];
				int tileSize = _allTileSets[mapLayer.Tileset].Properties.TileSize;
				Vector2Int clickedPosition = new Vector2Int(
					Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
					Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
				);

				if (mapLayer.EditorIsVisible && !mapLayer.EditorIsLocked && mapLayer.MapLayerType == MapLayerType.Normal) {
					for (int j = mapLayer.TileEditorObjects.Count - 1; j >= 0; j--) {
						TileEditorObject tileEditorEditorObject = mapLayer.TileEditorObjects[j];

						isHovered = clickedPosition.x >= tileEditorEditorObject.X &&
						            clickedPosition.x < tileEditorEditorObject.X + tileEditorEditorObject.Width &&
						            clickedPosition.y >= tileEditorEditorObject.Y &&
						            clickedPosition.y < tileEditorEditorObject.Y + tileEditorEditorObject.Height;

						if (isHovered) {
							hoveredTileEditorEditorObject = tileEditorEditorObject;
							break;
						}
					}
				}

				if (isHovered) {
					break;
				}
			}

			return hoveredTileEditorEditorObject;
		}

		private void InputDragTileEditorObjects(Vector2Int mapMousePosition) {
			for (int i = 0; i < _selectedTileEditorObjects.Count; i++) {
				TileEditorObject tileEditorEditorObject = _selectedTileEditorObjects[i];

				MapLayerProperties parentMapLayer = GetTileEditorObjectParentLayer(tileEditorEditorObject);
				int tileSize = _allTileSets[parentMapLayer.Tileset].Properties.TileSize;
				Vector2Int clickedPosition = new Vector2Int(
					Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
					Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
				);

				tileEditorEditorObject.X = clickedPosition.x - _selectedObjectOffsets[i].x;
				tileEditorEditorObject.Y = clickedPosition.y - _selectedObjectOffsets[i].y;
			}
		}

		private void InputScaleTileEditorObject(Vector2Int mapMousePosition) {
			TileEditorObject tileEditorEditorObject = _selectedTileEditorObjects[0];
			MapLayerProperties parentMapLayer = GetTileEditorObjectParentLayer(tileEditorEditorObject);
			int tileSize = _allTileSets[parentMapLayer.Tileset].Properties.TileSize;
			Vector2Int clickedPosition = new Vector2Int(
				Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
				Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
			);

			if (!_inputHasStartedScaling) {
				_scalingObjectOriginTopLeft = new Vector2Int(clickedPosition.x, clickedPosition.y);
				_scalingObjectOriginalPosition = new Vector2Int(tileEditorEditorObject.X, tileEditorEditorObject.Y);
				_scalingObjectOriginalSize = new Vector2Int(tileEditorEditorObject.Width, tileEditorEditorObject.Height);

				_scalingObjectWidthLocked = false;
				_scalingObjectHeightLocked = false;

				bool isBorderTile = _scalingObjectOriginTopLeft.x == tileEditorEditorObject.X ||
				                    _scalingObjectOriginTopLeft.y == tileEditorEditorObject.Y ||
				                    _scalingObjectOriginTopLeft.x == tileEditorEditorObject.X + tileEditorEditorObject.Width - 1 ||
				                    _scalingObjectOriginTopLeft.y == tileEditorEditorObject.Y + tileEditorEditorObject.Height - 1;

				if (!isBorderTile) {
					_scalingObjectOriginTopLeft = new Vector2Int(-1, -1);
					return;
				}

				int topRow = tileEditorEditorObject.Y;
				int bottomRow = tileEditorEditorObject.Y + tileEditorEditorObject.Height - 1;
				int leftColumn = tileEditorEditorObject.X;
				int rightColumn = tileEditorEditorObject.X + tileEditorEditorObject.Width - 1;

				bool isMoreThanTwoRows = tileEditorEditorObject.Height >= 3;
				bool isMoreThanTwoColumns = tileEditorEditorObject.Width >= 3;

				bool isOnRightColumn = _scalingObjectOriginTopLeft.x == rightColumn;
				bool isOnBottomRow = _scalingObjectOriginTopLeft.y == bottomRow;
				bool isOnLeftColumn = _scalingObjectOriginTopLeft.x == leftColumn;
				bool isOnTopRow = _scalingObjectOriginTopLeft.y == topRow;

				_objectIsScalingWidth = (isOnLeftColumn || isOnRightColumn) && (!isMoreThanTwoRows || !isOnTopRow && !isOnBottomRow);
				_objectIsScalingHeight = (isOnBottomRow || isOnTopRow) && (!isMoreThanTwoColumns || !isOnRightColumn && !isOnLeftColumn);

				if ((isOnLeftColumn || isOnRightColumn) && (isOnBottomRow || isOnTopRow)) {
					_objectIsScalingWidth = true;
					_objectIsScalingHeight = true;
				}

				_scalingObjectWidthLocked = _objectIsScalingWidth && isMoreThanTwoColumns;
				_objectScalingWidthPositive = _scalingObjectWidthLocked && isOnRightColumn;

				_scalingObjectHeightLocked = _objectIsScalingHeight && isMoreThanTwoRows;
				_objectScalingHeightPositive = _scalingObjectHeightLocked && isOnBottomRow;

				_inputHasStartedScaling = true;
			}

			if (_objectIsScalingWidth) {
				if (!_scalingObjectWidthLocked) {
					_scalingObjectWidthLocked = clickedPosition.x != _scalingObjectOriginalPosition.x;
					_objectScalingWidthPositive = clickedPosition.x > _scalingObjectOriginalPosition.x;
				} else {
					if (_objectScalingWidthPositive) {
						if (clickedPosition.x >= _scalingObjectOriginalPosition.x) {
							tileEditorEditorObject.Width = clickedPosition.x - _scalingObjectOriginalPosition.x + 1;
						}
					} else {
						if (clickedPosition.x < _scalingObjectOriginalPosition.x + _scalingObjectOriginalSize.x) {
							tileEditorEditorObject.X = clickedPosition.x;
							tileEditorEditorObject.Width = _scalingObjectOriginTopLeft.x - tileEditorEditorObject.X + _scalingObjectOriginalSize.x;
						}
					}
				}
			}

			if (_objectIsScalingHeight) {
				if (!_scalingObjectHeightLocked) {
					_scalingObjectHeightLocked = clickedPosition.y != _scalingObjectOriginalPosition.y;
					_objectScalingHeightPositive = clickedPosition.y > _scalingObjectOriginalPosition.y;
				} else {
					if (_objectScalingHeightPositive) {
						if (clickedPosition.y >= _scalingObjectOriginalPosition.y) {
							tileEditorEditorObject.Height = clickedPosition.y - _scalingObjectOriginalPosition.y + 1;
						}
					} else {
						if (clickedPosition.y < _scalingObjectOriginalPosition.y + _scalingObjectOriginalSize.y) {
							tileEditorEditorObject.Y = clickedPosition.y;
							tileEditorEditorObject.Height = _scalingObjectOriginTopLeft.y - tileEditorEditorObject.Y + _scalingObjectOriginalSize.y;
						}
					}
				}
			}
		}

		private void InputRemoveTileEditorObject(Vector2Int mapMousePosition) {
			bool isHovered = false;
			bool deletedObjectWasInSelection = false;

			for (int i = ActiveMap.Layers.Count - 1; i >= 0; i--) {
				MapLayerProperties thisLayer = ActiveMap.Layers[i];

				if (thisLayer.EditorIsLocked || !thisLayer.EditorIsVisible) {
					continue;
				}
				
				for (int j = thisLayer.TileEditorObjects.Count - 1; j >= 0; j--) {
					int tileSize = _allTileSets[thisLayer.Tileset].Properties.TileSize;
					TileEditorObject tileEditorEditorObject = thisLayer.TileEditorObjects[j];

					Vector2Int clickedPosition = new Vector2Int(
						Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
						Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
					);

					isHovered = clickedPosition.x >= tileEditorEditorObject.X &&
					            clickedPosition.x < tileEditorEditorObject.X + tileEditorEditorObject.Width &&
					            clickedPosition.y >= tileEditorEditorObject.Y &&
					            clickedPosition.y < tileEditorEditorObject.Y + tileEditorEditorObject.Height;

					if (isHovered) {
						deletedObjectWasInSelection = _selectedTileEditorObjects.Contains(tileEditorEditorObject);
						if (!deletedObjectWasInSelection) {
							_selectedTileEditorObjects.Clear();
							_selectedTileEditorObjects.Add(tileEditorEditorObject);
						}

						break;
					}
				}

				if (isHovered) {
					break;
				}
			}

			if (isHovered && _selectedTileEditorObjects.Count > 0) {
				foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
					List<TileEditorObject> markedForDeletion = new List<TileEditorObject>();

					foreach (TileEditorObject tileEditorObject in _selectedTileEditorObjects) {
						if (mapLayer.TileEditorObjects.Contains(tileEditorObject)) {
							markedForDeletion.Add(tileEditorObject);
						}
					}

					foreach (TileEditorObject tileEditorObject in markedForDeletion) {
						mapLayer.TileEditorObjects.Remove(tileEditorObject);
					}
				}
			}

			if (deletedObjectWasInSelection) {
				_selectedTileEditorObjects.Clear();
			}
		}

		private void InputShowSelectedTileUnderMouse(Vector2Int mapMousePosition) {
			int tileSize = _allTileSets[ActiveMapLayer.Tileset].Properties.TileSize;
			Vector2Int clickedPosition = new Vector2Int(
				Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
				Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
			);

			float x = clickedPosition.x * tileSize * MapEditingScale + _mapEditingAreaRect.x - _mapEditingAreaScrollPosition.x;
			float y = clickedPosition.y * tileSize * MapEditingScale + _mapEditingAreaRect.y - _mapEditingAreaScrollPosition.y;
			float width = tileSize * MapEditingScale;
			float height = tileSize * MapEditingScale;

			SuperForms.Texture(
				new Rect(x, y, width, height),
				ActiveTileset.Textures[ActiveTileset.Properties.TilesLookup[_selectedTileIndex].AnimationFrames[0]]
			);

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

		private void InputPlaceTileEditorObject(Vector2Int mapMousePosition) {
			int tileSize = _allTileSets[ActiveMapLayer.Tileset].Properties.TileSize;
			Vector2Int clickedPosition = new Vector2Int(
				Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
				Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
			);

			ActiveMapLayer.TileEditorObjects.Add(new TileEditorObject {
				Id = _selectedTileIndex,
				TileEditorType = TileEditorObjectType.Tile,
				Width = 1,
				Height = 1,
				X = clickedPosition.x,
				Y = clickedPosition.y
			});

			_tileEditingMode = TileEditingMode.NoneSelected;
		}

		private void InputShowSelectedStampUnderMouse(Vector2Int mapMousePosition) {
			int tileSize = _allTileSets[ActiveMapLayer.Tileset].Properties.TileSize;
			Vector2Int clickedPosition = new Vector2Int(
				Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
				Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
			);
			float x = clickedPosition.x * tileSize * MapEditingScale + _mapEditingAreaRect.x - _mapEditingAreaScrollPosition.x;
			float y = clickedPosition.y * tileSize * MapEditingScale + _mapEditingAreaRect.y - _mapEditingAreaScrollPosition.y;
			float width = ActiveTileset.StampTextures[_selectedTilesetStampIndex].width * MapEditingScale;
			float height = ActiveTileset.StampTextures[_selectedTilesetStampIndex].height * MapEditingScale;

			SuperForms.Texture(
				new Rect(x, y, width, height),
				ActiveTileset.StampTextures[_selectedTilesetStampIndex]
			);

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

		private void InputPlaceStampEditorObject(Vector2Int mapMousePosition) {
			int tileSize = _allTileSets[ActiveMapLayer.Tileset].Properties.TileSize;
			TilesetEditorStamp selectedStamp = ActiveTileset.Properties.Stamps[_selectedTilesetStampIndex];
			Vector2Int clickedPosition = new Vector2Int(
				Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
				Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
			);

			ActiveMapLayer.TileEditorObjects.Add(new TileEditorObject {
				Id = _selectedTilesetStampIndex,
				TileEditorType = TileEditorObjectType.Stamp,
				Width = selectedStamp.Width,
				Height = selectedStamp.Height,
				X = clickedPosition.x,
				Y = clickedPosition.y
			});

			_tileEditingMode = TileEditingMode.NoneSelected;
		}

		private void InputCloneTileEditorObjects() {
			List<TileEditorObject> newlyAddedTileEditorObjects = new List<TileEditorObject>();

			foreach (TileEditorObject tileEditorObject in _selectedTileEditorObjects) {
				TileEditorObject newTileEditorEditorObject = new TileEditorObject {
					Id = tileEditorObject.Id,
					TileEditorType = tileEditorObject.TileEditorType,
					X = tileEditorObject.X,
					Y = tileEditorObject.Y,
					Width = tileEditorObject.Width,
					Height = tileEditorObject.Height,
					BrushMapDefinition = tileEditorObject.BrushMapDefinition,
					CachedEditorTexture = tileEditorObject.CachedEditorTexture,
					CachedEditorTextureInfo = tileEditorObject.CachedEditorTextureInfo,
					ParsedBrushMapDefinition = tileEditorObject.ParsedBrushMapDefinition
				};

				newlyAddedTileEditorObjects.Add(newTileEditorEditorObject);
				ActiveMapLayer.TileEditorObjects.Add(newTileEditorEditorObject);
			}

			_selectedTileEditorObjects = newlyAddedTileEditorObjects;
			_selectionObjectJustCloned = true;
		}

		private void InputDeselectSelectedTile() {
			_tileEditingMode = TileEditingMode.NoneSelected;
		}

		private void InputDrawSelectionBox(Vector2Int mapMousePosition) {
			float x = _mapEditingAreaRect.x - _mapEditingAreaScrollPosition.x + _inputMouseDownPosition.x * MapEditingScale;
			float y = _mapEditingAreaRect.y - _mapEditingAreaScrollPosition.y + _inputMouseDownPosition.y * MapEditingScale;

			float width = mapMousePosition.x - _inputMouseDownPosition.x;
			float height = mapMousePosition.y - _inputMouseDownPosition.y;

			if (width >= 0) {
				width++;
			} else {
				x += MapEditingScale;
				width--;
			}

			if (height >= 0) {
				height++;
			} else {
				y += MapEditingScale;
				height--;
			}

			width *= MapEditingScale;
			height *= MapEditingScale;

			SuperForms.Texture(
				new Rect(x, y, width, height),
				_viewBorderTexture
			);
		}

		private void InputSelectTilesUnderSelectionBox(Vector2Int mapMousePosition) {
			Vector2 topLeft = _inputMouseDownPosition;
			Vector2 selectionSize = mapMousePosition - _inputMouseDownPosition;
			Vector2 bottomRight = topLeft + selectionSize;

			if (selectionSize.x < 0) {
				float temp = topLeft.x;
				topLeft.x = bottomRight.x;
				bottomRight.x = temp;
				selectionSize.x *= -1;
			}

			if (selectionSize.y < 0) {
				float temp = topLeft.y;
				topLeft.y = bottomRight.y;
				bottomRight.y = temp;
				selectionSize.y *= -1;
			}

			_selectedTileEditorObjects.Clear();

			foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
				if (mapLayer.EditorIsVisible && !mapLayer.EditorIsLocked && mapLayer.MapLayerType == MapLayerType.Normal) {
					int tileSize = _allTileSets[mapLayer.Tileset].Properties.TileSize;

					Vector2Int topLeftInTiles = new Vector2Int(
						Mathf.FloorToInt(topLeft.x / tileSize),
						Mathf.FloorToInt(topLeft.y / tileSize)
					);

					Vector2Int bottomRightInTiles = new Vector2Int(
						Mathf.FloorToInt(bottomRight.x / tileSize),
						Mathf.FloorToInt(bottomRight.y / tileSize)
					);

					foreach (TileEditorObject tileEditorObject in mapLayer.TileEditorObjects) {
						int tileEditorRight = tileEditorObject.X + tileEditorObject.Width - 1;
						int tileEditorBottom = tileEditorObject.Y + tileEditorObject.Height - 1;

						bool overlapsLeftEdge = topLeftInTiles.x <= tileEditorObject.X && bottomRightInTiles.x >= tileEditorObject.X;
						bool overlapsRightEdge = topLeftInTiles.x <= tileEditorRight && bottomRightInTiles.x >= tileEditorRight;
						bool isContainedHorizontally = topLeftInTiles.x >= tileEditorObject.X && bottomRightInTiles.x <= tileEditorRight;
						bool meetsCriteriaHorizontally = overlapsLeftEdge || overlapsRightEdge || isContainedHorizontally;

						bool overlapsTopEdge = topLeftInTiles.y <= tileEditorObject.Y && bottomRightInTiles.y >= tileEditorObject.Y;
						bool overlapsBottomEdge = topLeftInTiles.y <= tileEditorBottom && bottomRightInTiles.y >= tileEditorBottom;
						bool isContainedVertically = topLeftInTiles.y >= tileEditorObject.Y && bottomRightInTiles.y <= tileEditorBottom;
						bool meetsCriteriaVertically = overlapsTopEdge || overlapsBottomEdge || isContainedVertically;

						if (meetsCriteriaHorizontally && meetsCriteriaVertically) {
							_selectedTileEditorObjects.Add(tileEditorObject);
						}
					}
				}
			}
		}

		private void InputPaintBrushMatte(Vector2Int mapMousePosition) {
			int tileSize = _allTileSets[ActiveMapLayer.Tileset].Properties.TileSize;
			Vector2Int clickedPosition = new Vector2Int(
				Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
				Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
			);

			_brushEditorObjectBeingEdited.ParsedBrushMapDefinition[clickedPosition.y][clickedPosition.x] = 1;
		}

		private void InputEraseBrushMatte(Vector2Int mapMousePosition) {
			int tileSize = _allTileSets[ActiveMapLayer.Tileset].Properties.TileSize;
			Vector2Int clickedPosition = new Vector2Int(
				Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
				Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
			);

			_brushEditorObjectBeingEdited.ParsedBrushMapDefinition[clickedPosition.y][clickedPosition.x] = 0;
		}

		private void InputPaintAreaMatte(Vector2Int mapMousePosition) {
			int tileSize = _allTileSets[ActiveMapLayer.Tileset].Properties.TileSize;
			Vector2Int clickedPosition = new Vector2Int(
				Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
				Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
			);

			Vector2Int startPosition = new Vector2Int(
				Mathf.FloorToInt(_brushAreaStartLocation.x / (float) tileSize),
				Mathf.FloorToInt(_brushAreaStartLocation.y / (float) tileSize)
			);

			int startX = clickedPosition.x < startPosition.x
				? clickedPosition.x
				: startPosition.x;

			int startY = clickedPosition.y < startPosition.y
				? clickedPosition.y
				: startPosition.y;

			int endX = startX + Mathf.Abs(clickedPosition.x - startPosition.x);
			int endY = startY + Mathf.Abs(clickedPosition.y - startPosition.y);

			for (int y = startY; y <= endY; y++) {
				for (int x = startX; x <= endX; x++) {
					if (y > 0 && y < _brushEditorObjectBeingEdited.ParsedBrushMapDefinition.Count &&
					    x > 0 && x < _brushEditorObjectBeingEdited.ParsedBrushMapDefinition[0].Count
					) {
						_brushEditorObjectBeingEdited.ParsedBrushMapDefinition[y][x] = 1;
					}
				}
			}
		}

		private void InputEraseAreaMatte(Vector2Int mapMousePosition) {
			int tileSize = _allTileSets[ActiveMapLayer.Tileset].Properties.TileSize;
			Vector2Int clickedPosition = new Vector2Int(
				Mathf.FloorToInt(mapMousePosition.x / (float) tileSize),
				Mathf.FloorToInt(mapMousePosition.y / (float) tileSize)
			);

			Vector2Int startPosition = new Vector2Int(
				Mathf.FloorToInt(_brushAreaStartLocation.x / (float) tileSize),
				Mathf.FloorToInt(_brushAreaStartLocation.y / (float) tileSize)
			);

			int startX = clickedPosition.x < startPosition.x
				? clickedPosition.x
				: startPosition.x;

			int startY = clickedPosition.y < startPosition.y
				? clickedPosition.y
				: startPosition.y;

			int endX = startX + Mathf.Abs(clickedPosition.x - startPosition.x);
			int endY = startY + Mathf.Abs(clickedPosition.y - startPosition.y);

			for (int y = startY; y <= endY; y++) {
				for (int x = startX; x <= endX; x++) {
					if (y > 0 && y < _brushEditorObjectBeingEdited.ParsedBrushMapDefinition.Count &&
					    x > 0 && x < _brushEditorObjectBeingEdited.ParsedBrushMapDefinition[0].Count
					) {
						_brushEditorObjectBeingEdited.ParsedBrushMapDefinition[y][x] = 0;
					}
				}
			}
		}

		private void InputShowSelectedActorUnderMouse(Vector2Int mapMousePosition) {
			float x = Mathf.FloorToInt(mapMousePosition.x / (float) _actorSnap) * _actorSnap *
			          MapEditingScale + _mapEditingAreaRect.x - _mapEditingAreaScrollPosition.x;
			float y = Mathf.FloorToInt(mapMousePosition.y / (float) _actorSnap) * _actorSnap *
			          MapEditingScale + _mapEditingAreaRect.y - _mapEditingAreaScrollPosition.y;
			float width = SelectedActorToPlace.SpriteWidth * MapEditingScale;
			float height = SelectedActorToPlace.SpriteHeight * MapEditingScale;

			SuperForms.Texture(
				new Rect(x, y, width, height),
				_allActorTextures[AllActorNames[_selectedActorIndex]]
			);

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

		private void InputVerifyActorSelection(Vector2Int mapMousePosition) {
			_inputMouseDownPosition = mapMousePosition;
			_inputHoveredActorOnMouseDown = InputMouseIsHoveringAnActor(mapMousePosition);

			if (_inputHoveredActorOnMouseDown == null) {
				_inputSelectionBoxStarted = true;
				return;
			}

			if (!_selectedActorObjects.Contains(_inputHoveredActorOnMouseDown)) {
				_selectedActorObjects.Clear();
				_selectedActorObjects.Add(_inputHoveredActorOnMouseDown);
			}

			_selectedObjectOffsets.Clear();

			foreach (MapActorPlacementProperties mapActor in _selectedActorObjects) {
				_selectedObjectOffsets.Add(new Vector2Int(
					mapMousePosition.x - mapActor.Position.x,
					mapMousePosition.y - mapActor.Position.y
				));
			}

			if (_selectedActorObjects.Count == 1) {
				foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
					if (mapLayer.Actors.Contains(_selectedActorObjects[0])) {
						_selectedMapLayerIndex = ActiveMap.Layers.IndexOf(mapLayer);
						break;
					}
				}
			}
		}

		private MapActorPlacementProperties InputMouseIsHoveringAnActor(Vector2Int mapMousePosition) {
			MapActorPlacementProperties hoveredActor = null;
			bool isHovered = false;

			for (int i = ActiveMap.Layers.Count - 1; i >= 0; i--) {
				MapLayerProperties mapLayer = ActiveMap.Layers[i];
				if (mapLayer.EditorIsVisible && !mapLayer.EditorIsLocked && mapLayer.MapLayerType == MapLayerType.Normal) {
					for (int j = mapLayer.Actors.Count - 1; j >= 0; j--) {
						MapActorPlacementProperties mapLayerActor = mapLayer.Actors[j];
						float width = _allActorTextures[mapLayerActor.Actor].width;
						float height = _allActorTextures[mapLayerActor.Actor].height;

						isHovered = mapMousePosition.x >= mapLayerActor.Position.x &&
						            mapMousePosition.x < mapLayerActor.Position.x + width &&
						            mapMousePosition.y >= mapLayerActor.Position.y &&
						            mapMousePosition.y < mapLayerActor.Position.y + height;

						if (isHovered) {
							hoveredActor = mapLayerActor;
							break;
						}
					}
				}

				if (isHovered) {
					break;
				}
			}

			return hoveredActor;
		}

		private void InputSelectActorsUnderSelectionBox(Vector2Int mapMousePosition) {
			Vector2 topLeft = _inputMouseDownPosition;
			Vector2 size = mapMousePosition - _inputMouseDownPosition;
			Vector2 bottomRight = topLeft + size;

			if (size.x < 0) {
				float temp = topLeft.x;
				topLeft.x = bottomRight.x;
				bottomRight.x = temp;
				size.x *= -1;
			}

			if (size.y < 0) {
				float temp = topLeft.y;
				topLeft.y = bottomRight.y;
				bottomRight.y = temp;
				size.y *= -1;
			}

			size += new Vector2(1, 1);
			bottomRight = topLeft + size;

			_selectedActorObjects.Clear();

			foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
				if (mapLayer.EditorIsVisible && !mapLayer.EditorIsLocked && mapLayer.MapLayerType == MapLayerType.Normal) {
					foreach (MapActorPlacementProperties mapLayerActor in mapLayer.Actors) {
						float width = _allActorTextures[mapLayerActor.Actor].width;
						float height = _allActorTextures[mapLayerActor.Actor].height;

						int tileEditorRight = mapLayerActor.Position.x + (int) width;
						int tileEditorBottom = mapLayerActor.Position.y + (int) height;

						bool overlapsLeftEdge = topLeft.x <= mapLayerActor.Position.x && bottomRight.x > mapLayerActor.Position.x;
						bool overlapsRightEdge = topLeft.x < tileEditorRight && bottomRight.x >= tileEditorRight;
						bool isContainedHorizontally = topLeft.x >= mapLayerActor.Position.x && bottomRight.x <= tileEditorRight;
						bool meetsCriteriaHorizontally = overlapsLeftEdge || overlapsRightEdge || isContainedHorizontally;

						bool overlapsTopEdge = topLeft.y <= mapLayerActor.Position.y && bottomRight.y > mapLayerActor.Position.y;
						bool overlapsBottomEdge = topLeft.y < tileEditorBottom && bottomRight.y >= tileEditorBottom;
						bool isContainedVertically = topLeft.y >= mapLayerActor.Position.y && bottomRight.y <= tileEditorBottom;
						bool meetsCriteriaVertically = overlapsTopEdge || overlapsBottomEdge || isContainedVertically;

						if (meetsCriteriaHorizontally && meetsCriteriaVertically) {
							_selectedActorObjects.Add(mapLayerActor);
						}
					}
				}
			}
		}

		private void InputPlaceSelectedActor(Vector2Int mapMousePosition) {
			mapMousePosition.x = Mathf.FloorToInt(mapMousePosition.x / (float) _actorSnap) * _actorSnap;
			mapMousePosition.y = Mathf.FloorToInt(mapMousePosition.y / (float) _actorSnap) * _actorSnap;

			MapActorPlacementProperties newActor = new MapActorPlacementProperties {
				MapId = "",
				Actor = SelectedActorToPlace.Name,
				Position = mapMousePosition,
				ActorInstanceProperties = new ActorInstanceProperties(),
				ActorDefaultStatsBools = new List<BoolStatProperties>(),
				ActorDefaultStatsFloats = new List<FloatStatProperties>(),
				ActorDefaultStatsStrings = new List<StringStatProperties>()
			};

			foreach (BoolStatProperties statsBool in _allActorProperties[SelectedActorToPlace.Name].StatsBools) {
				newActor.ActorDefaultStatsBools.Add(new BoolStatProperties {
					Name = statsBool.Name,
					InitialValue = statsBool.InitialValue,
					Value = statsBool.Value
				});
			}

			foreach (FloatStatProperties statsBool in _allActorProperties[SelectedActorToPlace.Name].StatsFloats) {
				newActor.ActorDefaultStatsFloats.Add(new FloatStatProperties {
					Name = statsBool.Name,
					InitialValue = statsBool.InitialValue,
					Value = statsBool.Value
				});
			}

			foreach (StringStatProperties statsString in _allActorProperties[SelectedActorToPlace.Name].StatsStrings) {
				newActor.ActorDefaultStatsStrings.Add(new StringStatProperties {
					Name = statsString.Name,
					InitialValue = statsString.InitialValue,
					Value = statsString.Value
				});
			}

			ActiveMapLayer.Actors.Add(newActor);

			_actorEditingMode = ActorEditingMode.NoneSelected;
		}

		private void InputDeselectSelectedActor() {
			_actorEditingMode = ActorEditingMode.NoneSelected;
		}

		private void InputRemoveActorObject(Vector2Int mapMousePosition) {
			bool isHovered = false;
			bool deletedObjectWasInSelection = false;

			for (int i = ActiveMap.Layers.Count - 1; i >= 0; i--) {
				MapLayerProperties thisLayer = ActiveMap.Layers[i];

				for (int j = thisLayer.Actors.Count - 1; j >= 0; j--) {
					MapActorPlacementProperties mapLayerActor = thisLayer.Actors[j];

					float width = _allActorTextures[mapLayerActor.Actor].width;
					float height = _allActorTextures[mapLayerActor.Actor].height;

					isHovered = mapMousePosition.x >= mapLayerActor.Position.x &&
					            mapMousePosition.x < mapLayerActor.Position.x + width &&
					            mapMousePosition.y >= mapLayerActor.Position.y &&
					            mapMousePosition.y < mapLayerActor.Position.y + height;

					if (isHovered) {
						deletedObjectWasInSelection = _selectedActorObjects.Contains(mapLayerActor);
						if (!deletedObjectWasInSelection) {
							_selectedActorObjects.Clear();
							_selectedActorObjects.Add(mapLayerActor);
						}

						break;
					}
				}

				if (isHovered) {
					break;
				}
			}

			if (isHovered && _selectedActorObjects.Count > 0) {
				foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
					List<MapActorPlacementProperties> markedForDeletion = new List<MapActorPlacementProperties>();

					foreach (MapActorPlacementProperties mapLayerActor in _selectedActorObjects) {
						if (mapLayer.Actors.Contains(mapLayerActor)) {
							markedForDeletion.Add(mapLayerActor);
						}
					}

					foreach (MapActorPlacementProperties mapLayerActor in markedForDeletion) {
						mapLayer.Actors.Remove(mapLayerActor);
					}
				}
			}

			if (deletedObjectWasInSelection) {
				_selectedActorObjects.Clear();
			}
		}

		private void InputDragActorObjects(Vector2Int mapMousePosition) {
			for (int i = 0; i < _selectedActorObjects.Count; i++) {
				int xOffset = Mathf.FloorToInt(_selectedObjectOffsets[i].x / (float) _actorSnap) * _actorSnap;
				int yOffset = Mathf.FloorToInt(_selectedObjectOffsets[i].y / (float) _actorSnap) * _actorSnap;
				int x = Mathf.FloorToInt(mapMousePosition.x / (float) _actorSnap) * _actorSnap - xOffset;
				int y = Mathf.FloorToInt(mapMousePosition.y / (float) _actorSnap) * _actorSnap - yOffset;

				MapActorPlacementProperties mapActor = _selectedActorObjects[i];
				mapActor.Position.x = x;
				mapActor.Position.y = y;
			}
		}

		private void InputCloneActorObjects() {
			List<MapActorPlacementProperties> newlyAddedActorObjects = new List<MapActorPlacementProperties>();

			foreach (MapActorPlacementProperties actorObject in _selectedActorObjects) {
				MapActorPlacementProperties newActorObject = new MapActorPlacementProperties {
					Actor = actorObject.Actor,
					ActorDefaultStatsBools = actorObject.ActorDefaultStatsBools == null
						? new List<BoolStatProperties>()
						: new List<BoolStatProperties>(actorObject.ActorDefaultStatsBools),
					ActorDefaultStatsFloats = actorObject.ActorDefaultStatsFloats == null
						? new List<FloatStatProperties>()
						: new List<FloatStatProperties>(actorObject.ActorDefaultStatsFloats),
					ActorDefaultStatsStrings = actorObject.ActorDefaultStatsStrings == null
						? new List<StringStatProperties>()
						: new List<StringStatProperties>(actorObject.ActorDefaultStatsStrings),
					ActorInstanceProperties = actorObject.ActorInstanceProperties == null
						? new ActorInstanceProperties()
						: new ActorInstanceProperties {
							FacingDirection = actorObject.ActorInstanceProperties.FacingDirection,
							LinkedActors = actorObject.ActorInstanceProperties.LinkedActors == null
								? new List<string>()
								: new List<string>(actorObject.ActorInstanceProperties.LinkedActors)
						},
					MapId = actorObject.MapId
				};

				newlyAddedActorObjects.Add(newActorObject);
				ActiveMapLayer.Actors.Add(newActorObject);
			}

			_selectedActorObjects = newlyAddedActorObjects;
			_selectionObjectJustCloned = true;
		}
		
		private void InputVerifyPrefabSelection(Vector2Int mapMousePosition) {
			_inputMouseDownPosition = mapMousePosition;
			_inputHoveredPrefabOnMouseDown = InputMouseIsHoveringAnPrefab(mapMousePosition);

			if (_inputHoveredPrefabOnMouseDown == null) {
				_inputSelectionBoxStarted = true;
				return;
			}

			if (!_selectedPrefabObjects.Contains(_inputHoveredPrefabOnMouseDown)) {
				_selectedPrefabObjects.Clear();
				_selectedPrefabObjects.Add(_inputHoveredPrefabOnMouseDown);
			}

			_selectedObjectOffsets.Clear();

			foreach (MapPrefabPlacementProperties mapPrefab in _selectedPrefabObjects) {
				_selectedObjectOffsets.Add(new Vector2Int(
					mapMousePosition.x - mapPrefab.Position.x,
					mapMousePosition.y - mapPrefab.Position.y
				));
			}

			if (_selectedPrefabObjects.Count == 1) {
				foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
					if (mapLayer.Prefabs.Contains(_selectedPrefabObjects[0])) {
						_selectedMapLayerIndex = ActiveMap.Layers.IndexOf(mapLayer);
						break;
					}
				}
			}
		}

		private MapPrefabPlacementProperties InputMouseIsHoveringAnPrefab(Vector2Int mapMousePosition) {
			MapPrefabPlacementProperties hoveredPrefab = null;
			bool isHovered = false;

			for (int i = ActiveMap.Layers.Count - 1; i >= 0; i--) {
				MapLayerProperties mapLayer = ActiveMap.Layers[i];
				if (mapLayer.EditorIsVisible && !mapLayer.EditorIsLocked && mapLayer.MapLayerType == MapLayerType.Normal) {
					for (int j = mapLayer.Prefabs.Count - 1; j >= 0; j--) {
						MapPrefabPlacementProperties mapLayerPrefab = mapLayer.Prefabs[j];
						const float width = 16;
						const float height = 16;

						isHovered = mapMousePosition.x >= mapLayerPrefab.Position.x &&
						            mapMousePosition.x < mapLayerPrefab.Position.x + width &&
						            mapMousePosition.y >= mapLayerPrefab.Position.y &&
						            mapMousePosition.y < mapLayerPrefab.Position.y + height;

						if (isHovered) {
							hoveredPrefab = mapLayerPrefab;
							break;
						}
					}
				}

				if (isHovered) {
					break;
				}
			}

			return hoveredPrefab;
		}

		private void InputSelectPrefabsUnderSelectionBox(Vector2Int mapMousePosition) {
			Vector2 topLeft = _inputMouseDownPosition;
			Vector2 size = mapMousePosition - _inputMouseDownPosition;
			Vector2 bottomRight = topLeft + size;

			if (size.x < 0) {
				float temp = topLeft.x;
				topLeft.x = bottomRight.x;
				bottomRight.x = temp;
				size.x *= -1;
			}

			if (size.y < 0) {
				float temp = topLeft.y;
				topLeft.y = bottomRight.y;
				bottomRight.y = temp;
				size.y *= -1;
			}

			size += new Vector2(1, 1);
			bottomRight = topLeft + size;

			_selectedPrefabObjects.Clear();

			foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
				if (mapLayer.EditorIsVisible && !mapLayer.EditorIsLocked && mapLayer.MapLayerType == MapLayerType.Normal) {
					foreach (MapPrefabPlacementProperties mapLayerPrefab in mapLayer.Prefabs) {
						const float width = 16;
						const float height = 16;

						int tileEditorRight = mapLayerPrefab.Position.x + (int) width;
						int tileEditorBottom = mapLayerPrefab.Position.y + (int) height;

						bool overlapsLeftEdge = topLeft.x <= mapLayerPrefab.Position.x && bottomRight.x > mapLayerPrefab.Position.x;
						bool overlapsRightEdge = topLeft.x < tileEditorRight && bottomRight.x >= tileEditorRight;
						bool isContainedHorizontally = topLeft.x >= mapLayerPrefab.Position.x && bottomRight.x <= tileEditorRight;
						bool meetsCriteriaHorizontally = overlapsLeftEdge || overlapsRightEdge || isContainedHorizontally;

						bool overlapsTopEdge = topLeft.y <= mapLayerPrefab.Position.y && bottomRight.y > mapLayerPrefab.Position.y;
						bool overlapsBottomEdge = topLeft.y < tileEditorBottom && bottomRight.y >= tileEditorBottom;
						bool isContainedVertically = topLeft.y >= mapLayerPrefab.Position.y && bottomRight.y <= tileEditorBottom;
						bool meetsCriteriaVertically = overlapsTopEdge || overlapsBottomEdge || isContainedVertically;

						if (meetsCriteriaHorizontally && meetsCriteriaVertically) {
							_selectedPrefabObjects.Add(mapLayerPrefab);
						}
					}
				}
			}
		}
		
		private void InputRemovePrefabObject(Vector2Int mapMousePosition) {
			bool isHovered = false;
			bool deletedObjectWasInSelection = false;

			for (int i = ActiveMap.Layers.Count - 1; i >= 0; i--) {
				MapLayerProperties thisLayer = ActiveMap.Layers[i];

				for (int j = thisLayer.Prefabs.Count - 1; j >= 0; j--) {
					MapPrefabPlacementProperties mapLayerPrefab = thisLayer.Prefabs[j];

					const float width = 16;
					const float height = 16;

					isHovered = mapMousePosition.x >= mapLayerPrefab.Position.x &&
					            mapMousePosition.x < mapLayerPrefab.Position.x + width &&
					            mapMousePosition.y >= mapLayerPrefab.Position.y &&
					            mapMousePosition.y < mapLayerPrefab.Position.y + height;

					if (isHovered) {
						deletedObjectWasInSelection = _selectedPrefabObjects.Contains(mapLayerPrefab);
						if (!deletedObjectWasInSelection) {
							_selectedPrefabObjects.Clear();
							_selectedPrefabObjects.Add(mapLayerPrefab);
						}

						break;
					}
				}

				if (isHovered) {
					break;
				}
			}

			if (isHovered && _selectedPrefabObjects.Count > 0) {
				foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
					List<MapPrefabPlacementProperties> markedForDeletion = new List<MapPrefabPlacementProperties>();

					foreach (MapPrefabPlacementProperties mapLayerPrefab in _selectedPrefabObjects) {
						if (mapLayer.Prefabs.Contains(mapLayerPrefab)) {
							markedForDeletion.Add(mapLayerPrefab);
						}
					}

					foreach (MapPrefabPlacementProperties mapLayerPrefab in markedForDeletion) {
						mapLayer.Prefabs.Remove(mapLayerPrefab);
					}
				}
			}

			if (deletedObjectWasInSelection) {
				_selectedPrefabObjects.Clear();
			}
		}

		private void InputDragPrefabObjects(Vector2Int mapMousePosition) {
			for (int i = 0; i < _selectedPrefabObjects.Count; i++) {
				int xOffset = Mathf.FloorToInt(_selectedObjectOffsets[i].x / (float) _prefabSnap) * _prefabSnap;
				int yOffset = Mathf.FloorToInt(_selectedObjectOffsets[i].y / (float) _prefabSnap) * _prefabSnap;
				int x = Mathf.FloorToInt(mapMousePosition.x / (float) _prefabSnap) * _prefabSnap - xOffset;
				int y = Mathf.FloorToInt(mapMousePosition.y / (float) _prefabSnap) * _prefabSnap - yOffset;

				MapPrefabPlacementProperties mapPrefab = _selectedPrefabObjects[i];
				mapPrefab.Position.x = x;
				mapPrefab.Position.y = y;
			}
		}

		private void InputClonePrefabObjects() {
			List<MapPrefabPlacementProperties> newlyAddedPrefabObjects = new List<MapPrefabPlacementProperties>();

			foreach (MapPrefabPlacementProperties prefabObject in _selectedPrefabObjects) {
				MapPrefabPlacementProperties newPrefabObject = new MapPrefabPlacementProperties {
					Prefab = prefabObject.Prefab
				};

				newlyAddedPrefabObjects.Add(newPrefabObject);
				ActiveMapLayer.Prefabs.Add(newPrefabObject);
			}

			_selectedPrefabObjects = newlyAddedPrefabObjects;
			_selectionObjectJustCloned = true;
		}

		private void InputShowSelectedPrefabUnderMouse(Vector2Int mapMousePosition) {
			float x = Mathf.FloorToInt(mapMousePosition.x / (float) _prefabSnap) * _prefabSnap *
			          MapEditingScale + _mapEditingAreaRect.x - _mapEditingAreaScrollPosition.x;
			float y = Mathf.FloorToInt(mapMousePosition.y / (float) _prefabSnap) * _prefabSnap *
			          MapEditingScale + _mapEditingAreaRect.y - _mapEditingAreaScrollPosition.y;

			SuperForms.Texture(
				new Rect(x - MapEditingScale, y - MapEditingScale, 16 * MapEditingScale, 16 * MapEditingScale),
				SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.MapEditorPrefabPreview]
			);
		}

		private void InputPlaceSelectedPrefab(Vector2Int mapMousePosition) {
			mapMousePosition.x = Mathf.FloorToInt(mapMousePosition.x / (float) _prefabSnap) * _prefabSnap;
			mapMousePosition.y = Mathf.FloorToInt(mapMousePosition.y / (float) _prefabSnap) * _prefabSnap;

			List<string> allPrefabNames = new List<string>();

			foreach (KeyValuePair<string, GameObject> prefab in BoomerangDatabase.MapPrefabDatabaseEntries) {
				allPrefabNames.Add(prefab.Key);
			}

			MapPrefabPlacementProperties newPrefab = new MapPrefabPlacementProperties {
				Prefab = allPrefabNames[_selectedPrefabIndex],
				Position = mapMousePosition
			};

			ActiveMapLayer.Prefabs.Add(newPrefab);
			_prefabEditingMode = PrefabEditingMode.NoneSelected;
		}

		private void InputDeselectSelectedPrefab() {
			_prefabEditingMode = PrefabEditingMode.NoneSelected;
		}

		private void InputVerifyViewSelection(Vector2Int mapMousePosition) {
			_inputMouseDownPosition = mapMousePosition;
			_inputHoveredViewOnMouseDown = InputMouseIsHoveringAView(mapMousePosition);

			if (_inputHoveredViewOnMouseDown == null) {
				_inputSelectionBoxStarted = true;
				return;
			}

			if (!_selectedMapViews.Contains(_inputHoveredViewOnMouseDown)) {
				_selectedMapViews.Clear();
				_selectedMapViews.Add(_inputHoveredViewOnMouseDown);
			}

			_selectedObjectOffsets.Clear();

			foreach (MapViewProperties mapView in _selectedMapViews) {
				_selectedObjectOffsets.Add(new Vector2Int(
					mapMousePosition.x - mapView.Position.x,
					mapMousePosition.y - mapView.Position.y
				));
			}
		}

		private MapViewProperties InputMouseIsHoveringAView(Vector2Int mapMousePosition) {
			MapViewProperties hoveredView = null;

			for (int i = ActiveMap.Views.Count - 1; i >= 0; i--) {
				MapViewProperties mapLayerActor = ActiveMap.Views[i];

				bool isHovered = mapMousePosition.x >= mapLayerActor.Position.x &&
				                 mapMousePosition.x < mapLayerActor.Position.x + mapLayerActor.Dimensions.x &&
				                 mapMousePosition.y >= mapLayerActor.Position.y &&
				                 mapMousePosition.y < mapLayerActor.Position.y + mapLayerActor.Dimensions.y;

				if (isHovered) {
					hoveredView = mapLayerActor;
					break;
				}
			}

			return hoveredView;
		}

		private void InputSelectViewsUnderSelectionBox(Vector2Int mapMousePosition) {
			Vector2 topLeft = _inputMouseDownPosition;
			Vector2 size = mapMousePosition - _inputMouseDownPosition;
			Vector2 bottomRight = topLeft + size;

			if (size.x < 0) {
				float temp = topLeft.x;
				topLeft.x = bottomRight.x;
				bottomRight.x = temp;
				size.x *= -1;
			}

			if (size.y < 0) {
				float temp = topLeft.y;
				topLeft.y = bottomRight.y;
				bottomRight.y = temp;
				size.y *= -1;
			}

			bottomRight = topLeft + size;

			_selectedMapViews.Clear();

			foreach (MapViewProperties mapView in ActiveMap.Views) {
				int tileEditorRight = mapView.Position.x + mapView.Dimensions.x;
				int tileEditorBottom = mapView.Position.y + mapView.Dimensions.y;

				bool overlapsLeftEdge = topLeft.x <= mapView.Position.x && bottomRight.x > mapView.Position.x;
				bool overlapsRightEdge = topLeft.x < tileEditorRight && bottomRight.x >= tileEditorRight;
				bool isContainedHorizontally = topLeft.x >= mapView.Position.x && bottomRight.x <= tileEditorRight;
				bool meetsCriteriaHorizontally = overlapsLeftEdge || overlapsRightEdge || isContainedHorizontally;

				bool overlapsTopEdge = topLeft.y <= mapView.Position.y && bottomRight.y > mapView.Position.y;
				bool overlapsBottomEdge = topLeft.y < tileEditorBottom && bottomRight.y >= tileEditorBottom;
				bool isContainedVertically = topLeft.y >= mapView.Position.y && bottomRight.y <= tileEditorBottom;
				bool meetsCriteriaVertically = overlapsTopEdge || overlapsBottomEdge || isContainedVertically;

				if (meetsCriteriaHorizontally && meetsCriteriaVertically) {
					_selectedMapViews.Add(mapView);
				}
			}
		}

		private void InputRemoveViewObject(Vector2Int mapMousePosition) {
			bool isHovered = false;
			bool deletedObjectWasInSelection = false;

			for (int j = ActiveMap.Views.Count - 1; j >= 0; j--) {
				MapViewProperties mapView = ActiveMap.Views[j];

				isHovered = mapMousePosition.x >= mapView.Position.x &&
				            mapMousePosition.x < mapView.Position.x + mapView.Dimensions.x &&
				            mapMousePosition.y >= mapView.Position.y &&
				            mapMousePosition.y < mapView.Position.y + +mapView.Dimensions.y;

				if (isHovered) {
					deletedObjectWasInSelection = _selectedMapViews.Contains(mapView);
					if (!deletedObjectWasInSelection) {
						_selectedMapViews.Clear();
						_selectedMapViews.Add(mapView);
					}

					break;
				}
			}

			if (isHovered && _selectedMapViews.Count > 0) {
				foreach (MapViewProperties mapView in _selectedMapViews) {
					ActiveMap.Views.Remove(mapView);
				}
			}

			if (deletedObjectWasInSelection) {
				_selectedMapViews.Clear();
			}
		}

		private void InputDragViews(Vector2Int mapMousePosition) {
			for (int i = 0; i < _selectedMapViews.Count; i++) {
				MapViewProperties mapView = _selectedMapViews[i];
				mapView.Position.x = (int) Math.Round((mapMousePosition.x - _selectedObjectOffsets[i].x) / _viewInputSnapSize, MidpointRounding.AwayFromZero) * 
				                     (int) _viewInputSnapSize;
				mapView.Position.y = (int) Math.Round((mapMousePosition.y - _selectedObjectOffsets[i].y) / _viewInputSnapSize, MidpointRounding.AwayFromZero) * 
				                     (int) _viewInputSnapSize;
			}
		}

		private void InputScaleView(Vector2Int mapMousePosition) {
			MapViewProperties mapView = _selectedMapViews[0];
			const int handleSize = 16;

			if (!_inputHasStartedScaling) {
				_scalingObjectOriginTopLeft = new Vector2Int(mapMousePosition.x, mapMousePosition.y);
				_scalingObjectOriginalPosition = new Vector2Int(mapView.Position.x, mapView.Position.y);
				_scalingObjectOriginalSize = new Vector2Int(mapView.Dimensions.x, mapView.Dimensions.y);

				_scalingObjectWidthLocked = false;
				_scalingObjectHeightLocked = false;

				bool isOnTopRow = BoomerangUtils.ValueIsBetween(_scalingObjectOriginTopLeft.y, mapView.Position.y, mapView.Position.y + handleSize);
				bool isOnBottomRow = BoomerangUtils.ValueIsBetween(
					_scalingObjectOriginTopLeft.y,
					mapView.Position.y + mapView.Dimensions.y - handleSize,
					mapView.Position.y + mapView.Dimensions.y
				);

				bool isOnLeftColumn = BoomerangUtils.ValueIsBetween(_scalingObjectOriginTopLeft.x, mapView.Position.x, mapView.Position.x + handleSize);
				bool isOnRightColumn = BoomerangUtils.ValueIsBetween(
					_scalingObjectOriginTopLeft.x,
					mapView.Position.x + mapView.Dimensions.x - handleSize,
					mapView.Position.x + mapView.Dimensions.x
				);

				if (!isOnTopRow && !isOnBottomRow && !isOnLeftColumn && !isOnRightColumn) {
					_scalingObjectOriginTopLeft = new Vector2Int(-1, -1);
					return;
				}

				bool isMoreThanTwoRows = mapView.Dimensions.y >= 3 * handleSize;
				bool isMoreThanTwoColumns = mapView.Dimensions.x >= 3 * handleSize;

				_objectIsScalingWidth = (isOnLeftColumn || isOnRightColumn) && (!isMoreThanTwoRows || !isOnTopRow && !isOnBottomRow);
				_objectIsScalingHeight = (isOnBottomRow || isOnTopRow) && (!isMoreThanTwoColumns || !isOnRightColumn && !isOnLeftColumn);

				if ((isOnLeftColumn || isOnRightColumn) && (isOnBottomRow || isOnTopRow)) {
					_objectIsScalingWidth = true;
					_objectIsScalingHeight = true;
				}

				_scalingObjectWidthLocked = _objectIsScalingWidth && isMoreThanTwoColumns;
				_objectScalingWidthPositive = _scalingObjectWidthLocked && isOnRightColumn;

				_scalingObjectHeightLocked = _objectIsScalingHeight && isMoreThanTwoRows;
				_objectScalingHeightPositive = _scalingObjectHeightLocked && isOnBottomRow;

				_inputHasStartedScaling = true;
			}

			if (_objectIsScalingWidth) {
				if (!_scalingObjectWidthLocked) {
					_scalingObjectWidthLocked = mapMousePosition.x != _scalingObjectOriginalPosition.x;
					_objectScalingWidthPositive = mapMousePosition.x > _scalingObjectOriginalPosition.x;
				} else {
					if (_objectScalingWidthPositive) {
						if (mapMousePosition.x >= _scalingObjectOriginalPosition.x) {
							mapView.Dimensions.x = (int)Math.Round(mapMousePosition.x / _viewInputSnapSize, MidpointRounding.AwayFromZero) * (int) _viewInputSnapSize - 
								_scalingObjectOriginalPosition.x;
						}
					} else {
						if (mapMousePosition.x < _scalingObjectOriginalPosition.x + _scalingObjectOriginalSize.x) {
							int startX = mapView.Position.x;
							mapView.Position.x = (int) Math.Round(mapMousePosition.x / _viewInputSnapSize, MidpointRounding.AwayFromZero) * (int) _viewInputSnapSize;
							mapView.Dimensions.x += startX - mapView.Position.x;
						}
					}
				}
			}

			if (_objectIsScalingHeight) {
				if (!_scalingObjectHeightLocked) {
					_scalingObjectHeightLocked = mapMousePosition.y != _scalingObjectOriginalPosition.y;
					_objectScalingHeightPositive = mapMousePosition.y > _scalingObjectOriginalPosition.y;
				} else {
					if (_objectScalingHeightPositive) {
						if (mapMousePosition.y >= _scalingObjectOriginalPosition.y) {
							mapView.Dimensions.y = (int) Math.Round(mapMousePosition.y / _viewInputSnapSize, MidpointRounding.AwayFromZero) * (int) _viewInputSnapSize - 
								_scalingObjectOriginalPosition.y;
						}
					} else {
						if (mapMousePosition.y < _scalingObjectOriginalPosition.y + _scalingObjectOriginalSize.y) {
							int startY = mapView.Position.y;
							mapView.Position.y = (int) Math.Round(mapMousePosition.y / _viewInputSnapSize, MidpointRounding.AwayFromZero) * (int) _viewInputSnapSize;
							mapView.Dimensions.y += startY - mapView.Position.y;
						}
					}
				}
			}
		}

		private void InputCloneViews() {
			if (Event.current.type == EventType.Repaint) {
				List<MapViewProperties> newlyAddedViews = new List<MapViewProperties>();

				foreach (MapViewProperties mapView in _selectedMapViews) {
					MapViewProperties newView = new MapViewProperties {
						Position = mapView.Position,
						Dimensions = mapView.Dimensions,
						PlaysBackgroundMusic = mapView.PlaysBackgroundMusic,
						BackgroundMusic = mapView.BackgroundMusic,
						CrossFadeBackgroundMusic = mapView.CrossFadeBackgroundMusic,
						CrossFadeDuration = mapView.CrossFadeDuration,
						CameraBehaviorClass = mapView.CameraBehaviorClass,
						CameraBehaviorProperties = mapView.CameraBehaviorProperties,
						CameraBehaviorPropertiesClass = mapView.CameraBehaviorPropertiesClass,
						CameraTransitionMode = mapView.CameraTransitionMode
					};

					newlyAddedViews.Add(newView);
					ActiveMap.Views.Add(newView);
				}

				_selectedMapViews = newlyAddedViews;
				_selectionObjectJustCloned = true;
			}
		}


		private void InputVerifyRegionSelection(Vector2Int mapMousePosition) {
			_inputMouseDownPosition = mapMousePosition;
			_inputHoveredRegionOnMouseDown = InputMouseIsHoveringARegion(mapMousePosition);

			if (_inputHoveredRegionOnMouseDown == null) {
				_inputSelectionBoxStarted = true;
				return;
			}

			if (!_selectedMapRegions.Contains(_inputHoveredRegionOnMouseDown)) {
				_selectedMapRegions.Clear();
				_selectedMapRegions.Add(_inputHoveredRegionOnMouseDown);
			}

			_selectedObjectOffsets.Clear();

			foreach (MapRegionProperties mapRegion in _selectedMapRegions) {
				_selectedObjectOffsets.Add(new Vector2Int(
					mapMousePosition.x - mapRegion.Position.x,
					mapMousePosition.y - mapRegion.Position.y
				));
			}
		}

		private MapRegionProperties InputMouseIsHoveringARegion(Vector2Int mapMousePosition) {
			MapRegionProperties hoveredRegion = null;

			for (int i = ActiveMap.Regions.Count - 1; i >= 0; i--) {
				MapRegionProperties mapRegion = ActiveMap.Regions[i];

				bool isHovered = mapMousePosition.x >= mapRegion.Position.x &&
				                 mapMousePosition.x < mapRegion.Position.x + mapRegion.Dimensions.x &&
				                 mapMousePosition.y >= mapRegion.Position.y &&
				                 mapMousePosition.y < mapRegion.Position.y + mapRegion.Dimensions.y;

				if (isHovered) {
					hoveredRegion = mapRegion;
					break;
				}
			}

			return hoveredRegion;
		}

		private void InputSelectRegionsUnderSelectionBox(Vector2Int mapMousePositionInTiles) {
			Vector2 topLeft = _inputMouseDownPosition;
			Vector2 size = mapMousePositionInTiles - _inputMouseDownPosition;
			Vector2 bottomRight = topLeft + size;

			if (size.x < 0) {
				float temp = topLeft.x;
				topLeft.x = bottomRight.x;
				bottomRight.x = temp;
				size.x *= -1;
			}

			if (size.y < 0) {
				float temp = topLeft.y;
				topLeft.y = bottomRight.y;
				bottomRight.y = temp;
				size.y *= -1;
			}

			size += new Vector2(1, 1);
			bottomRight = topLeft + size;

			_selectedMapRegions.Clear();

			foreach (MapRegionProperties mapRegion in ActiveMap.Regions) {
				int tileEditorRight = mapRegion.Position.x + mapRegion.Dimensions.x;
				int tileEditorBottom = mapRegion.Position.y + mapRegion.Dimensions.y;

				bool overlapsLeftEdge = topLeft.x <= mapRegion.Position.x && bottomRight.x > mapRegion.Position.x;
				bool overlapsRightEdge = topLeft.x < tileEditorRight && bottomRight.x >= tileEditorRight;
				bool isContainedHorizontally = topLeft.x >= mapRegion.Position.x && bottomRight.x <= tileEditorRight;
				bool meetsCriteriaHorizontally = overlapsLeftEdge || overlapsRightEdge || isContainedHorizontally;

				bool overlapsTopEdge = topLeft.y <= mapRegion.Position.y && bottomRight.y > mapRegion.Position.y;
				bool overlapsBottomEdge = topLeft.y < tileEditorBottom && bottomRight.y >= tileEditorBottom;
				bool isContainedVertically = topLeft.y >= mapRegion.Position.y && bottomRight.y <= tileEditorBottom;
				bool meetsCriteriaVertically = overlapsTopEdge || overlapsBottomEdge || isContainedVertically;

				if (meetsCriteriaHorizontally && meetsCriteriaVertically) {
					_selectedMapRegions.Add(mapRegion);
				}
			}
		}

		private void InputRemoveRegionObject(Vector2Int mapMousePositionInTiles) {
			bool isHovered = false;
			bool deletedObjectWasInSelection = false;

			for (int j = ActiveMap.Regions.Count - 1; j >= 0; j--) {
				MapRegionProperties mapRegion = ActiveMap.Regions[j];

				isHovered = mapMousePositionInTiles.x >= mapRegion.Position.x &&
				            mapMousePositionInTiles.x < mapRegion.Position.x + mapRegion.Dimensions.x &&
				            mapMousePositionInTiles.y >= mapRegion.Position.y &&
				            mapMousePositionInTiles.y < mapRegion.Position.y + +mapRegion.Dimensions.y;

				if (isHovered) {
					deletedObjectWasInSelection = _selectedMapRegions.Contains(mapRegion);
					if (!deletedObjectWasInSelection) {
						_selectedMapRegions.Clear();
						_selectedMapRegions.Add(mapRegion);
					}

					break;
				}
			}

			if (isHovered && _selectedMapRegions.Count > 0) {
				foreach (MapRegionProperties mapRegion in _selectedMapRegions) {
					ActiveMap.Regions.Remove(mapRegion);
				}
			}

			if (deletedObjectWasInSelection) {
				_selectedMapRegions.Clear();
			}
		}

		private void InputDragRegions(Vector2Int mapMousePositionInTiles) {
			for (int i = 0; i < _selectedMapRegions.Count; i++) {
				MapRegionProperties mapRegion = _selectedMapRegions[i];
				mapRegion.Position.x = mapMousePositionInTiles.x - _selectedObjectOffsets[i].x;
				mapRegion.Position.y = mapMousePositionInTiles.y - _selectedObjectOffsets[i].y;
			}
		}

		private void InputScaleRegion(Vector2Int mapMousePosition) {
			MapRegionProperties mapRegion = _selectedMapRegions[0];
			const int handleSize = 16;

			if (!_inputHasStartedScaling) {
				_scalingObjectOriginTopLeft = new Vector2Int(mapMousePosition.x, mapMousePosition.y);
				_scalingObjectOriginalPosition = new Vector2Int(mapRegion.Position.x, mapRegion.Position.y);
				_scalingObjectOriginalSize = new Vector2Int(mapRegion.Dimensions.x, mapRegion.Dimensions.y);

				_scalingObjectWidthLocked = false;
				_scalingObjectHeightLocked = false;

				bool isOnTopRow = BoomerangUtils.ValueIsBetween(_scalingObjectOriginTopLeft.y, mapRegion.Position.y, mapRegion.Position.y + handleSize);
				bool isOnBottomRow = BoomerangUtils.ValueIsBetween(
					_scalingObjectOriginTopLeft.y,
					mapRegion.Position.y + mapRegion.Dimensions.y - handleSize,
					mapRegion.Position.y + mapRegion.Dimensions.y
				);

				bool isOnLeftColumn = BoomerangUtils.ValueIsBetween(_scalingObjectOriginTopLeft.x, mapRegion.Position.x, mapRegion.Position.x + handleSize);
				bool isOnRightColumn = BoomerangUtils.ValueIsBetween(
					_scalingObjectOriginTopLeft.x,
					mapRegion.Position.x + mapRegion.Dimensions.x - handleSize,
					mapRegion.Position.x + mapRegion.Dimensions.x
				);

				if (!isOnTopRow && !isOnBottomRow && !isOnLeftColumn && !isOnRightColumn) {
					_scalingObjectOriginTopLeft = new Vector2Int(-1, -1);
					return;
				}

				bool isMoreThanTwoRows = mapRegion.Dimensions.y >= 3 * handleSize;
				bool isMoreThanTwoColumns = mapRegion.Dimensions.x >= 3 * handleSize;

				_objectIsScalingWidth = (isOnLeftColumn || isOnRightColumn) && (!isMoreThanTwoRows || !isOnTopRow && !isOnBottomRow);
				_objectIsScalingHeight = (isOnBottomRow || isOnTopRow) && (!isMoreThanTwoColumns || !isOnRightColumn && !isOnLeftColumn);

				if ((isOnLeftColumn || isOnRightColumn) && (isOnBottomRow || isOnTopRow)) {
					_objectIsScalingWidth = true;
					_objectIsScalingHeight = true;
				}

				_scalingObjectWidthLocked = _objectIsScalingWidth && isMoreThanTwoColumns;
				_objectScalingWidthPositive = _scalingObjectWidthLocked && isOnRightColumn;

				_scalingObjectHeightLocked = _objectIsScalingHeight && isMoreThanTwoRows;
				_objectScalingHeightPositive = _scalingObjectHeightLocked && isOnBottomRow;

				_inputHasStartedScaling = true;
			}

			if (_objectIsScalingWidth) {
				if (!_scalingObjectWidthLocked) {
					_scalingObjectWidthLocked = mapMousePosition.x != _scalingObjectOriginalPosition.x;
					_objectScalingWidthPositive = mapMousePosition.x > _scalingObjectOriginalPosition.x;
				} else {
					if (_objectScalingWidthPositive) {
						if (mapMousePosition.x >= _scalingObjectOriginalPosition.x) {
							mapRegion.Dimensions.x = mapMousePosition.x - _scalingObjectOriginalPosition.x + 1;
						}
					} else {
						if (mapMousePosition.x < _scalingObjectOriginalPosition.x + _scalingObjectOriginalSize.x) {
							mapRegion.Position.x = mapMousePosition.x;
							mapRegion.Dimensions.x = _scalingObjectOriginTopLeft.x - mapRegion.Position.x + _scalingObjectOriginalSize.x;
						}
					}
				}
			}

			if (_objectIsScalingHeight) {
				if (!_scalingObjectHeightLocked) {
					_scalingObjectHeightLocked = mapMousePosition.y != _scalingObjectOriginalPosition.y;
					_objectScalingHeightPositive = mapMousePosition.y > _scalingObjectOriginalPosition.y;
				} else {
					if (_objectScalingHeightPositive) {
						if (mapMousePosition.y >= _scalingObjectOriginalPosition.y) {
							mapRegion.Dimensions.y = mapMousePosition.y - _scalingObjectOriginalPosition.y + 1;
						}
					} else {
						if (mapMousePosition.y < _scalingObjectOriginalPosition.y + _scalingObjectOriginalSize.y) {
							mapRegion.Position.y = mapMousePosition.y;
							mapRegion.Dimensions.y = _scalingObjectOriginTopLeft.y - mapRegion.Position.y + _scalingObjectOriginalSize.y;
						}
					}
				}
			}
		}

		private void InputCloneRegions() {
			if (Event.current.type == EventType.Repaint) {
				List<MapRegionProperties> newlyAddedRegions = new List<MapRegionProperties>();

				foreach (MapRegionProperties mapRegion in _selectedMapRegions) {
					MapRegionProperties newRegion = new MapRegionProperties {
						Name = mapRegion.Name,
						Position = mapRegion.Position,
						Dimensions = mapRegion.Dimensions
					};

					newlyAddedRegions.Add(newRegion);
					ActiveMap.Regions.Add(newRegion);
				}

				_selectedMapRegions = newlyAddedRegions;
				_selectionObjectJustCloned = true;
			}
		}

		private MapLayerProperties GetTileEditorObjectParentLayer(TileEditorObject tileEditorObject) {
			MapLayerProperties parentMapLayer = ActiveMap.Layers[0];

			foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
				if (mapLayer.TileEditorObjects.Contains(tileEditorObject)) {
					parentMapLayer = mapLayer;
					break;
				}
			}

			return parentMapLayer;
		}
	}
}