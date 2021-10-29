using System.Collections.Generic;
using Boomerang2DFramework.Framework.UiManagement.TriggerSystem;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements.Properties {
	[System.Serializable]
	public class ChoiceBox : UiElementProperties {
		public Vector2Int Position;
		public OriginCorner OriginCorner;

		public bool UsesContentId;
		public string ContentId;
		public List<string> Choices;

		public int ActiveIndex;
		
		public string UnselectedFont = "";
		public string SelectedFont = "";
		
		public int ItemSpacing = 1;
		public int LetterKerning = 1;

		public int ItemWidth = 6;

		public int BackgroundPaddingTop;
		public int BackgroundPaddingRight;
		public int BackgroundPaddingBottom;
		public int BackgroundPaddingLeft;

		public int ItemPaddingTop;
		public int ItemPaddingRight;
		public int ItemPaddingLeft;
		
		public string Tileset = "";
		public int BackgroundStampIndex;

		public int UnselectedStampIndex;
		public int SelectedStampIndex;

		public int PreviewChoiceCount;

		public List<HudObjectTriggerBuilder> NextItemTriggerBuilders;
		public List<HudObjectTriggerBuilder> PreviousItemTriggerBuilders;
		public List<HudObjectTriggerBuilder> MakeSelectionTriggerBuilders;
		public string SoundEffectOnItemChange = "";
		public string SoundEffectOnItemSelect = "";
		public bool AllowLooping;
	}
}