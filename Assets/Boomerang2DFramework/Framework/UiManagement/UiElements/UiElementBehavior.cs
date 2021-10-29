using Boomerang2DFramework.Framework.GameEvents;
using Boomerang2DFramework.Framework.GlobalTimeManagement;
using Boomerang2DFramework.Framework.UiManagement.InteractionEvents;
using Boomerang2DFramework.Framework.UiManagement.TriggerSystem;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements {
	/// <summary>
	/// The Base Class of the MonoBehavior that lives on a UiElement
	/// </summary>
	public class UiElementBehavior : MonoBehaviour {
		protected UiElementProperties Properties;
		protected Rect HudDimensions;
		protected string ContentId;
		protected GameObject HudObjectContainer;
		protected string HudObjectParent;
		protected HudObjectBehavior HudObjectBehavior;

		public virtual void Initialize(
			UiElementProperties properties,
			string hudObjectParent,
			GameObject hudObjectContainer,
			HudObjectBehavior hudObjectBehavior,
			Rect hudDimensions,
			string contentId = ""
		) {
			HudObjectParent = hudObjectParent;
			HudDimensions = hudDimensions;
			Properties = properties;
			ContentId = contentId;
			HudObjectContainer = hudObjectContainer;
			HudObjectBehavior = hudObjectBehavior;
		}

		public void SetActive(bool isActive) {
			Properties.IsActive = isActive;
		}

		public virtual void CleanUp() { }
	}
}