using System.Collections.Generic;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;

namespace Boomerang2DFramework.Framework.MapGeneration.PropertyClasses {
	/// <summary>
	/// JSON definition for a Tileset
	/// </summary>
	[System.Serializable]
	public class TilesetProperties {
		public string Name;
		public int TileSize = (int) GameProperties.PixelsPerUnit;
		public int EditorTilePerRow;
		public List<TileProperties> Tiles = new List<TileProperties>();
		public Dictionary<int, TileProperties> TilesLookup = new Dictionary<int, TileProperties>();

		public List<TilesetEditorStamp> Stamps = new List<TilesetEditorStamp>();
		public List<TilesetEditorBrush> Brushes = new List<TilesetEditorBrush>();

		public void PopulateLookupTable() {
			TilesLookup.Clear();

			foreach (TileProperties tile in Tiles) {
				if (!TilesLookup.ContainsKey(tile.Slot)) {
					TilesLookup.Add(tile.Slot, tile);
				}
			}
		}
	}
}