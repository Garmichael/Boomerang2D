using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.MapEditor {
	public partial class MapEditor : EditorWindow {
		private void DrawMapEditingPanelViews() {
			SuperForms.BoxHeader("Views");

			SuperForms.Region.VerticalBox(() => {
				SuperForms.Region.Scroll("MapEditorViewsList", () => {
					SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
						ActiveMap.Views.Add(new MapViewProperties {
							Dimensions = new Vector2Int(10, 10),
							Position = Vector2Int.zero
						});
					});

					MapViewProperties toDelete = null;

					foreach (MapViewProperties mapView in ActiveMap.Views) {
						SuperForms.Begin.Horizontal();

						if (SuperForms.Button("View", _selectedMapViews.Contains(mapView))) {
							_selectedMapViews = new List<MapViewProperties> {mapView};
						}

						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
							toDelete = mapView;
						}

						SuperForms.End.Horizontal();
					}

					if (toDelete != null) {
						ActiveMap.Views.Remove(toDelete);
					}
				});
			});

			if (_selectedMapViews.Count == 1) {
				MapViewProperties activeView = _selectedMapViews[0];

				SuperForms.Space();
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("View Properties");
					activeView.Position = SuperForms.Vector2Field("Position", activeView.Position);
					activeView.Dimensions = SuperForms.Vector2Field("Size", activeView.Dimensions);

					_viewInputSnapSize = SuperForms.IntField("Editor Snap Size", (int) _viewInputSnapSize);
					
					SuperForms.Space();
					SuperForms.BoxSubHeader("Background Music");
					activeView.PlaysBackgroundMusic = SuperForms.Checkbox("Plays Background Music", activeView.PlaysBackgroundMusic);
					if (activeView.PlaysBackgroundMusic) {
						activeView.BackgroundMusic = SuperForms.StringField("Background Music", activeView.BackgroundMusic);
					}
					
					activeView.CrossFadeBackgroundMusic = SuperForms.Checkbox("CrossFades", activeView.CrossFadeBackgroundMusic);

					if (activeView.CrossFadeBackgroundMusic) {
						activeView.CrossFadeDuration = SuperForms.FloatField("CrossFade Duration", activeView.CrossFadeDuration);
					}

					SuperForms.Space();
					
					SuperForms.BoxSubHeader("Camera Properties");
					Dictionary<string, string> stateClassesAssemblies = AssemblyFinder.Assemblies.CameraStates;
					Dictionary<string, string> statePropertyClassesAssemblies = AssemblyFinder.Assemblies.CameraStateProperties;

					activeView.CameraBehaviorClass = SuperForms.DictionaryDropDown("Behavior Class", stateClassesAssemblies, activeView.CameraBehaviorClass);

					string assemblyKey = stateClassesAssemblies.FirstOrDefault(assembly => assembly.Value == activeView.CameraBehaviorClass).Key;
					activeView.CameraBehaviorPropertiesClass = statePropertyClassesAssemblies[assemblyKey + "Properties"];

					if (string.IsNullOrEmpty(activeView.CameraBehaviorProperties)) {
						activeView.CameraBehaviorProperties = "{}";
					}

					Type statePropertiesType = Type.GetType(activeView.CameraBehaviorPropertiesClass);
					object stateProperties = JsonUtility.FromJson(
						activeView.CameraBehaviorProperties, statePropertiesType
					);

					FieldInfo[] fields = stateProperties.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
					fields = fields.OrderBy(f => f.MetadataToken).ToArray();

					foreach (FieldInfo field in fields) {
						field.SetValue(stateProperties, FieldBasedForms.DrawFormField(field, field.Name, field.GetValue(stateProperties)));
					}

					activeView.CameraBehaviorProperties = JsonUtility.ToJson(stateProperties);
				});
			}
		}

		private void DrawMapEditingAreaView(MapViewProperties mapView) {
			float x = mapView.Position.x * MapEditingScale;
			float y = mapView.Position.y * MapEditingScale;
			float width = mapView.Dimensions.x * MapEditingScale;
			float height = mapView.Dimensions.y * MapEditingScale;

			if (_editingMode == EditingMode.Views && _selectedMapViews.Contains(mapView)) {
				SuperForms.Texture(new Rect(
						mapView.Position.x * MapEditingScale,
						mapView.Position.y * MapEditingScale,
						mapView.Dimensions.x * MapEditingScale,
						mapView.Dimensions.y * MapEditingScale),
					SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.ViewsFill]
				);
			}

			SuperForms.Texture(
				new Rect(x, y, width, 2),
				SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Views]
			);
			SuperForms.Texture(
				new Rect(x, y, 2, height),
				SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Views]
			);
			SuperForms.Texture(
				new Rect(x, y + height - 2, width, 2),
				SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Views]
			);
			SuperForms.Texture(
				new Rect(x + width - 2, y, 2, height),
				SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Views]
			);
		}
	}
}