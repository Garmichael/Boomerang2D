using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.ActorEvents;
using Boomerang2DFramework.Framework.Actors.ActorFinderFilters;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.AnimationProperties;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.TriggerProperties;
using Boomerang2DFramework.Framework.Actors.InteractionEvents;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;
using Boomerang2DFramework.Framework.Actors.Weapons;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.StateManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Boomerang2DFramework.Framework.Actors {
	public partial class Actor {
		public void SetParent(Actor parentActor) {
			ParentActor = parentActor;
			parentActor.ChildrenActors.Add(this);
		}

		private void BuildChildren() {
			foreach (string childActor in ActorProperties.ChildrenActors) {
				string json = BoomerangDatabase.ActorJsonDatabaseEntries[childActor].text;

				if (json == null) {
					return;
				}

				ActorProperties actorPropertiesToLoad = JsonUtility.FromJson<ActorProperties>(json);

				Actor actorScript = Boomerang2D.ActorManager.GetActorFromPool(
					actorPropertiesToLoad,
					new ActorInstanceProperties(),
					Transform,
					new Vector3(0, 0, -0.1f),
					MapLayerBehavior,
					MapLayerName,
					""
				);

				actorScript.SetParent(this);
			}
		}

		private void SetSpriteData() {
			Sprites = BoomerangDatabase.ActorSpriteDatabaseEntries[ActorProperties.Name];

			if (Sprites != null && Sprites.Length >= 1) {
				SpriteRenderer.sprite = Sprites[0];
			}
		}

		private void BuildBoundingBoxes() {
			foreach (BoxCollider2D boxCollider2D in BoundingBoxColliders) {
				boxCollider2D.enabled = false;
			}

			while (BoundingBoxColliders.Count < ActorProperties.BoundingBoxes.Count) {
				BoxCollider2D boxCollider = GameObject.AddComponent<BoxCollider2D>();
				boxCollider.enabled = false;
				BoundingBoxColliders.Add(boxCollider);
			}
		}

		private void SetInitialStats() {
			foreach (FloatStatProperties stat in ActorProperties.StatsFloats) {
				stat.Value = stat.InitialValue;
			}

			foreach (BoolStatProperties stat in ActorProperties.StatsBools) {
				stat.Value = stat.InitialValue;
			}

			foreach (StringStatProperties stat in ActorProperties.StatsStrings) {
				stat.Value = stat.InitialValue;
			}
		}

		private void CreateStateInstances() {
			if (StateMachine == null) {
				StateMachine = new StateMachine();
			} else {
				StateMachine.ClearStates();
			}

			Dictionary<string, ActorState> states = new Dictionary<string, ActorState>();

			foreach (PropertyClasses.ActorStateProperties state in ActorProperties.States) {
				Type stateType = Type.GetType(state.Class);
				Type propertiesType = Type.GetType(state.PropertiesClass);
				if (stateType == null || propertiesType == null) {
					Debug.LogWarning("State Class " + state.Class + " or Properties not found");
					continue;
				}

				object stateProperties = JsonUtility.FromJson(state.Properties, propertiesType);
				ActorState newState =
					(ActorState) Activator.CreateInstance(stateType, this, StateMachine, stateProperties);
				newState.Name = state.Name;

				newState.SetAnimationCollection(state.Animations);
				newState.SetSoundEffects(state.SoundEffects);
				states.Add(state.Name, newState);
				StateMachine.AddState(newState);
			}

			ConfigureStates(states);

			StateMachine.SetNextState(states[ActorProperties.States[0].Name]);
		}

		private void ConfigureStates(Dictionary<string, ActorState> states) {
			foreach (PropertyClasses.ActorStateProperties state in ActorProperties.States) {
				foreach (TransitionTriggerProperties transition in state.TransitionTriggers) {
					states[state.Name].AddTransitionTrigger(transition, states[transition.NextState]);
				}

				foreach (RandomTransitionTriggerProperties transition in state.RandomTransitionTriggers) {
					states[state.Name].AddRandomTransitionTrigger(transition);
				}

				foreach (ModificationTriggerProperties modification in state.ModificationTriggers) {
					states[state.Name].AddModificationTrigger(modification);
				}

				foreach (StatePropertiesAnimation stateAnimation in state.Animations) {
					if (stateAnimation.AnimationConditions.Count == 0) {
						states[state.Name].AddAnimationTrigger(stateAnimation.Name, null);
					} else {
						foreach (StatePropertiesAnimationCondition animationCondition in stateAnimation
							.AnimationConditions) {
							states[state.Name].AddAnimationTrigger(stateAnimation.Name, animationCondition);
						}
					}
				}

				foreach (WeaponTriggerProperties weaponTriggerProperties in state.WeaponTriggers) {
					states[state.Name].AddWeaponTrigger(weaponTriggerProperties);
				}

				states[state.Name].BuildActorStateEntryEvents(state.StateEntryActorEvents);
				states[state.Name].BuildActorStateExitEvents(state.StateExitActorEvents);
			}
		}

		private void CreateParticleEffectInstances() {
			foreach (ActorParticleEffectProperties particleEffect in ActorProperties.ParticleEffects) {
				GameObject particleEffectInstance = Object.Instantiate(
					BoomerangDatabase.ParticleEffectEntries[particleEffect.Name],
					new Vector3(0, 0, 0), Quaternion.identity
				);

				particleEffectInstance.transform.parent = Transform;
				particleEffectInstance.transform.localPosition = particleEffect.DefaultOffsetPosition;
				ParticleEffects.Add(particleEffect.Name, particleEffectInstance);
			}
		}

		private void CreateWeaponInstances() {
			_weaponContainer = new GameObject {name = "Weapons"};
			_weaponContainer.transform.parent = Transform;
			_weaponContainer.transform.localPosition = new Vector3(0, 0, 0);

			foreach (string weaponName in ActorProperties.Weapons) {
				if (!BoomerangDatabase.WeaponDatabaseEntries.ContainsKey(weaponName)) {
					Debug.LogWarning("Weapon " + weaponName + " not found in the database");
					continue;
				}

				string json = BoomerangDatabase.WeaponDatabaseEntries[weaponName].text;
				WeaponProperties weaponProperties = JsonUtility.FromJson<WeaponProperties>(json);

				GameObject weaponGameObject = new GameObject(weaponName);
				weaponGameObject.transform.parent = _weaponContainer.transform;
				weaponGameObject.transform.localPosition = new Vector3(0, 0, 0);
				weaponGameObject.layer = LayerMask.NameToLayer("Weapon");

				WeaponBehavior weaponBehavior = weaponGameObject.AddComponent<WeaponBehavior>();
				weaponBehavior.Initialize(weaponProperties);

				_weapons.Add(weaponName, new BuiltWeapon {
					Name = weaponName,
					GameObject = weaponGameObject,
					Behavior = weaponBehavior
				});
			}
		}

		private void BuildInteractionEventObjects() {
			foreach (ActorInteractionEvent interactionEvent in ActorProperties.InteractionEvents) {
				interactionEvent.Triggers.Clear();
				foreach (ActorTriggerBuilder actorTriggerBuilder in interactionEvent.ActorTriggerBuilders) {
					interactionEvent.Triggers.Add(actorTriggerBuilder.BuildTrigger());
				}

				interactionEvent.AfterFinderFilters.Clear();
				foreach (ActorFinderFilterBuilder actorFinderFilterBuilder in interactionEvent.ActorFinderFilterBuilders
				) {
					interactionEvent.AfterFinderFilters.Add(actorFinderFilterBuilder.BuildActorFilter());
				}

				interactionEvent.ActorEvents.Clear();
				foreach (ActorEventBuilder actorEventBuilder in interactionEvent.ActorEventBuilders) {
					interactionEvent.ActorEvents.Add(actorEventBuilder.BuildActorEvent());
				}
			}
		}
	}
}