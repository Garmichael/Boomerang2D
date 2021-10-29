using Boomerang2DFramework.Framework.Actors.ActorFloatValues;

namespace Boomerang2DFramework.Framework.Actors.States {
	[System.Serializable]
	public class IdleProperties : ActorStateProperties.ActorStateProperties {
		public ActorFloatValueConstructor Deceleration = new ActorFloatValueConstructor {
			StartValue = 1.5f
		};

		public ActorFloatValueConstructor GlueHeight = new ActorFloatValueConstructor {
			StartValue = 0f
		};
	}
}