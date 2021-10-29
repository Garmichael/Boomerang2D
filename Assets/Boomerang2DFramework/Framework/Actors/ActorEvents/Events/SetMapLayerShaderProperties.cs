namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class SetMapLayerShaderProperties : ActorEventProperties {
		public string MapLayerName;
		public string Shader;
		
		public SerializableFloatDictionary FloatsProperties;
		public SerializableIntDictionary IntsProperties;
		public SerializableColorDictionary ColorsProperties;
		public SerializableStringDictionary TexturesProperties;
	}
}