using Boomerang2DFramework.Framework.MapGeneration;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SetMapLayerShader : ActorEvent {
		private SetMapLayerShaderProperties MyProperties => (SetMapLayerShaderProperties) Properties;

		public SetMapLayerShader(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
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