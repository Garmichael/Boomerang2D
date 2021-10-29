using System;
using UnityEngine;

namespace Boomerang2DFramework.Framework.GameEvents {
	/// <summary>
	/// JSON definition for Game Event Builders
	///  It stores a reference to the specific GameEvent class, it's Property class, and a JSON of its properties.
	/// </summary>
	[Serializable]
	public class GameEventBuilder {
		public float StartTime;
		public string GameEventClass;
		public string GameEventPropertiesClass;
		public string GameEventProperties = "{}";
		
		/// <summary>
		/// Uses the JSON properties to build a GameEvent class and set its properties.
		/// </summary>
		/// <returns>GameEvent</returns>
		public GameEvent BuildGameEvent() {
			Type gameEventType = Type.GetType(GameEventClass);
			Type gameEventPropertiesType = Type.GetType(GameEventPropertiesClass);

			if (gameEventType == null) {
				Debug.LogWarning("GameEvent Class" + GameEventClass + " not found");
				return null;
			}

			if (gameEventPropertiesType == null) {
				Debug.LogWarning("Game Event Properties Type Not Found: " + GameEventPropertiesClass);
				return null;
			}

			GameEventProperties gameEventProperties = (GameEventProperties) JsonUtility.FromJson(GameEventProperties, gameEventPropertiesType);

			if (gameEventProperties == null) {
				Debug.LogWarning(
					"Properties JSON for GameEvent Properties " + GameEventProperties + " Failed to Parse"
				);
				return null;
			}

			GameEvent gameEventInstance = (GameEvent) Activator.CreateInstance(gameEventType, gameEventProperties);
			gameEventInstance.StartTime = StartTime;

			return gameEventInstance;
		}
	}
}