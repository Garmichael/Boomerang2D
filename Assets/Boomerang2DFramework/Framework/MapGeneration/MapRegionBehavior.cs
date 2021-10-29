using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors;
using Boomerang2DFramework.Framework.Actors.ActorEvents;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;
using Boomerang2DFramework.Framework.GlobalTimeManagement;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Boomerang2DFramework.Framework.MapGeneration {
	/// <summary>
	/// The MonoBehavior attached to MapRegions
	/// </summary>
	public class MapRegionBehavior : MonoBehaviour {
		private MapRegionProperties _properties;
		private readonly List<MapRegionActorEvent> _mapRegionActorEvents = new List<MapRegionActorEvent>();
		private readonly Dictionary<Collider2D, StayedInstances> _stayed = new Dictionary<Collider2D, StayedInstances>();

		private struct StayedInstances {
			public Actor Actor;
			public float LastPlayTime;
		}

		/// <summary>
		/// MapRegions must be initialized with a set of properties
		/// </summary>
		/// <param name="properties">The properties to set (usually from a parsed JSON)</param>
		public void Initialize(MapRegionProperties properties) {
			_properties = properties;

			foreach (MapRegionActorEvent mapRegionActorEvent in _properties.RegionActorEvents) {
				mapRegionActorEvent.Triggers.Clear();
				foreach (ActorTriggerBuilder actorTriggerBuilder in mapRegionActorEvent.ActorTriggerBuilders) {
					mapRegionActorEvent.Triggers.Add(actorTriggerBuilder.BuildTrigger());
				}

				mapRegionActorEvent.ActorEvents.Clear();
				foreach (ActorEventBuilder actorEventBuilder in mapRegionActorEvent.ActorEventBuilders) {
					mapRegionActorEvent.ActorEvents.Add(actorEventBuilder.BuildActorEvent());
				}

				_mapRegionActorEvents.Add(mapRegionActorEvent);
			}

			BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
			boxCollider.isTrigger = true;
		}

		private void OnTriggerEnter2D(Collider2D enteredCollider2D) {
			Actor actor = GetActor(enteredCollider2D);
			IEnumerable<string> flags = GetFlagsForCollider(actor, enteredCollider2D);

			bool hasSameElements = _properties.EnteringFlag.Intersect(flags).Any();

			if (!hasSameElements) {
				return;
			}

			if (_properties.FiresOnStay) {
				_stayed.Add(enteredCollider2D, new StayedInstances {
					Actor = actor,
					LastPlayTime = GlobalTimeManager.TotalTime - _properties.FireOnStayDelay
				});
			}

			if (_properties.FiresOnEnter) {
				foreach (MapRegionActorEvent mapRegionActorEvent in _mapRegionActorEvents) {
					bool triggerConditionsMet = MeetsTriggerCriteria(mapRegionActorEvent, actor);

					if (triggerConditionsMet || mapRegionActorEvent.Triggers.Count == 0) {
						foreach (ActorEvent actorEvent in mapRegionActorEvent.ActorEvents) {
							GlobalTimeManager.PerformAfter(actorEvent.StartTime, () => { actorEvent.ApplyOutcome(actor, actor); });
						}
					}
				}
			}
		}

		private void OnTriggerExit2D(Collider2D exitedCollider2D) {
			Actor actor = GetActor(exitedCollider2D);
			IEnumerable<string> flags = GetFlagsForCollider(actor, exitedCollider2D);
			bool hasSameElements = _properties.EnteringFlag.Intersect(flags).Any();

			if (!hasSameElements) {
				return;
			}

			_stayed.Remove(exitedCollider2D);

			if (_properties.FiresOnExit) {
				foreach (MapRegionActorEvent mapRegionActorEvent in _mapRegionActorEvents) {
					bool triggerConditionsMet = MeetsTriggerCriteria(mapRegionActorEvent, actor);

					if (triggerConditionsMet || mapRegionActorEvent.Triggers.Count == 0) {
						foreach (ActorEvent actorEvent in mapRegionActorEvent.ActorEvents) {
							GlobalTimeManager.PerformAfter(actorEvent.StartTime, () => { actorEvent.ApplyOutcome(actor, actor); });
						}
					}
				}
			}
		}

		private void Update() {
			PlayStayedActorEvents();
		}

		private void PlayStayedActorEvents() {
			List<Collider2D> updatePlayTimeList = new List<Collider2D>();

			foreach (KeyValuePair<Collider2D, StayedInstances> stayed in _stayed) {
				foreach (MapRegionActorEvent mapRegionActorEvent in _mapRegionActorEvents) {
					bool triggerConditionsMet = mapRegionActorEvent.Triggers.Count == 0 ||
					                            MeetsTriggerCriteria(mapRegionActorEvent, stayed.Value.Actor);

					bool isReadyToPlay = GlobalTimeManager.TotalTime - stayed.Value.LastPlayTime >= _properties.FireOnStayDelay;

					if (triggerConditionsMet && isReadyToPlay) {
						foreach (ActorEvent actorEvent in mapRegionActorEvent.ActorEvents) {
							GlobalTimeManager.PerformAfter(actorEvent.StartTime, () => { actorEvent.ApplyOutcome(stayed.Value.Actor, stayed.Value.Actor); });
						}

						updatePlayTimeList.Add(stayed.Key);
					}
				}
			}

			foreach (Collider2D key in updatePlayTimeList) {
				Actor actor = _stayed[key].Actor;

				_stayed[key] = new StayedInstances {
					Actor = actor,
					LastPlayTime = GlobalTimeManager.TotalTime
				};
			}
		}

		private bool MeetsTriggerCriteria(MapRegionActorEvent mapRegionActorEvent, Actor actor) {
			bool triggerConditionsMet = true;

			foreach (ActorTrigger trigger in mapRegionActorEvent.Triggers) {
				if (!trigger.IsTriggered(actor, actor.StateMachine.GetCurrentState())) {
					triggerConditionsMet = false;
				}
			}

			return triggerConditionsMet;
		}

		private static Actor GetActor(Component enteredCollider) {
			ActorBehavior otherActorBehavior = enteredCollider.gameObject.GetComponent<ActorBehavior>();

			return otherActorBehavior == null
				? null
				: otherActorBehavior.Actor;
		}

		private static IEnumerable<string> GetFlagsForCollider(Actor actor, Object enteredCollider) {
			if (actor == null) {
				return new List<string>();
			}

			List<BoxCollider2D> otherBoundingBoxes = actor.BoundingBoxColliders;
			BoundingBoxProperties otherBoundingBoxProperties = null;

			for (int i = 0; i < otherBoundingBoxes.Count; i++) {
				if (otherBoundingBoxes[i] == enteredCollider && actor.CurrentBoundingBoxProperties != null) {
					otherBoundingBoxProperties = actor.CurrentBoundingBoxProperties[i];
					break;
				}
			}

			return otherBoundingBoxProperties == null
				? new List<string>()
				: new List<string>(otherBoundingBoxProperties.Flags);
		}
	}
}