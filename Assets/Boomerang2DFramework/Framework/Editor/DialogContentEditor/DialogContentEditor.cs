using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.DialogBoxContent;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.DialogContentEditor {
	public class DialogContentEditor : EditorWindow {
		private static DialogContentEditor _window;
		private static float _windowWidth;
		private static float _windowHeight;

		private float _repaintTime;
		private bool _repaintNext;
		private float _totalTime;
		private float _timeToCheckSave;
		private bool _fileHasChanged;

		private DialogContentItems _dialogContentStore;

		private string _filterQuery = "";
		private string _filterKey = "";
		private bool _hasFiltered;
		private readonly List<int> _filteredResultsIndexes = new List<int>();

		private DialogContentItem _contentItemToDelete;

		private enum SortOptions {
			DialogContentId,
			SpeakerName,
			DialogText
		}

		private SortOptions _previousSortMode = SortOptions.DialogContentId;
		private SortOptions _currentSortMode = SortOptions.DialogContentId;

		[MenuItem("Tools/Boomerang2D/Dialog Content Editor", false, 301)]
		public static void ShowWindow() {
			_window = (DialogContentEditor) GetWindow(typeof(DialogContentEditor), false, "Dialog Content Editor");
			_window.minSize = new Vector2(600, 400);
			_window.Show();
			_window.autoRepaintOnSceneChange = true;

			_window.titleContent = new GUIContent(
				"Dialog Content Editor",
				AssetDatabase.LoadAssetAtPath<Texture>("Assets/Boomerang2DFramework/Framework/EditorAssets/editorWindowIconDialogContentEditor.png")
			);
		}

		private void OnEnable() {
			_timeToCheckSave = 0;
			LoadDialogContentItems();
			LoadTilesets();
			ClearFilter();
		}

		private void LoadDialogContentItems() {
			_dialogContentStore = JsonUtility.FromJson<DialogContentItems>(BoomerangDatabase.DialogContentStore.text);
		}

		private readonly Dictionary<string, TilesetProperties> _allTileSets = new Dictionary<string, TilesetProperties>();

		private List<string> AllTileSetNames {
			get {
				List<string> tileSetNames = new List<string>();
				foreach (KeyValuePair<string, TilesetProperties> tileSet in _allTileSets) {
					tileSetNames.Add(tileSet.Key);
				}

				return tileSetNames;
			}
		}

		private void LoadTilesets() {
			_allTileSets.Clear();
			foreach (KeyValuePair<string, TextAsset> tilesetJson in BoomerangDatabase.TilesetJsonDatabaseEntries) {
				TilesetProperties tilesetProperties = JsonUtility.FromJson<TilesetProperties>(tilesetJson.Value.text);
				tilesetProperties.PopulateLookupTable();

				_allTileSets.Add(tilesetProperties.Name, tilesetProperties);
			}
		}

		private void SaveContentData() {
			File.WriteAllText(GameProperties.DialogContentContentFile, JsonUtility.ToJson(_dialogContentStore, true));
			AssetDatabase.Refresh();
			_fileHasChanged = false;
		}

		private void Update() {
			_totalTime = Time.realtimeSinceStartup;
			UpdateRepaint();
			UpdateCheckSaveTime();
		}

		private void UpdateCheckSaveTime() {
			if (_totalTime > _timeToCheckSave + 1) {
				_timeToCheckSave = _totalTime;
				string originalJson = BoomerangDatabase.DialogContentStore.text;
				_fileHasChanged = originalJson != JsonUtility.ToJson(_dialogContentStore, true);
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

		private void OnGUI() {
			_windowWidth = position.width;
			_windowHeight = position.height;

			EditorGUIUtility.labelWidth = 45.0f;
			SuperForms.Title("Dialog Box Content Editor");
			DrawMainMenuContainer();
			DrawMainArea();
		}

		private void DrawMainMenuContainer() {
			SuperForms.Region.MainOptionBarInline(() => {
				SuperForms.IconButton(SuperForms.IconButtons.ButtonAddLarge, () => { _dialogContentStore.ContentItems.Add(new DialogContentItem()); });

				SuperForms.IconButtons saveIcon = _fileHasChanged
					? SuperForms.IconButtons.ButtonSaveAlertLarge
					: SuperForms.IconButtons.ButtonSaveLarge;

				if (SuperForms.IconButton(saveIcon)) {
					SaveContentData();
				}

				SuperForms.IconButton(SuperForms.IconButtons.ButtonRefreshLarge, OnEnable);
			});
		}

		private void DrawMainArea() {
			SuperForms.Region.MainArea(new Rect(0, 80, _windowWidth, _windowHeight - 80), () => {
				if (_dialogContentStore.ContentItems.Count > 0) {
					DrawContentListingHeader();
				}

				SuperForms.Space();

				SuperForms.Region.Scroll("DialogContentEditorMainScroll", () => {
					for (int i = 0; i < _dialogContentStore.ContentItems.Count; i++) {
						if (!_hasFiltered || _filteredResultsIndexes.Contains(i)) {
							DrawContentItemForm(_dialogContentStore.ContentItems[i]);
						}
					}

					if (_contentItemToDelete != null) {
						_dialogContentStore.ContentItems.Remove(_contentItemToDelete);
						_contentItemToDelete = null;
					}
				});
			});
		}

		private void DrawContentListingHeader() {
			const int columnWidth = 150;

			SuperForms.Region.VerticalBox(() => {
				DrawFilterOption();
				DrawSortOption();
			});
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Dialog Box Content Entries " + (_hasFiltered ? "(Filtered By: " + _filterKey + ")" : ""));
				SuperForms.Region.Horizontal(() => {
					SuperForms.BoxSubHeader("Dialog Content Id", GUILayout.Width(columnWidth));
					SuperForms.BoxSubHeader("Speaker Name", GUILayout.Width(columnWidth));
					SuperForms.BoxSubHeader("Dialog Text", GUILayout.Width(columnWidth * 3));
					SuperForms.BoxSubHeader("Portrait", GUILayout.Width(columnWidth));
					SuperForms.BoxSubHeader("Choices", GUILayout.Width(columnWidth));
				});
			});
		}

		private void DrawFilterOption() {
			const int labelWidth = 100;
			const int columnWidth = 150;

			SuperForms.Region.Horizontal(() => {
				SuperForms.Label("Filter", GUILayout.Width(labelWidth));
				_filterQuery = SuperForms.StringField(_filterQuery, GUILayout.Width(columnWidth));

				SuperForms.Button("Filter", () => {
					if (_filterQuery.Length == 0) {
						return;
					}

					_hasFiltered = true;
					_filteredResultsIndexes.Clear();

					for (int i = 0; i < _dialogContentStore.ContentItems.Count; i++) {
						DialogContentItem contentItem = _dialogContentStore.ContentItems[i];
						_filterKey = _filterQuery;

						bool meetsCriteria = contentItem.DialogContentId.Contains(_filterQuery) ||
						                     contentItem.Text.Contains(_filterQuery) ||
						                     contentItem.SpeakerName.Contains(_filterQuery);

						if (meetsCriteria) {
							_filteredResultsIndexes.Add(i);
						}
					}
				}, GUILayout.ExpandWidth(false));

				if (_hasFiltered) {
					SuperForms.Button("Clear", ClearFilter, GUILayout.ExpandWidth(false));
				}
			});
		}

		private void DrawSortOption() {
			const int labelWidth = 100;

			SuperForms.Region.Horizontal(() => {
				SuperForms.Label("Sort By", GUILayout.Width(labelWidth));
				_currentSortMode = (SortOptions) SuperForms.EnumDropdown(_currentSortMode, GUILayout.Width(150));

				if (_previousSortMode != _currentSortMode) {
					SortDialogEntries();
					_previousSortMode = _currentSortMode;
				}
			});
		}

		private void DrawContentItemForm(DialogContentItem contentItem) {
			const int columnWidth = 150;

			SuperForms.Region.VerticalBox(() => {
				SuperForms.Region.Horizontal(() => {
					contentItem.DialogContentId = SuperForms.StringField(contentItem.DialogContentId, GUILayout.Width(columnWidth));
					contentItem.SpeakerName = SuperForms.StringField(contentItem.SpeakerName, GUILayout.Width(columnWidth));

					if (contentItem.Text == null) {
						contentItem.Text = "";
					} else {
						int textHeight = BoomerangUtils.ClampValue(contentItem.Text.Split('\n').Length * 16, 34, 16 * 6);
						contentItem.Text = SuperForms.TextArea(contentItem.Text, GUILayout.Height(textHeight), GUILayout.Width(columnWidth * 3));
					}

					SuperForms.Region.Vertical(() => {
						if (AllTileSetNames.Count == 0) {
							SuperForms.FullBoxLabel("No Tilesets Found");
							SuperForms.FullBoxLabel("Add one for Portraits");
						} else {
							SuperForms.Label("Tileset");
							if (AllTileSetNames.IndexOf(contentItem.SpeakerPortraitTileset) == -1) {
								contentItem.SpeakerPortraitTileset = AllTileSetNames[0];
							}

							contentItem.SpeakerPortraitTileset = AllTileSetNames[
								SuperForms.DropDown(AllTileSetNames.IndexOf(contentItem.SpeakerPortraitTileset), AllTileSetNames.ToArray())
							];
						}

						SuperForms.Label("TileId");
						contentItem.SpeakerPortraitStampIndex = SuperForms.IntField(contentItem.SpeakerPortraitStampIndex);
					}, GUILayout.Width(columnWidth));

					SuperForms.Region.Vertical(() => { contentItem.Choices = SuperForms.ListField("Choices", contentItem.Choices); },
						GUILayout.Width(columnWidth * 2));

					SuperForms.Region.Horizontal(() => { SuperForms.Button("Delete Content Item", () => { _contentItemToDelete = contentItem; }); },
						GUILayout.Width(100));
				}, GUILayout.ExpandWidth(false));
			});

			SuperForms.Space();
		}

		private void ClearFilter() {
			_filterQuery = "";
			_filterKey = "";
			_hasFiltered = false;
			_filteredResultsIndexes.Clear();
		}

		private void SortDialogEntries() {
			switch (_currentSortMode) {
				case SortOptions.DialogContentId:
					_dialogContentStore.ContentItems = _dialogContentStore.ContentItems.OrderBy(contentItem => contentItem.DialogContentId).ToList();
					break;
				case SortOptions.SpeakerName:
					_dialogContentStore.ContentItems = _dialogContentStore.ContentItems.OrderBy(contentItem => contentItem.SpeakerName).ToList();
					break;
				case SortOptions.DialogText:
					_dialogContentStore.ContentItems = _dialogContentStore.ContentItems.OrderBy(contentItem => contentItem.Text).ToList();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}