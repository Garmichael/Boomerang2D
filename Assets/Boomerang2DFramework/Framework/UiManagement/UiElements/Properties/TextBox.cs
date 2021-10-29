using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements.Properties {
	[System.Serializable]
	public class TextBox : UiElementProperties {
		public Vector2Int Position;
		public OriginCorner OriginCorner;

		public TextSource TextSource;
		public string ContentId;
		public MappedProperty MappedContentProperty = MappedProperty.Text;
		public string StaticText;
		public string GameFlag;

		public bool PrefixWithZeroes;
		public int FixedLengthOfText;

		public string Font = "";
		public string ActiveFont = "";
		public WordBreak WordBreak = WordBreak.None;
		public int RowSpacing = 1;
		public int LetterKerning = 1;

		public bool ScalesHorizontally;
		public int MinWidth = 6;
		public int MaxWidth = 6;

		public bool ScalesVertically;
		public int MinHeight = 3;
		public int MaxHeight = 3;

		public int PaddingTop;
		public int PaddingRight;
		public int PaddingBottom;
		public int PaddingLeft;
		
		public string Tileset = "";
		public int BackgroundStampIndex;
		public int ActiveBackgroundStampIndex;

		public bool UsesButtonPrompt;
		public bool OnlyShowButtonPromptOnActive;
		public Vector2Int ButtonPromptPosition;
		public Vector2Int ButtonPromptSize = new Vector2Int(1, 1);
		public string ButtonPromptTileset = "";
		public int ButtonPromptStampIndex;
		public OriginCorner ButtonPromptOriginCorner = OriginCorner.BottomRight;
		public HudElementContainer HudElementContainer;

		public int PreviewTextRowsToShow = 2;
	}
}