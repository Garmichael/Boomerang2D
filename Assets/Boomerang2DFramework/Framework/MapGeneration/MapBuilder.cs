using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.Camera;
using Boomerang2DFramework.Framework.GameEvents;
using Boomerang2DFramework.Framework.GlobalTimeManagement;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using UnityEngine;

namespace Boomerang2DFramework.Framework.MapGeneration {
	/// <summary>
	/// Builds maps and all of the things that go in them
	/// </summary>
	public class MapBuilder {
		private GameObject _mapContainer;
		private readonly List<GameEvent> _gameEvents = new List<GameEvent>();

		private readonly List<GlobalTimeManager.QueuedAction> _queuedActions =
			new List<GlobalTimeManager.QueuedAction>();

		/// <summary>
		/// Builds the map by its name as set in the Map Editor
		/// </summary>
		/// <param name="mapName">The Map's Name</param>
		/// <exception cref="Exception">When trying to load a map that doesn't exist</exception>
		/// <exception cref="Exception">When the map data is malformed</exception>
		/// <returns>The container GameObject that holds the map</returns>
		public GameObject BuildMap(string mapName) {
			if (!BoomerangDatabase.MapDatabaseEntries.ContainsKey(mapName)) {
				throw new Exception("Tried to load a map that doesnt exist: " + mapName);
			}

			TextAsset json = BoomerangDatabase.MapDatabaseEntries[mapName];

			if (json == null) {
				throw new Exception("Invalid Map Data: " + mapName);
			}

			MapProperties mapProperties = JsonUtility.FromJson<MapProperties>(json.text);

			BuildMapContainer(mapName);
			MapManager.SetActiveMapProperties(mapProperties);
			BuildMapViews(mapProperties.Views);
			BuildMapRegions(mapProperties.Regions);
			BuildMapLayers(mapProperties.Layers);
			PositionCamera();
			ExecuteInitialGameEvents(mapProperties.GameEventBuilders);

			return _mapContainer;
		}

		public void ClearMap() {
			foreach (GlobalTimeManager.QueuedAction queuedAction in _queuedActions) {
				GlobalTimeManager.CancelQueuedAction(queuedAction);
			}
			
			_queuedActions.Clear();
		}
		
		private void BuildMapContainer(string mapName) {
			_mapContainer = new GameObject {name = "MapContainer (" + mapName + ")"};
			_mapContainer.transform.localPosition = new Vector3(0, 0, 0);

			GameObject mapContainerGroup = new GameObject {name = "Views"};
			mapContainerGroup.transform.localPosition = new Vector3(0, 0, 0);
			mapContainerGroup.transform.parent = _mapContainer.transform;

			mapContainerGroup = new GameObject {name = "Regions"};
			mapContainerGroup.transform.localPosition = new Vector3(0, 0, 0);
			mapContainerGroup.transform.parent = _mapContainer.transform;

			mapContainerGroup = new GameObject {name = "Layers"};
			mapContainerGroup.transform.localPosition = new Vector3(0, 0, 0);
			mapContainerGroup.transform.parent = _mapContainer.transform;
		}

		private void BuildMapViews(IEnumerable<MapViewProperties> mapViews) {
			Transform parentTransform = _mapContainer.transform.Find("Views").transform;
			int viewCount = 0;

			foreach (MapViewProperties mapViewProperties in mapViews) {
				float x = mapViewProperties.Position.x / GameProperties.PixelsPerUnit;
				float y = mapViewProperties.Position.y / GameProperties.PixelsPerUnit;
				float width = mapViewProperties.Dimensions.x / GameProperties.PixelsPerUnit;
				float height = mapViewProperties.Dimensions.y / GameProperties.PixelsPerUnit;

				GameObject newView = GameObject.CreatePrimitive(PrimitiveType.Cube);
				newView.name = "View_" + viewCount;
				newView.transform.parent = parentTransform;
				newView.transform.localScale = new Vector3(width, height, 200);
				newView.transform.localPosition = new Vector3(
					x + width / 2f,
					MapManager.ActiveMapProperties.Height - (y + height / 2f),
					0f
				);

				MapViewBehavior newMapViewBehavior = newView.AddComponent<MapViewBehavior>();
				newMapViewBehavior.DefaultCameraState = BuildDefaultCameraState(mapViewProperties);
				newMapViewBehavior.SetProperties(mapViewProperties);
				viewCount++;
			}
		}

