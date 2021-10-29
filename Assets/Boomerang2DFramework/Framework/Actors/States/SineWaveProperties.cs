using Boomerang2DFramework.Framework.Actors.ActorFloatValues;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.States {
	[System.Serializable]
	public class SineWaveProperties : ActorStateProperties.ActorStateProperties {
		[Header("Movement Properties")]
		public ActorFloatValueConstructor Amplitude = new ActorFloatValueConstructor {StartValue = 2f};
		public ActorFloatValueConstructor Period = new ActorFloatValueConstructor {StartValue = 2f};
		public ActorFloatValueConstructor Speed = new ActorFloatValueConstructor {StartValue = 1f};

		[Header("Facing Direction On Entry")]
		public bool ForceFacingDirectionOnEntry;
		public Directions FacingDirectionOnEntry;
	}
}