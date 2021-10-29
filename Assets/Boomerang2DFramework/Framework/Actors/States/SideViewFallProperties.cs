using Boomerang2DFramework.Framework.Actors.ActorFloatValues;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.States {
	[System.Serializable]
	public class SideViewFallProperties : ActorStateProperties.ActorStateProperties {
		[Header("Fall Properties")]
		public float GlueDistance = 0.2f;

		public bool GlueOnlyOnDescent = true;
		public bool MidAirChangeFacing = true;
		public bool AutoForward;
		public bool AutoBackward;

		public ActorFloatValueConstructor FallAcceleration = new ActorFloatValueConstructor {
			StartValue = 1f
		};

		public ActorFloatValueConstructor TerminalVelocity = new ActorFloatValueConstructor {
			StartValue = 25f
		};

		[Header("Speed")] public bool AnalogSpeed;

		public ActorFloatValueConstructor MaxForwardSpeed = new ActorFloatValueConstructor {
			StartValue = 10f
		};

		public ActorFloatValueConstructor MaxBackwardSpeed = new ActorFloatValueConstructor {
			StartValue = 5f
		};

		public ActorFloatValueConstructor AccelerationForward = new ActorFloatValueConstructor {
			StartValue = 0.5f
		};

		public ActorFloatValueConstructor AccelerationBackward = new ActorFloatValueConstructor {
			StartValue = 0.5f
		};

		public ActorFloatValueConstructor DecelerationForward = new ActorFloatValueConstructor {
			StartValue = 0.05f
		};

		public ActorFloatValueConstructor DecelerationBackward = new ActorFloatValueConstructor {
			StartValue = 0.1f
		};

		[Header("Input")] public InputType MoveRightInputType = InputType.Axis;
		public string MoveRightInputValue = "Horizontal+";

		[Space] public InputType MoveLeftInputType = InputType.Axis;
		public string MoveLeftInputValue = "Horizontal-";
	}
}