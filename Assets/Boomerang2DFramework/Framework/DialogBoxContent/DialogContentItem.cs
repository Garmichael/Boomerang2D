using System;
using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.DialogBoxContent {
	[Serializable]
	public class DialogContentItems {
		public List<DialogContentItem> ContentItems;
	}

	[Serializable]
	public class DialogContentItem {
		public string DialogContentId = "";
		public string SpeakerName = "";
		public string SpeakerPortraitTileset = "";
		public int SpeakerPortraitStampIndex;
		public string Text = "";
		public List<string> Choices = new List<string>();
	}
}