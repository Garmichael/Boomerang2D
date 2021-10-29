using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Boomerang2DFramework.Framework.Actors.ActorEvents;
using Boomerang2DFramework.Framework.Actors.InteractionEvents;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.MapEditor {
	public partial class MapEditor : EditorWindow {
		private void DrawMapEditingAreaRegion(MapRegionProperties mapRegion) {
			float x = mapRegion.Position.x * MapEditingScale;
			float y = mapRegion.Position.y * MapEditingScale;
			float width = mapRegion.Dimensions.x * MapEditingScale;
			float height = mapRegion.Dimensions.y * MapEditingScale;

			if (_editingMode == EditingMode.Regions && _selectedMapRegions.Contains(mapRegion)) {
				SuperForms.Texture(new Rect(
						mapRegion.Position.x * MapEditingScale,
						mapRegion.Position.y * MapEditingScale,
						mapRegion.Dimensions.x * MapEditingScale,
						mapRegion.Dimensions.y * MapEditingScale),
					SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.RegionsFill]
				);
			}

			SuperForms.Texture(
				new Rect(x, y, width, 2),
				SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Regions]
			);
			SuperForms.Texture(
				new Rect(x, y, 2, height),
				SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Regions]
			);
			SuperForms.Texture(
				new Rect(x, y + height - 2, width, 2),
				SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Regions]
			);
			SuperForms.Texture(
				new Rect(x + width - 2, y, 2, height),
				SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Regions]
			);
		}

		private void DrawMapEditingPanelRegions() {
			SuperForms.BoxHeader("Regions");
			
			SuperForms.Region.VerticalBox(() => {
				SuperForms.Region.Scroll("MapEditorRegionList", () => {
					SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
						ActiveMap.Regions.Add(new MapRegionProperties {
							Name = "Unnamed",
							Dimensions = new Vector2Int(10, 10),
							Position = Vector2Int.zero
						});
					});

					MapRegionProperties toDelete = null;

					foreach (MapRegionProperties mapRegion in ActiveMap.Regions) {
						SuperForms.Begin.Horizontal();

						if (SuperForms.Button(mapRegion.Name, _selectedMapRegions.Contains(mapRegion))) {
							_selectedMapRegions = new List<MapRegionProperties> {mapRegion};
						}

						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
							toDelete = mapRegion;
						}

						SuperForms.End.Horizontal();
					}

					if (toDelete != null) {
						ActiveMap.Regions.Remove(toDelete);
					}


					if (_selectedMapRegions.Count == 1) {
						MapRegionProperties activeRegion = _selectedMapRegions[0];

						SuperForms.Space();
						SuperForms.Region.VerticalBox(() => {
							SuperForms.BoxHeader("Region Properties");
							activeRegion.Name = SuperForms.StringField("Name", activeRegion.Name);
							activeRegion.EnteringFlag = SuperForms.ListField("Entering Flag", activeRegion.EnteringFlag);

							activeRegion.Position = SuperForms.Vector2FieldSingleLine("Position", activeRegion.Position);
							activeRegion.Dimensions = SuperForms.Vector2FieldSingleLine("Size", activeRegion.Dimensions);

							activeRegion.FiresOnEnter = SuperForms.Checkbox("Fires on Enter", activeRegion.FiresOnEnter);
							activeRegion.FiresOnExit = SuperForms.Checkbox("Fires on Exit", activeRegion.FiresOnExit);
							activeRegion.FiresOnStay = SuperForms.Checkbox("Fires on Stay", activeRegion.FiresOnStay);

							if (activeRegion.FiresOnStay) {
								activeRegion.FireOnStayDelay = SuperForms.FloatField("Delay on Stay", activeRegion.FireOnStayDelay);
							}
						});

						SuperForms.BoxSubHeader("Game Events");

						SuperForms.Region.Horizontal(() => {
							SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
								activeRegion.RegionActorEvents.Add(new MapRegionActorEvent());
								_indexForSelectedRegionActorEvent = activeRegion.RegionActorEvents.Count - 1;
							});

							List<string> dropdownLabels = new List<string>();
							for (int i = 0; i < activeRegion.RegionActorEvents.Count; i++) {
								dropdownLabels.Add("Event " + (i + 1));
							}

							_indexForSelectedRegionActorEvent = SuperForms.DropDown(
								_indexForSelectedRegionActorEvent,
								dropdownLabels.ToArray(),
								GUILayout.Width(200)
							);
						});

						MapRegionActorEvent activeRegionActorEvent =
							_indexForSelectedRegionActorEvent >= 0 &&
							_indexForSelectedRegionActorEvent < activeRegion.RegionActorEvents.Count
								? activeRegion.RegionActorEvents[_indexForSelectedRegionActorEvent]
								: null;

						if (activeRegionActorEvent != null) {
							MapRegionActorEvent actorEventToDelete = null;

							SuperForms.Region.Horizontal(() => {
								if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
									actorEventToDelete = activeRegionActorEvent;
								}

								if (SuperForms.IconButton(SuperForms.IconButtons.ButtonClone)) {
									activeRegion.RegionActorEvents.Add(JsonUtility.FromJson<MapRegionActorEvent>(JsonUtility.ToJson(activeRegionActorEvent)));
									_indexForSelectedRegionActorEvent = activeRegion.RegionActorEvents.Count - 1;
								}
							});

							SuperForms.Region.VerticalBox(() => { DrawInteractionEventsTriggers(activeRegionActorEvent.ActorTriggerBuilders); },
								GUILayout.Width(260));
							DrawInteractionEventList(activeRegionActorEvent.ActorEventBuilders, false);

							if (actorEventToDelete != null) {
								activeRegion.RegionActorEvents.Remove(actorEventToDelete);
								_indexForSelectedRegionActorEvent = 0;
							}
						}
					}
				});
			});
		}

		private void DrawInteractionEventsTriggers(List<ActorTriggerBuilder> actorTriggerBuilders) {
			SuperForms.BoxHeader("Trigger Conditions");
			SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => { actorTriggerBuilders.Add(new ActorTriggerBuilder()); });

			ActorTriggerBuilder toDelete = null;

			Dictionary<string, string> triggerClasses = AssemblyFinder.Assemblies.ActorTriggers;
			Dictionary<string, string> triggerPropertyClasses = AssemblyFinder.Assemblies.ActorTriggerProperties;

			foreach (ActorTriggerBuilder actorTriggerBuilder in actorTriggerBuilders) {
				SuperForms.Begin.VerticalSubBox();

				if (string.IsNullOrEmpty(actorTriggerBuilder.TriggerClass)) {
					actorTriggerBuilder.TriggerClass = triggerClasses.First().Value;
				}

				string triggerName = "TRIGGER NOT FOUND";

				foreach (KeyValuePair<string, string> triggerClass in triggerClasses) {
					triggerName = triggerClass.Value;
					if (triggerClass.Value == actorTriggerBuilder.TriggerClass) {
						triggerName = triggerClass.Key;
						break;
					}
				}

				triggerName = Regex.Replace(triggerName, "(B2D: )", "");
				triggerName = Regex.Replace(triggerName, "(Ext: )", "");
				triggerName = Regex.Replace(triggerName, "(\\B[A-Z])", " $1");

				SuperForms.BoxSubHeader(triggerName);

				actorTriggerBuilder.TriggerClass = SuperForms.DictionaryDropDown(
					"Trigger Class",
					triggerClasses,
					actorTriggerBuilder.TriggerClass
				);

				string selectedTriggerClass = triggerClasses.FirstOrDefault(pair => pair.Value == actorTriggerBuilder.TriggerClass).Key;
				actorTriggerBuilder.TriggerPropertyClass = triggerPropertyClasses[selectedTriggerClass + "Properties"];
				Type triggerPropertiesType = Type.GetType(triggerPropertyClasses[selectedTriggerClass + "Properties"]);

				object triggerProperties = JsonUtility.FromJson(actorTriggerBuilder.TriggerProperties, triggerPropertiesType);

				if (triggerProperties != null) {
					FieldInfo[] fields = triggerProperties.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

					if (fields.Length > 0) {
						SuperForms.Space();

						foreach (FieldInfo field in fields) {
							field.SetValue(triggerProperties, FieldBasedForms.DrawFormField(field.Name, field.GetValue(triggerProperties)));
						}
					}

					actorTriggerBuilder.TriggerProperties = JsonUtility.ToJson(triggerProperties);
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
					toDelete = actorTriggerBuilder;
				}

				SuperForms.End.Vertical();
				SuperForms.Space();
			}

			if (toDelete != null) {
				actorTriggerBuilders.RemoveAt(actorTriggerBuilders.IndexOf(toDelete));
			}
		}

		private void DrawInteractionEventList(List<ActorEventBuilder> actorEventBuilders, bool isFilteredActorEvent) {
			SuperForms.BoxHeader("Events");
			if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
				actorEventBuilders.Add(new ActorEventBuilder());
			}

			Dictionary<string, string> actorEventClasses = AssemblyFinder.Assemblies.ActorEvents;
			Dictionary<string, string> actorEventPropertyClasses = AssemblyFinder.Assemblies.ActorEventProperties;

			ActorEventBuilder toDelete = null;
			foreach (ActorEventBuilder actorEventBuilder in actorEventBuilders) {
				SuperForms.Begin.VerticalSubBox();

				if (string.IsNullOrEmpty(actorEventBuilder.ActorEventClass)) {
					actorEventBuilder.ActorEventClass = actorEventClasses.First().Value;
				}

				string eventName = "EVENT NOT FOUND";

				foreach (KeyValuePair<string, string> eventClass in actorEventClasses) {
					eventName = eventClass.Value;
					if (eventClass.Value == actorEventBuilder.ActorEventClass) {
						eventName = eventClass.Key;
						break;
					}
				}

				eventName = Regex.Replace(eventName, "(B2D: )", "");
				eventName = Regex.Replace(eventName, "(Ext: )", "");
				eventName = Regex.Replace(eventName, "(\\B[A-Z])", " $1");

				SuperForms.BoxSubHeader(eventName);

				actorEventBuilder.ActorEventClass = SuperForms.DictionaryDropDown(
					"Event Class",
					actorEventClasses,
					actorEventBuilder.ActorEventClass
				);

				actorEventBuilder.StartTime = SuperForms.FloatField("Event Start Time", actorEventBuilder.StartTime);

				SuperForms.Space();

				if (isFilteredActorEvent) {
					int dropDownValue = SuperForms.DropDown("Affects", actorEventBuilder.AffectFilteredActors ? 1 : 0, new[] {"This Actor", "Found Actors"});
					actorEventBuilder.AffectFilteredActors = dropDownValue == 1;
					SuperForms.Space();
				}

				string selectedActorEventClass = actorEventClasses.FirstOrDefault(pair => pair.Value == actorEventBuilder.ActorEventClass).Key;

				if (!actorEventPropertyClasses.ContainsKey(selectedActorEventClass + "Properties")) {
					SuperForms.FullBoxLabel("COULDN'T FIND PROPERTIES CLASS FOR GAME EVENT!");
					SuperForms.End.Vertical();
					continue;
				}

				actorEventBuilder.ActorEventPropertiesClass = actorEventPropertyClasses[selectedActorEventClass + "Properties"];
				Type eventPropertiesType = Type.GetType(actorEventPropertyClasses[selectedActorEventClass + "Properties"]);
				object eventProperties = JsonUtility.FromJson(actorEventBuilder.ActorEventProperties, eventPropertiesType);

				if (eventProperties != null) {
					FieldInfo[] fields = eventProperties.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

					if (fields.Length > 0) {
						SuperForms.Space();

						foreach (FieldInfo field in fields) {
							field.SetValue(eventProperties, FieldBasedForms.DrawFormField(field.Name, field.GetValue(eventProperties)));
						}
					}

					actorEventBuilder.ActorEventProperties = JsonUtility.ToJson(eventProperties);
				}

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
					toDelete = actorEventBuilder;
				}

				SuperForms.End.Vertical();
			}

			if (toDelete != null) {
				actorEventBuilders.RemoveAt(actorEventBuilders.IndexOf(toDelete));
			}
		}
	}
}