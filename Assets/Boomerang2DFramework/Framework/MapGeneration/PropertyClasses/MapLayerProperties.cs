using System.Collections.Generic;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace Boomerang2DFramework.Framework.MapGeneration.PropertyClasses {
	/// <summary>
	/// JSON definition for a map's layer properties
	/// </summary>
	[System.Serializable]
	public class MapLayerProperties {
		public string Name;
		public string Tileset;
		public string Shader = "Unlit/Transparent";
		public bool UseColliders = true;
		public List<string> TileRows = new List<string>();
		public List<MapActorPlacementProperties> Actors = new List<MapActorPlacementProperties>();
		public List<MapPrefabPlacementProperties> Prefabs = new List<MapPrefabPlacementProperties>();

		public bool EditorIsVisible = true;
		public bool EditorIsLocked;
		public List<TileEditorObject> TileEditorObjects = new List<TileEditorObject>();

		public MapLayerType MapLayerType = MapLayerType.Normal;
		public int DepthLayerStampId = 0;
		public Vector2Int DepthLayerStampDimensions = new Vector2Int(5,5);
		public DepthLayerOrigin DepthLayerOrigin = DepthLayerOrigin.Map;
		public DepthLayerOriginCorner DepthLayerOriginCorner = DepthLayerOriginCorner.BottomLeft;
		public bool DepthLayerRepeatX = true;
		public bool DepthLayerRepeatY = true;
		public DepthLayerScrollMode DepthLayerScrollMode = DepthLayerScrollMode.Parallax;
		public Vector2 DepthLayerScrollSpeed = Vector2.zero;
	}

	public enum MapLayerType {
		Normal,
		DepthLayer
	}

	public enum DepthLayerScrollMode {
		Parallax,
		Autoscroll
	}

	public enum DepthLayerOrigin {
		Map,
		View
	}

	public enum DepthLayerOriginCorner {
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight 
	}
}