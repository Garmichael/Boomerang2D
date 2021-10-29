using Boomerang2DFramework.Framework.MapGeneration;
namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class SetMapLayerShader : GameEvent {
		private SetMapLayerShaderProperties MyProperties => (SetMapLayerShaderProperties) Properties;

		public SetMapLayerShader(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			MapLayerBehavior mapLayerBehavior = MapManager.GetMapLayerBehaviorByName(MyProperties.MapLayerName);
			if (mapLayerBehavior != null) {
				mapLayerBehavior.SetLayerShader(
					MyProperties.Shader,
					MyProperties.FloatsProperties.Dictionary,
					MyProperties.IntsProperties.Dictionary,
					MyProperties.ColorsProperties.Dictionary,
					MyProperties.TexturesProperties.Dictionary
				);
			}
		}
	}
}