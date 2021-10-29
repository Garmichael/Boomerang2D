using Boomerang2DFramework.Framework.Actors.ActorFloatValues;
using Boomerang2DFramework.Framework.Actors.TriggerSystem;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.States {
	[System.Serializable]
	public class SideViewJumpProperties : ActorStateProperties.ActorStateProperties {
		[Header("Jump Properties")] public ActorFloatValueConstructor JumpStrength = new ActorFloatValueConstructor {
			StartValue = 18f
		};

		public ActorFloatValueConstructor Gravity = new ActorFloatValueConstructor {
			StartValue = 1f
		};

		public bool SetInitialHorizontalJumpStrength;

		public ActorFloatValueConstructor HorizontalJumpStrength = new ActorFloatValueConstructor {
			StartValue = 0f
		};

		public bool MidAirChangeFacing = true;
		public bool AutoJumpForward;
		public bool AutoJumpBackward;

		public bool ForceFacingDirectionOnEntry;
		public Directions FacingDirectionOnEntry;

		[Header("Speed")] public bool AnalogSpeed;
		public ActorFloatValueConstructor MaxForwardSpeed = new ActorFloatValueConstructor {StartValue = 10f};
		public ActorFloatValueConstructor MaxBackwardSpeed = new ActorFloatValueConstructor {StartValue = 5f};
		public ActorFloatValueConstructor AccelerationForward = new ActorFloatValueConstructor {StartValue = 0.5f};
		public ActorFloatValueConstructor AccelerationBackward = new ActorFloatValueConstructor {StartValue = 0.5f};
		public ActorFloatValueConstructor DecelerationForward = new ActorFloatValueConstructor {StartValue = 0.05f};
		public ActorFloatValueConstructor DecelerationBackward = new ActorFloatValueConstructor {StartValue = 0.1f};


		[Header("Input")] public InputType MoveRightInputType = InputType.None;
		public string MoveRightInputValue = "Horizontal+";

		[Space] public InputType MoveLeftInputType = InputType.None;
		public string MoveLeftInputValue = "Horizontal-";
	}
}