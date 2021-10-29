using System;
using Boomerang2DFramework.Framework.UiManagement;
using Boomerang2DFramework.Framework.Utilities;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class DisplayHudObjectChain : ActorEvent {
		private DisplayHudObjectChainProperties MyProperties => (DisplayHudObjectChainProperties) Properties;

		public DisplayHudObjectChain(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (MyProperties.Chain.Count == 0) {
				return;
			}

			Action lastAction = null;

			for (int index = MyProperties.Chain.Count - 1; index >= 0; index--) {
				int localIndex = index;
				Action action = lastAction;

				lastAction = () => {
					string contentId = "";

					if (BoomerangUtils.IndexInRange(MyProperties.ContentIds, localIndex)) {
						contentId = MyProperties.ContentIds[localIndex];
					}

					UiManager.DisplayHudObject(MyProperties.Chain[localIndex], contentId, action);
				};
			}

			lastAction?.Invoke();
		}
	}
}