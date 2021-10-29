using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Boomerang2DFramework.Framework.Actors.Weapons;
using Boomerang2DFramework.Framework.Actors.Weapons.MeleeStrikeTypes;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.Weapons {
	public class WeaponStudio : EditorWindow {
		private static WeaponStudio _window;
		private static float _windowWidth;
		private static float _windowHeight;

		private float _repaintTime;
		private bool _repaintNext;
		private float _totalTime;
		private float _lastTotalTime;
		private float _timeToCheckSave;
		private bool _fileHasChanged;
		private float _deltaTime;


		private bool _previewIsPlaying = true;
		private float _previewTime;

		private string _inputName;

		private readonly string[] _easeModes = EasingModeFormulas.GetListOfEasingMethods().ToArray();

		private readonly List<WeaponProperties> _weaponProperties = new List<WeaponProperties>();
		private int _selectedWeaponIndex;
		private int _previousWeaponIndex;

		private WeaponProperties ActiveWeapon => BoomerangUtils.IndexInRange(_weaponProperties, _selectedWeaponIndex)
			? _weaponProperties[_selectedWeaponIndex]
			: null;

		private int _activeMeleeStrikeIndex;
		private int _previousMeleeStrikeIndex;

		private MeleeStrikeProperties ActiveMeleeStrike =>
			ActiveWeapon != null && BoomerangUtils.IndexInRange(ActiveWeapon.MeleeStrikes, _activeMeleeStrikeIndex)
				? ActiveWeapon.MeleeStrikes[_activeMeleeStrikeIndex]
				: null;

		private readonly List<MeleeStrikeType> _builtMeleeStrikes = new List<MeleeStrikeType>();

		private enum Mode {
			Normal,
			Create,
			Rename,
			Delete
		}

		private Mode _mode = Mode.Normal;

		[MenuItem("Tools/Boomerang2D/Weapon Studio", false, 103)]
		public static void ShowWindow() {
			_window = (WeaponStudio) GetWindow(typeof(WeaponStudio), false, "Weapon Studio");
			_window.minSize = new Vector2(600, 400);
			_window.Show();
			_window.autoRepaintOnSceneChange = true;
			_window.titleContent = new GUIContent(
				"Weapon Studio",
				AssetDatabase.LoadAssetAtPath<Texture>("Assets/Boomerang2DFramework/Framework/EditorAssets/editorWindowIconWeaponStudio.png")
			);
		}

		private void OnEnable() {
			_timeToCheckSave = 0;
			LoadWeapons();
		}

		private void LoadWeapons() {
			_weaponProperties.Clear();

			foreach (KeyValuePair<string, TextAsset> actorJson in BoomerangDatabase.WeaponDatabaseEntries) {
				WeaponProperties weaponProperties = JsonUtility.FromJson<WeaponProperties>(actorJson.Value.text);
				_weaponProperties.Add(weaponProperties);
			}
		}

		private void SaveWeapon() {
			File.WriteAllText(GameProperties.WeaponContentDirectory + "/" + ActiveWeapon.Name + ".json", JsonUtility.ToJson(ActiveWeapon, true));
			AssetDatabase.Refresh();
		}

		private void DeleteWeapon() {
			AssetDatabase.DeleteAsset(GameProperties.WeaponContentDirectory + "/" + ActiveWeapon.Name + ".json");
			AssetDatabase.Refresh();
			OnEnable();
		}

		private void Update() {
			_totalTime = Time.realtimeSinceStartup;
			_deltaTime = _totalTime - _lastTotalTime;
			_lastTotalTime = _totalTime;

			if (_previewIsPlaying) {
				_previewTime += _deltaTime;
			}

			UpdateRepaint();
			UpdateCheckSaveTime();
		}

		private void UpdateRepaint() {
			const float fps = 60f;

			if (_repaintTime > _totalTime + 1f) {
				_repaintTime = 0f;
			}

			if (_repaintTime + 1.0f / fps < _totalTime || _repaintNext) {
				Repaint();
				_repaintTime = _totalTime;
				_repaintNext = false;
			}
		}

		private void UpdateCheckSaveTime() {
			if (_totalTime > _timeToCheckSave + 1) {
				_timeToCheckSave = _totalTime;

				if (ActiveWeapon != null && BoomerangDatabase.WeaponDatabaseEntries.ContainsKey(ActiveWeapon.Name)) {
					string originalJson = BoomerangDatabase.WeaponDatabaseEntries[ActiveWeapon.Name].text;
					_fileHasChanged = originalJson != JsonUtility.ToJson(ActiveWeapon, true);
				}
			}
		}

		private void OnGUI() {
			_windowWidth = position.width;
			_windowHeight = position.height;

			SuperForms.Title("Weapon Studio");
			DrawWeaponSelectorBar();
			DrawMainArea();
		}

		private void DrawWeaponSelectorBar() {
			SuperForms.Region.MainOptionBarInline(() => {
				string[] weaponNames = _weaponProperties.Select(weaponProperties => weaponProperties.Name).ToArray();

				if (_mode != Mode.Normal) {
					return;
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAddLarge)) {
					_inputName = "";
					_mode = Mode.Create;
				}

				if (_selectedWeaponIndex > weaponNames.Length) {
					_selectedWeaponIndex = weaponNames.Length - 1;
				}

				_selectedWeaponIndex = SuperForms.DropDownLarge(
					_selectedWeaponIndex,
					weaponNames,
					GUILayout.Width(200)
				);

				if (_selectedWeaponIndex != _previousWeaponIndex) {
					_activeMeleeStrikeIndex = -1;
					_previousWeaponIndex = _selectedWeaponIndex;
				}

				if (ActiveWeapon != null) {
					SuperForms.IconButtons saveIcon = _fileHasChanged
						? SuperForms.IconButtons.ButtonSaveAlertLarge
						: SuperForms.IconButtons.ButtonSaveLarge;

					if (SuperForms.IconButton(saveIcon)) {
						SaveWeapon();
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonRenameLarge)) {
						_inputName = ActiveWeapon.Name;
						_mode = Mode.Rename;
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDeleteLarge)) {
						_mode = Mode.Delete;
					}
				}

				SuperForms.IconButton(SuperForms.IconButtons.ButtonRefreshLarge, OnEnable);
			});
		}

		private void DrawMainArea() {
			SuperForms.Region.MainArea(new Rect(0, 80, _windowWidth, _windowHeight - 80), () => {
				if (ActiveWeapon != null && _mode == Mode.Normal) {
					DrawLeftColumn();

					if (ActiveMeleeStrike != null) {
						DrawWeaponStrikeProperties();
						BuildStrikes();
						DrawWeaponPreview();
					}
				}

				if (_mode == Mode.Create) {
					DrawAddNewWeaponForm();
				}

				if (_mode == Mode.Rename) {
					DrawRenameWeaponForm();
				}

				if (_mode == Mode.Delete) {
					DrawDeleteWeaponForm();
				}
			});
		}

		private void DrawAddNewWeaponForm() {
			SuperForms.Region.Area(new Rect(0, 20, 260, _windowHeight - 120), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Create a New Weapon");
					_inputName = SuperForms.StringField("Name", _inputName);

					List<string> otherWeaponNames = _weaponProperties.Select(weapon => weapon.Name).ToList();
					StringValidation.StringValidationInfo stringValidationInfo = StringValidation.GetValidity(_inputName, otherWeaponNames);

					SuperForms.Region.Horizontal(() => {
						if (SuperForms.Button("Cancel")) {
							_mode = Mode.Normal;
						}

						if (stringValidationInfo.IsValid) {
							if (SuperForms.Button("Create")) {
								_weaponProperties.Add(new WeaponProperties {
									Name = _inputName
								});
								_mode = Mode.Normal;
							}
						}
					});

					if (!stringValidationInfo.IsValid) {
						SuperForms.FullBoxLabel(stringValidationInfo.ValidationMessage);
					}
				}, GUILayout.Width(260));
			});
		}

		private void DrawRenameWeaponForm() {
			SuperForms.Region.Area(new Rect(0, 20, 260, _windowHeight - 120), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Rename Weapon");
					_inputName = SuperForms.StringField("Name", _inputName);


					List<string> otherWeaponNames = (
						from otherWeapon in _weaponProperties
						where otherWeapon != ActiveWeapon
						select otherWeapon.Name
					).ToList();

					StringValidation.StringValidationInfo stringValidationInfo = StringValidation.GetValidity(_inputName, otherWeaponNames);


					SuperForms.Region.Horizontal(() => {
						if (SuperForms.Button("Cancel")) {
							_mode = Mode.Normal;
						}

						if (stringValidationInfo.IsValid) {
							if (SuperForms.Button("Rename")) {
								ActiveWeapon.Name = _inputName;
								SaveWeapon();
								DeleteWeapon();
								_mode = Mode.Normal;
							}
						}
					});

					if (!stringValidationInfo.IsValid) {
						SuperForms.FullBoxLabel(stringValidationInfo.ValidationMessage);
					}
				}, GUILayout.Width(260));
			});
		}

		private void DrawDeleteWeaponForm() {
			SuperForms.Region.Area(new Rect(0, 20, 260, _windowHeight - 120), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Delete this Weapon?");

					SuperForms.Region.Horizontal(() => {
						if (SuperForms.Button("Cancel")) {
							_mode = Mode.Normal;
						}

						if (SuperForms.Button("Delete")) {
							DeleteWeapon();
							_mode = Mode.Normal;
						}
					});
				}, GUILayout.Width(260));
			});
		}

		private void DrawLeftColumn() {
			SuperForms.Region.Area(new Rect(0, 20, 200, _windowHeight - 100), () => {
				DrawWeaponStrikeList();
				SuperForms.Space();
				DrawWeaponPreviewSettings();
				SuperForms.Space();
				SuperForms.Space();
			});
		}

		private void DrawWeaponPreviewSettings() {
			SuperForms.BoxHeader("Preview Settings");

			SuperForms.Region.VerticalBox(() => {
				SuperForms.Region.Horizontal(() => {
					SuperForms.Label("Preview Sprite");
					ActiveWeapon.EditorSprite = (Sprite) EditorGUILayout.ObjectField(ActiveWeapon.EditorSprite, typeof(Sprite), false);
				});

				SuperForms.Space();

				ActiveWeapon.EditorCrossLength = SuperForms.FloatField("Guide Line Length", ActiveWeapon.EditorCrossLength);

				SuperForms.Space();

				ActiveWeapon.EditorShowBounding = SuperForms.Checkbox("Show Box", ActiveWeapon.EditorShowBounding);

				if (ActiveWeapon.EditorShowBounding) {
					SuperForms.Label("Box Size");
					ActiveWeapon.EditorBoundingDimensions = SuperForms.Vector2FieldSingleLine(ActiveWeapon.EditorBoundingDimensions);
				}

				SuperForms.Space();

				ActiveWeapon.EditorGuideLineColor = SuperForms.ColorPicker("Guide Line Color", ActiveWeapon.EditorGuideLineColor);
				ActiveWeapon.EditorWeaponLineColor = SuperForms.ColorPicker("Weapon Line Color", ActiveWeapon.EditorWeaponLineColor);

				SuperForms.Space();

				SuperForms.Label("Draw Scale: " + ActiveWeapon.EditorDrawScale.ToString(CultureInfo.InvariantCulture) + "%");
				ActiveWeapon.EditorDrawScale = Mathf.FloorToInt(SuperForms.HorizontalSlider(ActiveWeapon.EditorDrawScale, 1, 200));

				int[] previewSnaps = {25, 50, 75, 100, 125, 150, 175};
				foreach (int snap in previewSnaps) {
					if (ActiveWeapon.EditorDrawScale >= snap - 5 && ActiveWeapon.EditorDrawScale <= snap + 5) {
						ActiveWeapon.EditorDrawScale = snap;
					}
				}
			});
		}

		private void DrawWeaponStrikeList() {
			SuperForms.Region.Scroll("WeaponStudioProperties", () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Melee Strikes");

					SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
						ActiveWeapon.MeleeStrikes.Add(new MeleeStrikeProperties {
							StrikeTypeClass = "",
							StrikeTypePropertiesClass = "",
							StartTime = 0,
							Actions = new List<MeleeStrikeAction>()
						});
					});

					for (int i = 0; i < ActiveWeapon.MeleeStrikes.Count; i++) {
						int index = i;
						SuperForms.Region.Horizontal(() => {
							SuperForms.Button("Strike " + index, _activeMeleeStrikeIndex == index, () => { _activeMeleeStrikeIndex = index; });
							SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete, () => { ActiveWeapon.MeleeStrikes.RemoveAt(index); });
						});
					}
				});
			});
		}

		private void DrawWeaponStrikeProperties() {
			SuperForms.Region.Area(new Rect(_windowWidth - 280, 20, 280, _windowHeight - 100), () => {
				SuperForms.Region.Scroll("WeaponEditorMeleeStrikePropertiesAndActions", () => {
					SuperForms.Region.VerticalBox(() => {
						SuperForms.BoxHeader("Properties");

						ActiveMeleeStrike.StrikeTypeClass = SuperForms.DictionaryDropDown(
							"Base Class",
							AssemblyFinder.Assemblies.WeaponStrikes, ActiveMeleeStrike.StrikeTypeClass
						);

						string className = AssemblyFinder.Assemblies.WeaponStrikes.FirstOrDefault(x => x.Value == ActiveMeleeStrike.StrikeTypeClass).Key;
						ActiveMeleeStrike.StrikeTypePropertiesClass = AssemblyFinder.Assemblies.WeaponStrikeProperties[className + "Properties"];

						SuperForms.Space();

						ActiveMeleeStrike.StartTime = SuperForms.FloatField("Start Time", ActiveMeleeStrike.StartTime);
					});

					Type propertiesType = Type.GetType(ActiveMeleeStrike.StrikeTypePropertiesClass);

					if (propertiesType == null) {
						Debug.LogWarning("No Properties Class Defined for State " + ActiveMeleeStrike);
						return;
					}

					SuperForms.Region.VerticalBox(() => {
						SuperForms.BoxHeader("Actions");

						SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd,
							() => { ActiveMeleeStrike.Actions.Add(new MeleeStrikeAction()); });

						MeleeStrikeAction toDelete = null;

						foreach (MeleeStrikeAction meleeStrikeAction in ActiveMeleeStrike.Actions) {
							object startProperties = JsonUtility.FromJson(meleeStrikeAction.StartStateProperties, propertiesType);
							object endProperties = JsonUtility.FromJson(meleeStrikeAction.EndStateProperties, propertiesType);
							FieldInfo[] startFields = startProperties.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
							FieldInfo[] endFields = endProperties.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

							SuperForms.Begin.VerticalSubBox();
							{
								meleeStrikeAction.Duration = SuperForms.FloatField("Duration", meleeStrikeAction.Duration);

								int selectedEaseMode = Array.IndexOf(_easeModes, meleeStrikeAction.EaseMode);
								selectedEaseMode = BoomerangUtils.ClampValue(selectedEaseMode, 0, _easeModes.Length);
								selectedEaseMode = SuperForms.DropDown("Ease Mode", selectedEaseMode, _easeModes);
								meleeStrikeAction.EaseMode = _easeModes[selectedEaseMode];

								SuperForms.Space();

								SuperForms.BoxSubHeader("Properties [Start ► End]");

								SuperForms.Space();

								for (int i = 0; i < startFields.Length; i++) {
									SuperForms.Begin.Horizontal();
									{
										SuperForms.Label(startFields[i].Name, GUILayout.Width(80));

										startFields[i].SetValue(
											startProperties,
											FieldBasedForms.DrawFormField(startFields[i].GetValue(startProperties))
										);

										SuperForms.Label("►", GUILayout.Width(20));

										endFields[i].SetValue(
											endProperties,
											FieldBasedForms.DrawFormField(endFields[i].GetValue(endProperties))
										);
									}
									SuperForms.End.Horizontal();
								}

								if (SuperForms.Button("Copy Start => End")) {
									endProperties = startProperties;
								}


								SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete, () => { toDelete = meleeStrikeAction; });
							}
							SuperForms.End.Vertical();

							meleeStrikeAction.StartStateProperties = JsonUtility.ToJson(startProperties);
							meleeStrikeAction.EndStateProperties = JsonUtility.ToJson(endProperties);
						}

						if (toDelete != null) {
							ActiveMeleeStrike.Actions.RemoveAt(ActiveMeleeStrike.Actions.IndexOf(toDelete));
						}
					});
				});
			});
		}

		private void BuildStrikes() {
			_builtMeleeStrikes.Clear();

			foreach (MeleeStrikeProperties meleeStrikeProperties in ActiveWeapon.MeleeStrikes) {
				Type strikeType = Type.GetType(meleeStrikeProperties.StrikeTypeClass);

				if (strikeType != null) {
					MeleeStrikeType newStrike = (MeleeStrikeType) Activator.CreateInstance(strikeType, meleeStrikeProperties, null);
					_builtMeleeStrikes.Add(newStrike);
				}
			}
		}

		private void DrawWeaponPreview() {
			SuperForms.Region.Area(new Rect(200 + 10, 20, _windowWidth - (280 + 10) - (200 + 10), _windowHeight - 120), () => {
				if (_previewTime > ActiveWeapon.TotalWeaponDuration) {
					_previewTime = 0;
				}

				SuperForms.Label("", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
				Rect labelSpace = GUILayoutUtility.GetLastRect();
				SuperForms.Texture(labelSpace, SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.TitleBackground]);


				if (ActiveWeapon.EditorSprite != null) {
					DrawWeaponPreviewSpriteImage(labelSpace);
				}

				GUI.BeginClip(labelSpace);
				{
					DrawWeaponPreviewBackground(labelSpace);

					if (ActiveWeapon.EditorShowBounding) {
						DrawWeaponPreviewBackgroundBounds(labelSpace);
					}

					foreach (MeleeStrikeType meleeStrikeType in _builtMeleeStrikes) {
						DrawWeaponPreviewMeleeStrike(labelSpace, meleeStrikeType);
					}
				}
				GUI.EndClip();

				DrawWeaponPreviewTimeline();
			});
		}

		private void DrawWeaponPreviewSpriteImage(Rect labelSpace) {
			Vector2 spriteSize = ActiveWeapon.EditorSprite.bounds.size;

			Rect spritePosition = new Rect(
				labelSpace.x + labelSpace.width / 2 - spriteSize.x / 2 * ActiveWeapon.EditorDrawScale,
				labelSpace.y + labelSpace.height / 2 - spriteSize.y / 2 * ActiveWeapon.EditorDrawScale,
				spriteSize.x * ActiveWeapon.EditorDrawScale,
				spriteSize.y * ActiveWeapon.EditorDrawScale
			);

			SuperForms.SpriteImage(spritePosition, ActiveWeapon.EditorSprite);
		}

		private void DrawWeaponPreviewBackground(Rect labelSpace) {
			Shader shader = Shader.Find("Hidden/Internal-Colored");
			Material mat = new Material(shader);

			mat.SetPass(0);

			GL.Begin(GL.LINE_STRIP);
			GL.Color(ActiveWeapon.EditorGuideLineColor);
			GL.Vertex3(labelSpace.width / 2, labelSpace.height / 2 - ActiveWeapon.EditorCrossLength, 0);
			GL.Vertex3(labelSpace.width / 2, labelSpace.height / 2 + ActiveWeapon.EditorCrossLength, 0);
			GL.End();

			GL.Begin(GL.LINE_STRIP);
			GL.Color(ActiveWeapon.EditorGuideLineColor);
			GL.Vertex3(labelSpace.width / 2 - ActiveWeapon.EditorCrossLength, labelSpace.height / 2, 0);
			GL.Vertex3(labelSpace.width / 2 + ActiveWeapon.EditorCrossLength, labelSpace.height / 2, 0);
			GL.End();

			GL.Begin(GL.LINE_STRIP);
			GL.Color(ActiveWeapon.EditorGuideLineColor);
			GL.Vertex3(labelSpace.width / 2 - ActiveWeapon.EditorCrossLength * 0.66f, labelSpace.height / 2 - ActiveWeapon.EditorCrossLength * 0.66f, 0);
			GL.Vertex3(labelSpace.width / 2 + ActiveWeapon.EditorCrossLength * 0.66f, labelSpace.height / 2 + ActiveWeapon.EditorCrossLength * 0.66f, 0);
			GL.End();

			GL.Begin(GL.LINE_STRIP);
			GL.Color(ActiveWeapon.EditorGuideLineColor);
			GL.Vertex3(labelSpace.width / 2 + ActiveWeapon.EditorCrossLength * 0.66f, labelSpace.height / 2 - ActiveWeapon.EditorCrossLength * 0.66f, 0);
			GL.Vertex3(labelSpace.width / 2 - ActiveWeapon.EditorCrossLength * 0.66f, labelSpace.height / 2 + ActiveWeapon.EditorCrossLength * 0.66f, 0);
			GL.End();
		}

		private void DrawWeaponPreviewBackgroundBounds(Rect labelSpace) {
			float boundsHalfWidth = ActiveWeapon.EditorBoundingDimensions.x / 2;
			float boundsHalfHeight = ActiveWeapon.EditorBoundingDimensions.y / 2;
			int previewDrawScale = ActiveWeapon.EditorDrawScale;

			GL.Begin(GL.LINE_STRIP);
			GL.Color(ActiveWeapon.EditorGuideLineColor);
			GL.Vertex3(labelSpace.width / 2 - boundsHalfWidth * previewDrawScale, labelSpace.height / 2 - boundsHalfHeight * previewDrawScale, 0);
			GL.Vertex3(labelSpace.width / 2 + boundsHalfWidth * previewDrawScale, labelSpace.height / 2 - boundsHalfHeight * previewDrawScale, 0);
			GL.Vertex3(labelSpace.width / 2 + boundsHalfWidth * previewDrawScale, labelSpace.height / 2 + boundsHalfHeight * previewDrawScale, 0);
			GL.Vertex3(labelSpace.width / 2 - boundsHalfWidth * previewDrawScale, labelSpace.height / 2 + boundsHalfHeight * previewDrawScale, 0);
			GL.Vertex3(labelSpace.width / 2 - boundsHalfWidth * previewDrawScale, labelSpace.height / 2 - boundsHalfHeight * previewDrawScale, 0);
			GL.End();
		}

		private void DrawWeaponPreviewMeleeStrike(Rect labelSpace, MeleeStrikeType meleeStrikeType) {
			meleeStrikeType.Animate(_previewTime);
			Vector2[] points = meleeStrikeType.GetColliderPoints();

			if (meleeStrikeType.IsEnabled && points.Length > 0) {
				GL.Begin(GL.LINE_STRIP);
				GL.Color(ActiveWeapon.EditorWeaponLineColor);

				foreach (Vector2 point in points) {
					GL.Vertex3(
						point.x * ActiveWeapon.EditorDrawScale + labelSpace.width / 2,
						-point.y * ActiveWeapon.EditorDrawScale + labelSpace.height / 2,
						0
					);
				}

				GL.Vertex3(
					points[0].x * ActiveWeapon.EditorDrawScale + labelSpace.width / 2,
					-points[0].y * ActiveWeapon.EditorDrawScale + labelSpace.height / 2,
					0
				);

				GL.End();
			}
		}

		private void DrawWeaponPreviewTimeline() {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.Region.Horizontal(() => {
					string currentTimeLabel = $"{Mathf.Floor(_previewTime * 100) / 100:N2}".Replace(".", ":");
					string endTimeLabel = $"{Mathf.Floor(ActiveWeapon.TotalWeaponDuration * 100) / 100:N2}".Replace(".", ":");

					_previewTime = SuperForms.HorizontalSlider(_previewTime, 0, ActiveWeapon.TotalWeaponDuration);
					_previewTime = BoomerangUtils.ClampValue(_previewTime, 0, ActiveWeapon.TotalWeaponDuration);

					SuperForms.Label(currentTimeLabel + " / " + endTimeLabel, GUILayout.Width(100));

					SuperForms.Button(_previewIsPlaying ? "Pause" : "Play",
						_previewIsPlaying, () => { _previewIsPlaying = !_previewIsPlaying; },
						GUILayout.Width(50)
					);
				});
			});
		}
	}
}