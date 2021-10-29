namespace Boomerang2DFramework.Framework.GameEvents.Events {
	[System.Serializable]
	public class SetHudObjectShaderProperties : GameEventProperties {
		public string HudObjectName;
		public string Shader;

		public SerializableFloatDictionary FloatsProperties;
		public SerializableIntDictionary IntsProperties;
		public SerializableColorDictionary ColorsProperties;
		public SerializableStringDictionary TexturesProperties;
	}
}