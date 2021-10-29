namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	[System.Serializable]
	public class SetSpriteShaderProperties : ActorEventProperties {
		public string ShaderName = "Sprites/Default";
		
		public SerializableFloatDictionary FloatsProperties;
		public SerializableIntDictionary IntsProperties;
		public SerializableColorDictionary ColorsProperties;
		public SerializableStringDictionary TexturesProperties;
	}
}