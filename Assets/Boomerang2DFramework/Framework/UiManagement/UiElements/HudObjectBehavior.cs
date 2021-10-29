using System.Collections.Generic;
using Boomerang2DFramework.Framework.GameEvents;
using Boomerang2DFramework.Framework.GlobalTimeManagement;
using Boomerang2DFramework.Framework.UiManagement.InteractionEvents;
using Boomerang2DFramework.Framework.UiManagement.TriggerSystem;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements {
	/// <summary>
	/// The MonoBehavior for HudObjects
	/// </summary>
	public class HudObjectBehavior : MonoBehaviour {
		private List<HudObjectInteractionEvent> _interactionEvents = new List<HudObjectInteractionEvent>();
		public float TimeDisplayed;

		public void Start() {
			TimeDisplayed = 0;
		}

		public void Update() {
			TimeDisplayed += GlobalTimeManager.DeltaTime;
			ExecuteInteractionEvents();
		}

		public void Initialize(HudObjectProperties hudObjectProperties) {
			_interactionEvents = hudObjectProperties.InteractionEvents;
			BuildInteractionEvents();
		}

		private void BuildInteractionEvents() {
			foreach (HudObjectInteractionEvent interactionEvent in _interactionEvents) {
				interactionEvent.Triggers.Clear();
				foreach (HudObjectTriggerBuilder hudObjectTriggerBuilder in interactionEvent.TriggerBuilders) {
					interactionEvent.Triggers.Add(hudObjectTriggerBuilder.BuildTrigger());
				}

				interactionEvent.GameEvents.Clear();
				foreach (GameEventBuilder gameEventBuilder in interactionEvent.GameEventBuilders) {
					interactionEvent.GameEvents.Add(gameEventBuilder.BuildGameEvent());
				}
			}
		}

		private void ExecuteInteractionEvents() {
			foreach (HudObjectInteractionEvent interactionEvent in _interactionEvents) {
				if (interactionEvent.HasExecuted || !interactionEvent.Enabled) {
					continue;
				}

				bool allTriggersMet = true;

				foreach (HudObjectTrigger trigger in interactionEvent.Triggers) {
					if (!trigger.IsTriggered(this)) {
						allTriggersMet = false;
					}
				}

				if (allTriggersMet) {
					ExecuteInteractionEvent(interactionEvent);
				}
			}
		}

		private void ExecuteInteractionEvent(HudObjectInteractionEvent interactionEvent) {
			if (interactionEvent.HasExecuted || !interactionEvent.Enabled) {
				return;
			}

			float latestStartTime = 0;
			interactionEvent.HasExecuted = true;

			foreach (GameEvent gameEvent in interactionEvent.GameEvents) {
				if (gameEvent.StartTime > latestStartTime) {
					latestStartTime = gameEvent.StartTime;
				}

				if (!gameEvent.HasExecuted) {
					GameEvent gameEventLocal = gameEvent;
					gameEventLocal.HasExecuted = true;
					GlobalTimeManager.PerformAfter(gameEventLocal.StartTime, () => { gameEventLocal.ApplyOutcome(); });
				}
			}

			HudObjectInteractionEvent interactionEventLocal = interactionEvent;

			GlobalTimeManager.PerformAfter(latestStartTime, () => {
				if (!interactionEvent.FireOnce) {
					interactionEventLocal.HasExecuted = false;
					foreach (GameEvent gameEvent in interactionEventLocal.GameEvents) {
						gameEvent.HasExecuted = false;
					}
				}
			});
		}
	}
}