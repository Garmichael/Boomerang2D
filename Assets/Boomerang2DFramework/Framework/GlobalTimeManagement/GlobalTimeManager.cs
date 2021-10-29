using System;
using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.GlobalTimeManagement {
	/// <summary>
	/// Boomerang 2D's Time Manager
	/// </summary>
	/// <remarks>
	/// Boomerang 2D is intended to run at 60 Frames Per Second, and time is measured in Frames. When measured in
	/// seconds, its done in 60 frames per Second, not in real-world second.
	/// </remarks>
	public class GlobalTimeManager : MonoBehaviour {
		/// <summary>
		/// The Total Time that has elapsed since the Game began running
		/// </summary>
		public static float TotalTime => (float) Time.frameCount / 60;
		
		/// <summary>
		/// The amount of time in seconds that has elapsed since the last frame
		/// </summary>
		public static float DeltaTime => 1f / 60f;

		public struct QueuedAction {
			public Action Action;
			public float TimeToExecute;
		}

		private static readonly List<QueuedAction> QueuedActions = new List<QueuedAction>();
		private static readonly List<int> CompletedActionIndexes = new List<int>();


		private static readonly Dictionary<string, TimeManagerIntervalAction> IntervalActions = new Dictionary<string, TimeManagerIntervalAction>();

		private void Update() {
			PerformQueuedActions();
			PerformIntervals();
		}

		/// <summary>
		/// Performs an Action after the given amount of time has elapsed
		/// </summary>
		/// <param name="timeFromNow">Time to perform in seconds</param>
		/// <param name="action">The Action to perform</param>
		public static QueuedAction PerformAfter(float timeFromNow, Action action) {
			if (timeFromNow < 0.0001f) {
				action();
				return new QueuedAction();
			}

			QueuedAction queuedAction = new QueuedAction {
				Action = action,
				TimeToExecute = TotalTime + timeFromNow
			};
				
			QueuedActions.Add(queuedAction);
			return queuedAction;
		}

		private static void PerformQueuedActions() {
			CompletedActionIndexes.Clear();

			for (int i = 0; i < QueuedActions.Count; i++) {
				QueuedAction queuedAction = QueuedActions[i];

				if (TotalTime >= queuedAction.TimeToExecute) {
					queuedAction.Action();
					CompletedActionIndexes.Add(i);
				}
			}

			for (int i = CompletedActionIndexes.Count - 1; i >= 0; i--) {
				QueuedActions.RemoveAt(CompletedActionIndexes[i]);
			}
		}

		public static bool QueuedActionExists(QueuedAction queuedAction) {
			return QueuedActions.Contains(queuedAction);
		}
		
		public static void CancelQueuedAction(QueuedAction queuedAction) {
			QueuedActions.Remove(queuedAction);
		}
		
		/// <summary>
		/// Performs an action repeatedly until told to stop
		/// </summary>
		/// <param name="interval">The amount of time in seconds to perform the Action</param>
		/// <param name="performFirstImmediately">When enabled, will perform the first instance immediately. When 
		/// disabled, will perform the first instance after interval time has elapsed</param>
		/// <param name="action">The Action to Perform</param>
		/// <returns>A string ID of this Interval which can be used to control it later</returns>
		public static string PerformInterval(float interval, bool performFirstImmediately, Action action) {
			string uniqueId = "intervalAction_" + BoomerangUtils.GenerateRandomString(2);

			while (IntervalActions.ContainsKey(uniqueId)) {
				uniqueId += BoomerangUtils.GenerateRandomString(1);
			}

			IntervalActions.Add(uniqueId, new TimeManagerIntervalAction {
				Action = action,
				Interval = interval
			});

			if (performFirstImmediately) {
				action();
			}

			return uniqueId;
		}

		/// <summary>
		/// Stops an Interval Action from executing
		/// </summary>
		/// <param name="uniqueId">The Id of the Interval to be stopped</param>
		public static void ClearInterval(string uniqueId) {
			if (IntervalActions.ContainsKey(uniqueId)) {
				IntervalActions.Remove(uniqueId);
			}
		}

		private static void PerformIntervals() {
			foreach (string key in IntervalActions.Keys.ToList()) {
				if (IntervalActions.ContainsKey(key)) {
					TimeManagerIntervalAction timeManagerIntervalAction = IntervalActions[key];

					timeManagerIntervalAction.Timer += DeltaTime;

					if (timeManagerIntervalAction.Timer >= timeManagerIntervalAction.Interval) {
						timeManagerIntervalAction.Action();
						timeManagerIntervalAction.Timer = 0;
					}
				}
			}
		}
	}
}