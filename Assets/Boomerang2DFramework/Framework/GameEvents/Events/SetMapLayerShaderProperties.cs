namespace Boomerang2DFramework.Framework.GameEvents.Events {
	[System.Serializable]
	public class SetMapLayerShaderProperties : GameEventProperties {
		public string MapLayerName;
		public string Shader;

		public SerializableFloatDictionary FloatsProperties;
		public SerializableIntDictionary IntsProperties;
		public SerializableColorDictionary ColorsProperties;
		public SerializableStringDictionary TexturesProperties;
	}
}