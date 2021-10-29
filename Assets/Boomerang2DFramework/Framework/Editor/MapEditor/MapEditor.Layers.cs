using System.Collections.Generic;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.MapEditor {
	public partial class MapEditor : EditorWindow {
		private void DrawMapEditingPanelLayers() {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Layers");

				if (_addMapLayerMode) {
					DrawMapEditingPanelLayersNewLayerForm();
					return;
				}

				if (_renameMapLayerMode) {
					DrawMapEditingPanelLayersRenameLayerForm();
					return;
				}

				foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
					SuperForms.Region.Horizontal(() => {
						bool isSelected = ActiveMapLayer == mapLayer;

						if (mapLayer.EditorIsVisible) {
							SuperForms.IconButton(SuperForms.IconButtons.ButtonEye, () => { mapLayer.EditorIsVisible = false; });
						} else {
							SuperForms.IconButton(SuperForms.IconButtons.ButtonEyeClosed, () => { mapLayer.EditorIsVisible = true; });
						}

						if (mapLayer.EditorIsLocked) {
							SuperForms.IconButton(SuperForms.IconButtons.ButtonLock, () => { mapLayer.EditorIsLocked = false; });
						} else {
							SuperForms.IconButton(SuperForms.IconButtons.ButtonUnlock, () => { mapLayer.EditorIsLocked = true; });
						}

						if (mapLayer.EditorIsLocked || !mapLayer.EditorIsVisible) {
							List<TileEditorObject> matchingTileEditorObjects = new List<TileEditorObject>();

							foreach (TileEditorObject tileEditorObject in _selectedTileEditorObjects) {
								if (mapLayer.TileEditorObjects.Contains(tileEditorObject)) {
									matchingTileEditorObjects.Add(tileEditorObject);
								}
							}

							foreach (TileEditorObject tileEditorObject in matchingTileEditorObjects) {
								_selectedTileEditorObjects.Remove(tileEditorObject);
							}
						}

						SuperForms.Button(
							mapLayer.Name,
							isSelected,
							() => { _selectedMapLayerIndex = ActiveMap.Layers.IndexOf(mapLayer); },
							GUILayout.ExpandWidth(true)
						);
					});
				}

				SuperForms.Region.Horizontal(() => {
					SuperForms.Space(26 * 2);
					SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
						_addMapLayerMode = true;
						_inputName = "";
					});

					if (ActiveMapLayer != null) {
						SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowUp, () => {
							if (_selectedMapLayerIndex > 0) {
								BoomerangUtils.Swap(ActiveMap.Layers, _selectedMapLayerIndex, _selectedMapLayerIndex - 1);
								_selectedMapLayerIndex -= 1;
							}
						});

						SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowDown, () => {
							if (_selectedMapLayerIndex < ActiveMap.Layers.Count - 1) {
								BoomerangUtils.Swap(ActiveMap.Layers, _selectedMapLayerIndex, _selectedMapLayerIndex + 1);
								_selectedMapLayerIndex += 1;
							}
						});

						SuperForms.IconButton(SuperForms.IconButtons.ButtonEdit, () => { _showMapLayerPropertiesPanel = !_showMapLayerPropertiesPanel; });

						SuperForms.IconButton(SuperForms.IconButtons.ButtonRename, () => {
							_renameMapLayerMode = true;
							_inputName = ActiveMapLayer.Name;
						});

						SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete, () => { ActiveMap.Layers.RemoveAt(_selectedMapLayerIndex); });
					}
				});


				if (ActiveMapLayer != null && _showMapLayerPropertiesPanel) {
					SuperForms.Space();
					SuperForms.BoxSubHeader("Layer Properties");

					ActiveMapLayer.UseColliders = SuperForms.Checkbox("Use Tile Colliders", ActiveMapLayer.UseColliders);

					List<string> availableShaders = new List<string>();
					foreach (KeyValuePair<string, Shader> shader in BoomerangDatabase.ShaderDatabaseEntries) {
						availableShaders.Add(shader.Key);
					}

					int index = BoomerangUtils.MinValue(availableShaders.IndexOf(ActiveMapLayer.Shader), 0);
					
					ActiveMapLayer.Shader = availableShaders[SuperForms.DropDown("Layer Shader", index, availableShaders.ToArray())];

					int selectedTileSetIndex = AllTileSetNames.IndexOf(ActiveMapLayer.Tileset);
					selectedTileSetIndex = SuperForms.DropDown("Tileset", selectedTileSetIndex, AllTileSetNames.ToArray());
					ActiveMapLayer.Tileset = AllTileSetNames[selectedTileSetIndex];

					ActiveMapLayer.MapLayerType = (MapLayerType) SuperForms.EnumDropdown("Layer Type", ActiveMapLayer.MapLayerType);
					
					if (ActiveMapLayer.MapLayerType == MapLayerType.DepthLayer) {
						SuperForms.Space();
						SuperForms.BoxSubHeader("Map Depth Layer Properties");
				
						List<string> stamps = new List<string>();

						TileSetData tileSetData = _allTileSets[ActiveMapLayer.Tileset];
						foreach (TilesetEditorStamp stamp in tileSetData.Properties.Stamps) {
							stamps.Add(stamp.Name);
						}

						ActiveMapLayer.DepthLayerStampId = SuperForms.DropDown("Stamp", ActiveMapLayer.DepthLayerStampId, stamps.ToArray());
						ActiveMapLayer.DepthLayerStampDimensions =SuperForms.Vector2FieldSingleLine("Stamp Dimensions", ActiveMapLayer.DepthLayerStampDimensions);
						ActiveMapLayer.DepthLayerOrigin = (DepthLayerOrigin) SuperForms.EnumDropdown("Origin Source", ActiveMapLayer.DepthLayerOrigin);
						ActiveMapLayer.DepthLayerOriginCorner = (DepthLayerOriginCorner) SuperForms.EnumDropdown("Origin Corner", ActiveMapLayer.DepthLayerOriginCorner);
						ActiveMapLayer.DepthLayerRepeatX = SuperForms.Checkbox("Repeat X", ActiveMapLayer.DepthLayerRepeatX); 
						ActiveMapLayer.DepthLayerRepeatY = SuperForms.Checkbox("Repeat Y", ActiveMapLayer.DepthLayerRepeatY);
						ActiveMapLayer.DepthLayerScrollMode = (DepthLayerScrollMode) SuperForms.EnumDropdown("Scroll Mode", ActiveMapLayer.DepthLayerScrollMode);
						ActiveMapLayer.DepthLayerScrollSpeed = SuperForms.Vector2Field("Scroll Speed", ActiveMapLayer.DepthLayerScrollSpeed);
					}
				}
			});
		}

		private void DrawMapEditingPanelLayersNewLayerForm() {
			List<string> otherNames = new List<string>();
			foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
				otherNames.Add(mapLayer.Name);
			}

			_inputName = SuperForms.StringField("Layer Name", _inputName);
			 
			List<string> availableShaders = new List<string>();
			foreach (KeyValuePair<string, Shader> shader in BoomerangDatabase.ShaderDatabaseEntries) {
				availableShaders.Add(shader.Key);
			}

			int index = BoomerangUtils.MinValue(availableShaders.IndexOf(_addMapLayerShader), 0);
			_addMapLayerShader = availableShaders[SuperForms.DropDown("Layer Shader", index, availableShaders.ToArray())];
			
			_addMapLayerTilesetIndex = SuperForms.DropDown("Tileset", _addMapLayerTilesetIndex, AllTileSetNames.ToArray());

			_addMapLayerType = (MapLayerType) SuperForms.EnumDropdown("Layer Type", _addMapLayerType);

			if (_addMapLayerType == MapLayerType.DepthLayer) {
				SuperForms.Space();
				SuperForms.BoxSubHeader("Map Depth Layer Properties");
				
				List<string> stamps = new List<string>();

				TileSetData tileSetData = _allTileSets[AllTileSetNames[_addMapLayerTilesetIndex]];
				foreach (TilesetEditorStamp stamp in tileSetData.Properties.Stamps) {
					stamps.Add(stamp.Name);
				}

				_addMapDepthLayerStampId = SuperForms.DropDown("Stamp", _addMapDepthLayerStampId, stamps.ToArray());
				_addMapDepthLayerStampDimensions = SuperForms.Vector2FieldSingleLine("Stamp Dimensions", _addMapDepthLayerStampDimensions);
				_addMapDepthLayerOrigin = (DepthLayerOrigin) SuperForms.EnumDropdown("Origin Source", _addMapDepthLayerOrigin);
				_addMapDepthLayerOriginCorner = (DepthLayerOriginCorner) SuperForms.EnumDropdown("Origin Corner", _addMapDepthLayerOriginCorner);
				_addMapDepthLayerRepeatX = SuperForms.Checkbox("Repeat X", _addMapDepthLayerRepeatX); 
				_addMapDepthLayerRepeatY = SuperForms.Checkbox("Repeat Y", _addMapDepthLayerRepeatY); 
				_addMapDepthLayerScrollMode = (DepthLayerScrollMode) SuperForms.EnumDropdown("Scroll Mode", _addMapDepthLayerScrollMode);
				_addMapDepthLayerScrollSpeed = SuperForms.Vector2Field("Scroll Speed", _addMapDepthLayerScrollSpeed);
			}
			
			StringValidation.StringValidationInfo stringValidationInfo = StringValidation.GetValidity(_inputName, otherNames);

			if (!stringValidationInfo.IsValid) {
				SuperForms.FullBoxLabel("Invalid Name: " + stringValidationInfo.ValidationMessage);
			} else {
				SuperForms.Button("Add Layer", () => {
					ActiveMap.Layers.Add(new MapLayerProperties {
						Name = _inputName,
						Shader = _addMapLayerShader,
						Tileset = AllTileSetNames[_addMapLayerTilesetIndex],
						MapLayerType = _addMapLayerType,
						DepthLayerStampId = _addMapDepthLayerStampId,
						DepthLayerStampDimensions = _addMapDepthLayerStampDimensions,
						DepthLayerOrigin = _addMapDepthLayerOrigin,
						DepthLayerOriginCorner = _addMapDepthLayerOriginCorner,
						DepthLayerScrollMode = _addMapDepthLayerScrollMode,
						DepthLayerScrollSpeed = _addMapDepthLayerScrollSpeed,
						DepthLayerRepeatX = _addMapDepthLayerRepeatX,
						DepthLayerRepeatY = _addMapDepthLayerRepeatY
					});

					_addMapLayerMode = false;
				});
			}

			SuperForms.Button("Cancel", () => { _addMapLayerMode = false; });
		}

		private void DrawMapEditingPanelLayersRenameLayerForm() {
			List<string> otherNames = new List<string>();
			foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
				if (mapLayer != ActiveMapLayer) {
					otherNames.Add(mapLayer.Name);
				}
			}

			_inputName = SuperForms.StringField("Layer Name", _inputName);

			StringValidation.StringValidationInfo stringValidationInfo = StringValidation.GetValidity(_inputName, otherNames);

			if (!stringValidationInfo.IsValid) {
				SuperForms.FullBoxLabel("Invalid Name: " + stringValidationInfo.ValidationMessage);
			} else {
				SuperForms.Button("Rename Layer", () => {
					ActiveMapLayer.Name = _inputName;
					_renameMapLayerMode = false;
				});
			}

			SuperForms.Button("Cancel", () => { _renameMapLayerMode = false; });
		}
	}
}