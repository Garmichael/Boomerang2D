using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.States {
	[System.Serializable]
	public class PointToPointProperties : ActorStateProperties.ActorStateProperties {
		public Vector2 EndPosition = Vector2.zero;
		public bool EndPointRelative = true;
		public float TimeToComplete = 1;
		public EasingModeFormulas.EasingModes EasingMode = EasingModeFormulas.EasingModes.Linear;
	}
}