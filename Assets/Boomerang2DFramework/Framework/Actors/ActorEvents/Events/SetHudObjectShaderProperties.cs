namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class SetHudObjectShaderProperties : ActorEventProperties {
		public string HudObjectName;
		public string Shader;

		public SerializableFloatDictionary FloatsProperties;
		public SerializableIntDictionary IntsProperties;
		public SerializableColorDictionary ColorsProperties;
		public SerializableStringDictionary TexturesProperties;
	}
}