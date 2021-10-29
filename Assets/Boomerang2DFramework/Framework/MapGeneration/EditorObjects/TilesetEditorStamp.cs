using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.MapGeneration.EditorObjects {
	[System.Serializable]
	public class TilesetEditorStamp {
		public string Name;
		public int Width;
		public int Height;
		public List<int> RepeatableRows = new List<int>();
		public List<int> RepeatableColumns = new List<int>();
		public List<string> TileRows = new List<string>();
		public List<List<int>> ParsedTileIds = new List<List<int>>();
	}
}