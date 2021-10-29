using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.MapGeneration.EditorObjects {
	[System.Serializable]
	public class TilesetEditorBrush {
		public string Name;
		public string Definitions;
		public Dictionary<int, int> ParsedDefinitions;
	}
}