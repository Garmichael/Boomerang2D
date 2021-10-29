using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SetStatFloat : ActorEvent {
		private SetStatFloatProperties MyProperties => (SetStatFloatProperties) Properties;

		public SetStatFloat(ActorEventProperties floatProperties) {
			Properties = floatProperties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null || sourceActor?.ActorProperties == null) {
				return;
			}
			
			Dictionary<string, FloatStatProperties> hashedStats =
				targetActor.ActorProperties.StatsFloats.ToDictionary(actorPropertiesStat => actorPropertiesStat.Name);

			if (hashedStats.ContainsKey(MyProperties.StatName)) {
				hashedStats[MyProperties.StatName].Value = MyProperties.NewValue.BuildValue(sourceActor);
			}
		}
	}
}