		private void BuildMapRegions(IEnumerable<MapRegionProperties> mapRegions) {
			Transform parentTransform = _mapContainer.transform.Find("Regions").transform;

			foreach (MapRegionProperties mapRegion in mapRegions) {
				float x = mapRegion.Position.x / GameProperties.PixelsPerUnit;
				float y = mapRegion.Position.y / GameProperties.PixelsPerUnit;
				float width = mapRegion.Dimensions.x / GameProperties.PixelsPerUnit;
				float height = mapRegion.Dimensions.y / GameProperties.PixelsPerUnit;

				GameObject newRegion = new GameObject {name = "Region_" + mapRegion.Name};
				newRegion.transform.parent = parentTransform;
				newRegion.transform.localScale = new Vector3(width, height, 200);
				newRegion.transform.localPosition = new Vector3(
					x + width / 2f,
					MapManager.ActiveMapProperties.Height - (y + height / 2f),
					0f
				);

				MapRegionBehavior newMapRegionBehavior = newRegion.AddComponent<MapRegionBehavior>();
				newMapRegionBehavior.Initialize(mapRegion);
			}
		}

		private CameraState BuildDefaultCameraState(MapViewProperties mapViewProperties) {
			Type stateType = Type.GetType(mapViewProperties.CameraBehaviorClass);
			Type propertiesType = Type.GetType(mapViewProperties.CameraBehaviorPropertiesClass);

			if (stateType == null || propertiesType == null) {
				Debug.LogWarning("State Class " + mapViewProperties.CameraBehaviorClass + " or Properties not found");
				return null;
			}

			object stateProperties = JsonUtility.FromJson(mapViewProperties.CameraBehaviorProperties, propertiesType);
			CameraState cameraState = (CameraState) Activator.CreateInstance(stateType, stateProperties);
			cameraState.Name = "Default";

			return cameraState;
		}

		private void BuildMapLayers(IEnumerable<MapLayerProperties> mapLayers) {
			int layerCount = 0;

			foreach (MapLayerProperties layerProperties in mapLayers) {
				GameObject layerContainer = new GameObject(layerProperties.Name);
				layerContainer.transform.parent = _mapContainer.transform.Find("Layers").transform;
				layerContainer.transform.localPosition = new Vector3(0, 0, layerCount * -GameProperties.MapLayerDepth);

				if (layerProperties.MapLayerType == MapLayerType.Normal) {
					MapNormalLayerBehavior layerBehavior = layerContainer.AddComponent<MapNormalLayerBehavior>();
					layerBehavior.Initialize(layerProperties);
					MapManager.AddMapLayerToCatalog(layerProperties.Name, layerContainer, layerBehavior);
					layerCount++;
				} else if (layerProperties.MapLayerType == MapLayerType.DepthLayer) {
					MapDepthLayerBehavior layerBehavior = layerContainer.AddComponent<MapDepthLayerBehavior>();
					layerBehavior.Initialize(layerProperties);
					layerCount++;
				}
			}
		}

		private void PositionCamera() {
			Actor player = Boomerang2D.Player;
			if (player == null) {
				Boomerang2D.MainCameraController.RealPosition = Vector3.zero;
				Boomerang2D.MainCameraController.DestinationPosition = Vector3.zero;
			} else {
				Boomerang2D.MainCameraController.Target = player;
				Boomerang2D.MainCameraController.RealPosition = player.RealPosition;
				Boomerang2D.MainCameraController.DestinationPosition = player.RealPosition;
			}
		}

		private void ExecuteInitialGameEvents(List<GameEventBuilder> gameEventBuilders) {
			_gameEvents.Clear();

			foreach (GameEventBuilder gameEventBuilder in gameEventBuilders) {
				_gameEvents.Add(gameEventBuilder.BuildGameEvent());
			}

			foreach (GameEvent gameEvent in _gameEvents) {
				_queuedActions.Add(
					GlobalTimeManager.PerformAfter(gameEvent.StartTime, () => { gameEvent.ApplyOutcome(); })
				);
			}
		}
	}
}