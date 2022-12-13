using Boomerang2DFramework.Framework.Actors.ActorFloatValues;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.States {
	[System.Serializable]
	public class SwoopProperties : ActorStateProperties.ActorStateProperties {
		[Header("End Point")] public ActorFloatValueConstructor X = new();
		public ActorFloatValueConstructor Y = new();

		[Header("Swoop")] public ActorFloatValueConstructor WidthPercent = new() {
			StartValue = 100,
		};

		[Header("Swoop")] public ActorFloatValueConstructor SpeedInPixels = new() {
			StartValue = 0.5f / GameProperties.PixelsPerUnit,
		};

		public EasingModeFormulas.EasingModes Easing = EasingModeFormulas.EasingModes.Linear;
	}
}