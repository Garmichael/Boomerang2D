using Boomerang2DFramework.Framework.Actors.ActorFloatValues;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.States {
	[System.Serializable]
	public class MoveOrthoProperties : ActorStateProperties.ActorStateProperties {
		public enum WalkSnapEnum {
			FourWay,
			EightWay
		}

		[Header("Movement Properties")]
		public ActorFloatValueConstructor Acceleration = new ActorFloatValueConstructor {StartValue = 0.5f};

		public ActorFloatValueConstructor Deceleration = new ActorFloatValueConstructor {StartValue = 10f};
		public ActorFloatValueConstructor SkidDeceleration = new ActorFloatValueConstructor {StartValue = 10f};
		public ActorFloatValueConstructor MaxSpeed = new ActorFloatValueConstructor {StartValue = 10f};

		public WalkSnapEnum WalkSnap = WalkSnapEnum.EightWay;
		public bool AnalogSpeed;

		public bool UpdateFacingDirection = true;

		public bool ConstrictDiagonalSpeed = true;
		[Header("Facing Direction On Entry")]
		public bool ForceFacingDirectionOnEntry;

		public Directions FacingDirectionOnEntry;

		[Header("Slopes")]
		public float MaxClimbableSlopeAngle = 60f;

		public float GlueDistanceUp;
		public float GlueDistanceDown;
		public float GlueDistanceLeft;
		public float GlueDistanceRight;

		[Header("Move Without Input")]
		public bool AutoWalk;

		public bool MoveInFacingDirection;
		public Directions MoveDirection;

		[Header("Input")]
		public InputType MoveRightInputType = InputType.None;

		public string MoveRightInputValue = "";

		[Space]
		public InputType MoveLeftInputType = InputType.None;

		public string MoveLeftInputValue = "";

		[Space]
		public InputType MoveUpInputType = InputType.None;

		public string MoveUpInputValue = "";

		[Space]
		public InputType MoveDownInputType = InputType.None;

		public string MoveDownInputValue = "";
	}
}