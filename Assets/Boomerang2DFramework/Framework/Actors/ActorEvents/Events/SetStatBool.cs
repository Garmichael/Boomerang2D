using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SetStatBool : ActorEvent {
		private SetStatBoolProperties MyProperties => (SetStatBoolProperties) Properties;

		public SetStatBool(ActorEventProperties floatProperties) {
			Properties = floatProperties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			Dictionary<string, BoolStatProperties> hashedStats = 
				targetActor.ActorProperties.StatsBools.ToDictionary(stat => stat.Name);

			if (hashedStats.ContainsKey(MyProperties.StatName)) {
				hashedStats[MyProperties.StatName].Value = MyProperties.NewValue;
			}
		}
	}
}