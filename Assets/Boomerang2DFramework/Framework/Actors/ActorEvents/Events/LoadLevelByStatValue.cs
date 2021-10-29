using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.MapGeneration;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class LoadLevelByStatValue : ActorEvent {
		private LoadLevelByStatValueProperties MyProperties => (LoadLevelByStatValueProperties) Properties;

		public LoadLevelByStatValue(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			Dictionary<string, StringStatProperties> hashedStats =
				targetActor.ActorProperties.StatsStrings.ToDictionary(stat => stat.Name);

			if (hashedStats.ContainsKey(MyProperties.StatStringName)) {
				MapManager.LoadMap(hashedStats[MyProperties.StatStringName].Value);
			}
		}
	}
}