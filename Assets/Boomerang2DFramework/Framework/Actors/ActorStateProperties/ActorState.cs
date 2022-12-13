using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Boomerang2DFramework.Framework.Actors.ActorEvents;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.AnimationProperties;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.SoundEffectProperties;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.TriggerProperties;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;
using Boomerang2DFramework.Framework.Actors.Weapons;
using Boomerang2DFramework.Framework.AudioManagement;
using Boomerang2DFramework.Framework.GlobalTimeManagement;
using Boomerang2DFramework.Framework.StateManagement;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorStateProperties {
	/// <summary>
	/// The Base class for all Actor State Behaviors. 
	/// </summary>
	public class ActorState : State {
		/// <summary>
		/// The Actor which this state is bound to
		/// </summary>
		protected Actor Actor;

		private List<StatePropertiesAnimation> _animations;
		private StatePropertiesAnimation _currentAnimation;
		private float _timeInCurrentAnimation;

		private List<StatePropertiesSoundEffect> _soundEffects = new List<StatePropertiesSoundEffect>();
		private readonly List<LoopingAudioEffect> _loopingSoundEffects = new List<LoopingAudioEffect>();

		/// <summary>
		/// The Actor's current frame of Animation
		/// </summary>
		public StatePropertiesAnimationFrame ActiveAnimationFrame;

		[Serializable]
		public struct TransitionTriggerSet {
			public List<ActorTrigger> Triggers;
			public string NextStateName;
		}

		[Serializable]
		public struct RandomTransitionTriggerSet {
			public List<ActorTrigger> MainTriggers;
			public List<RandomTransitionOptionSet> TransitionOptions;
		}

		[Serializable]
		public struct RandomTransitionOptionSet {
			public float Odds;
			public TransitionTriggerSet TransitionOption;
		}

		[Serializable]
		public struct ModificationTriggerSet {
			public List<ActorTrigger> Triggers;
			public string Method;
		}

		[Serializable]
		public struct AnimationTriggerSet {
			public List<ActorTrigger> Triggers;
			public string Animation;
		}

		[Serializable]
		public struct WeaponTriggerSet {
			public List<ActorTrigger> Triggers;
			public Vector2 Offset;
			public Vector2 Scale;
			public float Rotation;
			public bool FlipHorizontal;
			public bool FlipVertical;
			public bool TriggersWhileActive;
			public string Weapon;
		}

		private readonly List<TransitionTriggerSet> _transitionTriggers = new List<TransitionTriggerSet>();
		private readonly List<ModificationTriggerSet> _modificationTriggers = new List<ModificationTriggerSet>();
		private readonly List<AnimationTriggerSet> _animationTriggers = new List<AnimationTriggerSet>();
		private readonly List<WeaponTriggerSet> _weaponTriggers = new List<WeaponTriggerSet>();

		private readonly List<RandomTransitionTriggerSet> _randomTransitionTriggers =
			new List<RandomTransitionTriggerSet>();

		/// <summary>
		/// Builds and adds a Transition Trigger to this State. 
		/// </summary>
		/// <param name="transitionTriggerProperties">Transition Properties</param>
		/// <param name="nextState">The State the Trigger transitions to</param>
		/// <remarks>
		/// Called in the initialization of Actors.
		/// </remarks>
		public void AddTransitionTrigger(TransitionTriggerProperties transitionTriggerProperties, State nextState) {
			List<ActorTrigger> triggerCollection =
				BuildTriggerCollection(transitionTriggerProperties.ActorTriggerBuilders);

			if (triggerCollection != null) {
				_transitionTriggers.Add(new TransitionTriggerSet {
					NextStateName = nextState.Name,
					Triggers = triggerCollection
				});
			}
		}

		/// <summary>
		/// Builds and adds a Random-Transition Trigger to this State. 
		/// </summary>
		/// <param name="randomTransitionTriggerProperties">Transition Properties</param>
		/// <remarks>
		/// Called in the initialization of Actors.
		/// </remarks>
		public void AddRandomTransitionTrigger(RandomTransitionTriggerProperties randomTransitionTriggerProperties) {
			List<ActorTrigger> mainTriggers =
				BuildTriggerCollection(randomTransitionTriggerProperties.ActorTriggerBuilders);
			List<RandomTransitionOptionSet> transitionOptions = new List<RandomTransitionOptionSet>();

			foreach (RandomTransitionOption randomTransitionOption in randomTransitionTriggerProperties
				.TransitionOptions) {
				List<ActorTrigger> triggers =
					BuildTriggerCollection(randomTransitionOption.TransitionTriggerProperties.ActorTriggerBuilders);

				TransitionTriggerSet transitionTriggerSet = new TransitionTriggerSet {
					NextStateName = randomTransitionOption.TransitionTriggerProperties.NextState,
					Triggers = triggers
				};

				transitionOptions.Add(new RandomTransitionOptionSet {
					Odds = randomTransitionOption.Odds,
					TransitionOption = transitionTriggerSet
				});
			}

			if (mainTriggers != null) {
				_randomTransitionTriggers.Add(new RandomTransitionTriggerSet {
					MainTriggers = mainTriggers,
					TransitionOptions = transitionOptions
				});
			}
		}

		/// <summary>
		/// Builds and Adds Modification-Triggers to this State
		/// </summary>
		/// <param name="modificationTriggerProperties">Trigger Properties</param>
		/// <remarks>
		/// Called in the initialization of Actors.
		/// </remarks>
		public void AddModificationTrigger(ModificationTriggerProperties modificationTriggerProperties) {
			List<ActorTrigger> triggerCollection =
				BuildTriggerCollection(modificationTriggerProperties.ActorTriggerBuilders);

			if (triggerCollection != null) {
				_modificationTriggers.Add(new ModificationTriggerSet {
					Method = modificationTriggerProperties.Method,
					Triggers = triggerCollection
				});
			}
		}

		/// <summary>
		/// Builds and Adds Animation Triggers to this State
		/// </summary>
		/// <param name="animationName">Animation Name</param>
		/// <param name="animationTriggerProperties">Trigger Properties</param>
		/// <remarks>
		/// Called in the initialization of Actors.
		/// </remarks>
		public void AddAnimationTrigger(
			string animationName,
			StatePropertiesAnimationCondition animationTriggerProperties
		) {
			if (animationTriggerProperties == null) {
				_animationTriggers.Add(new AnimationTriggerSet {
					Animation = animationName,
					Triggers = new List<ActorTrigger>()
				});
			} else {
				List<ActorTrigger> triggerCollection =
					BuildTriggerCollection(animationTriggerProperties.ActorTriggerBuilders);

				if (triggerCollection != null) {
					_animationTriggers.Add(new AnimationTriggerSet {
						Animation = animationName,
						Triggers = triggerCollection
					});
				}
			}
		}

		/// <summary>
		/// Builds and Adds Weapon Triggers to this State
		/// </summary>
		/// <param name="weaponTriggerProperties"></param>
		/// <remarks>
		/// Called in the initialization of Actors.
		/// </remarks>
		public void AddWeaponTrigger(WeaponTriggerProperties weaponTriggerProperties) {
			List<ActorTrigger> triggerCollection = BuildTriggerCollection(weaponTriggerProperties.ActorTriggerBuilders);

			if (triggerCollection != null) {
				_weaponTriggers.Add(new WeaponTriggerSet {
					Weapon = weaponTriggerProperties.WeaponName,
					Offset = weaponTriggerProperties.Offset,
					Scale = weaponTriggerProperties.Scale,
					Rotation = weaponTriggerProperties.Rotation,
					FlipHorizontal = weaponTriggerProperties.FlipHorizontal,
					FlipVertical = weaponTriggerProperties.FlipVertical,
					TriggersWhileActive = weaponTriggerProperties.TriggersWhileActive,
					Triggers = triggerCollection
				});
			}
		}

		private List<StateEntryExitEventProperties> _entryEvents = new List<StateEntryExitEventProperties>();
		private List<StateEntryExitEventProperties> _exitEvents = new List<StateEntryExitEventProperties>();

		public void BuildActorStateEntryEvents(List<StateEntryExitEventProperties> actorEvents) {
			_entryEvents = BuildActorStateEvents(actorEvents);
		}
		
		public void BuildActorStateExitEvents(List<StateEntryExitEventProperties> actorEvents) {
			_exitEvents = BuildActorStateEvents(actorEvents);
		}

		/// <summary>
		/// Builds the Actor Events
		/// </summary>
		/// <param name="actorEvents">Unbuilt Actor event properties</param>
		private List<StateEntryExitEventProperties> BuildActorStateEvents(List<StateEntryExitEventProperties> actorEvents){ 
		List<StateEntryExitEventProperties> events = new List<StateEntryExitEventProperties>();
			foreach (StateEntryExitEventProperties actorEvent in actorEvents) {
				actorEvent.Triggers.Clear();
				foreach (ActorTriggerBuilder actorTriggerBuilder in actorEvent.ActorTriggerBuilders) {
					actorEvent.Triggers.Add(actorTriggerBuilder.BuildTrigger());
				}

				actorEvent.ActorEvents.Clear();
				foreach (ActorEventBuilder actorEventBuilder in actorEvent.ActorEventBuilders) {
					actorEvent.ActorEvents.Add(actorEventBuilder.BuildActorEvent());
				}

				actorEvent.ActorElseEvents.Clear();
				foreach (ActorEventBuilder actorEventBuilder in actorEvent.ActorElseEventBuilders) {
					actorEvent.ActorElseEvents.Add(actorEventBuilder.BuildActorEvent());
				}

				events.Add(actorEvent);
			}

			return events;
		}

		/// <summary>
		/// Common functionality that builds all types of Triggers
		/// </summary>
		/// <param name="actorTriggerBuilders"></param>
		/// <returns>The collection of Triggers</returns>
		private static List<ActorTrigger>
			BuildTriggerCollection(IEnumerable<ActorTriggerBuilder> actorTriggerBuilders) {
			List<ActorTrigger> triggerCollection = new List<ActorTrigger>();

			foreach (ActorTriggerBuilder actorTriggerBuilder in actorTriggerBuilders) {
				ActorTrigger actorTrigger = actorTriggerBuilder.BuildTrigger();

				if (actorTrigger == null) {
					continue;
				}

				triggerCollection.Add(actorTrigger);
			}

			return triggerCollection;
		}

		/// <summary>
		/// Processes Modification Triggers, Weapon Triggers, and Animation Triggers
		/// </summary>
		/// <remarks>
		/// Called as part of the Actor's lifespan
		/// </remarks>
		public void ProcessTriggers() {
			ProcessModificationTriggers();
			ProcessWeaponTriggers();
			ProcessAnimationTriggers();
		}

		/// <summary>
		/// Processes Transition Triggers
		/// </summary>
		/// <remarks>
		/// Called as part of the Actor's lifespan
		/// </remarks>
		public void ProcessTransitionTriggers() {
			CheckTransitionTriggers();
		}

		/// <summary>
		/// Processes Modification Triggers
		/// </summary>
		private void ProcessModificationTriggers() {
			foreach (ModificationTriggerSet triggerSet in _modificationTriggers) {
				bool allTriggersMet = true;

				foreach (ActorTrigger trigger in triggerSet.Triggers) {
					if (!trigger.IsTriggered(Actor, this)) {
						allTriggersMet = false;
					}
				}

				if (allTriggersMet) {
					MethodInfo method = MyStateMachine.GetCurrentState().GetType().GetMethod(triggerSet.Method);
					if (method == null) {
						Debug.LogWarning(
							"Attempted to call a method on a state that doesn't exist. State: " +
							MyStateMachine.GetCurrentState().Name +
							", Method: " +
							triggerSet.Method
						);
					} else {
						method.Invoke(this, null);
					}

					break;
				}
			}
		}

		/// <summary>
		/// Processes Transition Triggers
		/// </summary>
		private void CheckTransitionTriggers() {
			string newState = null;

			foreach (TransitionTriggerSet triggerSet in _transitionTriggers) {
				bool allTriggersMet = true;

				foreach (ActorTrigger trigger in triggerSet.Triggers) {
					if (!trigger.IsTriggered(Actor, this)) {
						allTriggersMet = false;
					}
				}

				if (allTriggersMet) {
					newState = triggerSet.NextStateName;
				}
			}

			foreach (RandomTransitionTriggerSet triggerSet in _randomTransitionTriggers) {
				bool allMainTriggersMet = true;

				foreach (ActorTrigger trigger in triggerSet.MainTriggers) {
					if (!trigger.IsTriggered(Actor, this)) {
						allMainTriggersMet = false;
					}
				}

				if (allMainTriggersMet) {
					List<float> odds = new List<float>();

					List<RandomTransitionOptionSet> triggeredOptions = new List<RandomTransitionOptionSet>();

					foreach (RandomTransitionOptionSet randomOptionSet in triggerSet.TransitionOptions) {
						bool allOptionTriggersMet = true;

						foreach (ActorTrigger trigger in randomOptionSet.TransitionOption.Triggers) {
							if (!trigger.IsTriggered(Actor, this)) {
								allOptionTriggersMet = false;
							}
						}

						if (allOptionTriggersMet) {
							triggeredOptions.Add(randomOptionSet);
						}
					}

					foreach (RandomTransitionOptionSet triggeredRandomOptionSet in triggeredOptions) {
						if (odds.Count == 0) {
							odds.Add(triggeredRandomOptionSet.Odds);
						} else {
							odds.Add(odds[odds.Count - 1] + triggeredRandomOptionSet.Odds);
						}
					}

					float rolledValue = UnityEngine.Random.Range(0, odds.Last());
					int triggeredOptionIndex = 0;

					for (int value = 0; value < odds.Count; value++) {
						if (rolledValue < odds[value]) {
							triggeredOptionIndex = value;
							break;
						}
					}

					newState = triggeredOptions[triggeredOptionIndex].TransitionOption.NextStateName;
				}
			}

			if (newState != null) {
				MyStateMachine.SetNextState(newState);
			}
		}

		/// <summary>
		/// Processes Weapon Triggers
		/// </summary>
		private void ProcessWeaponTriggers() {
			foreach (WeaponTriggerSet triggerSet in _weaponTriggers) {
				bool allTriggersMet = true;

				foreach (ActorTrigger trigger in triggerSet.Triggers) {
					if (!trigger.IsTriggered(Actor, this)) {
						allTriggersMet = false;
					}
				}

				if (allTriggersMet) {
					WeaponBehavior weaponBehavior = Actor.GetWeaponBehavior(triggerSet.Weapon);

					if (weaponBehavior != null) {
						GameObject weaponGameObject = Actor.GetWeaponGameObject(triggerSet.Weapon);
						weaponGameObject.transform.localScale = triggerSet.Scale;
						weaponGameObject.transform.localPosition = triggerSet.Offset;
						weaponGameObject.transform.eulerAngles = new Vector3(0, 0, triggerSet.Rotation);

						if (triggerSet.FlipHorizontal) {
							weaponGameObject.transform.localScale =
								new Vector3(-triggerSet.Scale.x, triggerSet.Scale.y, 0);
						}

						if (triggerSet.FlipVertical) {
							weaponGameObject.transform.localScale =
								new Vector3(triggerSet.Scale.x, -triggerSet.Scale.y, 0);
						}

						if (weaponBehavior.IsActing && !triggerSet.TriggersWhileActive) {
							return;
						}

						weaponBehavior.Attack();
					}
				}
			}
		}

		/// <summary>
		/// Processes Animation Triggers
		/// </summary>
		private void ProcessAnimationTriggers() {
			string animationToChangeTo = "";

			foreach (AnimationTriggerSet triggerSet in _animationTriggers) {
				bool allTriggersMet = true;

				foreach (ActorTrigger trigger in triggerSet.Triggers) {
					if (!trigger.IsTriggered(Actor, this)) {
						allTriggersMet = false;
					}
				}

				if (allTriggersMet) {
					animationToChangeTo = triggerSet.Animation;
				}
			}

			if (animationToChangeTo != "" && _currentAnimation.Name != animationToChangeTo) {
				SetAnimation(animationToChangeTo);
			}
		}

		/// <summary>
		/// Sets the Complete Animation Set
		/// </summary>
		/// <param name="animations"></param>
		/// <remarks>
		/// Called in the initialization of Actors.
		/// </remarks>
		public void SetAnimationCollection(List<StatePropertiesAnimation> animations) {
			_animations = animations;
		}

		/// <summary>
		/// Sets the Current Animation by name
		/// </summary>
		/// <param name="animationName">The animation's name</param>
		private void SetAnimation(string animationName) {
			StatePropertiesAnimation animation = _animations.First(item => item.Name == animationName);

			if (animation == null) {
				Debug.LogWarning(" Animation \"" + animationName + "\" Does Not Exist for State " + Name);
				return;
			}

			SetAnimation(animation);
		}

		/// <summary>
		/// Sets the Current Animation by object
		/// </summary>
		/// <param name="animation">The Animation Object</param>
		private void SetAnimation(StatePropertiesAnimation animation) {
			int lastSpriteFrame = ActiveAnimationFrame?.SpriteFrame ?? 0;

			_currentAnimation = animation;

			if (animation.StartOnExistingSpriteFrame) {
				float startTime = 0;
				foreach (StatePropertiesAnimationFrame animationFrame in _currentAnimation.AnimationFrames) {
					startTime += animationFrame.Duration;
					if (animationFrame.SpriteFrame == lastSpriteFrame) {
						break;
					}
				}

				_timeInCurrentAnimation = startTime;
			} else {
				_timeInCurrentAnimation = 0;
			}
		}

		/// <summary>
		/// Sets the current Animation Frame image to the Actor's Sprite Renderer
		/// </summary>
		private void SetAnimationFrame() {
			if (_currentAnimation == null || Math.Abs(_currentAnimation.TotalDuration) < 0.001f) {
				return;
			}

			float timeSyncValue = _currentAnimation.BoundToGlobalTimeManager
				? GlobalTimeManager.TotalTime
				: _timeInCurrentAnimation;

			float totalCycles = timeSyncValue / _currentAnimation.TotalDuration;
			float progressionIntoCycle = totalCycles - Mathf.Floor(totalCycles);
			float currentCycleFrame = progressionIntoCycle * _currentAnimation.TotalDuration;
			bool reachedEndOfFixedLoops =
				!_currentAnimation.IndefinitelyLoops && totalCycles > _currentAnimation.FixedLoopCount;
			int destinationFrame = 0;

			if (reachedEndOfFixedLoops) {
				destinationFrame = _currentAnimation.FinalFrameAfterLoops;
			} else {
				if (_currentAnimation.AnimationFrames.Count == 1) {
					destinationFrame = 0;
				} else {
					float frameMax = 0;

					if (_currentAnimation.BoundToWeaponDuration) {
						WeaponBehavior weapon = Actor.GetWeaponBehavior(_currentAnimation.WeaponBoundTo);
						if (weapon != null) {
							float percentComplete = weapon.GetPercentageComplete();
							float timestamp = _currentAnimation.TotalDuration * percentComplete;

							for (int i = 0; i < _currentAnimation.AnimationFrames.Count; i++) {
								frameMax += _currentAnimation.AnimationFrames[i].Duration;

								if (timestamp > frameMax) {
									continue;
								}

								destinationFrame = i;
								break;
							}
						}
					} else {
						for (int i = 0; i < _currentAnimation.AnimationFrames.Count; i++) {
							frameMax += _currentAnimation.AnimationFrames[i].Duration;

							if (currentCycleFrame > frameMax) {
								continue;
							}

							destinationFrame = i;
							break;
						}
					}
				}
			}

			ActiveAnimationFrame = _currentAnimation.AnimationFrames[destinationFrame];

			for (int i = 0; i < ActiveAnimationFrame.BoundingBoxProperties.Count; i++) {
				BoxCollider2D boundingBoxCollider = Actor.BoundingBoxColliders[i];
				BoundingBoxProperties boundingBoxProperties = ActiveAnimationFrame.BoundingBoxProperties[i];
				boundingBoxCollider.enabled = boundingBoxProperties.Enabled;
				Actor.ActorProperties.BoundingBoxes[i] = ActiveAnimationFrame.BoundingBoxProperties[i];
				boundingBoxCollider.size = new Vector2(
					BoomerangUtils.RoundToPixelPerfection(boundingBoxProperties.Size.x / GameProperties.PixelsPerUnit),
					BoomerangUtils.RoundToPixelPerfection(boundingBoxProperties.Size.y / GameProperties.PixelsPerUnit)
				);
				boundingBoxCollider.offset = new Vector2(
					BoomerangUtils.RoundToPixelPerfection(boundingBoxProperties.Offset.x /
					                                      GameProperties.PixelsPerUnit),
					BoomerangUtils.RoundToPixelPerfection(boundingBoxProperties.Offset.y / GameProperties.PixelsPerUnit)
				);
			}


			foreach (ActorParticleEffectProperties particleEffectProperties in ActiveAnimationFrame.ParticleEffectProperties) {
				ActorParticleEffectProperties frame = particleEffectProperties;
				GameObject particleEffect = Actor.ParticleEffects[frame.Name];
				particleEffect.transform.localPosition = frame.DefaultOffsetPosition;
				if (frame.Enabled) {
					if (!particleEffect.GetComponent<ParticleSystem>().isPlaying) {
						particleEffect.GetComponent<ParticleSystem>().Play();
					}
				} else {
					particleEffect.GetComponent<ParticleSystem>().Stop();
				}
			}

			if (ActiveAnimationFrame.SpriteFrame < Actor.Sprites.Length) {
				Actor.SpriteRenderer.sprite = Actor.Sprites[ActiveAnimationFrame.SpriteFrame];
				Actor.SpriteRenderer.flipX = ActiveAnimationFrame.FlipHorizontal;
				Actor.SpriteRenderer.flipY = ActiveAnimationFrame.FlipVertical;
				Actor.SpriteRenderer.transform.rotation = Quaternion.Euler(0, 0, ActiveAnimationFrame.Rotate ? 90 : 0);
			}
		}

		/// <summary>
		/// Processes the State Entry Events
		/// </summary>
		/// <remarks>
		/// Called as part of OnEnterState
		/// </remarks>
		private void ProcessStateEntryActorEvents() {
			foreach (StateEntryExitEventProperties stateActorEvent in _entryEvents) {
				if (stateActorEvent.ActorEvents.Count == 0 && stateActorEvent.ActorElseEvents.Count == 0) {
					continue;
				}

				bool allTriggersMet = true;
				bool usesTriggers = stateActorEvent.Triggers.Count > 0;

				if (usesTriggers) {
					foreach (ActorTrigger trigger in stateActorEvent.Triggers) {
						if (!trigger.IsTriggered(Actor, this)) {
							allTriggersMet = false;
						}
					}
				}

				if (allTriggersMet) {
					foreach (ActorEvent actorEvent in stateActorEvent.ActorEvents) {
						ActorEvent actorEventLocal = actorEvent;
						Actor.QueuedActions.Add(
							GlobalTimeManager.PerformAfter(actorEventLocal.StartTime,
								() => { actorEventLocal.ApplyOutcome(Actor, Actor); })
						);
					}
				} else {
					foreach (ActorEvent actorEvent in stateActorEvent.ActorElseEvents) {
						ActorEvent actorEventLocal = actorEvent;
						Actor.QueuedActions.Add(
							GlobalTimeManager.PerformAfter(actorEventLocal.StartTime,
								() => { actorEventLocal.ApplyOutcome(Actor, Actor); })
						);
					}	
				}
			}
		}

		/// <summary>
		/// Processes the State Exits Events
		/// </summary>
		/// <remarks>
		/// Called as part of OnExitState
		/// </remarks>
		private void ProcessStateExitActorEvents() {
			foreach (StateEntryExitEventProperties stateActorEvent in _exitEvents) {
				if (stateActorEvent.ActorEvents.Count == 0 && stateActorEvent.ActorElseEvents.Count == 0) {
					continue;
				}

				bool allTriggersMet = true;
				bool usesTriggers = stateActorEvent.Triggers.Count > 0;

				if (usesTriggers) {
					foreach (ActorTrigger trigger in stateActorEvent.Triggers) {
						if (!trigger.IsTriggered(Actor, this)) {
							allTriggersMet = false;
						}
					}
				}

				if (allTriggersMet) {
					foreach (ActorEvent actorEvent in stateActorEvent.ActorEvents) {
						ActorEvent actorEventLocal = actorEvent;
						Actor.QueuedActions.Add(
							GlobalTimeManager.PerformAfter(actorEventLocal.StartTime,
								() => { actorEventLocal.ApplyOutcome(Actor, Actor); })
						);
					}
				} else {
					foreach (ActorEvent actorEvent in stateActorEvent.ActorElseEvents) {
						ActorEvent actorEventLocal = actorEvent;
						Actor.QueuedActions.Add(
							GlobalTimeManager.PerformAfter(actorEventLocal.StartTime,
								() => { actorEventLocal.ApplyOutcome(Actor, Actor); })
						);
					}		
				}
			}
		}
		
		/// <summary>
		/// Sets the Sound Effects Collection for this State
		/// </summary>
		/// <param name="soundEffects"></param>
		/// <remarks>
		/// Called in the initialization of Actors.
		/// </remarks>
		public void SetSoundEffects(List<StatePropertiesSoundEffect> soundEffects) {
			_soundEffects = soundEffects;
		}

		/// <summary>
		/// Called once when the Actor transitions into this State
		/// </summary>
		public override void OnEnterState() {
			base.OnEnterState();

			if (_animations != null && _animations.Count > 0) {
				foreach (StatePropertiesAnimation animation in _animations) {
					animation.CalculateTotalDuration();
				}

				SetAnimation(_animations[0].Name);
			}

			if (_soundEffects != null && _soundEffects.Count > 0) {
				foreach (StatePropertiesSoundEffect soundEffect in _soundEffects) {
					soundEffect.HasStarted = false;
				}
			}

			_loopingSoundEffects.Clear();

			if (StateProperties != null) {
				Actor.SetCanBeGrounded(((ActorStateProperties) StateProperties).CanBeGrounded);
				Actor.SetCollidesWithGeometry(((ActorStateProperties) StateProperties).CollidesWithGeometry);
				Actor.SetOverlapsGeometry(((ActorStateProperties) StateProperties).OverlapsGeometry);
				Actor.SetCollidesWithActors(((ActorStateProperties) StateProperties).CollidesWithActors);
				Actor.SetOverlapsOtherActors(((ActorStateProperties) StateProperties).OverlapsOtherActors);
				Actor.SetOverlapsWeapons(((ActorStateProperties) StateProperties).OverlapsWeapons);
			}

			ProcessStateEntryActorEvents();
		}

		/// <summary>
		/// Called once when the Actor transitions out of this state
		/// </summary>
		public override void OnExitState() {
			foreach (LoopingAudioEffect loopingSoundEffect in _loopingSoundEffects) {
				AudioManager.StopLoop(loopingSoundEffect);
			}
			
			ProcessStateExitActorEvents();
		}

		/// <summary>
		/// Called once at the start of every frame while the Actor is in this State
		/// </summary>
		public override void ProcessSetUpFrameState() {
			TimeInState += GlobalTimeManager.DeltaTime;

			if (_animations != null && _animations.Count > 0) {
				_timeInCurrentAnimation += GlobalTimeManager.DeltaTime;
				SetAnimationFrame();
			}

			foreach (StatePropertiesSoundEffect soundEffect in _soundEffects) {
				if (!soundEffect.HasStarted && TimeInState >= soundEffect.StartTime) {
					soundEffect.HasStarted = true;

					if (soundEffect.LoopEffect) {
						if (soundEffect.SoundEffectPool.Count > 0) {
							_loopingSoundEffects.Add(AudioManager.PlayLoop(soundEffect));
						}
					} else {
						if (soundEffect.PlayCount == 1) {
							AudioManager.PlayOnceFromPool(soundEffect.SoundEffectPool, soundEffect.RandomOrder,
								soundEffect.Volume);
						} else if (soundEffect.PlayCount > 1) {
							AudioManager.PlayXTimesFromPool(soundEffect.SoundEffectPool, soundEffect.PlayCount,
								soundEffect.RandomOrder,
								soundEffect.Volume);
						}
					}
				}
			}
		}
	}
}