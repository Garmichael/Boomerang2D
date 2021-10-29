using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements.Properties {
	[System.Serializable]
	public class SingleGraphic : UiElementProperties {
		public Vector2Int Position;
		public OriginCorner OriginCorner;
		
		public bool UsesPortraitContent;
		public string ContentId;
		
		public string Tileset = "";
		public int StampIndex;
		
		public Vector2Int StampSize;
		
		public string PreviewTileset = "";
		public int PreviewStampIndex;
		
	}
}