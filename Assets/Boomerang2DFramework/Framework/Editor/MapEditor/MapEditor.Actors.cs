using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Boomerang2DFramework.Framework.Actors;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.MapEditor {
	public partial class MapEditor : EditorWindow {
		private void DrawMapEditingPanelActors() {
			DrawMapEditingPanelLayers();

			if (ActiveMapLayer == null) {
				return;
			}

			if (ActiveMapLayer.MapLayerType == MapLayerType.DepthLayer) {
				SuperForms.ParagraphLabel("Cannot Add Actors to a Depth Layer");
				return;
			}

			SuperForms.BoxHeader("Actors");

			SuperForms.Region.VerticalBox(() => {
				SuperForms.Region.Scroll("mapEditorActorOptionList", () => {
					List<string> tileSizeOptions = new List<string>();
					int power = 0;
					while (tileSizeOptions.Count < 8) {
						string size = Mathf.Pow(2, power).ToString(CultureInfo.InvariantCulture);
						tileSizeOptions.Add(size + "x");
						power++;
					}

					int dropdownIndex = (int) Mathf.Log(_actorSnap, 2);
					dropdownIndex = SuperForms.DropDown("Snap", dropdownIndex, tileSizeOptions.ToArray());
					_actorSnap = (int) Mathf.Pow(2, dropdownIndex);

					SuperForms.Region.Scroll("MapEditorActorList", () => {
						for (int i = 0; i < AllActorNames.Count; i++) {
							if (SuperForms.Button(AllActorNames[i], _selectedActorIndex == i && _actorEditingMode == ActorEditingMode.PlacingActor)) {
								_selectedActorIndex = i;
								_actorEditingMode = ActorEditingMode.PlacingActor;
								_selectedActorObjects.Clear();
							}
						}
					});
				});
			});

			if (_selectedActorObjects.Count == 1) {
				SuperForms.Region.VerticalBox(() => {
					MapActorPlacementProperties selectedActorObject = _selectedActorObjects[0];
					SuperForms.BoxHeader("Selected Actor " + selectedActorObject.Actor);

					SuperForms.BoxSubHeader("Map Properties");

					SuperForms.Begin.VerticalSubBox();
					selectedActorObject.Position.x = SuperForms.IntField("X", selectedActorObject.Position.x);
					selectedActorObject.Position.y = SuperForms.IntField("Y", selectedActorObject.Position.y);
					selectedActorObject.MapId = SuperForms.StringField("Map Id", selectedActorObject.MapId);
					SuperForms.End.Vertical();

					SuperForms.Space();
					SuperForms.BoxSubHeader("Instance Properties");
					SuperForms.Begin.VerticalSubBox();

					selectedActorObject.ActorInstanceProperties.FacingDirection = (Directions) SuperForms.EnumDropdown(
						"Facing Direction",
						selectedActorObject.ActorInstanceProperties.FacingDirection
					);

					selectedActorObject.ActorInstanceProperties.LinkedActors = SuperForms.ListField(
						"Linked Actors",
						selectedActorObject.ActorInstanceProperties.LinkedActors
					);

					SuperForms.End.Vertical();

					bool shouldShowStats = selectedActorObject.ActorDefaultStatsBools.Count > 0 ||
					                       selectedActorObject.ActorDefaultStatsFloats.Count > 0 ||
					                       selectedActorObject.ActorDefaultStatsStrings.Count > 0;

					if (shouldShowStats) {
						SuperForms.Space();
						SuperForms.BoxSubHeader("Default Stats");


						foreach (BoolStatProperties statBool in selectedActorObject.ActorDefaultStatsBools) {
							SuperForms.Label(">> " + statBool.Name);
							statBool.MapOverride = SuperForms.Checkbox("Override Actor Default", statBool.MapOverride);
							if (statBool.MapOverride) {
								statBool.InitialValue = SuperForms.Checkbox("Value", statBool.InitialValue);
							}
							
							SuperForms.Space();
						}

						foreach (StringStatProperties statString in selectedActorObject.ActorDefaultStatsStrings) {
							SuperForms.Label(">>" + statString.Name);
							statString.MapOverride = SuperForms.Checkbox("Override Actor Default", statString.MapOverride);
							if (statString.MapOverride) {
								statString.InitialValue = SuperForms.StringField(statString.Name, statString.InitialValue);
							}
							
							SuperForms.Space();
						}

						foreach (FloatStatProperties statFloat in selectedActorObject.ActorDefaultStatsFloats) {
							SuperForms.Label(">>" + statFloat.Name);
							statFloat.MapOverride = SuperForms.Checkbox("Override Actor Default", statFloat.MapOverride);
							if (statFloat.MapOverride) {
								statFloat.InitialValue = SuperForms.FloatField("Value", statFloat.InitialValue);
							}

							SuperForms.Space();
						}
					}
				});
			}
		}

		private void DrawMapEditingAreaActors(MapActorPlacementProperties mapActorPlacementProperties) {
			if (!_allActorTextures.ContainsKey(mapActorPlacementProperties.Actor)) {
				Debug.LogWarning("Map Editor Couldn't Find a placed Actor's Data. Close the Map Editor and Rebuild the Database and then re-open it");
				return;
			}
			
			Texture2D actorTexture = _allActorTextures[mapActorPlacementProperties.Actor];
			float width = actorTexture.width * MapEditingScale;
			float height = actorTexture.height * MapEditingScale;

			SuperForms.Texture(new Rect(
					mapActorPlacementProperties.Position.x * MapEditingScale,
					mapActorPlacementProperties.Position.y * MapEditingScale,
					width,
					height)
				, actorTexture);

			bool isSelected = _selectedActorObjects.Contains(mapActorPlacementProperties);
			bool isInCorrectMode = _editingMode == EditingMode.Actors;

			if (isSelected && isInCorrectMode) {
				float x = mapActorPlacementProperties.Position.x * MapEditingScale;
				float y = mapActorPlacementProperties.Position.y * MapEditingScale;

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