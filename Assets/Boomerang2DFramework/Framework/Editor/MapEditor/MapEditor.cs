using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.GameEvents;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.MapEditor {
	public partial class MapEditor : EditorWindow {
		[MenuItem("Tools/Boomerang2D/Map Editor", false, 152)]
		public static void ShowWindow() {
			_window = (MapEditor) GetWindow(typeof(MapEditor), false, "Map Editor");
			_window.minSize = new Vector2(600, 400);
			_window.Show();
			_window.autoRepaintOnSceneChange = true;
			const string textureForTabIcon = "Assets/Boomerang2DFramework/Framework/EditorAssets/editorWindowIconMapEditor.png";
			_window.titleContent = new GUIContent("Map Editor", AssetDatabase.LoadAssetAtPath<Texture>(textureForTabIcon));
		}

		private void Update() {
			_totalTime = Time.realtimeSinceStartup;
			UpdateRepaint();
			UpdateCheckSaveTime();
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

		private void UpdateCheckSaveTime() {
			if (_totalTime > _timeToCheckSave + 1) {
				_timeToCheckSave = _totalTime;

				if (ActiveMap != null && BoomerangDatabase.MapDatabaseEntries.ContainsKey(ActiveMap.Name)) {
					string originalJson = BoomerangDatabase.MapDatabaseEntries[ActiveMap.Name].text;
					_fileHasChanged = originalJson != JsonUtility.ToJson(ActiveMap, true);
				}
			}
		}

		private void OnEnable() {
			_timeToCheckSave = 0;

			_selectedTileEditorObjects.Clear();
			_selectedActorObjects.Clear();
			_selectedMapViews.Clear();
			_selectedMapRegions.Clear();

			_tileEditingMode = TileEditingMode.NoneSelected;
			_actorEditingMode = ActorEditingMode.NoneSelected;

			_lastZoomTime = 0;

			LoadData();
		}

		private void OnGUI() {
			EditorGUIUtility.labelWidth = 45.0f;
			_windowWidth = position.width;
			_windowHeight = position.height;

			if (ShouldBuildMapEditorTextures()) {
				BuildMapEditorTextures();
			}

			if (ShouldReloadData()) {
				OnEnable();
			}

			SuperForms.Title("Map Editor");

			DrawSelectorBar();
			DrawMainArea();

			HandleInput();
		}

		private void DrawSelectorBar() {
			SuperForms.Region.MainOptionBarInline(() => {
				if (_mainMode != Mode.Normal) {
					return;
				}

				if (_selectedMapIndex > AllMapNames.Count) {
					_selectedMapIndex = AllMapNames.Count - 1;
				}

				if (AllMapNames.Count > 0) {
					_selectedMapIndex = SuperForms.DropDownLarge(
						_selectedMapIndex,
						AllMapNames.ToArray(),
						GUILayout.Width(200)
					);
				}

				if (_selectedMapIndex != _previousSelectedMapIndex) {
					OnMapChange();
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAddLarge)) {
					_inputName = "";
					_mainMode = Mode.Create;
				}

				if (ActiveMap != null) {
					SuperForms.IconButtons saveIcon = _fileHasChanged
						? SuperForms.IconButtons.ButtonSaveAlertLarge
						: SuperForms.IconButtons.ButtonSaveLarge;

					if (SuperForms.IconButton(saveIcon)) {
						SaveMap();
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonRenameLarge)) {
						_inputName = ActiveMap.Name;
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
						if (ActiveMap != null) {
							DrawMapEditingArea();
							DrawMapEditingPanel();
						}

						break;
					case Mode.Create:
						DrawAddNewMapForm();
						break;
					case Mode.Rename:
						DrawRenameMapForm();
						break;
					case Mode.Delete:
						DrawDeleteMapForm();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			});
		}

		private void DrawAddNewMapForm() {
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Create New Map");
				_inputName = SuperForms.StringField("Map Name", _inputName);
				_mapWidthInput = SuperForms.IntField("Tiles Wide", _mapWidthInput);
				_mapHeightInput = SuperForms.IntField("Tiles High", _mapHeightInput);

				if (_mapWidthInput < 1) {
					_mapWidthInput = 1;
				}

				if (_mapHeightInput < 1) {
					_mapHeightInput = 1;
				}

				List<string> otherNames = _allMaps.Select(mapProperties => mapProperties.Name).ToList();
				StringValidation.StringValidationInfo stringValidationInfo = StringValidation.GetValidity(_inputName, otherNames);

				if (stringValidationInfo.IsValid) {
					SuperForms.Button("Create Map", () => {
						MapProperties newMap = new MapProperties {
							Name = _inputName,
							Width = _mapWidthInput,
							Height = _mapHeightInput
						};

						_allMaps.Add(newMap);
						_mainMode = Mode.Normal;
						_selectedMapIndex = _allMaps.IndexOf(newMap);
						SaveMap();
					});
				} else {
					SuperForms.FullBoxLabel("Invalid Name: " + stringValidationInfo.ValidationMessage);
				}

				SuperForms.Button("Cancel", () => { _mainMode = Mode.Normal; });
			}, GUILayout.Width(260));
		}

		private void DrawRenameMapForm() {
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Rename Map");
				_inputName = SuperForms.StringField("New Name", _inputName);

				List<string> otherNames = new List<string>();
				foreach (MapProperties mapProperties in _allMaps) {
					if (mapProperties != ActiveMap) {
						otherNames.Add(mapProperties.Name);
					}
				}

				StringValidation.StringValidationInfo stringValidationInfo = StringValidation.GetValidity(_inputName, otherNames);

				if (stringValidationInfo.IsValid) {
					SuperForms.Button("Rename Map", () => {
						if (_inputName != ActiveMap.Name) {
							AssetDatabase.DeleteAsset(GameProperties.MapContentDirectory + "/" + ActiveMap.Name + ".json");

							ActiveMap.Name = _inputName;
							SaveMap();
						}

						_mainMode = Mode.Normal;
					});
				} else {
					SuperForms.FullBoxLabel("Invalid Name: " + stringValidationInfo.ValidationMessage);
				}

				SuperForms.Button("Cancel", () => { _mainMode = Mode.Normal; });
			}, GUILayout.Width(260));
		}

		private void DrawDeleteMapForm() {
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Delete Map");
				SuperForms.Region.Horizontal(() => {
					SuperForms.Button("NO!", () => { _mainMode = Mode.Normal; });
					SuperForms.Button("DELETE", () => {
						AssetDatabase.DeleteAsset(GameProperties.MapContentDirectory + "/" + ActiveMap.Name + ".json");
						AssetDatabase.Refresh();
						BoomerangDatabase.PopulateDatabase();
						_mainMode = Mode.Normal;
						OnEnable();
					});
				});
			}, GUILayout.Width(260));
		}

		private void DrawMapEditingArea() {
			_mapEditingAreaRect = new Rect(0, 20, _windowWidth - 260, _windowHeight - 100);

			SuperForms.Region.Horizontal(() => {
				SuperForms.Label("Zoom", GUILayout.Width(50));
				for (int i = 0; i < _mapScales.Count; i++) {
					string label = _mapScales[i].ToString(CultureInfo.InvariantCulture) + "x";

					if (SuperForms.Button(label, _selectedMapScaleIndex == i, GUILayout.Width(30))) {
						_selectedMapScaleIndex = i;
					}
				}

				SuperForms.Space();

				SuperForms.Button("Select", _inputMode == InputMode.Selection, () => { _inputMode = InputMode.Selection; }, GUILayout.ExpandWidth(false));
				SuperForms.Button("Pan", _inputMode == InputMode.Pan, () => { _inputMode = InputMode.Pan; }, GUILayout.ExpandWidth(false));
			});

			SuperForms.Region.Area(_mapEditingAreaRect, () => {
				_mapEditingAreaScrollPosition = SuperForms.Region.ScrollDisabledWheel(_mapEditingAreaScrollPosition, () => {
					SuperForms.BlockedArea(
						Mathf.CeilToInt(ActiveMap.Width * GameProperties.PixelsPerUnit * MapEditingScale),
						Mathf.CeilToInt(ActiveMap.Height * GameProperties.PixelsPerUnit * MapEditingScale)
					);

					SuperForms.Texture(
						new Rect(0, 0,
							ActiveMap.Width * GameProperties.PixelsPerUnit * MapEditingScale,
							ActiveMap.Height * GameProperties.PixelsPerUnit * MapEditingScale
						),
						_mapEditingTransparentBackground
					);

					foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
						if (mapLayer.EditorIsVisible && mapLayer.MapLayerType == MapLayerType.Normal) {
							foreach (TileEditorObject mapLayerTileEditorObject in mapLayer.TileEditorObjects) {
								DrawMapEditingAreaTileEditorObject(mapLayer, mapLayerTileEditorObject);
							}

							foreach (MapActorPlacementProperties mapActorPlacementProperties in mapLayer.Actors) {
								DrawMapEditingAreaActors(mapActorPlacementProperties);
							}

							foreach (MapPrefabPlacementProperties mapPrefabPlacementProperties in mapLayer.Prefabs) {
								DrawMapEditingAreaPrefabs(mapPrefabPlacementProperties);
							}
						}
					}

					if (_tileEditingMode == TileEditingMode.PaintingBrush && _brushEditorObjectBeingEdited.CachedEditorTexture != null) {
						DrawMapEditingAreaActiveEditableBrush();
					}

					foreach (MapRegionProperties mapRegion in ActiveMap.Regions) {
						DrawMapEditingAreaRegion(mapRegion);
					}

					foreach (MapViewProperties mapView in ActiveMap.Views) {
						DrawMapEditingAreaView(mapView);
					}
				});
			});

			_mapEditingAreaRect.y += 80;
		}

		private void DrawMapEditingPanel() {
			SuperForms.Region.Area(new Rect(_windowWidth - 260, 20, 260, _windowHeight - 100), () => {
				SuperForms.Region.Horizontal(() => {
					SuperForms.IconButton(_editingMode == EditingMode.Properties
							? SuperForms.IconButtons.MapEditorDimensionsSelected
							: SuperForms.IconButtons.MapEditorDimensions,
						() => { _editingMode = EditingMode.Properties; });

					SuperForms.Space();

					SuperForms.IconButton(_editingMode == EditingMode.TilesTileSheet
							? SuperForms.IconButtons.MapEditorTilesSelected
							: SuperForms.IconButtons.MapEditorTiles,
						() => { _editingMode = EditingMode.TilesTileSheet; });

					SuperForms.IconButton(_editingMode == EditingMode.Actors
							? SuperForms.IconButtons.MapEditorActorsSelected
							: SuperForms.IconButtons.MapEditorActors,
						() => { _editingMode = EditingMode.Actors; });

					SuperForms.Space();

					SuperForms.IconButton(_editingMode == EditingMode.Prefabs
							? SuperForms.IconButtons.MapEditorPrefabsSelected
							: SuperForms.IconButtons.MapEditorPrefabs,
						() => { _editingMode = EditingMode.Prefabs; });

					SuperForms.Space();

					SuperForms.IconButton(_editingMode == EditingMode.Views
							? SuperForms.IconButtons.MapEditorViewsSelected
							: SuperForms.IconButtons.MapEditorViews,
						() => { _editingMode = EditingMode.Views; });

					SuperForms.IconButton(_editingMode == EditingMode.Regions
							? SuperForms.IconButtons.MapEditorRegionsSelected
							: SuperForms.IconButtons.MapEditorRegions,
						() => { _editingMode = EditingMode.Regions; });
				});

				switch (_editingMode) {
					case EditingMode.Properties:
						DrawMapEditingPanelProperties();
						break;
					case EditingMode.TilesTileSheet:
					case EditingMode.TilesStamps:
					case EditingMode.TilesBrushes:
						DrawMapEditingPanelTiles();
						break;
					case EditingMode.Actors:
						DrawMapEditingPanelActors();
						break;
					case EditingMode.Prefabs:
						DrawMapEditingPanelPrefabs();
						break;
					case EditingMode.Views:
						DrawMapEditingPanelViews();
						break;
					case EditingMode.Regions:
						DrawMapEditingPanelRegions();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			});
		}

		private void DrawMapEditingPanelProperties() {
			SuperForms.BoxHeader("Map Properties");
			SuperForms.Region.VerticalBox(() => {
				if (_resizeMapMode) {
					DrawMapEditingPanelResize();
				} else {
					SuperForms.FullBoxLabel("Dimensions: " + ActiveMap.Width + "x" + ActiveMap.Height);
					ActiveMap.ChunkDimensions = SuperForms.Vector2FieldSingleLine("Chunk Size", ActiveMap.ChunkDimensions);
					ActiveMap.ChunkDimensions.x = BoomerangUtils.MinValue(ActiveMap.ChunkDimensions.x, 1);
					ActiveMap.ChunkDimensions.y = BoomerangUtils.MinValue(ActiveMap.ChunkDimensions.y, 1);

					SuperForms.Button("Resize Map", () => {
						_mapWidthInput = ActiveMap.Width;
						_mapHeightInput = ActiveMap.Height;
						_resizeMapMode = true;
					});

					SuperForms.Space();

					DrawMapEditingPanelInitialEvents();
				}
			});
		}

		private void DrawMapEditingPanelResize() {
			_mapWidthInput = SuperForms.IntField("New Width", _mapWidthInput);
			_mapHeightInput = SuperForms.IntField("New Height", _mapHeightInput);

			_resizeStyle = (ResizeStyle) SuperForms.EnumDropdown("Resize Position", _resizeStyle);
			SuperForms.Region.Horizontal(() => {
				SuperForms.Button("Cancel", () => { _resizeMapMode = false; });
				SuperForms.Button("Resize", () => {
					bool modifyLeft = _resizeStyle == ResizeStyle.TopMiddle || _resizeStyle == ResizeStyle.TopRight || _resizeStyle == ResizeStyle.Middle ||
					                  _resizeStyle == ResizeStyle.Right || _resizeStyle == ResizeStyle.BottomMiddle || _resizeStyle == ResizeStyle.BottomRight;

					bool modifyRight = _resizeStyle == ResizeStyle.TopMiddle || _resizeStyle == ResizeStyle.TopLeft || _resizeStyle == ResizeStyle.Middle ||
					                   _resizeStyle == ResizeStyle.Left || _resizeStyle == ResizeStyle.BottomMiddle || _resizeStyle == ResizeStyle.BottomLeft;

					bool modifyTop = _resizeStyle == ResizeStyle.Left || _resizeStyle == ResizeStyle.Middle || _resizeStyle == ResizeStyle.Right ||
					                 _resizeStyle == ResizeStyle.BottomLeft || _resizeStyle == ResizeStyle.BottomMiddle ||
					                 _resizeStyle == ResizeStyle.BottomRight;

					bool modifyBottom = _resizeStyle == ResizeStyle.TopLeft || _resizeStyle == ResizeStyle.TopMiddle || _resizeStyle == ResizeStyle.TopRight ||
					                    _resizeStyle == ResizeStyle.Left || _resizeStyle == ResizeStyle.Middle || _resizeStyle == ResizeStyle.Right;

					Vector2Int resizeDifference = new Vector2Int(_mapWidthInput - ActiveMap.Width, _mapHeightInput - ActiveMap.Height);

					int leftDifference = 0;
					int topDifference = 0;

					if (modifyLeft && modifyRight) {
						leftDifference = Mathf.CeilToInt((float) resizeDifference.x / 2);
					} else if (modifyLeft) {
						leftDifference = resizeDifference.x;
					}

					if (modifyTop && modifyBottom) {
						topDifference = Mathf.CeilToInt((float) resizeDifference.y / 2);
					} else if (modifyTop) {
						topDifference = resizeDifference.y;
					}

					foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
						foreach (TileEditorObject tileEditorObject in mapLayer.TileEditorObjects) {
							tileEditorObject.X += leftDifference;
							tileEditorObject.Y += topDifference;
						}

						foreach (MapActorPlacementProperties mapActor in mapLayer.Actors) {
							mapActor.Position.x += leftDifference;
							mapActor.Position.y += topDifference;
						}
					}

					foreach (MapViewProperties mapView in ActiveMap.Views) {
						mapView.Position.x += leftDifference;
						mapView.Position.y += topDifference;
					}

					foreach (MapRegionProperties mapRegion in ActiveMap.Regions) {
						mapRegion.Position.x += leftDifference;
						mapRegion.Position.y += topDifference;
					}


					ActiveMap.Width = _mapWidthInput;
					ActiveMap.Height = _mapHeightInput;
					_resizeMapMode = false;
				});
			});
		}

		private void DrawMapEditingPanelInitialEvents() {
			SuperForms.BoxSubHeader("On-Load Events");
			if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
				ActiveMap.GameEventBuilders.Add(new GameEventBuilder());
			}

			Dictionary<string, string> gameEventClasses = AssemblyFinder.Assemblies.GameEvents;
			Dictionary<string, string> gameEventPropertyClasses = AssemblyFinder.Assemblies.GameEventProperties;

			GameEventBuilder gameEventBuilderToDelete = null;

			SuperForms.Region.Scroll("MapEditorOnLoadEventsList", () => {
				foreach (GameEventBuilder gameEventBuilder in ActiveMap.GameEventBuilders) {
					SuperForms.Begin.VerticalSubBox();

					if (string.IsNullOrEmpty(gameEventBuilder.GameEventClass)) {
						gameEventBuilder.GameEventClass = gameEventClasses.First().Value;
					}

					string eventName = "EVENT NOT FOUND";

					foreach (KeyValuePair<string, string> eventClass in gameEventClasses) {
						eventName = eventClass.Value;
						if (eventClass.Value == gameEventBuilder.GameEventClass) {
							eventName = eventClass.Key;
							break;
						}
					}

					eventName = Regex.Replace(eventName, "(B2D: )", "");
					eventName = Regex.Replace(eventName, "(Ext: )", "");
					eventName = Regex.Replace(eventName, "(\\B[A-Z])", " $1");

					SuperForms.BoxSubHeader(eventName);

					gameEventBuilder.GameEventClass = SuperForms.DictionaryDropDown(
						"Event Class",
						gameEventClasses,
						gameEventBuilder.GameEventClass
					);

					gameEventBuilder.StartTime = SuperForms.FloatField("Event Start Time", gameEventBuilder.StartTime);

					SuperForms.Space();

					string selectedGameEventClass = gameEventClasses.FirstOrDefault(pair => pair.Value == gameEventBuilder.GameEventClass).Key;

					if (!gameEventPropertyClasses.ContainsKey(selectedGameEventClass + "Properties")) {
						SuperForms.FullBoxLabel("COULDN'T FIND PROPERTIES CLASS FOR GAME EVENT!");
						SuperForms.End.Vertical();
						continue;
					}

					gameEventBuilder.GameEventPropertiesClass = gameEventPropertyClasses[selectedGameEventClass + "Properties"];
					Type eventPropertiesType = Type.GetType(gameEventPropertyClasses[selectedGameEventClass + "Properties"]);
					object eventProperties = JsonUtility.FromJson(gameEventBuilder.GameEventProperties, eventPropertiesType);

					if (eventProperties != null) {
						FieldInfo[] fields = eventProperties.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

						if (fields.Length > 0) {
							SuperForms.Space();

							foreach (FieldInfo field in fields) {
								field.SetValue(eventProperties, FieldBasedForms.DrawFormField(field.Name, field.GetValue(eventProperties)));
							}
						}

						gameEventBuilder.GameEventProperties = JsonUtility.ToJson(eventProperties);
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						gameEventBuilderToDelete = gameEventBuilder;
					}

					SuperForms.End.Vertical();
				}
			});
			if (gameEventBuilderToDelete != null) {
				ActiveMap.GameEventBuilders.Remove(gameEventBuilderToDelete);
			}
		}
	}
}