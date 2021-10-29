using Boomerang2DFramework.Framework.UiManagement;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class SetHudObjectShader : GameEvent {
		private SetHudObjectShaderProperties MyProperties => (SetHudObjectShaderProperties) Properties;

		public SetHudObjectShader(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
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