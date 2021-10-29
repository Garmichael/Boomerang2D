using Boomerang2DFramework.Framework.UiManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SetHudObjectShader : ActorEvent {
		private SetHudObjectShaderProperties MyProperties => (SetHudObjectShaderProperties) Properties;

		public SetHudObjectShader(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			UiManager.SetShader(
				MyProperties.HudObjectName,
				MyProperties.Shader,
				MyProperties.FloatsProperties.Dictionary,
				MyProperties.IntsProperties.Dictionary,
				MyProperties.ColorsProperties.Dictionary,
				MyProperties.TexturesProperties.Dictionary
			);
		}
	}
}