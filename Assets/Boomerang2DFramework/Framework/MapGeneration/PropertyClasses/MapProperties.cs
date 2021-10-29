using System.Collections.Generic;
using Boomerang2DFramework.Framework.GameEvents;
using UnityEngine;

namespace Boomerang2DFramework.Framework.MapGeneration.PropertyClasses {
	/// <summary>
	/// JSON definition for a map's main properties
	/// </summary>
	[System.Serializable]
	public class MapProperties {
		public string Name = "";
		public int Width;
		public int Height;
		public Vector2Int ChunkDimensions = new Vector2Int(10, 10);
		public List<MapLayerProperties> Layers = new List<MapLayerProperties>();
		public List<MapViewProperties> Views = new List<MapViewProperties>();
		public List<MapRegionProperties> Regions = new List<MapRegionProperties>();
		public List<GameEventBuilder> GameEventBuilders = new List<GameEventBuilder>();
	}
}