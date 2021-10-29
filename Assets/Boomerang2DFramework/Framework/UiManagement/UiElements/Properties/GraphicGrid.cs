using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements.Properties {
	[System.Serializable]
	public class GraphicGrid : UiElementProperties {
		public Vector2Int Position;
		public OriginCorner OriginCorner;

		public ValueSource MaxValueSource;
		public int StaticMaxValue = 1;
		public string MaxValueGameFlag;

		public ValueSource CurrentValueSource;
		public int StaticCurrentValue = 1;
		public string CurrentValueGameFlag;

		public int ItemsPerRow = 1;

		public string Tileset;
		public int FilledStampIndex;
		public int FilledActiveStampIndex;
		public bool UsesEmptyStamp;
		public int EmptyStampIndex;
		public int EmptyActiveStampIndex;

		public Vector2Int StampSize;
		public Vector2Int Spacing;

		public int PreviewMaxCount;
		public int PreviewCurrentCount;
	}
}