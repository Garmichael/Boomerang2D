namespace Boomerang2DFramework.Framework.UiManagement.UiElements {
	public enum MappedProperty {
		DialogContentId,
		SpeakerName,
		Text
	}

	public enum OriginCorner {
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

	public enum WordBreak {
		None,
		OnSpaces,
		OnSpecialCharacter,
		OnAnyCharacter
	}

	public enum HudElementContainer {
		DialogBox,
		Screen
	}

	public enum TextSource {
		StaticText,
		GameFlagFloat,
		GameFlagString,
		GameFlagBool,
		ContentDatabase
	}

	public enum ValueSource {
		StaticNumber,
		GameFlagFloat
	}
}