using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SetStatString : ActorEvent {
		private SetStatStringProperties MyProperties => (SetStatStringProperties) Properties;

		public SetStatString(ActorEventProperties floatProperties) {
			Properties = floatProperties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			Dictionary<string, StringStatProperties> hashedStats =
				targetActor.ActorProperties.StatsStrings.ToDictionary(stat => stat.Name);

			if (hashedStats.ContainsKey(MyProperties.StatName)) {
				hashedStats[MyProperties.StatName].Value = MyProperties.NewValue;
			}
		}
	}
}