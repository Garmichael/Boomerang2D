using System.Collections.Generic;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.TilesetStudio {
	public partial class TilesetStudio : EditorWindow {
		private static TilesetStudio _window;

		private float _repaintTime;
		private bool _repaintNext;
		private float _totalTime;
		private float _timeToCheckSave;
		private bool _fileHasChanged;

		private static float _windowWidth;
		private static float _windowHeight;
		private string _inputName;

		private enum Mode {
			Normal,
			Create,
			Rename,
			Delete,
			SelectFrame
		}

		private Mode _mainMode = Mode.Normal;

		private enum TileEditingMode {
			Tiles,
			Stamps,
			StampsSelectingTile,
			Brushes,
			BrushesSelectingTile
		}

		private TileEditingMode _tileEditingMode = TileEditingMode.Tiles;

		private bool _autoGenerateTilesOnImport = true;


		private readonly Dictionary<string, Collider2D[]> _allColliders = new Dictionary<string, Collider2D[]>();

		private List<string> AllColliderNames {
			get {
				List<string> colliderNames = new List<string>();
				foreach (KeyValuePair<string, Collider2D[]> colliderSet in _allColliders) {
					colliderNames.Add(colliderSet.Key);
				}

				return colliderNames;
			}
		}

		private readonly List<TilesetProperties> _allTilesets = new List<TilesetProperties>();
		private readonly List<List<Texture2D>> _allTilesetTextures = new List<List<Texture2D>>();

		private int _selectedTilesetIndex;
		private int _previousSelectedTilesetIndex;

		private List<string> AllTilesetNames {
			get {
				List<string> tilesetNames = new List<string>();
				foreach (TilesetProperties tileSetData in _allTilesets) {
					tilesetNames.Add(tileSetData.Name);
				}

				return tilesetNames;
			}
		}

		private TilesetProperties ActiveTileset {
			get {
				TilesetProperties activeTileset = null;

				if (_selectedTilesetIndex < _allTilesets.Count && _selectedTilesetIndex >= 0) {
					activeTileset = _allTilesets[_selectedTilesetIndex];
				}

				return activeTileset;
			}
		}

		private List<Texture2D> ActiveTilesetTextures => _allTilesetTextures[_selectedTilesetIndex];

		private int _selectFrameId;
		private int _selectedTileIndex;
		private int _selectedColliderShape;

		private TileProperties ActiveTile {
			get {
				TileProperties activeTile = null;

				if (_selectedTileIndex < ActiveTileset.Tiles.Count && _selectedTileIndex >= 0) {
					activeTile = ActiveTileset.Tiles[_selectedTileIndex];
				}

				return activeTile;
			}
		}

		private int _selectedStampIndex;

		private TilesetEditorStamp ActiveStamp {
			get {
				TilesetEditorStamp activeStamp = null;

				if (_selectedStampIndex < ActiveTileset.Stamps.Count && _selectedStampIndex >= 0) {
					activeStamp = ActiveTileset.Stamps[_selectedStampIndex];
				}

				return activeStamp;
			}
		}

		private int _selectedBrushIndex;

		private TilesetEditorBrush ActiveBrush {
			get {
				TilesetEditorBrush activeBrush = null;

				if (_selectedBrushIndex < ActiveTileset.Brushes.Count && _selectedBrushIndex >= 0) {
					activeBrush = ActiveTileset.Brushes[_selectedBrushIndex];
				}

				return activeBrush;
			}
		}

		private Rect _tileSelectorContainer;
		private int _selectTilePanelTileId;
		
		private readonly List<Texture2D> _brushModeAtlasTextures = new List<Texture2D>();
		private TileProperties _colliderPasteSource;
		private bool _autoPasteColliderInfo;
	}
}