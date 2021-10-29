using System.Collections.Generic;
using System.IO;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.GameFlagManagement;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.GameFlags {
	public class GameFlagsEditor : EditorWindow {
		private static GameFlagsEditor _window;
		private static float _windowWidth;
		private static float _windowHeight;

		private float _repaintTime;
		private bool _repaintNext;
		private float _totalTime;
		private float _timeToCheckSave;
		private bool _fileHasChanged;

		private FlagList _flagList;
		
		[MenuItem("Tools/Boomerang2D/Game Flags Editor", false, 53)]
		public static void ShowWindow() {
			_window = (GameFlagsEditor) GetWindow(typeof(GameFlagsEditor), false, "Dialog Content Editor");
			_window.minSize = new Vector2(600, 400);
			_window.Show();
			_window.autoRepaintOnSceneChange = true;

			_window.titleContent = new GUIContent(
				"Game Flags Editor",
				AssetDatabase.LoadAssetAtPath<Texture>("Assets/Boomerang2DFramework/Framework/EditorAssets/editorWindowIconFlagEditor.png")
			);
		}

		private void OnEnable() {
			_timeToCheckSave = 0;
			_fileHasChanged = false;

			_flagList = JsonUtility.FromJson<FlagList>(BoomerangDatabase.GameFlagsStore.text);
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
				string originalJson = BoomerangDatabase.GameFlagsStore.text;
				_fileHasChanged = originalJson != JsonUtility.ToJson(_flagList, true);
			}
		}

		private void SaveGameFlags() {
			File.WriteAllText(GameProperties.GameFlagContentFile, JsonUtility.ToJson(_flagList, true));
			AssetDatabase.Refresh();
			_fileHasChanged = false;
		}

		private void OnGUI() {
			_windowWidth = position.width;
			_windowHeight = position.height;

			SuperForms.Title("Game Flags Editor");
			DrawMainMenuContainer();
			DrawMainArea();
		}

		private void DrawMainMenuContainer() {
			SuperForms.Region.MainOptionBarInline(() => {
				SuperForms.Space();

				SuperForms.IconButtons saveIcon = _fileHasChanged
					? SuperForms.IconButtons.ButtonSaveAlertLarge
					: SuperForms.IconButtons.ButtonSaveLarge;

				if (SuperForms.IconButton(saveIcon)) {
					SaveGameFlags();
				}

				SuperForms.IconButton(SuperForms.IconButtons.ButtonRefreshLarge, OnEnable);
			});
		}

		private void DrawMainArea() {
			SuperForms.Region.MainArea(new Rect(0, 80, _windowWidth, _windowHeight - 80), () => {
				const int fieldWidth = 140;
				StringFlag stringFlagToDelete = null;
				FloatFlag floatFlagToDelete = null;
				BoolFlag boolFlagToDelete = null;

				SuperForms.Space();

				DrawWarningsBox();

				SuperForms.Region.Scroll("GameFlagsEditor", () => {
					SuperForms.Region.Horizontal(() => {
						SuperForms.Space();

						SuperForms.Region.VerticalBox(() => {
							SuperForms.BoxHeader("Strings");

							SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => { _flagList.StringFlags.Add(new StringFlag()); });

							SuperForms.Region.Horizontal(() => {
								SuperForms.Label("Name", GUILayout.Width(fieldWidth));
								SuperForms.Label("Value", GUILayout.Width(fieldWidth));
							});

							foreach (StringFlag flag in _flagList.StringFlags) {
								SuperForms.Region.Horizontal(() => {
									SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete, () => { stringFlagToDelete = flag; });
									flag.Key = SuperForms.StringField(flag.Key, GUILayout.Width(fieldWidth));
									flag.Value = SuperForms.StringField(flag.Value, GUILayout.Width(fieldWidth));
								});
							}
						}, GUILayout.Width(300));

						SuperForms.Space();

						SuperForms.Region.VerticalBox(() => {
							SuperForms.BoxHeader("Floats");

							SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => { _flagList.FloatFlags.Add(new FloatFlag()); });

							SuperForms.Region.Horizontal(() => {
								SuperForms.Label("Name", GUILayout.Width(fieldWidth));
								SuperForms.Label("Value", GUILayout.Width(fieldWidth));
							});

							foreach (FloatFlag flag in _flagList.FloatFlags) {
								SuperForms.Region.Horizontal(() => {
									SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete, () => { floatFlagToDelete = flag; });
									flag.Key = SuperForms.StringField(flag.Key, GUILayout.Width(fieldWidth));
									flag.Value = SuperForms.FloatField(flag.Value, GUILayout.Width(fieldWidth));
								});
							}
						}, GUILayout.Width(300));

						SuperForms.Space();

						SuperForms.Region.VerticalBox(() => {
							SuperForms.BoxHeader("Bools");

							SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => { _flagList.BoolFlags.Add(new BoolFlag()); });

							SuperForms.Region.Horizontal(() => {
								SuperForms.Label("Name", GUILayout.Width(fieldWidth));
								SuperForms.Label("Value", GUILayout.Width(fieldWidth));
							});

							foreach (BoolFlag flag in _flagList.BoolFlags) {
								SuperForms.Region.Horizontal(() => {
									SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete, () => { boolFlagToDelete = flag; });
									flag.Key = SuperForms.StringField(flag.Key, GUILayout.Width(fieldWidth));
									flag.Value = SuperForms.Checkbox(flag.Value, GUILayout.Width(fieldWidth));
								});
							}
						}, GUILayout.Width(300));


						if (stringFlagToDelete != null) {
							_flagList.StringFlags.Remove(stringFlagToDelete);
						}

						if (floatFlagToDelete != null) {
							_flagList.FloatFlags.Remove(floatFlagToDelete);
						}

						if (boolFlagToDelete != null) {
							_flagList.BoolFlags.Remove(boolFlagToDelete);
						}
					});
				});
			});
		}

		private void DrawWarningsBox() {
			List<string> warningsFound = new List<string>();
			List<string> foundStringKeys = new List<string>();
			List<string> foundFloatKeys = new List<string>();
			List<string> foundBoolKeys = new List<string>();

			foreach (StringFlag flag in _flagList.StringFlags) {
				if (foundStringKeys.IndexOf(flag.Key) > -1) {
					string warningMessage = "Duplicate String Flag Name Found: " + flag.Key;

					if (warningsFound.IndexOf(warningMessage) == -1) {
						warningsFound.Add(warningMessage);
					}
				} else {
					foundStringKeys.Add(flag.Key);
				}
			}

			foreach (FloatFlag flag in _flagList.FloatFlags) {
				if (foundFloatKeys.IndexOf(flag.Key) > -1) {
					string warningMessage = "Duplicate Float Flag Name Found: " + flag.Key;

					if (warningsFound.IndexOf(warningMessage) == -1) {
						warningsFound.Add(warningMessage);
					}
				} else {
					foundFloatKeys.Add(flag.Key);
				}
			}

			foreach (BoolFlag flag in _flagList.BoolFlags) {
				if (foundBoolKeys.IndexOf(flag.Key) > -1) {
					string warningMessage = "Duplicate Bool Flag Name Found: " + flag.Key;

					if (warningsFound.IndexOf(warningMessage) == -1) {
						warningsFound.Add(warningMessage);
					}
				} else {
					foundBoolKeys.Add(flag.Key);
				}
			}

			if (warningsFound.Count > 0) {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Warnings: ");

					foreach (string warning in warningsFound) {
						SuperForms.ParagraphLabel(warning);
					}
				});

				SuperForms.Space();
			}
		}
	}
}