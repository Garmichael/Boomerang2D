using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.GameEvents;
using Boomerang2DFramework.Framework.UiManagement.InteractionEvents;
using Boomerang2DFramework.Framework.UiManagement.TriggerSystem;
using Boomerang2DFramework.Framework.UiManagement.UiElements;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.HudObjectEditor {
	public class HudObjectEditor : EditorWindow {
		private static HudObjectEditor _window;
		private float _windowWidth;
		private float _windowHeight;

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

		private Mode _mode = Mode.Normal;

		private string _inputName;

		private readonly List<HudObjectProperties> _allHudObjects = new List<HudObjectProperties>();
		private int _indexForActiveHudObject;
		private int _previousIndexForActiveHudObject;

		private HudObjectProperties ActiveHudObject => BoomerangUtils.IndexInRange(_allHudObjects, _indexForActiveHudObject)
			? _allHudObjects[_indexForActiveHudObject]
			: null;

		private int _activeInteractionEventIndex;
		
		private int _indexForActiveElement;

		private HudElementProperties ActiveElement => BoomerangUtils.IndexInRange(ActiveHudObject.Elements, _indexForActiveElement)
			? ActiveHudObject.Elements[_indexForActiveElement]
			: null;

		private readonly List<UiElementEditor> _elementEditorBehaviors = new List<UiElementEditor>();

		private int _layoutScale = 1;
		private Rect _layoutEditingAreaRect;
		private Texture2D _layoutEditingTransparentBackground;

		private bool _reloadElementData;

		[MenuItem("Tools/Boomerang2D/HUD Object Editor", false, 252)]
		public static void ShowWindow() {
			_window = (HudObjectEditor) GetWindow(typeof(HudObjectEditor), false, "HUD Object Editor");
			_window.minSize = new Vector2(600, 400);
			_window.Show();
			_window.autoRepaintOnSceneChange = true;
			const string textureForTabIcon = "Assets/Boomerang2DFramework/Framework/EditorAssets/editorWindowIconHudObjectEditor.png";
			_window.titleContent = new GUIContent("HUD Object Editor", AssetDatabase.LoadAssetAtPath<Texture>(textureForTabIcon));
		}

		private void Update() {
			_totalTime = Time.realtimeSinceStartup;

			const float fps = 60f;

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
			if (ActiveHudObject != null && BoomerangDatabase.HudObjectEntries.ContainsKey(ActiveHudObject.Name)) {
				string originalJson = BoomerangDatabase.HudObjectEntries[ActiveHudObject.Name].text;
				_fileHasChanged = originalJson != JsonUtility.ToJson(ActiveHudObject, true);
			}
		}

		private void OnEnable() {
			BoomerangDatabase.IndexContent();

			LoadData();

			_reloadElementData = true;
		}

		private void SaveHudObject() {
			if (ActiveHudObject == null) {
				return;
			}

			File.WriteAllText(GameProperties.HudObjectsContentDirectory + "/" + ActiveHudObject.Name + ".json", JsonUtility.ToJson(ActiveHudObject, true));
			AssetDatabase.Refresh();
			BoomerangDatabase.PopulateDatabase();
			OnEnable();
		}

		private void LoadData() {
			_allHudObjects.Clear();

			foreach (KeyValuePair<string, TextAsset> hudObjectEntry in BoomerangDatabase.HudObjectEntries) {
				HudObjectProperties hudObjectProperties = JsonUtility.FromJson<HudObjectProperties>(hudObjectEntry.Value.text);
				_allHudObjects.Add(hudObjectProperties);
			}
		}

		private bool ShouldBuildEditorTextures() {
			return _layoutEditingTransparentBackground == null ||
			       _layoutEditingTransparentBackground.width != GameProperties.RenderDimensionsWidth ||
			       _layoutEditingTransparentBackground.height != GameProperties.RenderDimensionsHeight;
		}

		private void BuildEditorTextures() {
			const int tileSize = (int) GameProperties.PixelsPerUnit;
			int width = Mathf.CeilToInt(GameProperties.RenderDimensionsWidth / (float) tileSize) * tileSize;
			int height = Mathf.CeilToInt(GameProperties.RenderDimensionsHeight / (float) tileSize) * tileSize;

			Texture2D transparentBackgroundUncropped = new Texture2D(width, height) {
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point
			};

			for (int x = 0; x < width; x += tileSize) {
				for (int y = 0; y < height; y += tileSize) {
					transparentBackgroundUncropped.SetPixels32(
						x, y, tileSize, tileSize,
						SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.TransparentTile].GetPixels32()
					);
				}
			}

			_layoutEditingTransparentBackground = new Texture2D(GameProperties.RenderDimensionsWidth, GameProperties.RenderDimensionsHeight) {
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point
			};

			Color[] croppedPixels = transparentBackgroundUncropped
				.GetPixels(0, 0, GameProperties.RenderDimensionsWidth, GameProperties.RenderDimensionsHeight)
				.Reverse()
				.ToArray();

			_layoutEditingTransparentBackground.SetPixels(0, 0, GameProperties.RenderDimensionsWidth, GameProperties.RenderDimensionsHeight, croppedPixels);
			_layoutEditingTransparentBackground.Apply();
		}

		private bool ShouldRebuildElementEditorBehaviors() {
			return _reloadElementData ||
			       _elementEditorBehaviors != null &&
			       ActiveHudObject != null &&
			       _elementEditorBehaviors.Count != ActiveHudObject.Elements.Count;
		}

		private void RebuildElementEditorBehaviors() {
			_reloadElementData = false;
			_elementEditorBehaviors.Clear();

			foreach (HudElementProperties element in ActiveHudObject.Elements) {
				Type stateType = Type.GetType(element.ElementEditorClass);

				if (stateType != null) {
					_elementEditorBehaviors.Add((UiElementEditor) Activator.CreateInstance(stateType, this));
				}
			}
		}

		private void OnActiveHudObjectChange() {
			_previousIndexForActiveHudObject = _indexForActiveHudObject;
			RebuildElementEditorBehaviors();
		}

		private void OnGUI() {
			EditorGUIUtility.labelWidth = 45.0f;
			_windowWidth = position.width;
			_windowHeight = position.height;

			if (ShouldRebuildElementEditorBehaviors()) {
				RebuildElementEditorBehaviors();
			}

			if (ShouldBuildEditorTextures()) {
				BuildEditorTextures();
			}

			SuperForms.Title("HUD Object Editor");
			DrawSelectorContainer();
			DrawMainArea();
		}

		private void DrawSelectorContainer() {
			SuperForms.Region.MainOptionBarInline(() => {
				string[] hudObjectNames = _allHudObjects.Select(hudObjectProperties => hudObjectProperties.Name).ToArray();
				if (_mode != Mode.Normal) {
					return;
				}

				SuperForms.IconButton(SuperForms.IconButtons.ButtonAddLarge, () => {
					_inputName = "";
					_mode = Mode.Create;
				});

				if (_indexForActiveHudObject > hudObjectNames.Length) {
					_indexForActiveHudObject = hudObjectNames.Length - 1;
				}

				if (hudObjectNames.Length > 0) {
					_indexForActiveHudObject = SuperForms.DropDownLarge(
						_indexForActiveHudObject,
						hudObjectNames,
						GUILayout.Width(200)
					);
				}

				if (_indexForActiveHudObject != _previousIndexForActiveHudObject) {
					OnActiveHudObjectChange();
				}

				if (ActiveHudObject != null) {
					SuperForms.IconButtons saveIcon = _fileHasChanged
						? SuperForms.IconButtons.ButtonSaveAlertLarge
						: SuperForms.IconButtons.ButtonSaveLarge;

					if (SuperForms.IconButton(saveIcon)) {
						SaveHudObject();
					}

					SuperForms.IconButton(SuperForms.IconButtons.ButtonRenameLarge, () => {
						_inputName = ActiveHudObject.Name;
						_mode = Mode.Rename;
					});

					SuperForms.IconButton(SuperForms.IconButtons.ButtonDeleteLarge, () => { _mode = Mode.Delete; });
				}

				SuperForms.IconButton(SuperForms.IconButtons.ButtonRefreshLarge, OnEnable);
			});
		}

		private void DrawMainArea() {
			SuperForms.Region.MainArea(new Rect(0, 80, _windowWidth, _windowHeight - 80), () => {
				if (_mode == Mode.Normal && ActiveHudObject != null) {
					DrawHudObjectPreview();
					DrawEditingPanel();
				}

				if (_mode == Mode.Create) {
					DrawCreateNewHudObject();
				}

				if (_mode == Mode.Rename) {
					DrawRenameHudObjectForm();
				}

				if (_mode == Mode.Delete) {
					DrawDeleteHudObjectForm();
				}
			});
		}

		private void DrawCreateNewHudObject() {
			SuperForms.Region.Area(new Rect(10, 20, 300, _windowHeight - 120), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Create a new HUD Object");
					_inputName = SuperForms.StringField("HUD Object Name", _inputName);
					SuperForms.Region.Horizontal(() => {
						List<string> otherNames = _allHudObjects.Select(hudObjectProperties => hudObjectProperties.Name).ToList();
						StringValidation.StringValidationInfo stringValidationInfo = StringValidation.GetValidity(_inputName, otherNames);

						SuperForms.Button("Cancel", () => { _mode = Mode.Normal; });

						if (stringValidationInfo.IsValid) {
							SuperForms.Button("Create", () => {
								HudObjectProperties newHudObjectProperties = new HudObjectProperties {
									Name = _inputName
								};

								string fileName = GameProperties.HudObjectsContentDirectory + "/" + _inputName + ".json";

								File.WriteAllText(fileName, JsonUtility.ToJson(newHudObjectProperties, true));
								SaveHudObject();
								OnEnable();

								_mode = Mode.Normal;
							});
						} else {
							SuperForms.FullBoxLabel("Invalid Name: " + stringValidationInfo.ValidationMessage);
						}
					});
				});
			});
		}

		private void DrawRenameHudObjectForm() {
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Rename this Hud Object");
				_inputName = SuperForms.StringField("Hud Object Name", _inputName);


				List<string> otherNames = new List<string>();
				foreach (HudObjectProperties hudObjectProperties in _allHudObjects) {
					if (hudObjectProperties != ActiveHudObject) {
						otherNames.Add(hudObjectProperties.Name);
					}
				}

				StringValidation.StringValidationInfo stringValidationInfo = StringValidation.GetValidity(_inputName, otherNames);

				if (stringValidationInfo.IsValid) {
					SuperForms.Button("Rename", () => {
						AssetDatabase.DeleteAsset(GameProperties.HudObjectsContentDirectory + "/" + ActiveHudObject.Name + ".json");

						ActiveHudObject.Name = _inputName;
						SaveHudObject();

						_mode = Mode.Normal;
					});
				} else {
					SuperForms.FullBoxLabel(stringValidationInfo.ValidationMessage);
				}

				SuperForms.Button("Cancel", () => { _mode = Mode.Normal; });
			}, GUILayout.Width(300));
		}

		private void DrawDeleteHudObjectForm() {
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Delete Map");
				SuperForms.Region.Horizontal(() => {
					SuperForms.Button("NO!", () => { _mode = Mode.Normal; });
					SuperForms.Button("DELETE", () => {
						AssetDatabase.DeleteAsset(GameProperties.HudObjectsContentDirectory + "/" + ActiveHudObject.Name + ".json");
						AssetDatabase.Refresh();
						BoomerangDatabase.PopulateDatabase();
						_mode = Mode.Normal;
						OnEnable();
					});
				});
			}, GUILayout.Width(300));
		}

		private void DrawHudObjectPreview() {
			SuperForms.Region.Horizontal(() => {
				SuperForms.Button("1x", _layoutScale == 1, () => { _layoutScale = 1; }, GUILayout.ExpandWidth(false));
				SuperForms.Button("2x", _layoutScale == 2, () => { _layoutScale = 2; }, GUILayout.ExpandWidth(false));
				SuperForms.Button("3x", _layoutScale == 3, () => { _layoutScale = 3; }, GUILayout.ExpandWidth(false));
			});

			_layoutEditingAreaRect = new Rect(0, 20, _windowWidth - 320, _windowHeight - 110);

			SuperForms.Region.Area(_layoutEditingAreaRect, () => {
				SuperForms.Region.Scroll("HudObjectEditorLayoutArea", () => {
					int width = GameProperties.RenderDimensionsWidth * _layoutScale;
					int height = GameProperties.RenderDimensionsHeight * _layoutScale;

					Rect layoutSpace = new Rect(0, 0, width, height);
					SuperForms.BlockedArea(width, height);

					SuperForms.Texture(layoutSpace, _layoutEditingTransparentBackground);

					for (int index = 0; index < _elementEditorBehaviors.Count; index++) {
						if (!BoomerangUtils.IndexInRange(ActiveHudObject.Elements, index)) {
							break;
						}

						UiElementEditor elementEditorBehavior = _elementEditorBehaviors[index];
						string properties = ActiveHudObject.Elements[index].Properties;
						Type uiElementType = Type.GetType(ActiveHudObject.Elements[index].ElementPropertiesClass);

						if (uiElementType != null) {
							elementEditorBehavior?.RenderPreview(_layoutScale, properties, uiElementType);
						}
					}
				}, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			}, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

			_layoutEditingAreaRect.y += 80;
		}

		private readonly List<string> _panelTabs = new List<string> {
			"Properties",
			"Elements",
			"Interaction Events"
		};

		private int _panelTabIndex = 0;

		private void DrawEditingPanel() {
			SuperForms.Region.Area(new Rect(_windowWidth - 280, 20, 280, _windowHeight - 100), () => {
				SuperForms.Region.Horizontal(() => {
					for (int index = 0; index < _panelTabs.Count; index++) {
						string tabLabel = _panelTabs[index];
						int indexLocal = index;

						SuperForms.Button(tabLabel, _panelTabIndex == index, () => { _panelTabIndex = indexLocal; });
					}
				});

				if (_panelTabs[_panelTabIndex] == "Properties") {
					DrawHudObjectProperties();
				} else if (_panelTabs[_panelTabIndex] == "Elements") {
					DrawHudObjectElements();
				} else if (_panelTabs[_panelTabIndex] == "Interaction Events") {
					DrawHudObjectInteractionEvents();
				}
			});
		}

		private void DrawHudObjectProperties() {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Properties");
				
				List<string> availableShaders = new List<string>();
				foreach (KeyValuePair<string, Shader> shader in BoomerangDatabase.ShaderDatabaseEntries) {
					availableShaders.Add(shader.Key);
				}
				
				int index = BoomerangUtils.MinValue(availableShaders.IndexOf(ActiveHudObject.Shader), 0);
					
				ActiveHudObject.Shader = availableShaders[SuperForms.DropDown("Shader", index, availableShaders.ToArray())];
			});
		}
		
		private void DrawHudObjectElements() {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Elements");

				SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
					string elementName = AssemblyFinder.Assemblies.UiElementProperties.FirstOrDefault().Key;

					ActiveHudObject.Elements.Add(new HudElementProperties {
						ElementName = elementName,
						ElementPropertiesClass = AssemblyFinder.Assemblies.UiElementProperties[elementName],
						ElementBehaviorClass = AssemblyFinder.Assemblies.UiElementBehaviors[elementName + "Behavior"],
						ElementEditorClass = AssemblyFinder.Assemblies.UiElementEditors[elementName + "Editor"],
						Properties = "{}"
					});
				});

				SuperForms.Region.Scroll("HudObjectEditorElementList", () => {
					int indexToDelete = -1;
					int indexToMove = -1;
					int moveDirection = -1;

					for (int i = 0; i < ActiveHudObject.Elements.Count; i++) {
						HudElementProperties element = ActiveHudObject.Elements[i];
						int index = i;
						SuperForms.Region.Horizontal(() => {
							SuperForms.Label(index.ToString(), GUILayout.Width(20));

							SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowUp, () => {
								indexToMove = index;
								moveDirection = -1;
							});

							SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowDown, () => {
								indexToMove = index;
								moveDirection = 1;
							});

							SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete, () => { indexToDelete = index; });

							SuperForms.Button(element.ElementName, _indexForActiveElement == index, () => { _indexForActiveElement = index; });
						});
					}

					if (indexToDelete > -1) {
						ActiveHudObject.Elements.RemoveAt(indexToDelete);
						RebuildElementEditorBehaviors();
					}

					if (indexToMove > -1) {
						BoomerangUtils.Swap(ActiveHudObject.Elements, indexToMove, indexToMove + moveDirection);
						RebuildElementEditorBehaviors();
					}
				}, GUILayout.Height(200));
			});

			SuperForms.Space();

			if (ActiveElement != null) {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Element Properties");
					SuperForms.Region.Scroll("HudObjectEditorPropertiesList", () => {
						string previousPropertiesClass = ActiveElement.ElementPropertiesClass;

						Dictionary<string, string> uiElementClasses = AssemblyFinder.Assemblies.UiElementProperties;
						ActiveElement.ElementPropertiesClass = SuperForms.DictionaryDropDown(
							"Ui Element",
							uiElementClasses,
							ActiveElement.ElementPropertiesClass
						);

						ActiveElement.ElementName = uiElementClasses.FirstOrDefault(pair => pair.Value == ActiveElement.ElementPropertiesClass).Key;
						ActiveElement.ElementBehaviorClass = AssemblyFinder.Assemblies.UiElementBehaviors[ActiveElement.ElementName + "Behavior"];
						ActiveElement.ElementEditorClass = AssemblyFinder.Assemblies.UiElementEditors[ActiveElement.ElementName + "Editor"];

						if (previousPropertiesClass != ActiveElement.ElementPropertiesClass) {
							RebuildElementEditorBehaviors();
							return;
						}

						Type uiElementType = Type.GetType(ActiveElement.ElementPropertiesClass);

						SuperForms.Space();

						if (BoomerangUtils.IndexInRange(_elementEditorBehaviors, _indexForActiveElement)) {
							UiElementEditor uiEditorClass = _elementEditorBehaviors[_indexForActiveElement];
							ActiveElement.Properties = uiEditorClass.RenderPropertiesForm(ActiveElement.Properties, uiElementType);
						}
					});
				});
			}
		}

		private void DrawHudObjectInteractionEvents() {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Interaction Events");

				HudObjectInteractionEvent interactionInteractionEventToDelete = null;
				HudObjectTriggerBuilder triggerBuilderToDelete = null;
				GameEventBuilder gameEventBuilderToDelete = null;

				Dictionary<string, string> triggerClasses = AssemblyFinder.Assemblies.HudObjectTriggers;
				Dictionary<string, string> triggerPropertyClasses = AssemblyFinder.Assemblies.HudObjectTriggerProperties;

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					ActiveHudObject.InteractionEvents.Add(new HudObjectInteractionEvent {
						Name = "Unnamed Event"
					});
				}

				for (int index = 0; index < ActiveHudObject.InteractionEvents.Count; index++) {
					int localIndex = index;
					SuperForms.Region.Horizontal(() => {
						SuperForms.Button(ActiveHudObject.InteractionEvents[localIndex].Name,
							_activeInteractionEventIndex == localIndex,
							() => { _activeInteractionEventIndex = localIndex; }
						);
						SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete,
							() => { interactionInteractionEventToDelete = ActiveHudObject.InteractionEvents[localIndex]; });
					});
				}

				SuperForms.Space();

				if (!BoomerangUtils.IndexInRange(ActiveHudObject.InteractionEvents, _activeInteractionEventIndex)) {
					return;
				}

				HudObjectInteractionEvent interactionInteractionEvent = ActiveHudObject.InteractionEvents[_activeInteractionEventIndex];

				interactionInteractionEvent.Name = SuperForms.StringField("Name", interactionInteractionEvent.Name);
				interactionInteractionEvent.FireOnce = SuperForms.Checkbox("Fire Once", interactionInteractionEvent.FireOnce);
				
				SuperForms.Space();

				SuperForms.FullBoxLabel("Trigger Conditions");

				SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd,
					() => { interactionInteractionEvent.TriggerBuilders.Add(new HudObjectTriggerBuilder()); });

				foreach (HudObjectTriggerBuilder UiHudObjectTriggerBuilder in interactionInteractionEvent.TriggerBuilders) {
					SuperForms.Begin.VerticalSubBox();

					if (string.IsNullOrEmpty(UiHudObjectTriggerBuilder.TriggerClass)) {
						UiHudObjectTriggerBuilder.TriggerClass = triggerClasses.First().Value;
					}

					string triggerName = "TRIGGER NOT FOUND";

					foreach (KeyValuePair<string, string> triggerClass in triggerClasses) {
						triggerName = triggerClass.Value;
						if (triggerClass.Value == UiHudObjectTriggerBuilder.TriggerClass) {
							triggerName = triggerClass.Key;
							break;
						}
					}

					triggerName = Regex.Replace(triggerName, "(B2D: )", "");
					triggerName = Regex.Replace(triggerName, "(Ext: )", "");
					triggerName = Regex.Replace(triggerName, "(\\B[A-Z])", " $1");

					SuperForms.BoxSubHeader(triggerName);

					UiHudObjectTriggerBuilder.TriggerClass = SuperForms.DictionaryDropDown(
						"Trigger Class",
						triggerClasses,
						UiHudObjectTriggerBuilder.TriggerClass
					);

					string selectedTriggerClass = triggerClasses.FirstOrDefault(pair => pair.Value == UiHudObjectTriggerBuilder.TriggerClass).Key;
					UiHudObjectTriggerBuilder.TriggerPropertyClass = triggerPropertyClasses[selectedTriggerClass + "Properties"];
					Type triggerPropertiesType = Type.GetType(triggerPropertyClasses[selectedTriggerClass + "Properties"]);

					object triggerProperties = JsonUtility.FromJson(UiHudObjectTriggerBuilder.TriggerProperties, triggerPropertiesType);

					if (triggerProperties != null) {
						FieldInfo[] fields = triggerProperties.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

						if (fields.Length > 0) {
							SuperForms.Space();

							foreach (FieldInfo field in fields) {
								field.SetValue(triggerProperties, FieldBasedForms.DrawFormField(field.Name, field.GetValue(triggerProperties)));
							}
						}

						UiHudObjectTriggerBuilder.TriggerProperties = JsonUtility.ToJson(triggerProperties);
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						triggerBuilderToDelete = UiHudObjectTriggerBuilder;
					}

					SuperForms.End.Vertical();
					SuperForms.Space();
				}

				SuperForms.Space();

				SuperForms.FullBoxLabel("Events");

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					ActiveHudObject.InteractionEvents[_activeInteractionEventIndex].GameEventBuilders.Add(new GameEventBuilder());
				}

				Dictionary<string, string> gameEventClasses = AssemblyFinder.Assemblies.GameEvents;
				Dictionary<string, string> gameEventPropertyClasses = AssemblyFinder.Assemblies.GameEventProperties;

				foreach (GameEventBuilder gameEventBuilder in ActiveHudObject.InteractionEvents[_activeInteractionEventIndex].GameEventBuilders) {
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

				if (interactionInteractionEventToDelete != null) {
					ActiveHudObject.InteractionEvents.Remove(interactionInteractionEventToDelete);
				}

				if (triggerBuilderToDelete != null) {
					interactionInteractionEvent.TriggerBuilders.Remove(triggerBuilderToDelete);
				}

				if (gameEventBuilderToDelete != null) {
					ActiveHudObject.InteractionEvents[_activeInteractionEventIndex].GameEventBuilders.Remove(gameEventBuilderToDelete);
				}
			});
		}
	}
}