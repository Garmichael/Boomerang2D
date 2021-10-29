using System.Collections.Generic;
using System.Globalization;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.MapEditor {
	public partial class MapEditor : EditorWindow {
		private void DrawMapEditingPanelPrefabs() {
			DrawMapEditingPanelLayers();

			if (ActiveMapLayer == null) {
				return;
			}

			SuperForms.BoxHeader("Prefabs");

			SuperForms.Region.VerticalBox(() => {
				SuperForms.Region.Scroll("mapEditorPrefabOptionList", () => {
					List<string> tileSizeOptions = new List<string>();
					int power = 0;
					while (tileSizeOptions.Count < 8) {
						string size = Mathf.Pow(2, power).ToString(CultureInfo.InvariantCulture);
						tileSizeOptions.Add(size + "x");
						power++;
					}

					int dropdownIndex = (int) Mathf.Log(_prefabSnap, 2);
					dropdownIndex = SuperForms.DropDown("Snap", dropdownIndex, tileSizeOptions.ToArray());
					_prefabSnap = (int) Mathf.Pow(2, dropdownIndex);

					List<string> allPrefabNames = new List<string>();

					foreach (KeyValuePair<string, GameObject> prefab in BoomerangDatabase.MapPrefabDatabaseEntries) {
						allPrefabNames.Add(prefab.Key);
					}

					SuperForms.Region.Scroll("MapEditorPrefabList", () => {
						for (int i = 0; i < allPrefabNames.Count; i++) {
							if (SuperForms.Button(allPrefabNames[i], _selectedPrefabIndex == i && _prefabEditingMode == PrefabEditingMode.PlacingPrefab)) {
								_selectedPrefabIndex = i;
								_prefabEditingMode = PrefabEditingMode.PlacingPrefab;
							}
						}
					});
				});
			});

			if (_selectedPrefabObjects.Count == 1) {
				SuperForms.Region.VerticalBox(() => {
					MapPrefabPlacementProperties selectedPrefabObject = _selectedPrefabObjects[0];
					SuperForms.BoxHeader("Selected Prefab " + selectedPrefabObject.Prefab);

					SuperForms.BoxSubHeader("Map Properties");

					SuperForms.Begin.VerticalSubBox();
					selectedPrefabObject.Position.x = SuperForms.IntField("X", selectedPrefabObject.Position.x);
					selectedPrefabObject.Position.y = SuperForms.IntField("Y", selectedPrefabObject.Position.y);
					selectedPrefabObject.DistanceAwayFromCamera = SuperForms.FloatField("Distance From Camera", selectedPrefabObject.DistanceAwayFromCamera);
					SuperForms.End.Vertical();
				});
			}
		}

		private void DrawMapEditingAreaPrefabs(MapPrefabPlacementProperties mapPrefabPlacementProperties) {
			Texture2D prefabTexture = SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.MapEditorPrefabPreview];
			float width = 16 * MapEditingScale;
			float height = 16 * MapEditingScale;

			SuperForms.Texture(new Rect(
					mapPrefabPlacementProperties.Position.x * MapEditingScale - MapEditingScale,
					mapPrefabPlacementProperties.Position.y * MapEditingScale - MapEditingScale,
					width,
					height)
				, prefabTexture);

			bool isSelected = _selectedPrefabObjects.Contains(mapPrefabPlacementProperties);
			bool isInCorrectMode = _editingMode == EditingMode.Prefabs;

			if (isSelected && isInCorrectMode) {
				float x = mapPrefabPlacementProperties.Position.x * MapEditingScale - MapEditingScale;
				float y = mapPrefabPlacementProperties.Position.y * MapEditingScale - MapEditingScale;

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
	}
}