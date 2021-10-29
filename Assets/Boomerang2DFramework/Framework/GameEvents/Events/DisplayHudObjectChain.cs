using System;
using Boomerang2DFramework.Framework.UiManagement;
using Boomerang2DFramework.Framework.Utilities;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class DisplayHudObjectChain : GameEvent {
		private DisplayHudObjectChainProperties MyProperties => (DisplayHudObjectChainProperties) Properties;

		public DisplayHudObjectChain(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
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