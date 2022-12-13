using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Boomerang2DFramework.Framework.Actors.ActorEvents;
using Boomerang2DFramework.Framework.Actors.ActorFinderFilters;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.AnimationProperties;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.SoundEffectProperties;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.TriggerProperties;
using Boomerang2DFramework.Framework.Actors.InteractionEvents;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.StateManagement;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.ActorStudio {
	public partial class ActorStudio : EditorWindow {
		[MenuItem("Tools/Boomerang2D/Actor Studio", false, 101)]
		public static void ShowWindow() {
			_window = (ActorStudio) GetWindow(typeof(ActorStudio), false, "Actor Studio");
			_window.minSize = new Vector2(600, 400);
			_window.Show();
			_window.autoRepaintOnSceneChange = true;

			_window.titleContent = new GUIContent(
				"Actor Studio",
				AssetDatabase.LoadAssetAtPath<Texture>("Assets/Boomerang2DFramework/Framework/EditorAssets/editorWindowIconActorStudio.png")
			);
		}

		private void OnEnable() {
			_timeToCheckSave = 0;
			LoadActors();
			LoadWeapons();
			LoadParticleEffects();
		}

		private void Update() {
			_totalTime = Time.realtimeSinceStartup;
			UpdateRepaint();
			UpdateCheckSaveTime();
		}

		private void UpdateCheckSaveTime() {
			if (_totalTime > _timeToCheckSave + 1) {
				_timeToCheckSave = _totalTime;

				if (ActiveActor != null && BoomerangDatabase.ActorJsonDatabaseEntries.ContainsKey(ActiveActor.Name)) {
					string originalJson = BoomerangDatabase.ActorJsonDatabaseEntries[ActiveActor.Name].text;
					_fileHasChanged = originalJson != JsonUtility.ToJson(ActiveActor, true);
				}
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
			SuperForms.Title("Actor Studio");
			DrawActorSelectorContainer();
			DrawMainArea();
		}

		private void DrawActorSelectorContainer() {
			string[] actorNames = _allActorProperties.Select(actorProperties => actorProperties.Name).ToArray();

			SuperForms.Region.MainOptionBarInline(() => {
				if (_mode != Mode.Normal) {
					return;
				}

				SuperForms.IconButton(SuperForms.IconButtons.ButtonAddLarge, () => {
					_inputName = "";
					_mode = Mode.Create;
				});

				if (_indexForActiveActor > actorNames.Length) {
					_indexForActiveActor = actorNames.Length - 1;
				}

				if (actorNames.Length > 0) {
					_indexForActiveActor = SuperForms.DropDownLarge(
						_indexForActiveActor,
						actorNames,
						GUILayout.Width(200)
					);
				}

				if (_indexForActiveActor != _previousIndexForActiveActor) {
					OnSelectNewActor();
				}

				if (ActiveActor != null) {
					SuperForms.IconButtons saveIcon = _fileHasChanged
						? SuperForms.IconButtons.ButtonSaveAlertLarge
						: SuperForms.IconButtons.ButtonSaveLarge;

					if (SuperForms.IconButton(saveIcon)) {
						SaveActor();
					}

					SuperForms.IconButton(SuperForms.IconButtons.ButtonRenameLarge, () => {
						_inputName = ActiveActor.Name;
						_mode = Mode.Rename;
					});

					SuperForms.IconButton(SuperForms.IconButtons.ButtonDeleteLarge, () => { _mode = Mode.Delete; });
				}

				SuperForms.IconButton(SuperForms.IconButtons.ButtonRefreshLarge, OnEnable);
			});
		}

		private void DrawMainArea() {
			SuperForms.Region.MainArea(new Rect(0, 80, _windowWidth, _windowHeight - 80), () => {
				if (_mode == Mode.Create) {
					DrawCreateActorForm();
				}

				if (_mode == Mode.Rename) {
					DrawRenameActorForm();
				}

				if (_mode == Mode.Delete) {
					DrawDeleteActorForm();
				}

				if (_mode == Mode.Normal && ActiveActor != null) {
					DrawTabSelector();

					if (_mainTabSelection < 0 || _mainTabSelection >= MainTabs.Count) {
						return;
					}

					switch (MainTabs[_mainTabSelection]) {
						case "Actor Properties":
							DrawActorProperties();
							break;
						case "Bounding Boxes":
							DrawBoundingBoxesForm();
							break;
						case "Particle Effects":
							DrawParticleEffectsForm();
							break;
						case "Attached Weapons":
							DrawAvailableWeapons();
							break;
						case "Stats":
							DrawStatsForm();
							break;
						case "Interaction Events":
							DrawInteractionEventsArea();
							break;
						case "Children Actors":
							DrawChildrenActorsArea();
							break;
						case "States":
							if (_addNewStateMode) {
								DrawAddStateArea();
							} else if (_cloneStateMode) {
								DrawCloneStateArea();
							} else {
								DrawStatesArea();
							}

							break;
						default:
							return;
					}
				}
			});
		}

		private void DrawCreateActorForm() {
			SuperForms.Region.Area(new Rect(10, 20, 300, _windowHeight - 120), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Create a new Actor");
					_inputName = SuperForms.StringField("Actor Name", _inputName);
					SuperForms.Region.Horizontal(() => {
						SuperForms.Button("Cancel", () => { _mode = Mode.Normal; });

						SuperForms.Button("Create", () => {
							CreateActor(_inputName);
							_mode = Mode.Normal;
						});
					});
				});
			});
		}

		private void DrawRenameActorForm() {
			SuperForms.Region.Area(new Rect(10, 20, 300, _windowHeight - 120), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Rename this Actor");
					_inputName = SuperForms.StringField("Actor Name", _inputName);
					SuperForms.Region.Horizontal(() => {
						SuperForms.Button("Cancel", () => { _mode = Mode.Normal; });

						SuperForms.Button("Rename", () => {
							RenameActor(_inputName);
							_mode = Mode.Normal;
						});
					});
				});
			});
		}

		private void DrawDeleteActorForm() {
			SuperForms.Region.Area(new Rect(10, 20, 300, _windowHeight - 120), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Delete this Actor?");
					SuperForms.Region.Horizontal(() => {
						SuperForms.Button("Cancel", () => { _mode = Mode.Normal; });

						SuperForms.Button("Delete", () => {
							DeleteActor();
							_mode = Mode.Normal;
						});
					});
				});
			});
		}

		private void DrawTabSelector() {
			SuperForms.Region.Area(new Rect(10, 20, 200, _windowHeight - 120), () => {
				_mainTabSelection = GUILayout.SelectionGrid(
					_mainTabSelection,
					MainTabs.Select(tab => tab.Value).ToArray(),
					1,
					SuperFormsStyles.VerticalSelectionGrid
				);

				if (_mainTabSelection < 0 || _mainTabSelection > MainTabs.Count) {
					return;
				}

				string mainTabSelection = MainTabs[_mainTabSelection];

				if (mainTabSelection == "States") {
					DrawSubTabStates();
				}

				if (mainTabSelection == "Interaction Events") {
					DrawSubTabInteractionEvents();
				}
			});
		}

		private void DrawSubTabStates() {
			SuperForms.Space();

			if (ActiveState != null) {
				_statesSubSelection = BoomerangUtils.ClampValue(_statesSubSelection, 0, StateSelectionOptions.Length - 1);

				_statesSubSelection = GUILayout.SelectionGrid(
					_statesSubSelection,
					StateSelectionOptions.ToArray(),
					1,
					SuperFormsStyles.VerticalSelectionGrid
				);
			}

			if (!_addNewStateMode) {
				SuperForms.Button("Add New State", () => {
					_inputName = "";
					_addNewStateMode = true;
					_statesSubSelection = StateSelectionOptions.ToList().IndexOf("State Properties");
				});
			}

			SuperForms.Space();

			if (ActiveState == null) {
				return;
			}

			if (ActiveActor.States.IndexOf(ActiveState) == 0) {
				SuperForms.Label("This is the Initial State");
			} else {
				if (SuperForms.Button("Set as Initial State")) {
					List<ActorStateProperties> newStates = new List<ActorStateProperties> {ActiveState};
					newStates.AddRange(ActiveActor.States.Where(state => state != ActiveState));
					ActiveActor.States = newStates;
				}
			}

			SuperForms.Button("Clone State", () => {
				_inputName = ActiveState.Name;
				_cloneStateMode = true;
			});

			if (SuperForms.Button("Rename State")) {
				_inputName = ActiveState.Name;
				_renameStateMode = true;
				_statesSubSelection = StateSelectionOptions.ToList().IndexOf("State Properties");
			}


			if (SuperForms.Button("Delete State")) {
				_deleteStateMode = true;
				_statesSubSelection = StateSelectionOptions.ToList().IndexOf("State Properties");
			}

			SuperForms.Space();
		}

		private void DrawSubTabInteractionEvents() {
			SuperForms.Space();

			SuperForms.Button("Add Event", () => { ActiveActor.InteractionEvents.Add(new ActorInteractionEvent {Name = "Un-Named Event"}); });

			List<string> buttonNames = new List<string>();

			for (int i = 0; i < ActiveActor.InteractionEvents.Count; i++) {
				string idPrefix = i + 1 + ": ";
				string buttonName = ActiveActor.InteractionEvents[i].Name != null && ActiveActor.InteractionEvents[i].Name.Trim() != ""
					? ActiveActor.InteractionEvents[i].Name
					: "Un-Named Event";

				buttonNames.Add(idPrefix + buttonName);
			}

			_selectedInteractionEvent = GUILayout.SelectionGrid(
				_selectedInteractionEvent,
				buttonNames.ToArray(),
				1,
				SuperFormsStyles.VerticalSelectionGrid
			);
		}

		private void DrawActorProperties() {
			SuperForms.Region.Area(new Rect(220, 20, 260, _windowHeight - 120), () => {
				SuperForms.Region.Scroll("ActorStudioActorProperties", () => {
					SuperForms.Region.VerticalBox(() => {
						SuperForms.BoxHeader("Actor Properties");

						ActiveActor.IsPlayer = SuperForms.Checkbox("Is Player Actor", ActiveActor.IsPlayer);
						ActiveActor.ObeysChunking = SuperForms.Checkbox("Obeys Chunking", ActiveActor.ObeysChunking);

						SuperForms.Space();

						SuperForms.BoxHeader("Sprite");

						ActiveActor.SpriteWidth = SuperForms.FloatField("Sprite Width (px)", ActiveActor.SpriteWidth);
						ActiveActor.SpriteHeight = SuperForms.FloatField("Sprite Height (px)", ActiveActor.SpriteHeight);

						if (SuperForms.Button("Import Sprite Sheet", GUILayout.ExpandWidth(false))) {
							string importedImagePath = EditorUtility.OpenFilePanel(
								"Import TileSheet Image",
								"",
								"png"
							);

							if (importedImagePath.Length != 0) {
								ImportImage(importedImagePath);
							}
						}
					});
				});
			});
		}

		private void DrawBoundingBoxesForm() {
			const int mainColumnWidth = 380;
			const int buttonWidth = 70;
			int deleteIndex = -1;
			
			SuperForms.Region.Area(new Rect(220, 20, _windowWidth - 250, _windowHeight - 120), () => {
				SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
					ActiveActor.BoundingBoxes.Add(new BoundingBoxProperties());
					foreach (ActorStateProperties state in ActiveActor.States) {
						foreach (StatePropertiesAnimation animation in state.Animations) {
							foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
								animationFrame.BoundingBoxProperties.Add(new BoundingBoxProperties());
							}
						}
					}
				});
				
				SuperForms.Region.Scroll("ActorStudioBoundingBoxesFormList", () => {
					SuperForms.Begin.Horizontal();

					foreach (BoundingBoxProperties boundingBox in ActiveActor.BoundingBoxes) {
						int boundingBoxIndex = ActiveActor.BoundingBoxes.IndexOf(boundingBox);
						
						SuperForms.Region.VerticalBox(() => {
								SuperForms.BoxHeader("Bounding Box " + boundingBoxIndex);

								SuperForms.BoxSubHeader("Default Properties");

								SuperForms.Region.Horizontal(() => {
									boundingBox.Enabled = SuperForms.Checkbox("Default Enabled", boundingBox.Enabled);
									SuperForms.Button("Set to All", () => {
										int index = ActiveActor.BoundingBoxes.IndexOf(boundingBox);

										foreach (ActorStateProperties state in ActiveActor.States) {
											foreach (StatePropertiesAnimation animation in state.Animations) {
												foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
													animationFrame.BoundingBoxProperties[index].Enabled = boundingBox.Enabled;
												}
											}
										}
									}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
								});

								SuperForms.Region.Horizontal(() => {
									SuperForms.Region.Horizontal(() => {
										boundingBox.Size = SuperForms.Vector2FieldSingleLine("Size", boundingBox.Size, 35);

										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].Size.x = boundingBox.Size.x;
														animationFrame.BoundingBoxProperties[boundingBoxIndex].Size.y = boundingBox.Size.y;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});
								});

								SuperForms.Region.Horizontal(() => {
									boundingBox.Offset = SuperForms.Vector2FieldSingleLine("Offset", boundingBox.Offset, 35);
									SuperForms.Button("Set to All", () => {
										foreach (ActorStateProperties state in ActiveActor.States) {
											foreach (StatePropertiesAnimation animation in state.Animations) {
												foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
													animationFrame.BoundingBoxProperties[boundingBoxIndex].Offset.x = boundingBox.Offset.x;
													animationFrame.BoundingBoxProperties[boundingBoxIndex].Offset.y = boundingBox.Offset.y;

												}
											}
										}
									}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
								});

								SuperForms.Region.Horizontal(() => {
									boundingBox.Flags = SuperForms.ListField("Default Flags", boundingBox.Flags);

									SuperForms.Button("Set to All", () => {
										foreach (ActorStateProperties state in ActiveActor.States) {
											foreach (StatePropertiesAnimation animation in state.Animations) {
												foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
													animationFrame.BoundingBoxProperties[boundingBoxIndex].Flags = boundingBox.Flags;
												}
											}
										}
									}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
								});

								SuperForms.Space();

								SuperForms.Region.Horizontal(() => {
									boundingBox.RayCastUp = SuperForms.Checkbox("Ray Cast Up", boundingBox.RayCastUp);
									SuperForms.Button("Set to All", () => {
										foreach (ActorStateProperties state in ActiveActor.States) {
											foreach (StatePropertiesAnimation animation in state.Animations) {
												foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
													animationFrame.BoundingBoxProperties[boundingBoxIndex].RayCastUp = boundingBox.RayCastUp;
												}
											}
										}
									}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
								});

								if (boundingBox.RayCastUp) {
									SuperForms.Begin.VerticalIndentedBox();

									SuperForms.Region.Horizontal(() => {
										boundingBox.RayCountUp = SuperForms.IntField("Rays To Cast", boundingBox.RayCountUp);
										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].RayCountUp = boundingBox.RayCountUp;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});

									SuperForms.Region.Horizontal(() => {
										boundingBox.RayLengthUp = SuperForms.FloatField("Ray Length", boundingBox.RayLengthUp);
										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].RayLengthUp = boundingBox.RayLengthUp;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});

									if (boundingBox.RayCountUp == 0) {
										boundingBox.RayCountUp = 1;
									}

									SuperForms.Region.Horizontal(() => {
										boundingBox.RayInsetUp = SuperForms.FloatField("Ray Inset From Edge", boundingBox.RayInsetUp);
										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].RayInsetUp = boundingBox.RayInsetUp;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});


									if (boundingBox.RayCountUp > 1) {
										SuperForms.Region.Horizontal(() => {
											boundingBox.RayInsetFirstUp = SuperForms.FloatField("Ray Left Inset", boundingBox.RayInsetFirstUp);
											SuperForms.Button("Set to All", () => {
												foreach (ActorStateProperties state in ActiveActor.States) {
													foreach (StatePropertiesAnimation animation in state.Animations) {
														foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
															animationFrame.BoundingBoxProperties[boundingBoxIndex].RayInsetFirstUp = boundingBox.RayInsetFirstUp;
														}
													}
												}
											}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
										});

										SuperForms.Region.Horizontal(() => {
											boundingBox.RayInsetLastUp = SuperForms.FloatField("Ray Right Inset", boundingBox.RayInsetLastUp);
											SuperForms.Button("Set to All", () => {
												foreach (ActorStateProperties state in ActiveActor.States) {
													foreach (StatePropertiesAnimation animation in state.Animations) {
														foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
															animationFrame.BoundingBoxProperties[boundingBoxIndex].RayInsetLastUp = boundingBox.RayInsetLastUp;
														}
													}
												}
											}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
										});
									}

									SuperForms.End.Vertical();
								}

								SuperForms.Region.Horizontal(() => {
									boundingBox.RayCastRight = SuperForms.Checkbox("Ray Cast Right", boundingBox.RayCastRight);
									SuperForms.Button("Set to All", () => {
										foreach (ActorStateProperties state in ActiveActor.States) {
											foreach (StatePropertiesAnimation animation in state.Animations) {
												foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
													animationFrame.BoundingBoxProperties[boundingBoxIndex].RayCastRight = boundingBox.RayCastRight;
												}
											}
										}
									}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
								});

								if (boundingBox.RayCastRight) {
									SuperForms.Begin.VerticalIndentedBox();

									SuperForms.Region.Horizontal(() => {
										boundingBox.RayCountRight = SuperForms.IntField("Rays To Cast", boundingBox.RayCountRight);
										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].RayCountRight = boundingBox.RayCountRight;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});

									SuperForms.Region.Horizontal(() => {
										boundingBox.RayLengthRight = SuperForms.FloatField("Ray Length", boundingBox.RayLengthRight);
										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].RayLengthRight = boundingBox.RayLengthRight;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});

									if (boundingBox.RayCountRight == 0) {
										boundingBox.RayCountRight = 1;
									}


									SuperForms.Region.Horizontal(() => {
										boundingBox.RayInsetRight = SuperForms.FloatField("Ray Inset From Edge", boundingBox.RayInsetRight);
										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].RayInsetRight = boundingBox.RayInsetRight;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});

									if (boundingBox.RayCountRight > 1) {
										SuperForms.Region.Horizontal(() => {
											boundingBox.RayInsetFirstRight = SuperForms.FloatField("Ray Top Inset", boundingBox.RayInsetFirstRight);
											SuperForms.Button("Set to All", () => {
												foreach (ActorStateProperties state in ActiveActor.States) {
													foreach (StatePropertiesAnimation animation in state.Animations) {
														foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
															animationFrame.BoundingBoxProperties[boundingBoxIndex].RayInsetFirstRight =
																boundingBox.RayInsetFirstRight;
														}
													}
												}
											}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
										});

										SuperForms.Region.Horizontal(() => {
											boundingBox.RayInsetLastRight = SuperForms.FloatField("Ray Bottom Inset", boundingBox.RayInsetLastRight);
											SuperForms.Button("Set to All", () => {
												foreach (ActorStateProperties state in ActiveActor.States) {
													foreach (StatePropertiesAnimation animation in state.Animations) {
														foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
															animationFrame.BoundingBoxProperties[boundingBoxIndex].RayInsetLastRight =
																boundingBox.RayInsetLastRight;
														}
													}
												}
											}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
										});
									}

									SuperForms.End.Vertical();
								}

								SuperForms.Region.Horizontal(() => {
									boundingBox.RayCastDown = SuperForms.Checkbox("Ray Cast Down", boundingBox.RayCastDown);
									SuperForms.Button("Set to All", () => {
										foreach (ActorStateProperties state in ActiveActor.States) {
											foreach (StatePropertiesAnimation animation in state.Animations) {
												foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
													animationFrame.BoundingBoxProperties[boundingBoxIndex].RayCastDown = boundingBox.RayCastDown;
												}
											}
										}
									}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
								});

								if (boundingBox.RayCastDown) {
									SuperForms.Begin.VerticalIndentedBox();

									SuperForms.Region.Horizontal(() => {
										boundingBox.RayCountDown = SuperForms.IntField("Rays To Cast", boundingBox.RayCountDown);
										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].RayCountDown = boundingBox.RayCountDown;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});

									SuperForms.Region.Horizontal(() => {
										boundingBox.RayLengthDown = SuperForms.FloatField("Ray Length", boundingBox.RayLengthDown);
										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].RayLengthDown = boundingBox.RayLengthDown;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});

									if (boundingBox.RayCountDown == 0) {
										boundingBox.RayCountDown = 1;
									}

									SuperForms.Region.Horizontal(() => {
										boundingBox.RayInsetDown = SuperForms.FloatField("Ray Inset From Edge", boundingBox.RayInsetDown);
										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].RayInsetDown = boundingBox.RayInsetDown;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});

									if (boundingBox.RayCountDown > 1) {
										SuperForms.Region.Horizontal(() => {
											boundingBox.RayInsetFirstDown = SuperForms.FloatField("Ray Left Inset", boundingBox.RayInsetFirstDown);
											SuperForms.Button("Set to All", () => {
												foreach (ActorStateProperties state in ActiveActor.States) {
													foreach (StatePropertiesAnimation animation in state.Animations) {
														foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
															animationFrame.BoundingBoxProperties[boundingBoxIndex].RayInsetFirstDown =
																boundingBox.RayInsetFirstDown;
														}
													}
												}
											}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
										});

										SuperForms.Region.Horizontal(() => {
											boundingBox.RayInsetLastDown = SuperForms.FloatField("Ray Right Inset", boundingBox.RayInsetLastDown);
											SuperForms.Button("Set to All", () => {
												foreach (ActorStateProperties state in ActiveActor.States) {
													foreach (StatePropertiesAnimation animation in state.Animations) {
														foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
															animationFrame.BoundingBoxProperties[boundingBoxIndex].RayInsetLastDown = boundingBox.RayInsetLastDown;
														}
													}
												}
											}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
										});
									}

									SuperForms.End.Vertical();
								}

								SuperForms.Region.Horizontal(() => {
									boundingBox.RayCastLeft = SuperForms.Checkbox("Ray Cast Left", boundingBox.RayCastLeft);
									SuperForms.Button("Set to All", () => {
										foreach (ActorStateProperties state in ActiveActor.States) {
											foreach (StatePropertiesAnimation animation in state.Animations) {
												foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
													animationFrame.BoundingBoxProperties[boundingBoxIndex].RayCastLeft = boundingBox.RayCastLeft;
												}
											}
										}
									}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
								});

								if (boundingBox.RayCastLeft) {
									SuperForms.Begin.VerticalIndentedBox();
									SuperForms.Region.Horizontal(() => {
										boundingBox.RayCountLeft = SuperForms.IntField("Rays To Cast", boundingBox.RayCountLeft);
										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].RayCountLeft = boundingBox.RayCountLeft;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});

									SuperForms.Region.Horizontal(() => {
										boundingBox.RayLengthLeft = SuperForms.FloatField("Ray Length", boundingBox.RayLengthLeft);
										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].RayLengthLeft = boundingBox.RayLengthLeft;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});

									if (boundingBox.RayCountLeft == 0) {
										boundingBox.RayCountLeft = 1;
									}

									SuperForms.Region.Horizontal(() => {
										boundingBox.RayInsetLeft = SuperForms.FloatField("Ray Inset From Edge", boundingBox.RayInsetLeft);
										SuperForms.Button("Set to All", () => {
											foreach (ActorStateProperties state in ActiveActor.States) {
												foreach (StatePropertiesAnimation animation in state.Animations) {
													foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
														animationFrame.BoundingBoxProperties[boundingBoxIndex].RayInsetLeft = boundingBox.RayInsetLeft;
													}
												}
											}
										}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
									});

									if (boundingBox.RayCountLeft > 1) {
										SuperForms.Region.Horizontal(() => {
											boundingBox.RayInsetFirstLeft = SuperForms.FloatField("Ray Top Inset", boundingBox.RayInsetFirstLeft);
											SuperForms.Button("Set to All", () => {
												foreach (ActorStateProperties state in ActiveActor.States) {
													foreach (StatePropertiesAnimation animation in state.Animations) {
														foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
															animationFrame.BoundingBoxProperties[boundingBoxIndex].RayInsetFirstLeft =
																boundingBox.RayInsetFirstLeft;
														}
													}
												}
											}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
										});

										SuperForms.Region.Horizontal(() => {
											boundingBox.RayInsetLastLeft = SuperForms.FloatField("Ray Bottom Inset", boundingBox.RayInsetLastLeft);
											SuperForms.Button("Set to All", () => {
												foreach (ActorStateProperties state in ActiveActor.States) {
													foreach (StatePropertiesAnimation animation in state.Animations) {
														foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
															animationFrame.BoundingBoxProperties[boundingBoxIndex].RayInsetLastLeft = boundingBox.RayInsetLastLeft;
														}
													}
												}
											}, new GUIStyle(SuperFormsStyles.Button) { fixedWidth = buttonWidth });
										});
									}

									SuperForms.End.Vertical();
								}

								if (boundingBox.RayCountUp < 1) {
									boundingBox.RayCountUp = 1;
								}

								if (boundingBox.RayCountRight < 1) {
									boundingBox.RayCountRight = 1;
								}

								if (boundingBox.RayCountDown < 1) {
									boundingBox.RayCountDown = 1;
								}

								if (boundingBox.RayCountLeft < 1) {
									boundingBox.RayCountLeft = 1;
								}

								if (boundingBox.RayLengthUp < 0.01f) {
									boundingBox.RayLengthUp = 0.01f;
								}

								if (boundingBox.RayLengthRight < 0.01f) {
									boundingBox.RayLengthRight = 0.01f;
								}

								if (boundingBox.RayLengthDown < 0.01f) {
									boundingBox.RayLengthDown = 0.01f;
								}

								if (boundingBox.RayLengthLeft < 0.01f) {
									boundingBox.RayLengthLeft = 0.01f;
								}

								if (SuperForms.Button("Set all to all Animation Frames")) {
									int index = ActiveActor.BoundingBoxes.IndexOf(boundingBox);

									foreach (ActorStateProperties state in ActiveActor.States) {
										foreach (StatePropertiesAnimation animation in state.Animations) {
											foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
												animationFrame.BoundingBoxProperties[index] =
													JsonUtility.FromJson<BoundingBoxProperties>(JsonUtility.ToJson(boundingBox));
												animationFrame.BoundingBoxProperties[index].Flags = animationFrame.BoundingBoxProperties[index].Flags;
												animationFrame.BoundingBoxProperties[index].Offset = animationFrame.BoundingBoxProperties[index].Offset;
												animationFrame.BoundingBoxProperties[index].Size = animationFrame.BoundingBoxProperties[index].Size;
											}
										}
									}
								}

								if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
									deleteIndex = ActiveActor.BoundingBoxes.IndexOf(boundingBox);
								}

						}, new GUIStyle(SuperFormsStyles.BoxStyle) { fixedWidth = mainColumnWidth, margin = new RectOffset(0, 10, 0, 0)});
					}
					
					SuperForms.End.Horizontal();
				});
			});

			if (deleteIndex >= 0) {
				foreach (ActorStateProperties state in ActiveActor.States) {
					foreach (StatePropertiesAnimation animation in state.Animations) {
						foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
							if (animationFrame.BoundingBoxProperties.Count > deleteIndex) {
								animationFrame.BoundingBoxProperties.RemoveAt(deleteIndex);
							}
						}
					}
				}

				ActiveActor.BoundingBoxes.RemoveAt(deleteIndex);
			}
		}

		private void DrawParticleEffectsForm() {
			SuperForms.Region.Area(new Rect(220, 20, 280, _windowHeight - 120), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Attach Particle Effects");
					if (_allParticleEffects.Count == 0) {
						SuperForms.FullBoxLabel("No Particle Effects have been created.");
						SuperForms.FullBoxLabel("Drop them in the Particle Effect Content Directory");
						return;
					}

					if (_indexForNewParticleEffect < 0 || _indexForNewParticleEffect >= _allActorProperties.Count) {
						_indexForNewParticleEffect = 0;
					}

					SuperForms.Region.Horizontal(() => {
						_indexForNewWeapon = SuperForms.DropDown(_indexForNewWeapon, _allParticleEffects.ToArray(), GUILayout.ExpandWidth(true));

						SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
							ActiveActor.ParticleEffects.Add(new ActorParticleEffectProperties {
								Name = _allParticleEffects[_indexForNewParticleEffect],
								DefaultOffsetPosition = Vector2.zero
							});

							foreach (ActorStateProperties state in ActiveActor.States) {
								foreach (StatePropertiesAnimation animation in state.Animations) {
									foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
										animationFrame.ParticleEffectProperties.Add(new ActorParticleEffectProperties {
											Name = _allParticleEffects[_indexForNewParticleEffect],
											Enabled = false,
											DefaultOffsetPosition = Vector2.zero
										});
									}
								}
							}
						});
					});
				});

				SuperForms.Space();

				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Attached Particle Effects");

					if (ActiveActor.ParticleEffects.Count == 0) {
						SuperForms.FullBoxLabel("No Particle Effects have been attached");
						return;
					}

					ActorParticleEffectProperties toDelete = null;
					foreach (ActorParticleEffectProperties particleEffect in ActiveActor.ParticleEffects) {
						SuperForms.Begin.VerticalSubBox();
						SuperForms.BoxSubHeader(particleEffect.Name);
						SuperForms.Vector2Field("Default Offset Position", particleEffect.DefaultOffsetPosition);

						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
							toDelete = particleEffect;
						}

						SuperForms.End.Vertical();
						SuperForms.Space();
					}

					if (toDelete != null) {
						int index = ActiveActor.ParticleEffects.IndexOf(toDelete);
						ActiveActor.ParticleEffects.RemoveAt(index);
						foreach (ActorStateProperties state in ActiveActor.States) {
							foreach (StatePropertiesAnimation animation in state.Animations) {
								foreach (StatePropertiesAnimationFrame animationFrame in animation.AnimationFrames) {
									animationFrame.ParticleEffectProperties.RemoveAt(index);
								}
							}
						}
					}
				});
			});
		}

		private void DrawAvailableWeapons() {
			SuperForms.Region.Area(new Rect(220, 20, 280, _windowHeight - 120), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Attach Weapons");

					List<string> availableWeaponsToAdd = _allWeapons.Where(availableWeapon => ActiveActor.Weapons.IndexOf(availableWeapon) == -1).ToList();

					if (_allWeapons.Count == 0) {
						SuperForms.FullBoxLabel("No Weapons have been created.");
						SuperForms.FullBoxLabel("Use the Weapon Studio to make one.");
						return;
					}

					if (availableWeaponsToAdd.Count == 0) {
						SuperForms.FullBoxLabel("All available weapons have been attached");
					} else {
						if (_indexForNewWeapon < 0 || _indexForNewWeapon >= availableWeaponsToAdd.Count) {
							_indexForNewWeapon = 0;
						}

						SuperForms.Region.Horizontal(() => {
							_indexForNewWeapon = SuperForms.DropDown(_indexForNewWeapon, availableWeaponsToAdd.ToArray(), GUILayout.ExpandWidth(true));
							SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd,
								() => { ActiveActor.Weapons.Add(availableWeaponsToAdd[_indexForNewWeapon]); });
						});
					}
				});

				SuperForms.Space();

				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Attached Weapons");

					if (ActiveActor.Weapons.Count == 0) {
						SuperForms.FullBoxLabel("No Weapons have been attached");
						return;
					}

					string toDelete = null;
					foreach (string weapon in ActiveActor.Weapons) {
						SuperForms.Begin.Horizontal();
						SuperForms.FullBoxLabel(weapon);
						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
							toDelete = weapon;
						}

						SuperForms.End.Horizontal();
					}

					if (toDelete != null) {
						ActiveActor.Weapons.RemoveAt(ActiveActor.Weapons.IndexOf(toDelete));
					}
				});
			});
		}

		private void DrawStatsForm() {
			SuperForms.Region.Area(new Rect(220, 20, 280, _windowHeight - 120), () => {
				SuperForms.Region.Scroll("ActorStudioStatsForm", () => {
					SuperForms.Region.VerticalBox(() => {
						SuperForms.BoxHeader("Actor Stats (Floats)");

						Dictionary<string, FloatStatProperties> hashedStats =
							ActiveActor.StatsFloats.ToDictionary(actorPropertiesStat => actorPropertiesStat.Name);

						if (!_addNewStatFloatMode) {
							SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
								_inputName = "";
								_addNewStatFloatMode = true;
							});
						} else {
							_inputName = SuperForms.StringField("Stat Name", _inputName);

							bool isValidName = true;
							string validationMessage = "";

							if (hashedStats.ContainsKey(_inputName.Trim())) {
								isValidName = false;
								validationMessage = "Name already in use";
							}

							if (_inputName.Trim() == "") {
								isValidName = false;
								validationMessage = "Name cannot be blank";
							}

							if (!isValidName) {
								SuperForms.FullBoxLabel(validationMessage);
							}

							SuperForms.Region.Horizontal(() => {
								SuperForms.Button("Cancel", () => { _addNewStatFloatMode = false; });

								SuperForms.Button("Add", () => {
									if (isValidName) {
										_addNewStatFloatMode = false;
										ActiveActor.StatsFloats.Add(new FloatStatProperties {
											Name = _inputName.Trim()
										});
									}
								});
							});
						}

						FloatStatProperties toDelete = null;

						foreach (FloatStatProperties stat in ActiveActor.StatsFloats) {
							SuperForms.Begin.VerticalSubBox();

							SuperForms.FullBoxLabel(stat.Name);
							stat.InitialValue = SuperForms.FloatField("Default Value", stat.InitialValue);
							if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
								toDelete = stat;
							}

							SuperForms.End.Vertical();
						}

						if (toDelete != null) {
							ActiveActor.StatsFloats.Remove(toDelete);
						}
					});

					SuperForms.Region.VerticalBox(() => {
						SuperForms.BoxHeader("Actor Stats (Strings)");

						Dictionary<string, StringStatProperties> hashedStats =
							ActiveActor.StatsStrings.ToDictionary(actorPropertiesStat => actorPropertiesStat.Name);

						if (!_addNewStatStringMode) {
							SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
								_inputName = "";
								_addNewStatStringMode = true;
							});
						} else {
							_inputName = SuperForms.StringField("Stat Name", _inputName);

							bool isValidName = true;
							string validationMessage = "";

							if (hashedStats.ContainsKey(_inputName.Trim())) {
								isValidName = false;
								validationMessage = "Name already in use";
							}

							if (_inputName.Trim() == "") {
								isValidName = false;
								validationMessage = "Name cannot be blank";
							}

							if (!isValidName) {
								SuperForms.FullBoxLabel(validationMessage);
							}

							SuperForms.Region.Horizontal(() => {
								SuperForms.Button("Cancel", () => { _addNewStatStringMode = false; });

								SuperForms.Button("Add", () => {
									if (isValidName) {
										_addNewStatStringMode = false;
										ActiveActor.StatsStrings.Add(new StringStatProperties {
											Name = _inputName.Trim()
										});
									}
								});
							});
						}

						StringStatProperties toDelete = null;

						foreach (StringStatProperties stat in ActiveActor.StatsStrings) {
							SuperForms.Begin.VerticalSubBox();

							SuperForms.FullBoxLabel(stat.Name);
							stat.InitialValue = SuperForms.StringField("Default Value", stat.InitialValue);
							if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
								toDelete = stat;
							}

							SuperForms.End.Vertical();
						}

						if (toDelete != null) {
							ActiveActor.StatsStrings.Remove(toDelete);
						}
					});

					SuperForms.Region.VerticalBox(() => {
						SuperForms.BoxHeader("Actor Stats (Bools)");

						Dictionary<string, BoolStatProperties> hashedStats =
							ActiveActor.StatsBools.ToDictionary(actorPropertiesStat => actorPropertiesStat.Name);

						if (!_addNewStatBoolMode) {
							SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
								_inputName = "";
								_addNewStatBoolMode = true;
							});
						} else {
							_inputName = SuperForms.StringField("Stat Name", _inputName);

							bool isValidName = true;
							string validationMessage = "";

							if (hashedStats.ContainsKey(_inputName.Trim())) {
								isValidName = false;
								validationMessage = "Name already in use";
							}

							if (_inputName.Trim() == "") {
								isValidName = false;
								validationMessage = "Name cannot be blank";
							}

							if (!isValidName) {
								SuperForms.FullBoxLabel(validationMessage);
							}

							SuperForms.Region.Horizontal(() => {
								SuperForms.Button("Cancel", () => { _addNewStatBoolMode = false; });

								SuperForms.Button("Add", () => {
									if (isValidName) {
										_addNewStatBoolMode = false;
										ActiveActor.StatsBools.Add(new BoolStatProperties {
											Name = _inputName.Trim()
										});
									}
								});
							});
						}

						BoolStatProperties toDelete = null;

						foreach (BoolStatProperties stat in ActiveActor.StatsBools) {
							SuperForms.Begin.VerticalSubBox();

							SuperForms.FullBoxLabel(stat.Name);
							stat.InitialValue = SuperForms.Checkbox("Default Value", stat.InitialValue);
							if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
								toDelete = stat;
							}

							SuperForms.End.Vertical();
						}

						if (toDelete != null) {
							ActiveActor.StatsBools.Remove(toDelete);
						}
					});
				});
			});
		}

		private void DrawInteractionEventsArea() {
			if (ActiveActorInteractionEvent == null) {
				return;
			}

			SuperForms.Region.Area(new Rect(220, 20, 860, _windowHeight - 120), () => {
				ActorInteractionEvent toDelete = null;

				SuperForms.BoxHeader("= EVENT " + (_selectedInteractionEvent + 1) + " = " + ActiveActorInteractionEvent.Name);

				SuperForms.Region.VerticalBox(() => {
					SuperForms.Begin.Horizontal();
					{
						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
							toDelete = ActiveActorInteractionEvent;
						}

						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonClone)) {
							ActiveActor.InteractionEvents.Add(JsonUtility.FromJson<ActorInteractionEvent>(JsonUtility.ToJson(ActiveActorInteractionEvent)));
						}

						if (ActiveActor.InteractionEvents.First() != ActiveActorInteractionEvent && SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowUp)) {
							BoomerangUtils.Swap(ActiveActor.InteractionEvents, _selectedInteractionEvent, _selectedInteractionEvent - 1);
							_selectedInteractionEvent--;
						}

						if (ActiveActor.InteractionEvents.Last() != ActiveActorInteractionEvent && SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowDown)) {
							BoomerangUtils.Swap(ActiveActor.InteractionEvents, _selectedInteractionEvent, _selectedInteractionEvent + 1);
							_selectedInteractionEvent++;
						}

						ActiveActorInteractionEvent.Name = SuperForms.StringField(ActiveActorInteractionEvent.Name);
					}
					SuperForms.End.Horizontal();
				});

				SuperForms.Space();

				SuperForms.Region.Horizontal(() => {
					SuperForms.Region.VerticalBox(() => {
						DrawInteractionActorFilterConditions(ActiveActorInteractionEvent.ActorFinderFilterBuilders);
						if (ActiveActorInteractionEvent.ActorFinderFilterBuilders.Count > 0) {
							SuperForms.Space();
							DrawInteractionActorFilterMatchConditions(ActiveActorInteractionEvent);
						}
					}, GUILayout.Width(280));
					SuperForms.Space();

					SuperForms.Region.VerticalBox(() => {
						 DrawInteractionEventsTriggers(ActiveActorInteractionEvent.ActorTriggerBuilders);
					}, GUILayout.Width(280));
					SuperForms.Space();

					SuperForms.Region.VerticalBox(() => {
						bool showActorSelector = ActiveActorInteractionEvent.ActorFinderFilterBuilders.Count > 0;
						DrawInteractionEventList(ActiveActorInteractionEvent.ActorEventBuilders, showActorSelector);
					}, GUILayout.Width(280));
				});

				if (toDelete != null) {
					ActiveActor.InteractionEvents.Remove(toDelete);
				}
			});
		}

		private static void DrawInteractionActorFilterMatchConditions(ActorInteractionEvent actorInteractionEvent) {
			SuperForms.Space();
			SuperForms.BoxSubHeader("Match Conditions");

			SuperForms.FullBoxLabel("Execute if Matching Actor Count is...");
			SuperForms.Region.Horizontal(() => {
				actorInteractionEvent.FoundActorsComparison = (ValueComparison) SuperForms.EnumDropdown(actorInteractionEvent.FoundActorsComparison);
				actorInteractionEvent.FoundActorsCount = SuperForms.IntField(actorInteractionEvent.FoundActorsCount);
			});
		}

		private static void DrawInteractionActorFilterConditions(IList<ActorFinderFilterBuilder> actorFinderFilterBuilders) {
			SuperForms.BoxHeader("Actor Filter Conditions");
			SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => { actorFinderFilterBuilders.Add(new ActorFinderFilterBuilder()); });

			SuperForms.Region.Scroll("ActorStudioListOfInteractionEvents", () => {
				ActorFinderFilterBuilder toDelete = null;

				Dictionary<string, string> filterClasses = AssemblyFinder.Assemblies.ActorFilters;
				Dictionary<string, string> filterPropertyClasses = AssemblyFinder.Assemblies.ActorFilterProperties;

				foreach (ActorFinderFilterBuilder actorFinderFilterBuilder in actorFinderFilterBuilders) {
					SuperForms.Begin.VerticalSubBox();

					if (string.IsNullOrEmpty(actorFinderFilterBuilder.FilterClass)) {
						actorFinderFilterBuilder.FilterClass = filterClasses.First().Value;
					}

					string selectedKey = AssemblyFinder.Assemblies.ActorFilters.FirstOrDefault(x => x.Value == actorFinderFilterBuilder.FilterClass).Key;
					string filterName = selectedKey;
					filterName = Regex.Replace(filterName, "(B2D: )", "");
					filterName = Regex.Replace(filterName, "(Ext: )", "");
					filterName = Regex.Replace(filterName, "(\\B[A-Z])", " $1");

					SuperForms.BoxSubHeader(filterName);

					actorFinderFilterBuilder.FilterClass = SuperForms.DictionaryDropDown(
						"Filter Class",
						filterClasses,
						actorFinderFilterBuilder.FilterClass
					);

					string selectedFilterClass = filterClasses.FirstOrDefault(pair => pair.Value == actorFinderFilterBuilder.FilterClass).Key;
					actorFinderFilterBuilder.FilterPropertiesClass = filterPropertyClasses[selectedFilterClass + "Properties"];
					Type filterPropertiesType = Type.GetType(AssemblyFinder.Assemblies.ActorFilterProperties[selectedFilterClass + "Properties"]);

					object filterProperties = JsonUtility.FromJson(actorFinderFilterBuilder.FilterProperties, filterPropertiesType);

					if (filterProperties == null) {
						actorFinderFilterBuilder.FilterProperties = "{}";
						filterProperties = JsonUtility.FromJson(actorFinderFilterBuilder.FilterProperties, filterPropertiesType);
					}

					FieldInfo[] fields = filterProperties.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

					if (fields.Length > 0) {
						foreach (FieldInfo field in fields) {
							field.SetValue(filterProperties, FieldBasedForms.DrawFormField(field.Name, field.GetValue(filterProperties)));
						}
					}

					actorFinderFilterBuilder.FilterProperties = JsonUtility.ToJson(filterProperties);


					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						toDelete = actorFinderFilterBuilder;
					}

					SuperForms.End.Vertical();
				}

				if (toDelete != null) {
					actorFinderFilterBuilders.RemoveAt(actorFinderFilterBuilders.IndexOf(toDelete));
				}
			});
		}

		private void DrawInteractionEventsTriggers(List<ActorTriggerBuilder> actorTriggerBuilders) {
			SuperForms.BoxHeader("Trigger Conditions");
			SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => { actorTriggerBuilders.Add(new ActorTriggerBuilder()); });

			SuperForms.Region.Scroll("ActorStudioListOfInteractionEventsEventTriggers", () => {
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

					string selectedTriggerClass = triggerClasses
						.FirstOrDefault(pair => pair.Value == actorTriggerBuilder.TriggerClass).Key;
					actorTriggerBuilder.TriggerPropertyClass =
						triggerPropertyClasses[selectedTriggerClass + "Properties"];
					Type triggerPropertiesType =
						Type.GetType(triggerPropertyClasses[selectedTriggerClass + "Properties"]);

					object triggerProperties =
						JsonUtility.FromJson(actorTriggerBuilder.TriggerProperties, triggerPropertiesType);

					if (triggerProperties != null) {
						FieldInfo[] fields = triggerProperties.GetType()
							.GetFields(BindingFlags.Public | BindingFlags.Instance);

						if (fields.Length > 0) {
							SuperForms.Space();

							foreach (FieldInfo field in fields) {
								field.SetValue(triggerProperties,
									FieldBasedForms.DrawFormField(field.Name, field.GetValue(triggerProperties)));
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
			});
		}

		private void DrawInteractionEventList(List<ActorEventBuilder> actorEventBuilders, bool isFilteredActorEvent, string eventSubType = "") {
			SuperForms.BoxHeader(eventSubType + "Events");
			if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
				actorEventBuilders.Add(new ActorEventBuilder());
			}

			SuperForms.Region.Scroll("ActorStudioListOfInteractionEventsEventList" + eventSubType, () => {
				Dictionary<string, string> actorEventClasses = AssemblyFinder.Assemblies.ActorEvents;
				Dictionary<string, string> actorEventPropertyClasses = AssemblyFinder.Assemblies.ActorEventProperties;

				ActorEventBuilder toDelete = null;
				ActorEventBuilder toClone = null;
				
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

					actorEventBuilder.StartTime =
						SuperForms.FloatField("Event Start Time", actorEventBuilder.StartTime);

					SuperForms.Space();

					if (isFilteredActorEvent) {
						int dropDownValue = SuperForms.DropDown("Affects",
							actorEventBuilder.AffectFilteredActors ? 1 : 0,
							new[] {"This Actor", "Found Actors"});
						actorEventBuilder.AffectFilteredActors = dropDownValue == 1;
						SuperForms.Space();
					}

					string selectedActorEventClass = actorEventClasses
						.FirstOrDefault(pair => pair.Value == actorEventBuilder.ActorEventClass).Key;

					if (!actorEventPropertyClasses.ContainsKey(selectedActorEventClass + "Properties")) {
						SuperForms.FullBoxLabel("COULDN'T FIND PROPERTIES CLASS FOR GAME EVENT!");
						SuperForms.End.Vertical();
						continue;
					}

					actorEventBuilder.ActorEventPropertiesClass =
						actorEventPropertyClasses[selectedActorEventClass + "Properties"];
					Type eventPropertiesType =
						Type.GetType(actorEventPropertyClasses[selectedActorEventClass + "Properties"]);
					object eventProperties =
						JsonUtility.FromJson(actorEventBuilder.ActorEventProperties, eventPropertiesType);

					if (eventProperties != null) {
						FieldInfo[] fields = eventProperties.GetType()
							.GetFields(BindingFlags.Public | BindingFlags.Instance);

						if (fields.Length > 0) {
							SuperForms.Space();

							foreach (FieldInfo field in fields) {
								field.SetValue(eventProperties,
									FieldBasedForms.DrawFormField(field.Name, field.GetValue(eventProperties)));
							}
						}

						actorEventBuilder.ActorEventProperties = JsonUtility.ToJson(eventProperties);
					}

					SuperForms.Region.Horizontal(() => {
						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
							toDelete = actorEventBuilder;
						}

						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonClone)) {
							toClone = actorEventBuilder;
						}
					});

					SuperForms.End.Vertical();
				}

				if (toDelete != null) {
					actorEventBuilders.RemoveAt(actorEventBuilders.IndexOf(toDelete));
				}

				if (toClone != null) {
					int cloneIndex = actorEventBuilders.IndexOf(toClone);
					ActorEventBuilder clonedEvent = JsonUtility.FromJson<ActorEventBuilder>(
						JsonUtility.ToJson(actorEventBuilders[cloneIndex])
					);

					actorEventBuilders.Add(clonedEvent);
				}
			});
		}

		private void DrawChildrenActorsArea() {
			SuperForms.Region.Area(new Rect(220, 20, 280, _windowHeight - 120), () => {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Children Actors List");
					ActiveActor.ChildrenActors = SuperForms.ListField("", ActiveActor.ChildrenActors);
				});
			});
		}

		private void DrawAddStateArea() {
			SuperForms.Region.Area(new Rect(220, 20, _windowWidth - 510, _windowHeight - 120), () => {
				SuperForms.Region.VerticalBox(() => {
					string validationMessage = "";
					bool isValidName = true;

					List<string> otherStateNames = ActiveActor.States.Select(x => x.Name).ToList();

					if (otherStateNames.IndexOf(_inputName.Trim()) > -1) {
						isValidName = false;
						validationMessage = "A state with that name already exists";
					}

					if (_inputName.Trim() == "") {
						isValidName = false;
						validationMessage = "A state cannot have a blank name";
					}

					SuperForms.BoxHeader("Add a State");
					_inputName = SuperForms.StringField("State Name", _inputName);

					if (!isValidName) {
						SuperForms.FullBoxLabel(validationMessage);
					}

					SuperForms.Region.Horizontal(() => {
						SuperForms.Button("Cancel", () => { _addNewStateMode = false; });

						if (isValidName) {
							SuperForms.Button("Add State", () => {
								ActiveActor.States.Add(new ActorStateProperties {
									Name = _inputName.Trim()
								});
								_addNewStateMode = false;
								_statesSubSelection = StateSelectionOptions.ToList().IndexOf("State Properties");
							});
						}
					});
				}, GUILayout.Width(280));
			});
		}

		private void DrawCloneStateArea() {
			SuperForms.Region.Area(new Rect(220, 20, _windowWidth - 510, _windowHeight - 120), () => {
				SuperForms.Region.VerticalBox(() => {
					string validationMessage = "";
					bool isValidName = true;

					List<string> otherStateNames = ActiveActor.States.Select(x => x.Name).ToList();

					if (otherStateNames.IndexOf(_inputName.Trim()) > -1) {
						isValidName = false;
						validationMessage = "A state with that name already exists";
					}

					if (_inputName.Trim() == "") {
						isValidName = false;
						validationMessage = "A state cannot have a blank name";
					}

					SuperForms.BoxHeader("Clone State");
					_inputName = SuperForms.StringField("State Name", _inputName);

					if (!isValidName) {
						SuperForms.FullBoxLabel(validationMessage);
					}

					SuperForms.Region.Horizontal(() => {
						SuperForms.Button("Cancel", () => { _cloneStateMode = false; });

						if (isValidName) {
							SuperForms.Button("Clone State", () => {
								ActorStateProperties newClone = JsonUtility.FromJson<ActorStateProperties>(JsonUtility.ToJson(ActiveState));
								newClone.Name = _inputName.Trim();
								newClone.EditorPosition = Vector2.zero;

								ActiveActor.States.Add(newClone);
								_cloneStateMode = false;
							});
						}
					});
				}, GUILayout.Width(280));
			});
		}

		private void DrawStatesArea() {
			_highlighted = null;

			_stateAreaPosition = new Rect(220, 20, _windowWidth - 510, _windowHeight - 120);

			SuperForms.Region.Area(_stateAreaPosition, () => {
				_stateAreaScrollPosition = SuperForms.Region.Scroll("ActorStudioStatesArea", () => {
					Rect space = new Rect(0, 0, _stateAreaPosition.width, _stateAreaPosition.height);
					Vector2 maxDimensions = GetMaxStateAreaSize();

					if (maxDimensions.x > space.width) {
						space.width = maxDimensions.x;
					}

					if (maxDimensions.y > space.height) {
						space.height = maxDimensions.y;
					}

					SuperForms.BlockedArea(GUILayout.Width(space.width), GUILayout.Height(space.height));
					SuperForms.Texture(space, SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.TitleBackground]);

					DetectHiglightedState();
					DrawStatePlates();
				}, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			});

			if (ActiveState != null) {
				SuperForms.Region.Area(new Rect(_windowWidth - 280, 20, 280, _windowHeight - 120), DrawSelectedStatePanel);
			}

			CheckStateAreaControls();
		}

		private Vector2 GetMaxStateAreaSize() {
			Vector2 maxDimensions = Vector2.zero;
			foreach (ActorStateProperties stateProperties in ActiveActor.States) {
				if (stateProperties.EditorPosition.x + 200 > maxDimensions.x) {
					maxDimensions.x = stateProperties.EditorPosition.x + 200;
				}

				if (stateProperties.EditorPosition.y + 50 > maxDimensions.y) {
					maxDimensions.y = stateProperties.EditorPosition.y + 50;
				}
			}

			return maxDimensions;
		}

		private void DetectHiglightedState() {
			foreach (ActorStateProperties stateProperties in ActiveActor.States) {
				if (_mousePositionInStateArea.x >= stateProperties.EditorPosition.x &&
				    _mousePositionInStateArea.x <= stateProperties.EditorPosition.x + 200 &&
				    _mousePositionInStateArea.y >= stateProperties.EditorPosition.y &&
				    _mousePositionInStateArea.y <= stateProperties.EditorPosition.y + 50
				) {
					_highlighted = stateProperties;
				}
			}
		}

		private void DrawStatePlates() {
			Dictionary<string, ActorStateProperties> stateHashMap = ActiveActor.States.ToDictionary(stateProperties => stateProperties.Name);

			foreach (ActorStateProperties stateProperties in ActiveActor.States) {
				DrawStateConnectors(stateProperties, stateHashMap);
			}

			foreach (ActorStateProperties stateProperties in ActiveActor.States) {
				DrawStatePlate(stateProperties);
			}
		}

		private void DrawStatePlate(ActorStateProperties stateProperties) {
			if (stateProperties.EditorPosition.x < 0) {
				stateProperties.EditorPosition.x = 0;
			}

			if (stateProperties.EditorPosition.y < 0) {
				stateProperties.EditorPosition.y = 0;
			}

			SuperForms.Begin.Area(new Rect(stateProperties.EditorPosition.x, stateProperties.EditorPosition.y, 200, 50));

			if (stateProperties == _highlighted && _dragged == null && stateProperties != ActiveState) {
				SuperForms.Begin.PlateBoxHighlighted(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			} else if (stateProperties == ActiveState) {
				SuperForms.Begin.PlateBoxSelected(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			} else {
				SuperForms.Begin.PlateBox(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			}

			SuperForms.Begin.Area(new Rect(1, 1, 48, 48));
			{
				if (stateProperties.Animations.Count > 0 && _activeActorTextures.Count > 0) {
					StatePropertiesAnimation animationToPlay = stateProperties.Animations[0];
					DrawAnimationsSpritePreviewBox(animationToPlay, 50, 50, 0, false, false);
				}
			}
			SuperForms.End.Area();

			SuperForms.Begin.Area(new Rect(56, 10, 140, 40));
			{
				SuperForms.FullBoxLabel(stateProperties.Name);
			}
			SuperForms.End.Area();

			SuperForms.End.Vertical();
			SuperForms.End.Area();
		}

		private void DrawStateConnectors(ActorStateProperties stateProperties, Dictionary<string, ActorStateProperties> stateHashMap) {
			foreach (TransitionTriggerProperties transitionTrigger in stateProperties.TransitionTriggers) {
				if (ActiveState != null && ActiveState != stateProperties && stateHashMap[transitionTrigger.NextState] != ActiveState) {
					continue;
				}

				Vector2 myLocation = stateProperties.EditorPosition;
				Vector2 destinationLocation = stateHashMap[transitionTrigger.NextState].EditorPosition;

				const int triangleSize = 8;
				const int nodeSplit = 10;

				const int horizontalThreshold = 100;
				const int verticalThreshold = 25;

				Dictionary<string, Vector2> nodeOffsets = new Dictionary<string, Vector2> {
					{"topOut", new Vector2(100 + nodeSplit, 0 - triangleSize)},
					{"topIn", new Vector2(100 - nodeSplit, 0 - triangleSize)},
					{"rightOut", new Vector2(200 + triangleSize, 25 - nodeSplit)},
					{"rightIn", new Vector2(200 + triangleSize, 25 + nodeSplit)},
					{"bottomOut", new Vector2(100 - nodeSplit, 50 + triangleSize)},
					{"bottomIn", new Vector2(100 + nodeSplit, 50 + triangleSize)},
					{"leftOut", new Vector2(0 - triangleSize, 25 + nodeSplit)},
					{"leftIn", new Vector2(0 - triangleSize, 25 - nodeSplit)}
				};

				string horizontalPlacement = "center";
				string verticalPlacement = "center";
				string outDirection = "";

				if (destinationLocation.x >= myLocation.x + horizontalThreshold) {
					horizontalPlacement = "right";
				} else if (destinationLocation.x + horizontalThreshold <= myLocation.x) {
					horizontalPlacement = "left";
				}

				if (destinationLocation.y >= myLocation.y + verticalThreshold) {
					verticalPlacement = "bottom";
				} else if (destinationLocation.y + horizontalThreshold <= myLocation.y) {
					verticalPlacement = "top";
				}

				if (horizontalPlacement == "right") {
					if (verticalPlacement == "top") {
						myLocation += nodeOffsets["topOut"];
						destinationLocation += nodeOffsets["bottomIn"];
						outDirection = "top";
					} else if (verticalPlacement == "center") {
						myLocation += nodeOffsets["rightOut"];
						destinationLocation += nodeOffsets["leftIn"];
						outDirection = "right";
					} else if (verticalPlacement == "bottom") {
						myLocation += nodeOffsets["bottomOut"];
						destinationLocation += nodeOffsets["topIn"];
						outDirection = "bottom";
					}
				} else if (horizontalPlacement == "left") {
					if (verticalPlacement == "top") {
						myLocation += nodeOffsets["topOut"];
						destinationLocation += nodeOffsets["bottomIn"];
						outDirection = "top";
					} else if (verticalPlacement == "center") {
						myLocation += nodeOffsets["leftOut"];
						destinationLocation += nodeOffsets["rightIn"];
						outDirection = "left";
					} else if (verticalPlacement == "bottom") {
						myLocation += nodeOffsets["bottomOut"];
						destinationLocation += nodeOffsets["topIn"];
						outDirection = "bottom";
					}
				} else if (horizontalPlacement == "center") {
					if (verticalPlacement == "top") {
						myLocation += nodeOffsets["topOut"];
						destinationLocation += nodeOffsets["bottomIn"];
						outDirection = "top";
					} else if (verticalPlacement == "bottom") {
						myLocation += nodeOffsets["bottomOut"];
						destinationLocation += nodeOffsets["topIn"];
						outDirection = "bottom";
					}
				}

				Color color = stateProperties == ActiveState
					? new Color(0.5f, 0.6f, 1f, 1f)
					: new Color(0.5f, 0.5f, 0.5f, 1f);

				color = stateHashMap[transitionTrigger.NextState] == ActiveState
					? new Color(.8f, 0.5f, 0.6f, 1f)
					: color;

				Vector3 startTangent;
				Vector3 endTangent;

				const int tangentOffset = 50;
				switch (outDirection) {
					case "top":
						startTangent = new Vector3(myLocation.x, myLocation.y - tangentOffset, 0);
						endTangent = new Vector3(destinationLocation.x, destinationLocation.y + tangentOffset, 0);
						break;
					case "bottom":
						startTangent = new Vector3(myLocation.x, myLocation.y + tangentOffset, 0);
						endTangent = new Vector3(destinationLocation.x, destinationLocation.y - tangentOffset, 0);
						break;
					case "right":
						startTangent = new Vector3(myLocation.x + tangentOffset, myLocation.y, 0);
						endTangent = new Vector3(destinationLocation.x - tangentOffset, destinationLocation.y, 0);
						break;
					default:
						startTangent = new Vector3(myLocation.x - tangentOffset, myLocation.y, 0);
						endTangent = new Vector3(destinationLocation.x + tangentOffset, destinationLocation.y, 0);
						break;
				}

				Handles.BeginGUI();
				Handles.color = color;
				Handles.DrawBezier(
					new Vector3(myLocation.x, myLocation.y, 0),
					new Vector3(destinationLocation.x, destinationLocation.y, 0),
					startTangent, endTangent, color, null, 3
				);

				List<Vector3> points = new List<Vector3>();
				switch (outDirection) {
					case "top":
						points.Add(new Vector3(myLocation.x, myLocation.y, 0));
						points.Add(new Vector3(myLocation.x - triangleSize, myLocation.y + triangleSize, 0));
						points.Add(new Vector3(myLocation.x + triangleSize, myLocation.y + triangleSize, 0));
						points.Add(new Vector3(myLocation.x, myLocation.y, 0));
						break;
					case "bottom":
						points.Add(new Vector3(myLocation.x, myLocation.y, 0));
						points.Add(new Vector3(myLocation.x - triangleSize, myLocation.y - triangleSize, 0));
						points.Add(new Vector3(myLocation.x + triangleSize, myLocation.y - triangleSize, 0));
						points.Add(new Vector3(myLocation.x, myLocation.y, 0));
						break;
					case "right":
						points.Add(new Vector3(myLocation.x, myLocation.y, 0));
						points.Add(new Vector3(myLocation.x - triangleSize, myLocation.y - triangleSize, 0));
						points.Add(new Vector3(myLocation.x - triangleSize, myLocation.y + triangleSize, 0));
						points.Add(new Vector3(myLocation.x, myLocation.y, 0));
						break;
					default:
						points.Add(new Vector3(myLocation.x, myLocation.y, 0));
						points.Add(new Vector3(myLocation.x + triangleSize, myLocation.y - triangleSize, 0));
						points.Add(new Vector3(myLocation.x + triangleSize, myLocation.y + triangleSize, 0));
						points.Add(new Vector3(myLocation.x, myLocation.y, 0));
						break;
				}

				Handles.DrawAAPolyLine(points.ToArray());

				points.Clear();

				switch (outDirection) {
					case "top":
						points.Add(new Vector3(destinationLocation.x, destinationLocation.y - triangleSize, 0));
						points.Add(new Vector3(destinationLocation.x - triangleSize, destinationLocation.y, 0));
						points.Add(new Vector3(destinationLocation.x + triangleSize, destinationLocation.y, 0));
						points.Add(new Vector3(destinationLocation.x, destinationLocation.y - triangleSize, 0));
						break;
					case "bottom":
						points.Add(new Vector3(destinationLocation.x, destinationLocation.y + triangleSize, 0));
						points.Add(new Vector3(destinationLocation.x - triangleSize, destinationLocation.y, 0));
						points.Add(new Vector3(destinationLocation.x + triangleSize, destinationLocation.y, 0));
						points.Add(new Vector3(destinationLocation.x, destinationLocation.y + triangleSize, 0));
						break;
					case "right":
						points.Add(new Vector3(destinationLocation.x + triangleSize, destinationLocation.y, 0));
						points.Add(new Vector3(destinationLocation.x, destinationLocation.y - triangleSize, 0));
						points.Add(new Vector3(destinationLocation.x, destinationLocation.y + triangleSize, 0));
						points.Add(new Vector3(destinationLocation.x + triangleSize, destinationLocation.y, 0));
						break;
					default:
						points.Add(new Vector3(destinationLocation.x - triangleSize, destinationLocation.y, 0));
						points.Add(new Vector3(destinationLocation.x, destinationLocation.y - triangleSize, 0));
						points.Add(new Vector3(destinationLocation.x, destinationLocation.y + triangleSize, 0));
						points.Add(new Vector3(destinationLocation.x - triangleSize, destinationLocation.y, 0));
						break;
				}

				Handles.DrawAAPolyLine(points.ToArray());
				Handles.EndGUI();
			}
		}

		private void CheckStateAreaControls() {
			EventType eventType = Event.current.type;
			_isMouseLeftJustClicked = false;

			if (eventType == EventType.MouseUp) {
				_isMouseLeftClick = false;
				_isMouseRightClick = false;
				_dragged = null;
			}

			if (!_isMouseLeftClick && !_isMouseRightClick) {
				_dragged = null;
			}

			_mousePositionInStateArea = Event.current.mousePosition - new Vector2(_stateAreaPosition.x, _stateAreaPosition.y) + _stateAreaScrollPosition;

			if (eventType == EventType.MouseDown && _stateAreaPosition.Contains(Event.current.mousePosition)) {
				if (Event.current.button == 0) {
					if (!_isMouseLeftClick) {
						_isMouseLeftJustClicked = true;
					}

					_isMouseLeftClick = true;
				}
			}

			if (_isMouseLeftClick && _isMouseRightClick) {
				return;
			}

			if (_isMouseLeftJustClicked) {
				_indexForActiveState = ActiveActor.States.IndexOf(_highlighted);
				_dragged = ActiveState;

				if (_dragged != null) {
					_draggedStateOffset = _mousePositionInStateArea - _dragged.EditorPosition;
				}
			}

			if (_dragged != null) {
				const int snapSize = 10;
				Vector2 snappedPosition = new Vector2(
					Mathf.Ceil((_mousePositionInStateArea.x - _draggedStateOffset.x) / snapSize) * snapSize,
					Mathf.Ceil((_mousePositionInStateArea.y - _draggedStateOffset.y) / snapSize) * snapSize
				);

				_dragged.EditorPosition = snappedPosition;
			}
		}

		private void DrawSelectedStatePanel() {
			List<string> callableMethods = new List<string>();

			Type statePropertiesType = Type.GetType(ActiveState.Class);

			if (statePropertiesType != null) {
				List<MethodInfo> methods = statePropertiesType.GetMethods(BindingFlags.Public | BindingFlags.Instance).ToList();
				callableMethods.AddRange(
					from method in methods
					where method.GetCustomAttributes(typeof(CanBeCalledByModificationTriggerAttribute), false).Length > 0
					select method.Name
				);
			}

			string stateSubSelectionValue = StateSelectionOptions[_statesSubSelection];

			SuperForms.Region.Scroll("ActorStudioStatePanel", () => {
				switch (stateSubSelectionValue) {
					case "State Properties":
						DrawStateProperties();
						break;
					case "Animations":
						DrawAnimationsManagementForm();
						break;
					case "Sound Effects":
						DrawSoundEffectsForm();
						break;
					case "Transition Triggers":
						DrawTransitionTriggerManagementForm();
						break;
					case "Modification Triggers":
						DrawModificationTriggerManagementForm(callableMethods);
						break;
					case "Weapon Triggers":
						DrawWeaponTriggerManagementForm();
						break;
					case "Entry Events":
						DrawEntryEventsManagementForm();
						break;
					case "Exit Events":
						DrawExitEventsManagementForm();
						break;
					default:
						return;
				}
			}, GUILayout.Width(280));
		}

		private void DrawStateProperties() {
			Dictionary<string, string> stateClassesAssemblies = AssemblyFinder.Assemblies.ActorStates;
			Dictionary<string, string> statePropertyClassesAssemblies = AssemblyFinder.Assemblies.ActorStateProperties;

			SuperForms.Begin.VerticalBox();

			if (_deleteStateMode) {
				SuperForms.BoxHeader("Delete this State?");
				SuperForms.Region.Horizontal(() => {
					SuperForms.Button("Cancel", () => { _deleteStateMode = false; });

					SuperForms.Button("Delete", () => {
						foreach (ActorStateProperties actorState in ActiveActor.States) {
							List<TransitionTriggerProperties> deletableTransitions = actorState.TransitionTriggers
								.Where(transitionTriggerProperties => transitionTriggerProperties.NextState == ActiveState.Name)
								.ToList();

							foreach (TransitionTriggerProperties deletableTransition in deletableTransitions) {
								actorState.TransitionTriggers.RemoveAt(actorState.TransitionTriggers.IndexOf(deletableTransition));
							}
						}

						ActiveActor.States.RemoveAt(ActiveActor.States.IndexOf(ActiveState));
						_indexForActiveState = 0;
						_deleteStateMode = false;
					});
				});

				SuperForms.End.Vertical();
				return;
			}

			if (_renameStateMode) {
				bool isValidName = true;
				string validationMessage = "";

				if (_inputName.Trim() == "") {
					isValidName = false;
					validationMessage = "Name Cannot be blank";
				}

				List<string> otherNames = (from state in ActiveActor.States where state != ActiveState select state.Name).ToList();

				if (otherNames.IndexOf(_inputName.Trim()) > -1) {
					isValidName = false;
					validationMessage = "Another State has this name";
				}

				SuperForms.BoxHeader("Rename State");
				_inputName = SuperForms.StringField("New Name", _inputName);

				if (!isValidName) {
					SuperForms.FullBoxLabel(validationMessage);
				}

				SuperForms.Region.Horizontal(() => {
					SuperForms.Button("Cancel", () => { _renameStateMode = false; });

					SuperForms.Button("Rename", () => {
						if (!isValidName) {
							return;
						}

						foreach (ActorStateProperties actorState in ActiveActor.States) {
							if (actorState.Name != ActiveState.Name) {
								foreach (TransitionTriggerProperties transitionProperties in actorState.TransitionTriggers) {
									if (transitionProperties.NextState == ActiveState.Name) {
										transitionProperties.NextState = _inputName.Trim();
									}
								}
							}
						}

						ActiveState.Name = _inputName.Trim();
						_renameStateMode = false;
					});
				});

				SuperForms.End.Vertical();
				return;
			}

			ActiveState.Class = SuperForms.DictionaryDropDown("Behavior Class", stateClassesAssemblies, ActiveState.Class);
			string assemblyKey = stateClassesAssemblies.FirstOrDefault(assembly => assembly.Value == ActiveState.Class).Key;
			ActiveState.PropertiesClass = AssemblyFinder.Assemblies.ActorStateProperties[assemblyKey + "Properties"];

			SuperForms.Space();

			string selectedStateClassKey = stateClassesAssemblies.FirstOrDefault(x => x.Value == ActiveState.Class).Key;
			string activeStatePropertiesClass = statePropertyClassesAssemblies[selectedStateClassKey + "Properties"];

			Type statePropertiesType = Type.GetType(activeStatePropertiesClass);
			object stateProperties = JsonUtility.FromJson(ActiveState.Properties, statePropertiesType);

			FieldInfo[] fields = stateProperties.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			fields = fields.OrderBy(f => f.MetadataToken).ToArray();

			foreach (FieldInfo field in fields) {
				field.SetValue(stateProperties, FieldBasedForms.DrawFormField(field, field.Name, field.GetValue(stateProperties)));
			}

			ActiveState.Properties = JsonUtility.ToJson(stateProperties);

			SuperForms.End.Vertical();
		}

		private void DrawSoundEffectsForm() {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => { ActiveState.SoundEffects.Add(new StatePropertiesSoundEffect()); });

				foreach (StatePropertiesSoundEffect soundEffect in ActiveState.SoundEffects) {
					SuperForms.Begin.VerticalSubBox();
					{
						soundEffect.SoundEffectPool = SuperForms.ListField("SFX Pool", soundEffect.SoundEffectPool);

						SuperForms.Space();

						if (soundEffect.SoundEffectPool.Count > 0) {
							soundEffect.Volume = SuperForms.FloatField("Volume", soundEffect.Volume);
							soundEffect.Volume = BoomerangUtils.ClampValue(soundEffect.Volume, 0f, 1f);

							soundEffect.StartTime = SuperForms.FloatField("Start Time", soundEffect.StartTime);
							soundEffect.LoopEffect = SuperForms.Checkbox("Loops", soundEffect.LoopEffect);

							if (!soundEffect.LoopEffect) {
								soundEffect.PlayCount = SuperForms.IntField("Play Count", soundEffect.PlayCount);
							} else {
								soundEffect.ImmediateKillOnExitState = SuperForms.Checkbox("Kill on State Exit", soundEffect.ImmediateKillOnExitState);
							}

							if (soundEffect.SoundEffectPool.Count > 1) {
								soundEffect.RandomOrder = SuperForms.Checkbox("Play Random", soundEffect.RandomOrder);
							}
						}

						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
							ActiveState.SoundEffects.RemoveAt(ActiveState.SoundEffects.IndexOf(soundEffect));
							return;
						}
					}
					SuperForms.End.Vertical();
				}
			});
		}

		private void DrawTransitionTriggerManagementForm() {
			_selectedTransitionTriggerType = GUILayout.SelectionGrid(
				_selectedTransitionTriggerType,
				new[] {"Static", "Random"},
				2,
				SuperFormsStyles.VerticalSelectionGrid
			);

			SuperForms.Space();

			if (_selectedTransitionTriggerType == 0) {
				DrawStaticTransitionTriggerManagementForm();
			} else {
				DrawRandomTransitionTriggerManagementForm();
			}
		}

		private void DrawStaticTransitionTriggerManagementForm() {
			List<string> otherStates = (from state in ActiveActor.States where state != ActiveState select state.Name).ToList();

			SuperForms.Region.VerticalBox(() => {
				if (otherStates.Count > 0) {
					SuperForms.Region.Horizontal(() => {
						SuperForms.Label("Add Transition To: ", GUILayout.ExpandWidth(false));

						_indexForNewTransitionTrigger = SuperForms.DropDown(_indexForNewTransitionTrigger, otherStates.ToArray(), GUILayout.ExpandWidth(true));

						SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
							if (_indexForNewTransitionTrigger >= 0) {
								ActiveState.TransitionTriggers.Add(new TransitionTriggerProperties {
									NextState = otherStates[_indexForNewTransitionTrigger]
								});
							}
						});
					});
				} else {
					SuperForms.FullBoxLabel("Create another State to add a Transition");
				}
			});

			SuperForms.Space();

			SuperForms.Region.Scroll("ActorStudioTransitionTriggerList", () => {
				foreach (TransitionTriggerProperties transitionTrigger in ActiveState.TransitionTriggers) {
					if (DrawStaticTransitionTriggerForm(transitionTrigger, ActiveState.TransitionTriggers)) {
						return;
					}
				}
			});
		}

		private bool DrawStaticTransitionTriggerForm(TransitionTriggerProperties transitionTrigger, List<TransitionTriggerProperties> triggerContainer) {
			bool containerChanged = false;

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("> " + transitionTrigger.NextState);

				List<string> allStates = ActiveActor.States.Select(state => state.Name).ToList();

				int newIndex = SuperForms.DropDown(allStates.IndexOf(transitionTrigger.NextState), allStates.ToArray(), GUILayout.ExpandWidth(true));
				transitionTrigger.NextState = allStates[newIndex];

				if (DrawTriggerConditionsForm(transitionTrigger.ActorTriggerBuilders)) {
					containerChanged = true;
				}

				SuperForms.Space();

				SuperForms.Region.Horizontal(() => {
					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						triggerContainer.Remove(transitionTrigger);
						containerChanged = true;
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonClone)) {
						TransitionTriggerProperties triggerClone = JsonUtility.FromJson<TransitionTriggerProperties>(JsonUtility.ToJson(transitionTrigger));
						triggerContainer.Add(triggerClone);
						containerChanged = true;
					}
					
					if (triggerContainer.Count > 1) {
						int activeIndex = triggerContainer.IndexOf(transitionTrigger);

						if (activeIndex > 0) {
							if (SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowUp)) {
								BoomerangUtils.Swap(triggerContainer, activeIndex, activeIndex - 1);
								containerChanged = true;
							}
						}

						if (activeIndex < triggerContainer.Count - 1) {
							if (SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowDown)) {
								BoomerangUtils.Swap(triggerContainer, activeIndex, activeIndex + 1);
								containerChanged = true;
							}
						}
					}
				});
			});

			SuperForms.Space();

			return containerChanged;
		}

		private void DrawRandomTransitionTriggerManagementForm() {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.Region.Horizontal(() => {
					SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
						ActiveState.RandomTransitionTriggers.Add(new RandomTransitionTriggerProperties());
						_selectedRandomTransitionTriggerIndex = ActiveState.RandomTransitionTriggers.Count - 1;
					});

					if (ActiveState.RandomTransitionTriggers.Count == 0) {
						return;
					}

					List<string> groupNumbers = new List<string>();
					for (int i = 0; i < ActiveState.RandomTransitionTriggers.Count; i++) {
						groupNumbers.Add((i + 1).ToString());
					}

					_selectedRandomTransitionTriggerIndex = SuperForms.DropDown("group", _selectedRandomTransitionTriggerIndex, groupNumbers.ToArray());

					SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete, () => {
						ActiveState.RandomTransitionTriggers.RemoveAt(_selectedRandomTransitionTriggerIndex);
						if (_selectedRandomTransitionTriggerIndex > ActiveState.RandomTransitionTriggers.Count - 1) {
							_selectedRandomTransitionTriggerIndex = ActiveState.RandomTransitionTriggers.Count - 1;
						}
					});
				});
			});

			if (ActiveState.RandomTransitionTriggers.Count == 0 || ActiveRandomTransitionTrigger == null) {
				return;
			}

			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Group " + (_selectedRandomTransitionTriggerIndex + 1));
				DrawTriggerConditionsForm(ActiveRandomTransitionTrigger.ActorTriggerBuilders);
			});

			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.Region.Horizontal(() => {
					List<string> otherStates = (from state in ActiveActor.States select state.Name).ToList();

					SuperForms.Label("Add Transition To: ", GUILayout.ExpandWidth(false));

					_indexForNewTransitionTrigger = SuperForms.DropDown(_indexForNewTransitionTrigger, otherStates.ToArray(), GUILayout.ExpandWidth(true));

					SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
						if (_indexForNewTransitionTrigger >= 0) {
							ActiveRandomTransitionTrigger.TransitionOptions.Add(
								new RandomTransitionOption {
									Odds = 1,
									TransitionTriggerProperties = new TransitionTriggerProperties {
										NextState = otherStates[_indexForNewTransitionTrigger]
									}
								});
						}
					});
				});
			});

			SuperForms.Space();

			foreach (RandomTransitionOption randomTransitionOption in ActiveRandomTransitionTrigger.TransitionOptions) {
				DrawRandomTransitionTriggerForm(randomTransitionOption, ActiveRandomTransitionTrigger.TransitionOptions);
			}
		}

		private void DrawRandomTransitionTriggerForm(RandomTransitionOption randomTransitionOption,
			List<RandomTransitionOption> randomTransitionOptionContainer) {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("> " + randomTransitionOption.TransitionTriggerProperties.NextState);

				List<string> allStates = ActiveActor.States.Select(state => state.Name).ToList();

				int newIndex = SuperForms.DropDown(allStates.IndexOf(randomTransitionOption.TransitionTriggerProperties.NextState), allStates.ToArray(),
					GUILayout.ExpandWidth(true));
				randomTransitionOption.TransitionTriggerProperties.NextState = allStates[newIndex];

				randomTransitionOption.Odds = SuperForms.FloatField("Odds", randomTransitionOption.Odds);

				if (DrawTriggerConditionsForm(randomTransitionOption.TransitionTriggerProperties.ActorTriggerBuilders)) { }

				SuperForms.Region.Horizontal(() => {
					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						randomTransitionOptionContainer.Remove(randomTransitionOption);
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						RandomTransitionOption triggerClone = JsonUtility.FromJson<RandomTransitionOption>(JsonUtility.ToJson(randomTransitionOption));
						randomTransitionOptionContainer.Add(triggerClone);
					}

					if (randomTransitionOptionContainer.Count > 1) {
						SuperForms.Space();

						int activeIndex = randomTransitionOptionContainer.IndexOf(randomTransitionOption);

						if (activeIndex > 0) {
							if (SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowUp)) {
								BoomerangUtils.Swap(randomTransitionOptionContainer, activeIndex, activeIndex - 1);
							}
						}

						if (activeIndex < randomTransitionOptionContainer.Count - 1) {
							if (SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowDown)) {
								BoomerangUtils.Swap(randomTransitionOptionContainer, activeIndex, activeIndex + 1);
							}
						}
					}
				});
			});

			SuperForms.Space();
		}

		private static bool DrawTriggerConditionsForm(ICollection<ActorTriggerBuilder> actorTriggerBuilders) {
			bool loopChanged = false;

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
					actorTriggerBuilders.Remove(actorTriggerBuilder);
					loopChanged = true;
					SuperForms.End.Vertical();
					break;
				}

				SuperForms.End.Vertical();
				SuperForms.Space();
			}

			if (SuperForms.Button("Add Condition", GUILayout.ExpandWidth(true))) {
				actorTriggerBuilders.Add(new ActorTriggerBuilder());
				loopChanged = true;
			}

			return loopChanged;
		}

		private void DrawModificationTriggerManagementForm(List<string> callableMethods) {
			SuperForms.Begin.VerticalBox();
			SuperForms.BoxHeader("Modification Triggers");

			if (callableMethods.Count > 0) {
				SuperForms.Begin.Horizontal();

				SuperForms.Label("Add Trigger To Call ", SuperFormsStyles.BoxedLabel, GUILayout.Width(120));

				_indexForNewModTrigger = SuperForms.DropDown(_indexForNewModTrigger, callableMethods.ToArray(), GUILayout.Width(100));

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					if (_indexForNewModTrigger >= 0) {
						ActiveState.ModificationTriggers.Add(new ModificationTriggerProperties {
							Method = callableMethods[_indexForNewModTrigger]
						});

						ActiveState.ModificationTriggers = ActiveState.ModificationTriggers.OrderBy(o => o.Method).ToList();
					}
				}

				SuperForms.End.Horizontal();
			} else {
				SuperForms.Label("State Class doesn't have any Modification Methods", GUILayout.Width(200));
			}

			SuperForms.End.Vertical();
			SuperForms.Space();

			SuperForms.Region.Scroll("ActorStudioModificationsTrigger", () => {
				foreach (ModificationTriggerProperties modificationTrigger in ActiveState.ModificationTriggers) {
					SuperForms.Begin.VerticalBox();

					SuperForms.BoxHeader("> " + modificationTrigger.Method);

					if (DrawTriggerConditionsForm(modificationTrigger.ActorTriggerBuilders)) {
						SuperForms.End.Vertical();
						break;
					}

					SuperForms.Begin.Horizontal();
					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						ActiveState.ModificationTriggers.Remove(modificationTrigger);
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonClone)) {
						ModificationTriggerProperties triggerClone =
							JsonUtility.FromJson<ModificationTriggerProperties>(JsonUtility.ToJson(modificationTrigger));
						ActiveState.ModificationTriggers.Add(triggerClone);
					}

					if (ActiveState.ModificationTriggers.Count > 1) {
						SuperForms.Space();

						int activeIndex = ActiveState.ModificationTriggers.IndexOf(modificationTrigger);

						if (activeIndex > 0) {
							if (SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowUp)) {
								BoomerangUtils.Swap(ActiveState.ModificationTriggers, activeIndex, activeIndex - 1);
							}
						}

						if (activeIndex < ActiveState.ModificationTriggers.Count - 1) {
							if (SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowDown)) {
								BoomerangUtils.Swap(ActiveState.ModificationTriggers, activeIndex, activeIndex + 1);
							}
						}
					}

					SuperForms.End.Horizontal();
					SuperForms.End.Vertical();
					SuperForms.Space();
				}
			});
		}

		private void DrawWeaponTriggerManagementForm() {
			if (ActiveActor.Weapons.Count == 0) {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.FullBoxLabel("There are no attached Weapons");
					SuperForms.FullBoxLabel("Use the option on the far left to add one");
				});
				return;
			}

			SuperForms.Region.VerticalBox(() => {
				SuperForms.Region.Horizontal(() => {
					if (_newWeaponToAdd < 0 || _newWeaponToAdd >= ActiveActor.Weapons.Count) {
						_newWeaponToAdd = 0;
					}

					SuperForms.FullBoxLabel("Add Trigger for ");
					_newWeaponToAdd = SuperForms.DropDown(_newWeaponToAdd, ActiveActor.Weapons.ToArray());

					SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
						ActiveState.WeaponTriggers.Add(new WeaponTriggerProperties {
							WeaponName = ActiveActor.Weapons[_newWeaponToAdd],
							ActorTriggerBuilders = new List<ActorTriggerBuilder>()
						});
					});
				});
			});

			SuperForms.Space();

			SuperForms.Region.Scroll("ActorStudioWeaponsTrigger", () => {
				foreach (WeaponTriggerProperties weaponTrigger in ActiveState.WeaponTriggers) {
					SuperForms.Begin.VerticalBox();

					SuperForms.BoxHeader("> " + weaponTrigger.WeaponName);

					SuperForms.Begin.VerticalSubBox();
					SuperForms.BoxSubHeader("Adjustments");
					weaponTrigger.Offset = SuperForms.Vector2Field("Offset", weaponTrigger.Offset);
					weaponTrigger.Scale = SuperForms.Vector2Field("Scale", weaponTrigger.Scale);
					weaponTrigger.Rotation = SuperForms.FloatField("Rotation", weaponTrigger.Rotation);
					weaponTrigger.FlipHorizontal = SuperForms.Checkbox("Flip Horizontal", weaponTrigger.FlipHorizontal);
					weaponTrigger.FlipVertical = SuperForms.Checkbox("Flip Vertical", weaponTrigger.FlipVertical);
					weaponTrigger.TriggersWhileActive = SuperForms.Checkbox("Triggers While Active", weaponTrigger.TriggersWhileActive);
					SuperForms.End.Vertical();

					if (weaponTrigger.ActorTriggerBuilders.Count > 0) {
						SuperForms.Space();
						SuperForms.BoxSubHeader("Triggers");
					}

					if (DrawTriggerConditionsForm(weaponTrigger.ActorTriggerBuilders)) {
						SuperForms.End.Vertical();
						break;
					}

					SuperForms.Region.Horizontal(() => {
						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
							ActiveState.WeaponTriggers.Remove(weaponTrigger);
						}

						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonClone)) {
							WeaponTriggerProperties triggerClone = JsonUtility.FromJson<WeaponTriggerProperties>(JsonUtility.ToJson(weaponTrigger));
							ActiveState.WeaponTriggers.Add(triggerClone);
						}

						if (ActiveState.WeaponTriggers.Count > 1) {
							SuperForms.Space();

							int activeIndex = ActiveState.WeaponTriggers.IndexOf(weaponTrigger);

							if (activeIndex > 0) {
								if (SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowUp)) {
									BoomerangUtils.Swap(ActiveState.WeaponTriggers, activeIndex, activeIndex - 1);
								}
							}

							if (activeIndex < ActiveState.WeaponTriggers.Count - 1) {
								if (SuperForms.IconButton(SuperForms.IconButtons.ButtonArrowDown)) {
									BoomerangUtils.Swap(ActiveState.WeaponTriggers, activeIndex, activeIndex + 1);
								}
							}
						}
					});

					SuperForms.End.Vertical();
					SuperForms.Space();
				}
			});
		}

		private void DrawEntryEventsManagementForm() {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.Region.Horizontal(() => {
					SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
						ActiveState.StateEntryActorEvents.Add(new StateEntryExitEventProperties());
						_indexForSelectedStateEntryActorEvent = ActiveState.StateEntryActorEvents.Count - 1;
					});

					List<string> dropdownLabels = new List<string>();
					for (int i = 0; i < ActiveState.StateEntryActorEvents.Count; i++) {
						dropdownLabels.Add("Event " + (i + 1));
					}

					_indexForSelectedStateEntryActorEvent = SuperForms.DropDown(
						_indexForSelectedStateEntryActorEvent,
						dropdownLabels.ToArray(),
						GUILayout.Width(200)
					);
				});

				if (StateEntryEvent == null) {
					return;
				}

				StateEntryExitEventProperties toDelete = null;

				SuperForms.Region.Horizontal(() => {
					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						toDelete = StateEntryEvent;
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonClone)) {
						ActiveState.StateEntryActorEvents.Add(
							JsonUtility.FromJson<StateEntryExitEventProperties>(JsonUtility.ToJson(StateEntryEvent)));
						_indexForSelectedStateEntryActorEvent = ActiveState.StateEntryActorEvents.Count - 1;
					}
				});

				SuperForms.Region.VerticalBox(() => { DrawInteractionEventsTriggers(StateEntryEvent.ActorTriggerBuilders); }, GUILayout.Width(280));
				DrawInteractionEventList(StateEntryEvent.ActorEventBuilders, false);
				DrawInteractionEventList(StateEntryEvent.ActorElseEventBuilders, false, "Else ");

				if (toDelete != null) {
					ActiveState.StateEntryActorEvents.Remove(toDelete);
					_indexForSelectedStateEntryActorEvent = 0;
				}
			});
		}

		private void DrawExitEventsManagementForm() {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.Region.Horizontal(() => {
					SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd, () => {
						ActiveState.StateExitActorEvents.Add(new StateEntryExitEventProperties());
						_indexForSelectedStateExitActorEvent = ActiveState.StateExitActorEvents.Count - 1;
					});

					List<string> dropdownLabels = new List<string>();
					for (int i = 0; i < ActiveState.StateExitActorEvents.Count; i++) {
						dropdownLabels.Add("Event " + (i + 1));
					}

					_indexForSelectedStateExitActorEvent = SuperForms.DropDown(
						_indexForSelectedStateExitActorEvent,
						dropdownLabels.ToArray(),
						GUILayout.Width(200)
					);
				});

				if (StateExitEvent == null) {
					return;
				}

				StateEntryExitEventProperties toDelete = null;

				SuperForms.Region.Horizontal(() => {
					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						toDelete = StateExitEvent;
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonClone)) {
						ActiveState.StateExitActorEvents.Add(
							JsonUtility.FromJson<StateEntryExitEventProperties>(JsonUtility.ToJson(StateExitEvent)));
						_indexForSelectedStateExitActorEvent = ActiveState.StateExitActorEvents.Count - 1;
					}
				});

				SuperForms.Region.VerticalBox(() => { DrawInteractionEventsTriggers(StateExitEvent.ActorTriggerBuilders); }, GUILayout.Width(280));
				DrawInteractionEventList(StateExitEvent.ActorEventBuilders, false);
				DrawInteractionEventList(StateExitEvent.ActorElseEventBuilders, false, "Else ");

				if (toDelete != null) {
					ActiveState.StateExitActorEvents.Remove(toDelete);
					_indexForSelectedStateExitActorEvent = 0;
				}
			});
		}

		private void DrawAnimationsManagementForm() {
			string[] animationNames = ActiveState.Animations.Select(properties => properties.Name).ToArray();

			if (_animationMode == Mode.Normal) {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.Region.Horizontal(() => {
						if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
							_inputName = "";
							_animationMode = Mode.Create;
						}

						if (_indexForActiveAnimation > animationNames.Length) {
							_indexForActiveAnimation = animationNames.Length - 1;
						}

						_indexForActiveAnimation = SuperForms.DropDown(
							_indexForActiveAnimation,
							animationNames,
							GUILayout.Width(160)
						);
					});

					SuperForms.Region.Horizontal(() => {
						if (ActiveAnimation != null) {
							if (SuperForms.Button("Rename")) {
								_inputName = ActiveAnimation.Name;
								_animationMode = Mode.Rename;
							}

							if (SuperForms.Button("Clone")) {
								_inputName = "";
								_animationMode = Mode.Clone;
							}

							if (SuperForms.Button("Delete")) {
								_animationMode = Mode.Delete;
							}
						}
					});
				});
			}

			if (_animationMode == Mode.Rename) {
				bool isValidName = true;
				string validationMessage = "";

				if (_inputName.Trim() == "") {
					isValidName = false;
					validationMessage = "Name cannot be blank";
				}

				List<string> otherNames = (from animations in ActiveState.Animations where animations != ActiveAnimation select animations.Name).ToList();

				if (otherNames.IndexOf(_inputName.Trim()) > -1) {
					isValidName = false;
					validationMessage = "Name is already used";
				}

				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Rename Animation");
					_inputName = SuperForms.StringField("New Name", _inputName);

					if (!isValidName) {
						SuperForms.FullBoxLabel(validationMessage);
					}

					SuperForms.Region.Horizontal(() => {
						SuperForms.Button("Cancel", () => { _animationMode = Mode.Normal; });

						SuperForms.Button("Rename", () => {
							if (!isValidName) {
								return;
							}

							ActiveAnimation.Name = _inputName.Trim();
							_animationMode = Mode.Normal;
						});
					});
				});
			}

			if (_animationMode == Mode.Clone) {
				bool isValidName = true;
				string validationMessage = "";

				if (_inputName.Trim() == "") {
					isValidName = false;
					validationMessage = "Name cannot be blank";
				}

				List<string> otherNames = (from animations in ActiveState.Animations select animations.Name).ToList();

				if (otherNames.IndexOf(_inputName.Trim()) > -1) {
					isValidName = false;
					validationMessage = "Name is already used";
				}

				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Clone Animation");
					_inputName = SuperForms.StringField("New Name", _inputName);

					if (!isValidName) {
						SuperForms.FullBoxLabel(validationMessage);
					}

					SuperForms.Region.Horizontal(() => {
						SuperForms.Button("Cancel", () => { _animationMode = Mode.Normal; });

						SuperForms.Button("Clone", () => {
							if (!isValidName) {
								return;
							}

							StatePropertiesAnimation newAnimation = JsonUtility.FromJson<StatePropertiesAnimation>(JsonUtility.ToJson(ActiveAnimation));
							newAnimation.Name = _inputName.Trim();

							ActiveState.Animations.Add(newAnimation);
							_indexForActiveAnimation = ActiveState.Animations.IndexOf(newAnimation);
							_animationMode = Mode.Normal;
						});
					});
				});
			}

			if (_animationMode == Mode.Delete) {
				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Delete this Animation?");
					SuperForms.Region.Horizontal(() => {
						SuperForms.Button("Cancel", () => { _animationMode = Mode.Normal; });

						SuperForms.Button("Delete", () => {
							ActiveState.Animations.RemoveAt(ActiveState.Animations.IndexOf(ActiveAnimation));
							_indexForActiveAnimation = -1;
							_animationMode = Mode.Normal;
						});
					});
				});
			}

			if (_animationMode == Mode.Create) {
				bool isValidName = true;
				string validationMessage = "";

				if (_inputName.Trim() == "") {
					isValidName = false;
					validationMessage = "Name cannot be blank";
				}

				List<string> otherNames = ActiveState.Animations.Select(animation => animation.Name).ToList();

				if (otherNames.IndexOf(_inputName.Trim()) > -1) {
					isValidName = false;
					validationMessage = "Name is already used";
				}

				SuperForms.Region.VerticalBox(() => {
					SuperForms.BoxHeader("Add an Animation");
					_inputName = SuperForms.StringField("Name", _inputName);

					if (!isValidName) {
						SuperForms.FullBoxLabel(validationMessage);
					}

					SuperForms.Region.Horizontal(() => {
						SuperForms.Button("Cancel", () => { _animationMode = Mode.Normal; });

						SuperForms.Button("Create", () => {
							if (!isValidName) {
								return;
							}

							StatePropertiesAnimation newAnimation = new StatePropertiesAnimation {
								Name = _inputName.Trim(),
								AnimationFrames = new List<StatePropertiesAnimationFrame> {
									new StatePropertiesAnimationFrame()
								}
							};

							ActiveState.Animations.Add(newAnimation);
							_indexForActiveAnimation = ActiveState.Animations.IndexOf(newAnimation);
							_animationMode = Mode.Normal;
						});
					});
				});
			}

			if (_animationMode == Mode.Normal && ActiveAnimation != null) {
				DrawAnimationPanel();
			}
		}

		private void DrawAnimationPanel() {
			DrawAnimationsSpritePreviewBox(ActiveAnimation, 280, 200, 40, true, true);

			SuperForms.Space();

			SuperForms.Region.Scroll("ActorStudioAnimationPanel", () => {
				DrawAnimationLoopingProperties();
				SuperForms.Space();
				DrawAnimationFrameSelection();
				SuperForms.Space();
				DrawAnimationFramesProperties();
				SuperForms.Space();
				DrawAnimationBoundingBoxProperties();
				SuperForms.Space();
				DrawAnimationParticleEffectsProperties();
				SuperForms.Space();
				DrawAnimationConditions();
			});
		}

		private void DrawAnimationsSpritePreviewBox(
			StatePropertiesAnimation animationToPlay,
			int width,
			int height,
			int padding,
			bool showPercentageBar,
			bool showRaysAndBounds
		) {
			bool shouldRegenerateTextures = _activeActorTextures.Any(texture => texture == null);

			if (shouldRegenerateTextures || _activeActorTextures.Count == 0) {
				GenerateTexturesForSprite();
			}

			SuperForms.BlockedArea(GUILayout.Width(width), GUILayout.Height(height));

			Rect spritePreviewBoxBounds = GUILayoutUtility.GetLastRect();

			Rect spritePreviewImageBounds = new Rect(
				spritePreviewBoxBounds.x + padding,
				spritePreviewBoxBounds.y + padding,
				spritePreviewBoxBounds.width - padding * 2,
				spritePreviewBoxBounds.height - padding * 2
			);

			SuperForms.Texture(spritePreviewBoxBounds, SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.TitleBackground]);

			if (_activeActorTextures.Count > 0) {
				if (_activeActorTextures[0].width > _activeActorTextures[0].height) {
					float ratio = (float) _activeActorTextures[0].height / _activeActorTextures[0].width;
					spritePreviewImageBounds.height = spritePreviewImageBounds.width * ratio;
				} else {
					float ratio = (float) _activeActorTextures[0].width / _activeActorTextures[0].height;
					spritePreviewImageBounds.width = spritePreviewImageBounds.height * ratio;
				}
			}

			spritePreviewImageBounds.x = (spritePreviewBoxBounds.width - spritePreviewImageBounds.width) / 2;
			spritePreviewImageBounds.x += spritePreviewBoxBounds.x;
			spritePreviewImageBounds.y = (spritePreviewBoxBounds.height - spritePreviewImageBounds.height) / 2;
			spritePreviewImageBounds.y += spritePreviewBoxBounds.y;

			AnimationFrameDetails frameToDraw = GetAnimationFrame(animationToPlay);

			if (frameToDraw.SpriteFrame < _activeActorTextures.Count) {
				float rotation = animationToPlay.AnimationFrames[frameToDraw.AnimationFrame].Rotate ? -90 : 0;
				Matrix4x4 rotationAndPivotBackup = GUI.matrix;
				GUIUtility.RotateAroundPivot(rotation, new Vector2(
					spritePreviewImageBounds.x + spritePreviewImageBounds.width / 2,
					spritePreviewImageBounds.y + spritePreviewImageBounds.height / 2
				));

				SuperForms.Texture(
					spritePreviewImageBounds,
					_activeActorTextures[frameToDraw.SpriteFrame],
					animationToPlay.AnimationFrames[frameToDraw.AnimationFrame].FlipHorizontal,
					animationToPlay.AnimationFrames[frameToDraw.AnimationFrame].FlipVertical
				);

				GUI.matrix = rotationAndPivotBackup;
			}

			if (showPercentageBar) {
				Rect percentageBar = new Rect(
					spritePreviewBoxBounds.x,
					spritePreviewBoxBounds.y + spritePreviewBoxBounds.height - 10,
					spritePreviewBoxBounds.width * (frameToDraw.AnimationFrame + 1) / animationToPlay.AnimationFrames.Count,
					10
				);

				SuperForms.Texture(
					percentageBar,
					SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.SelectionBarBackground]
				);
			}

			if (showRaysAndBounds) {
				DrawAnimationsSpritePreviewBoxBounds(frameToDraw, spritePreviewImageBounds);
				DrawAnimationSpritePreviewParticleEffects(frameToDraw, spritePreviewBoxBounds);
			}
		}

		private AnimationFrameDetails GetAnimationFrame(StatePropertiesAnimation animationToPlay) {
			animationToPlay.CalculateTotalDuration();
			float totalAnimationTime = animationToPlay.TotalDuration;

			float totalCycles = _totalTime / totalAnimationTime;
			float progressionIntoCycle = totalCycles - Mathf.Floor(totalCycles);
			float currentCycleFrame = progressionIntoCycle * totalAnimationTime;
			float frameMax = 0;
			int spriteFrame = 0;
			int animationFrame = 0;

			if (totalAnimationTime > 0 && animationToPlay.AnimationFrames.Count > 1) {
				for (int i = 0; i < animationToPlay.AnimationFrames.Count; i++) {
					frameMax += animationToPlay.AnimationFrames[i].Duration;

					if (currentCycleFrame > frameMax) {
						continue;
					}

					spriteFrame = animationToPlay.AnimationFrames[i].SpriteFrame;
					animationFrame = i;
					break;
				}
			} else {
				spriteFrame = animationToPlay.AnimationFrames[0].SpriteFrame;
			}

			return new AnimationFrameDetails {
				SpriteFrame = spriteFrame,
				AnimationFrame = animationFrame
			};
		}

		private void DrawAnimationsSpritePreviewBoxBounds(AnimationFrameDetails frameToDraw, Rect spritePreviewImageBounds) {
			List<BoundingBoxProperties> boundingBoxes = ActiveAnimation.AnimationFrames[frameToDraw.AnimationFrame].BoundingBoxProperties;

			if (boundingBoxes.Count > ActiveActor.BoundingBoxes.Count) {
				boundingBoxes.RemoveRange(ActiveActor.BoundingBoxes.Count, boundingBoxes.Count - 1);
			}

			foreach (BoundingBoxProperties boundingBox in boundingBoxes) {
				float boundsVisualWidth = boundingBox.Size.x * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;
				float boundsVisualHeight = boundingBox.Size.y * spritePreviewImageBounds.height / ActiveActor.SpriteHeight;
				float boundsVisualOffsetX = boundingBox.Offset.x * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;
				float boundsVisualOffsetY = boundingBox.Offset.y * spritePreviewImageBounds.height / ActiveActor.SpriteHeight;

				float boundsVisualX = spritePreviewImageBounds.x;
				boundsVisualX += spritePreviewImageBounds.width / 2 - boundsVisualWidth / 2 + boundsVisualOffsetX;

				float boundsVisualY = spritePreviewImageBounds.y;
				boundsVisualY += spritePreviewImageBounds.height / 2 - boundsVisualHeight / 2 - boundsVisualOffsetY;

				Rect boundsBox = new Rect(boundsVisualX, boundsVisualY, boundsVisualWidth, boundsVisualHeight);

				if (boundingBox.Enabled) {
					Texture2D boundingBoxTexture = SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.BoundingBox];
					SuperForms.Texture(new Rect(boundsBox.x, boundsBox.y, boundsBox.width, 2), boundingBoxTexture);
					SuperForms.Texture(new Rect(boundsBox.x, boundsBox.y + boundsBox.height - 2, boundsBox.width, 2), boundingBoxTexture);
					SuperForms.Texture(new Rect(boundsBox.x, boundsBox.y + 2, 2, boundsBox.height - 4), boundingBoxTexture);
					SuperForms.Texture(new Rect(boundsBox.x + boundsBox.width - 2, boundsBox.y + 2, 2, boundsBox.height - 4), boundingBoxTexture);
				}

				DrawAnimationsSpritePreviewBoxRays(boundingBox, boundsBox, spritePreviewImageBounds);
			}
		}

		private void DrawAnimationsSpritePreviewBoxRays(BoundingBoxProperties boundingBox, Rect boundsBox, Rect spritePreviewImageBounds) {
			if (boundingBox.RayCastUp) {
				DrawAnimationsSpritePreviewBoxRaysUp(boundingBox, boundsBox, spritePreviewImageBounds);
			}

			if (boundingBox.RayCastRight) {
				DrawAnimationsSpritePreviewBoxRaysRight(boundingBox, boundsBox, spritePreviewImageBounds);
			}

			if (boundingBox.RayCastDown) {
				DrawAnimationsSpritePreviewBoxRaysDown(boundingBox, boundsBox, spritePreviewImageBounds);
			}

			if (boundingBox.RayCastLeft) {
				DrawAnimationsSpritePreviewBoxRaysLeft(boundingBox, boundsBox, spritePreviewImageBounds);
			}
		}

		private void DrawAnimationsSpritePreviewBoxRaysUp(BoundingBoxProperties boundingBox, Rect boundsBox, Rect spritePreviewImageBounds) {
			float rayLength = 20 * boundingBox.RayLengthUp;
			float rayInsetVisual = boundingBox.RayInsetUp * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;
			float rayInsetLeftVisual = boundingBox.RayInsetFirstUp * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;
			float rayInsetRightVisual = boundingBox.RayInsetLastUp * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;

			if (boundingBox.RayCountUp == 1) {
				SuperForms.Texture(new Rect(boundsBox.x + boundsBox.width / 2, boundsBox.y + rayInsetVisual, 1, -rayLength),
					SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);
			} else {
				Vector2 topLeft = new Vector2(boundsBox.x + rayInsetLeftVisual, boundsBox.y + rayInsetVisual);
				Vector2 topRight = new Vector2(boundsBox.x + boundsBox.width - rayInsetRightVisual, boundsBox.y + rayInsetVisual);

				SuperForms.Texture(new Rect(topLeft.x, topLeft.y, 1, -rayLength), SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);
				SuperForms.Texture(new Rect(topRight.x, topRight.y, 1, -rayLength), SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);

				int extraRays = boundingBox.RayCountUp - 2;

				float newPointSpacing = (topRight.x - topLeft.x) / (extraRays + 1);

				for (int j = 1; j <= extraRays; j++) {
					SuperForms.Texture(new Rect(topLeft.x + newPointSpacing * j, topLeft.y, 1, -rayLength),
						SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);
				}
			}
		}

		private void DrawAnimationsSpritePreviewBoxRaysRight(BoundingBoxProperties boundingBox, Rect boundsBox, Rect spritePreviewImageBounds) {
			float rayLength = 20 * boundingBox.RayLengthRight;
			float rayInsetVisual = boundingBox.RayInsetRight * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;
			float rayInsetTopVisual = boundingBox.RayInsetFirstRight * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;
			float rayInsetBottomVisual = boundingBox.RayInsetLastRight * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;

			if (boundingBox.RayCountRight == 1) {
				SuperForms.Texture(new Rect(boundsBox.x + boundsBox.width - rayInsetVisual, boundsBox.y + boundsBox.height / 2, rayLength, 1),
					SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);
			} else {
				Vector2 topRight = new Vector2(boundsBox.x + boundsBox.width - rayInsetVisual, boundsBox.y + rayInsetTopVisual);
				Vector2 bottomRight = new Vector2(boundsBox.x + boundsBox.width - rayInsetVisual, boundsBox.y + boundsBox.height - rayInsetBottomVisual);

				SuperForms.Texture(new Rect(bottomRight.x, bottomRight.y, rayLength, 1), SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);
				SuperForms.Texture(new Rect(topRight.x, topRight.y, rayLength, 1), SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);

				int extraRays = boundingBox.RayCountRight - 2;

				float newPointSpacing = (topRight.y - bottomRight.y) / (extraRays + 1);

				for (int j = 1; j <= extraRays; j++) {
					SuperForms.Texture(new Rect(bottomRight.x, bottomRight.y + newPointSpacing * j, rayLength, 1),
						SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);
				}
			}
		}

		private void DrawAnimationsSpritePreviewBoxRaysDown(BoundingBoxProperties boundingBox, Rect boundsBox, Rect spritePreviewImageBounds) {
			float rayLength = 20 * boundingBox.RayLengthDown;
			float rayInsetVisual = boundingBox.RayInsetDown * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;
			float rayInsetLeftVisual = boundingBox.RayInsetFirstDown * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;
			float rayInsetRightVisual = boundingBox.RayInsetLastDown * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;

			if (boundingBox.RayCountDown == 1) {
				SuperForms.Texture(new Rect(boundsBox.x + boundsBox.width / 2, boundsBox.y + boundsBox.height - rayInsetVisual, 1, rayLength),
					SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);
			} else {
				Vector2 bottomLeft = new Vector2(boundsBox.x + rayInsetLeftVisual, boundsBox.y + boundsBox.height - rayInsetVisual);
				Vector2 bottomRight = new Vector2(boundsBox.x + boundsBox.width - rayInsetRightVisual, boundsBox.y + boundsBox.height - rayInsetVisual);

				SuperForms.Texture(new Rect(bottomLeft.x, bottomLeft.y, 1, rayLength), SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);
				SuperForms.Texture(new Rect(bottomRight.x, bottomRight.y, 1, rayLength), SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);

				int extraRays = boundingBox.RayCountDown - 2;

				float newPointSpacing = (bottomRight.x - bottomLeft.x) / (extraRays + 1);

				for (int j = 1; j <= extraRays; j++) {
					SuperForms.Texture(new Rect(bottomLeft.x + (newPointSpacing * j), bottomLeft.y, 1, rayLength),
						SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);
				}
			}
		}

		private void DrawAnimationsSpritePreviewBoxRaysLeft(BoundingBoxProperties boundingBox, Rect boundsBox, Rect spritePreviewImageBounds) {
			float rayLength = 20 * boundingBox.RayLengthLeft;
			float rayInsetVisual = boundingBox.RayInsetLeft * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;
			float rayInsetTopVisual = boundingBox.RayInsetFirstLeft * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;
			float rayInsetBottomVisual = boundingBox.RayInsetLastLeft * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;

			if (boundingBox.RayCountLeft == 1) {
				SuperForms.Texture(new Rect(boundsBox.x + rayInsetVisual, boundsBox.y + boundsBox.height / 2, -rayLength, 1),
					SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);
			} else {
				Vector2 topLeft = new Vector2(boundsBox.x + rayInsetVisual, boundsBox.y + rayInsetTopVisual);
				Vector2 bottomLeft = new Vector2(boundsBox.x + rayInsetVisual, boundsBox.y + boundsBox.height - rayInsetBottomVisual);

				SuperForms.Texture(new Rect(bottomLeft.x, bottomLeft.y, -rayLength, 1), SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);
				SuperForms.Texture(new Rect(topLeft.x, topLeft.y, -rayLength, 1), SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);

				int extraRays = boundingBox.RayCountLeft - 2;

				float newPointSpacing = (topLeft.y - bottomLeft.y) / (extraRays + 1);

				for (int j = 1; j <= extraRays; j++) {
					SuperForms.Texture(new Rect(bottomLeft.x, bottomLeft.y + newPointSpacing * j, -rayLength, 1),
						SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays]);
				}
			}
		}

		private void DrawAnimationSpritePreviewParticleEffects(AnimationFrameDetails frameToDraw, Rect spritePreviewImageBounds) {
			List<ActorParticleEffectProperties>
				particleEffects = ActiveAnimation.AnimationFrames[frameToDraw.AnimationFrame].ParticleEffectProperties;

			if (particleEffects.Count > ActiveActor.ParticleEffects.Count) {
				particleEffects.RemoveRange(ActiveActor.ParticleEffects.Count, particleEffects.Count - 1);
			}

			foreach (ActorParticleEffectProperties particleEffect in particleEffects) {
				float centerX = particleEffect.DefaultOffsetPosition.x * spritePreviewImageBounds.width / ActiveActor.SpriteWidth;
				float centerY = particleEffect.DefaultOffsetPosition.y * spritePreviewImageBounds.height / ActiveActor.SpriteHeight;

				Texture2D boundingBoxTexture = SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.BoundingBox];
				SuperForms.Texture(new Rect(centerX, centerY, 4, 4), boundingBoxTexture);
			}
		}

		private void DrawAnimationLoopingProperties() {
			SuperForms.Region.VerticalBox(() => {
				ActiveAnimation.BoundToGlobalTimeManager = SuperForms.Checkbox("Bound to Global Time", ActiveAnimation.BoundToGlobalTimeManager);

				if (ActiveActor.Weapons.Count > 0) {
					ActiveAnimation.BoundToWeaponDuration = SuperForms.Checkbox("Bound to Weapon", ActiveAnimation.BoundToWeaponDuration);

					if (ActiveAnimation.BoundToWeaponDuration) {
						int index = ActiveActor.Weapons.IndexOf(ActiveAnimation.WeaponBoundTo);
						if (index < 0) {
							index = 0;
						}

						ActiveAnimation.WeaponBoundTo = ActiveActor.Weapons[SuperForms.DropDown("Weapon", index, ActiveActor.Weapons.ToArray())];
					}
				} else {
					ActiveAnimation.BoundToWeaponDuration = false;
				}

				if (!ActiveAnimation.BoundToWeaponDuration) {
					ActiveAnimation.IndefinitelyLoops = SuperForms.Checkbox("Loops Indefinitely", ActiveAnimation.IndefinitelyLoops);

					if (!ActiveAnimation.IndefinitelyLoops) {
						ActiveAnimation.FixedLoopCount = SuperForms.IntField("Total Loops", ActiveAnimation.FixedLoopCount);

						if (ActiveAnimation.FixedLoopCount < 1) {
							ActiveAnimation.FixedLoopCount = 1;
						}

						SuperForms.Space();

						SuperForms.BoxSubHeader("After Loops");

						ActiveAnimation.FinalFrameAfterLoops = SuperForms.IntField("Final Frame Id", ActiveAnimation.FinalFrameAfterLoops);

						if (ActiveAnimation.FinalFrameAfterLoops < 0) {
							ActiveAnimation.FinalFrameAfterLoops = 0;
						}

						if (ActiveAnimation.FinalFrameAfterLoops > ActiveAnimation.AnimationFrames.Count - 1) {
							ActiveAnimation.FinalFrameAfterLoops = ActiveAnimation.AnimationFrames.Count - 1;
						}
					}

					ActiveAnimation.StartOnExistingSpriteFrame =
						SuperForms.Checkbox("Start on Existing Sprite Frame", ActiveAnimation.StartOnExistingSpriteFrame);
				} else {
					ActiveAnimation.IndefinitelyLoops = false;
					ActiveAnimation.StartOnExistingSpriteFrame = false;
				}
			});
		}

		private void DrawAnimationFrameSelection() {
			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Frames");

				const int buttonWidth = 25;
				int totalFrames = ActiveAnimation.AnimationFrames.Count;
				int perRow = 280 / (buttonWidth + 6);
				if (perRow == 0) {
					perRow = 1;
				}

				int totalRows = totalFrames / perRow;

				for (int j = 0; j <= totalRows; j++) {
					int currentRow = j;
					SuperForms.Region.Horizontal(() => {
						for (int i = currentRow * perRow; i < currentRow * perRow + perRow; i++) {
							if (i >= totalFrames) {
								break;
							}

							int buttonId = i;
							if (_selectedAnimationFrame != buttonId) {
								SuperForms.TinyButton(buttonId.ToString(), () => { _selectedAnimationFrame = buttonId; },
									GUILayout.Width(buttonWidth));
							} else {
								SuperForms.TinyButtonSelected(buttonId.ToString(), () => { _selectedAnimationFrame = buttonId; },
									GUILayout.Width(buttonWidth));
							}
						}
					});
				}

				SuperForms.Button("Add Frame", () => {
					StatePropertiesAnimationFrame newAnimationFrame = new StatePropertiesAnimationFrame();
					List<BoundingBoxProperties> allBoundingBoxes = ActiveActor.BoundingBoxes
						.Select(box => new BoundingBoxProperties {
							Size = box.Size,
							Offset = box.Offset,
							Flags = box.Flags,

							RayCastUp = box.RayCastUp,
							RayCountUp = box.RayCountUp,
							RayInsetUp = box.RayInsetUp,
							RayLengthUp = box.RayLengthUp,
							RayInsetFirstUp = box.RayInsetFirstUp,
							RayInsetLastUp = box.RayInsetLastUp,

							RayCastRight = box.RayCastRight,
							RayCountRight = box.RayCountRight,
							RayInsetRight = box.RayInsetRight,
							RayLengthRight = box.RayLengthRight,
							RayInsetFirstRight = box.RayInsetFirstRight,
							RayInsetLastRight = box.RayInsetLastRight,

							RayCastDown = box.RayCastDown,
							RayCountDown = box.RayCountDown,
							RayInsetDown = box.RayInsetDown,
							RayLengthDown = box.RayLengthDown,
							RayInsetFirstDown = box.RayInsetFirstDown,
							RayInsetLastDown = box.RayInsetLastDown,

							RayCastLeft = box.RayCastLeft,
							RayCountLeft = box.RayCountLeft,
							RayInsetLeft = box.RayInsetLeft,
							RayLengthLeft = box.RayLengthLeft,
							RayInsetFirstLeft = box.RayInsetFirstLeft,
							RayInsetLastLeft = box.RayInsetLastLeft
						}).ToList();

					newAnimationFrame.BoundingBoxProperties = allBoundingBoxes;
					ActiveAnimation.AnimationFrames.Add(newAnimationFrame);
				});
			});
		}

		private void DrawAnimationFramesProperties() {
			if (_selectedAnimationFrame < 0 || _selectedAnimationFrame >= ActiveAnimation.AnimationFrames.Count) {
				_selectedAnimationFrame = 0;
			}

			StatePropertiesAnimationFrame animationFrame = ActiveAnimation.AnimationFrames[_selectedAnimationFrame];

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Frame Id: " + _selectedAnimationFrame);
				SuperForms.Space();

				animationFrame.Duration = SuperForms.FloatField("Duration", animationFrame.Duration);

				if (_activeActorTextures.Count > 0) {
					animationFrame.SpriteFrame = SuperForms.IntField("Sprite Frame", animationFrame.SpriteFrame);
					animationFrame.FlipHorizontal = SuperForms.Checkbox("Flip Horizontal", animationFrame.FlipHorizontal);
					animationFrame.FlipVertical = SuperForms.Checkbox("Flip Vertical", animationFrame.FlipVertical);
					animationFrame.Rotate = SuperForms.Checkbox("Rotate 90º", animationFrame.Rotate);

					if (animationFrame.SpriteFrame < 0 || animationFrame.SpriteFrame >= _activeActorTextures.Count) {
						animationFrame.SpriteFrame = 0;
					}
				}

				if (ActiveAnimation.AnimationFrames.Count > 1 && SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
					ActiveAnimation.AnimationFrames.RemoveAt(_selectedAnimationFrame);
				}
			});
		}

		private void DrawAnimationBoundingBoxProperties() {
			if (ActiveActor.BoundingBoxes.Count == 0) {
				return;
			}

			StatePropertiesAnimationFrame animationFrame = ActiveAnimation.AnimationFrames[_selectedAnimationFrame];

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Bounding Boxes");

				if (animationFrame.BoundingBoxProperties.Count != ActiveActor.BoundingBoxes.Count) {
					animationFrame.BoundingBoxProperties = ActiveActor.BoundingBoxes
						.Select(box => new BoundingBoxProperties {
							Size = box.Size,
							Offset = box.Offset,
							Flags = box.Flags,

							RayCastUp = box.RayCastUp,
							RayCountUp = box.RayCountUp,
							RayInsetUp = box.RayInsetUp,
							RayLengthUp = box.RayLengthUp,
							RayInsetFirstUp = box.RayInsetFirstUp,
							RayInsetLastUp = box.RayInsetLastUp,

							RayCastRight = box.RayCastRight,
							RayCountRight = box.RayCountRight,
							RayInsetRight = box.RayInsetRight,
							RayLengthRight = box.RayLengthRight,
							RayInsetFirstRight = box.RayInsetFirstRight,
							RayInsetLastRight = box.RayInsetLastRight,

							RayCastDown = box.RayCastDown,
							RayCountDown = box.RayCountDown,
							RayInsetDown = box.RayInsetDown,
							RayLengthDown = box.RayLengthDown,
							RayInsetFirstDown = box.RayInsetFirstDown,
							RayInsetLastDown = box.RayInsetLastDown,

							RayCastLeft = box.RayCastLeft,
							RayCountLeft = box.RayCountLeft,
							RayInsetLeft = box.RayInsetLeft,
							RayLengthLeft = box.RayLengthLeft,
							RayInsetFirstLeft = box.RayInsetFirstLeft,
							RayInsetLastLeft = box.RayInsetLastLeft
						}).ToList();
				}

				foreach (BoundingBoxProperties boundingBox in animationFrame.BoundingBoxProperties) {
					BoundingBoxProperties box = boundingBox;
					SuperForms.Begin.VerticalSubBox();
					box.Enabled = SuperForms.Checkbox("Enabled", box.Enabled);

					SuperForms.Region.Horizontal(() => {
						SuperForms.Label("Bounds Size", GUILayout.Width(110));

						SuperForms.Label("X", GUILayout.Width(14));
						box.Size.x = SuperForms.FloatField(box.Size.x, GUILayout.Width(30));

						SuperForms.Label("Y", GUILayout.Width(14));
						box.Size.y = SuperForms.FloatField(box.Size.y, GUILayout.Width(30));
					});

					SuperForms.Region.Horizontal(() => {
						SuperForms.Label("Bounds Offset", GUILayout.Width(110));

						SuperForms.Label("X", GUILayout.Width(14));
						box.Offset.x = SuperForms.FloatField(box.Offset.x, GUILayout.Width(30));

						SuperForms.Label("Y", GUILayout.Width(14));
						box.Offset.y = SuperForms.FloatField(box.Offset.y, GUILayout.Width(30));
					});

					boundingBox.Flags = SuperForms.ListField("Flags", boundingBox.Flags);

					boundingBox.RayCastUp = SuperForms.Checkbox("Cast Rays Up", boundingBox.RayCastUp);
					if (boundingBox.RayCastUp) {
						boundingBox.RayCountUp = SuperForms.IntField("Ray Count", boundingBox.RayCountUp);
						boundingBox.RayInsetUp = SuperForms.FloatField("Ray Inset", boundingBox.RayInsetUp);
						boundingBox.RayLengthUp = SuperForms.FloatField("Ray Length", boundingBox.RayLengthUp);
						if (boundingBox.RayCountUp > 1) {
							boundingBox.RayInsetFirstUp = SuperForms.FloatField("Ray Inset Left", boundingBox.RayInsetFirstUp);
							boundingBox.RayInsetLastUp = SuperForms.FloatField("Ray Inset Right", boundingBox.RayInsetLastUp);
						}
					}

					SuperForms.Space();

					boundingBox.RayCastRight = SuperForms.Checkbox("Cast Rays Right", boundingBox.RayCastRight);
					if (boundingBox.RayCastRight) {
						boundingBox.RayCountRight = SuperForms.IntField("Ray Count", boundingBox.RayCountRight);
						boundingBox.RayInsetRight = SuperForms.FloatField("Ray Inset", boundingBox.RayInsetRight);
						boundingBox.RayLengthRight = SuperForms.FloatField("Ray Length", boundingBox.RayLengthRight);
						if (boundingBox.RayCountRight > 1) {
							boundingBox.RayInsetFirstRight = SuperForms.FloatField("Ray Inset Top", boundingBox.RayInsetFirstRight);
							boundingBox.RayInsetLastRight = SuperForms.FloatField("Ray Inset Bottom", boundingBox.RayInsetLastRight);
						}
					}

					SuperForms.Space();

					boundingBox.RayCastDown = SuperForms.Checkbox("Cast Rays Down", boundingBox.RayCastDown);
					if (boundingBox.RayCastDown) {
						boundingBox.RayCountDown = SuperForms.IntField("Ray Count", boundingBox.RayCountDown);
						boundingBox.RayInsetDown = SuperForms.FloatField("Ray Inset", boundingBox.RayInsetDown);
						boundingBox.RayLengthDown = SuperForms.FloatField("Ray Length", boundingBox.RayLengthDown);
						if (boundingBox.RayCountDown > 1) {
							boundingBox.RayInsetFirstDown = SuperForms.FloatField("Ray Inset Left", boundingBox.RayInsetFirstDown);
							boundingBox.RayInsetLastDown = SuperForms.FloatField("Ray Inset Right", boundingBox.RayInsetLastDown);
						}
					}

					SuperForms.Space();

					boundingBox.RayCastLeft = SuperForms.Checkbox("Cast Rays Left", boundingBox.RayCastLeft);
					if (boundingBox.RayCastLeft) {
						boundingBox.RayCountLeft = SuperForms.IntField("Ray Count", boundingBox.RayCountLeft);
						boundingBox.RayInsetLeft = SuperForms.FloatField("Ray Inset", boundingBox.RayInsetLeft);
						boundingBox.RayLengthLeft = SuperForms.FloatField("Ray Length", boundingBox.RayLengthLeft);
						if (boundingBox.RayCountLeft > 1) {
							boundingBox.RayInsetFirstLeft = SuperForms.FloatField("Ray Inset Top", boundingBox.RayInsetFirstLeft);
							boundingBox.RayInsetLastLeft = SuperForms.FloatField("Ray Inset Bottom", boundingBox.RayInsetLastLeft);
						}
					}

					if (boundingBox.RayCountUp < 1) {
						boundingBox.RayCountUp = 1;
					}

					if (boundingBox.RayCountRight < 1) {
						boundingBox.RayCountRight = 1;
					}

					if (boundingBox.RayCountDown < 1) {
						boundingBox.RayCountDown = 1;
					}

					if (boundingBox.RayCountLeft < 1) {
						boundingBox.RayCountLeft = 1;
					}

					if (boundingBox.RayLengthUp < 0.01f) {
						boundingBox.RayLengthUp = 0.01f;
					}

					if (boundingBox.RayLengthRight < 0.01f) {
						boundingBox.RayLengthRight = 0.01f;
					}

					if (boundingBox.RayLengthDown < 0.01f) {
						boundingBox.RayLengthDown = 0.01f;
					}

					if (boundingBox.RayLengthLeft < 0.01f) {
						boundingBox.RayLengthLeft = 0.01f;
					}

					SuperForms.End.Vertical();
				}
			});
		}

		private void DrawAnimationParticleEffectsProperties() {
			if (ActiveActor.ParticleEffects.Count == 0) {
				return;
			}

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Particle Effects");

				StatePropertiesAnimationFrame animationFrame = ActiveAnimation.AnimationFrames[_selectedAnimationFrame];

				if (animationFrame.ParticleEffectProperties.Count != ActiveActor.ParticleEffects.Count) {
					animationFrame.ParticleEffectProperties = ActiveActor.ParticleEffects
						.Select(particleEffect => new ActorParticleEffectProperties {
							Name = particleEffect.Name,
							DefaultOffsetPosition = particleEffect.DefaultOffsetPosition
						}).ToList();
				}

				foreach (ActorParticleEffectProperties particleEffect in animationFrame.ParticleEffectProperties) {
					SuperForms.Begin.VerticalSubBox();
					SuperForms.BoxSubHeader(particleEffect.Name);
					particleEffect.Enabled = SuperForms.Checkbox("Emits", particleEffect.Enabled);
					particleEffect.DefaultOffsetPosition = SuperForms.Vector2Field("Offset", particleEffect.DefaultOffsetPosition);
					SuperForms.End.Vertical();
				}
			});
		}

		private void DrawAnimationConditions() {
			SuperForms.Space();

			SuperForms.Region.VerticalBox(() => {
				SuperForms.BoxHeader("Play Conditions");

				if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
					ActiveAnimation.AnimationConditions.Add(
						new StatePropertiesAnimationCondition {ActorTriggerBuilders = new List<ActorTriggerBuilder> {new ActorTriggerBuilder()}}
					);
				}
			});

			SuperForms.Space();

			StatePropertiesAnimationCondition toDelete = null;

			foreach (StatePropertiesAnimationCondition animationCondition in ActiveAnimation.AnimationConditions) {
				SuperForms.Begin.VerticalBox();

				SuperForms.Begin.Horizontal();
				{
					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonAdd)) {
						animationCondition.ActorTriggerBuilders.Add(new ActorTriggerBuilder());
					}

					if (SuperForms.IconButton(SuperForms.IconButtons.ButtonDelete)) {
						toDelete = animationCondition;
					}
				}
				SuperForms.End.Horizontal();

				if (DrawTriggerConditionsForm(animationCondition.ActorTriggerBuilders)) {
					SuperForms.End.Vertical();
					break;
				}

				SuperForms.End.Vertical();
				SuperForms.Space();
			}

			if (toDelete != null) {
				ActiveAnimation.AnimationConditions.RemoveAt(ActiveAnimation.AnimationConditions.IndexOf(toDelete));
			}
		}
	}
}