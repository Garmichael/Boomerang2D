using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.UiManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class DisplayHudObject : ActorEvent {
		private DisplayHudObjectProperties MyProperties => (DisplayHudObjectProperties) Properties;

		public DisplayHudObject(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}

			Dictionary<string, StringStatProperties> hashedStats =
				targetActor.ActorProperties.StatsStrings.ToDictionary(stat => stat.Name);

			string contentId = MyProperties.OptionalContentId;

			if (MyProperties.UsesStatForContentId && hashedStats.ContainsKey(MyProperties.StatName)) {
				contentId = hashedStats[MyProperties.StatName].Value;
			}

			UiManager.DisplayHudObject(MyProperties.HudObject, contentId);
		}
	}
}