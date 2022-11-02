using System.Collections.Generic;
using UnityEngine;

namespace Boomerang2DFramework.Framework.MapGeneration.EditorObjects {
	[System.Serializable]
	public class TileEditorObject {
		public TileEditorObjectType TileEditorType;
		public int Id;
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public string BrushMapDefinition;
		public bool BrushTreatEdgeLikeSolid;
		public List<List<int>> ParsedBrushMapDefinition;
		public CachedTileEditorObjectInfo CachedEditorTextureInfo;
		[System.NonSerialized] public Texture2D CachedEditorTexture;
	}
}