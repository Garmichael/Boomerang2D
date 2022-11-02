using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.MapEditor {
	public partial class MapEditor : EditorWindow {
		private static MapEditor _window;
		private float _windowWidth;
		private float _windowHeight;

		private float _repaintTime;
		private bool _repaintNext;
		private float _totalTime;
		private float _timeToCheckSave;
		private bool _fileHasChanged;

		private string _inputName;

		private enum Mode {
			Normal,
			Create,
			Rename,
			Delete
		}

		private Mode _mainMode = Mode.Normal;

		private readonly List<float> _mapScales = new List<float> {0.25f, 0.5f, 1, 2, 3, 4, 5};
		private int _selectedMapScaleIndex = 2;

		private float MapEditingScale => _mapScales[_selectedMapScaleIndex];

		private float _lastZoomTime;
		private const float ZoomDelay = 0.25f;

		private enum InputMode {
			Selection,
			Pan
		}

		private InputMode _inputMode = InputMode.Selection;

		private struct TileSetData {
			public TilesetProperties Properties;
			public List<Texture2D> Textures;
			public List<Color32[]> PixelDatas;
			public List<Texture2D> StampTextures;
		}

		private readonly Dictionary<string, TileSetData> _allTileSets = new Dictionary<string, TileSetData>();

		private readonly Dictionary<string, ActorProperties> _allActorProperties = new Dictionary<string, ActorProperties>();
		private readonly Dictionary<string, Texture2D> _allActorTextures = new Dictionary<string, Texture2D>();

		private List<string> AllTileSetNames {
			get {
				List<string> tileSetNames = new List<string>();
				foreach (KeyValuePair<string, TileSetData> tileSet in _allTileSets) {
					tileSetNames.Add(tileSet.Key);
				}

				return tileSetNames;
			}
		}

		private List<string> AllActorNames {
			get {
				List<string> actorNames = new List<string>();
				foreach (KeyValuePair<string, ActorProperties> actor in _allActorProperties) {
					actorNames.Add(actor.Key);
				}

				return actorNames;
			}
		}

		private TileSetData ActiveTileset => _allTileSets[ActiveMapLayer.Tileset];
		private readonly List<MapProperties> _allMaps = new List<MapProperties>();
		private int _selectedMapIndex;
		private int _previousSelectedMapIndex;

		private List<string> AllMapNames {
			get {
				List<string> mapNames = new List<string>();
				foreach (MapProperties mapProperties in _allMaps) {
					mapNames.Add(mapProperties.Name);
				}

				return mapNames;
			}
		}

		private MapProperties ActiveMap => BoomerangUtils.IndexInRange(_allMaps, _selectedMapIndex)
			? _allMaps[_selectedMapIndex]
			: null;

		private int _mapWidthInput;
		private int _mapHeightInput;

		private enum ResizeStyle {
			TopLeft,
			TopMiddle,
			TopRight,
			Left,
			Middle,
			Right,
			BottomLeft,
			BottomMiddle,
			BottomRight
		}

		private bool _resizeMapMode;
		private ResizeStyle _resizeStyle;

		private int _selectedMapLayerIndex;

		private MapLayerProperties ActiveMapLayer => BoomerangUtils.IndexInRange(ActiveMap.Layers, _selectedMapLayerIndex)
			? ActiveMap.Layers[_selectedMapLayerIndex]
			: null;

		private bool _showMapLayerPropertiesPanel;
		private bool _renameMapLayerMode;
		private bool _addMapLayerMode;
		private int _addMapLayerTilesetIndex;
		private string _addMapLayerShader = "Unlit/Transparent";

		private MapLayerType _addMapLayerType;
		private int _addMapDepthLayerStampId;
		private Vector2Int _addMapDepthLayerStampDimensions;
		private DepthLayerOrigin _addMapDepthLayerOrigin;
		private DepthLayerOriginCorner _addMapDepthLayerOriginCorner;
		private bool _addMapDepthLayerRepeatX;
		private bool _addMapDepthLayerRepeatY;
		private DepthLayerScrollMode _addMapDepthLayerScrollMode;
		private Vector2 _addMapDepthLayerScrollSpeed = new Vector2(1, 1);
		
		private enum EditingMode {
			Properties,
			TilesTileSheet,
			TilesStamps,
			TilesBrushes,
			Prefabs,
			Actors,
			Views,
			Regions
		}

		private EditingMode _editingMode = EditingMode.Properties;

		private enum TileEditingMode {
			NoneSelected,
			PlacingTile,
			PlacingStamp,
			PaintingBrush
		}

		private TileEditingMode _tileEditingMode = TileEditingMode.NoneSelected;

		private Rect _mapEditingAreaRect;
		private Vector2 _mapEditingAreaScrollPosition;

		private Rect _tileSelectorContainer;
		private int _selectedTileIndex;
		private int _selectedTilesetStampIndex;

		private readonly float[] _tileSelectorScales = {0.1f, 0.25f, 0.5f, 0.75f, 1f, 2f, 4f, 8f};
		private int _tileSelectorScale = 4;

		private Vector2 _mapMousePosition;
		private List<TileEditorObject> _selectedTileEditorObjects = new List<TileEditorObject>();
		private readonly List<Vector2Int> _selectedObjectOffsets = new List<Vector2Int>();

		private bool _selectionObjectJustCloned;

		private bool _inputHasStartedScaling;
		private Vector2Int _scalingObjectOriginTopLeft;
		private Vector2Int _scalingObjectOriginalPosition;
		private Vector2Int _scalingObjectOriginalSize;
		private bool _scalingObjectWidthLocked;
		private bool _objectScalingWidthPositive;
		private bool _scalingObjectHeightLocked;
		private bool _objectScalingHeightPositive;

		private bool _objectIsScalingWidth;
		private bool _objectIsScalingHeight;

		private Vector2 _inputMouseDownPosition;
		private bool _inputSelectionBoxStarted;
		private TileEditorObject _inputHoveredTileEditorOnMouseDown;

		private enum BrushModes {
			Brush,
			Area
		}

		private enum ActorEditingMode {
			NoneSelected,
			PlacingActor
		}

		private ActorEditingMode _actorEditingMode = ActorEditingMode.NoneSelected;
		private int _selectedActorIndex = -1;
		private ActorProperties SelectedActorToPlace => _allActorProperties[AllActorNames[_selectedActorIndex]];
		private MapActorPlacementProperties _inputHoveredActorOnMouseDown;
		private List<MapActorPlacementProperties> _selectedActorObjects = new List<MapActorPlacementProperties>();
		private int _actorSnap = 16;

		private enum PrefabEditingMode {
			NoneSelected,
			PlacingPrefab
		}

		private PrefabEditingMode _prefabEditingMode = PrefabEditingMode.NoneSelected;
		private int _selectedPrefabIndex = -1;
		private MapPrefabPlacementProperties _inputHoveredPrefabOnMouseDown;
		private List<MapPrefabPlacementProperties> _selectedPrefabObjects = new List<MapPrefabPlacementProperties>();
		private int _prefabSnap = 16;

		private MapViewProperties _inputHoveredViewOnMouseDown;
		private List<MapViewProperties> _selectedMapViews = new List<MapViewProperties>();
		private float _viewInputSnapSize = 16;

		private MapRegionProperties _inputHoveredRegionOnMouseDown;
		private List<MapRegionProperties> _selectedMapRegions = new List<MapRegionProperties>();
		private int _indexForSelectedRegionActorEvent;

		private Texture2D _cachedSelectedStampBorderTexture;
		private Vector2Int _cachedSelectedStampBorderTextureDimensions;
		private BrushModes _brushMode = BrushModes.Brush;
		private TileEditorObject _brushEditorObjectBeingEdited;
		private Vector2Int _brushAreaStartLocation;
		private int _brushAreaMode;

		private Texture2D _mapEditingTransparentBackground;
		private Texture2D _viewBorderTexture;
		private Texture2D _missingTileGraphic;
		private Color32[] _missingTileGraphicPixelData;
		private Color32[] _transparentTileGraphicPixelData;
	}
}