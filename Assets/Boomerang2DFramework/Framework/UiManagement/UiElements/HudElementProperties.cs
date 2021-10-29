namespace Boomerang2DFramework.Framework.UiManagement.UiElements {
	/// <summary>
	/// JSON definition for Elements on a HudObject
	/// It stores a reference to the specific Element Class, Behavior Class, EditorClass, and JSON properties
	/// </summary>
	[System.Serializable]
	public class HudElementProperties {
		public string ElementName;
		public string ElementPropertiesClass;
		public string ElementBehaviorClass;
		public string ElementEditorClass;
		public string Properties;
	}
}