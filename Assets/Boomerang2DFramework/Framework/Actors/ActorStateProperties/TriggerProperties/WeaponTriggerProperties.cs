using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorStateProperties.TriggerProperties {
	/// <summary>
	/// JSON definition for Weapon Triggers
	/// </summary>
	[System.Serializable]
	public class WeaponTriggerProperties {
		public string WeaponName;
		public Vector2 Offset = new Vector2(0, 0);
		public Vector2 Scale = new Vector2(1, 1);
		public float Rotation;
		public bool FlipHorizontal;
		public bool FlipVertical;
		public bool TriggersWhileActive;
		public List<ActorTriggerBuilder> ActorTriggerBuilders = new List<ActorTriggerBuilder>();
	}
}