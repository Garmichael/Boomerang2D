using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using UnityEngine;

namespace Boomerang2DFramework.Framework.MapGeneration {
	/// <summary>
	/// Static class for building maps, managing map layers, and Accessing placed Map Actors
	/// </summary>
	public static class MapManager {
		/// <summary>
		/// The current map's properties
		/// </summary>
		public static MapProperties ActiveMapProperties;
		private static GameObject _currentMapContainer;
		private static MapBuilder _currentMapBuilder;

		private struct MapLayerData {
			public GameObject GameObject;
			public MapLayerBehavior MapLayerBehavior;
			public GameObject ActorContainer;
			public GameObject TileContainer;
		}

		/// <summary>
		/// A collection of Map Layers, hashed by their string Name
		/// </summary>
		private static readonly Dictionary<string, MapLayerData> MapLayersByName =
			new Dictionary<string, MapLayerData>();

		/// <summary>
		/// A collection of Map Layers, hashed by their GameObject
		/// </summary>
		public static readonly Dictionary<GameObject, TileBehavior> MapTilesByGameObject =
			new Dictionary<GameObject, TileBehavior>();

		/// <summary>
		/// A collection of all Actors currently loaded into the map
		/// </summary>
		private static readonly List<Actor> MapActorList = new List<Actor>();

		/// <summary>
		/// A collection of all Actors currently loaded into the map, hashed by the actor name
		/// </summary>
		private static readonly Dictionary<string, List<Actor>> MapActorsByName = new Dictionary<string, List<Actor>>();

		/// <summary>
		/// A collection of all Actors currently loaded into the map, hashed by the actor Id
		/// </summary>
		private static readonly Dictionary<string, List<Actor>>
			MapActorsByMapId = new Dictionary<string, List<Actor>>();

		/// <summary>
		/// A collection of all Actors currently loaded into the map, hashed by the Actor's Map Layer
		/// </summary>
		private static readonly Dictionary<string, List<Actor>> MapActorsByMapLayer =
			new Dictionary<string, List<Actor>>();

		/// <summary>
		/// A collection of all Actors currently loaded into the map, hashed by the Actor's MapLayerBehavior
		/// </summary>
		private static readonly Dictionary<MapLayerBehavior, List<Actor>> MapActorsByMapLayerBehavior =
			new Dictionary<MapLayerBehavior, List<Actor>>();

		/// <summary>
		/// A collection of all Actors currently loaded into the map, hashed by the actor's Game Object
		/// </summary>
		private static readonly Dictionary<GameObject, Actor> MapActorsByGameObject =
			new Dictionary<GameObject, Actor>();

		/// <summary>
		/// A collection of all Actors currently loaded into the map, hashed by the Actor's BoundingBox
		/// </summary>
		private static readonly Dictionary<BoundingBoxProperties, Actor> MapActorsByBoundingBox =
			new Dictionary<BoundingBoxProperties, Actor>();

		/// <summary>
		/// Loads a map
		/// </summary>
		/// <param name="mapName">The Name of the map as set in the Map Editor</param>
		public static void LoadMap(string mapName) {
			ClearOldMap();
			_currentMapBuilder = new MapBuilder();
			_currentMapContainer = _currentMapBuilder.BuildMap(mapName);
		}

		/// <summary>
		/// Clears all the collections of actors and discards old information, preparing it for a new map to be built
		/// </summary>
		private static void ClearOldMap() {
			_currentMapBuilder?.ClearMap();

			MapLayersByName.Clear();
			MapTilesByGameObject.Clear();

			foreach (Actor actor in MapActorList) {
				actor.Kill();
			}

			Boomerang2D.ActorManager.ClearPool();

			MapActorList.Clear();
			MapActorsByName.Clear();
			MapActorsByMapId.Clear();
			MapActorsByMapLayer.Clear();
			MapActorsByMapLayerBehavior.Clear();
			MapActorsByGameObject.Clear();
			MapActorsByBoundingBox.Clear();

			Boomerang2D.MainCameraController.ClearAllMapLayers();
			Boomerang2D.Player = null;

			if (_currentMapContainer) {
				Object.Destroy(_currentMapContainer);
			}
		}

		/// <summary>
		/// Assigns the map's properties. This must be set for the map to build properly.
		/// </summary>
		/// <param name="mapProperties">The Properties object to set</param>
		public static void SetActiveMapProperties(MapProperties mapProperties) {
			ActiveMapProperties = mapProperties;
		}

		/// <summary>
		/// Adds a map layer to the layer Catalog
		/// </summary>
		public static void AddMapLayerToCatalog(
			string layerName,
			GameObject mapLayer,
			MapLayerBehavior mapLayerBehavior
		) {
			MapLayersByName.Add(layerName, new MapLayerData {
				GameObject = mapLayer,
				MapLayerBehavior = mapLayerBehavior,
				ActorContainer = mapLayer.transform.Find("Actors").gameObject,
				TileContainer = mapLayer.transform.Find("Tiles").gameObject
			});
		}

		/// <summary>
		/// Adds a tile to the tile Catalog
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="tileBehavior"></param>
		public static void AddTileToCatalog(GameObject tile, TileBehavior tileBehavior) {
			MapTilesByGameObject.Add(tile, tileBehavior);
		}

		/// <summary>
		/// Adds an actor to the actor Catalogs
		/// </summary>
		/// <param name="actor"></param>
		public static void AddActorToCatalog(Actor actor) {
			if (!MapActorsByMapId.ContainsKey(actor.MapId)) {
				MapActorsByMapId.Add(actor.MapId, new List<Actor>());
			}

			if (!MapActorsByName.ContainsKey(actor.ActorProperties.Name)) {
				MapActorsByName.Add(actor.ActorProperties.Name, new List<Actor>());
			}

			if (!MapActorsByMapLayer.ContainsKey(actor.MapLayerName)) {
				MapActorsByMapLayer.Add(actor.MapLayerName, new List<Actor>());
			}

			if (!MapActorsByMapLayerBehavior.ContainsKey(actor.MapLayerBehavior)) {
				MapActorsByMapLayerBehavior.Add(actor.MapLayerBehavior, new List<Actor>());
			}

			MapActorList.Add(actor);
			MapActorsByName[actor.ActorProperties.Name].Add(actor);
			MapActorsByMapId[actor.MapId].Add(actor);
			MapActorsByMapLayer[actor.MapLayerName].Add(actor);
			MapActorsByMapLayerBehavior[actor.MapLayerBehavior].Add(actor);
			MapActorsByGameObject.Add(actor.GameObject, actor);

			foreach (BoundingBoxProperties actorPropertiesBoundingBox in actor.ActorProperties.BoundingBoxes) {
				MapActorsByBoundingBox.Add(actorPropertiesBoundingBox, actor);
			}
		}

		/// <summary>
		/// Destroys and Removes an actor from the actor Catalogs
		/// </summary>
		/// <param name="actor"></param>
		public static void RemoveActor(Actor actor) {
			RemoveActorFromCatalogs(actor);

			if (actor == Boomerang2D.Player) {
				Boomerang2D.Player = null;
			}
		}

		private static void RemoveActorFromCatalogs(Actor actor) {
			if (MapActorList.Contains(actor)) {
				MapActorList.Remove(actor);
				MapActorsByName[actor.ActorProperties.Name].Remove(actor);
				MapActorsByMapId[actor.MapId].Remove(actor);
				MapActorsByMapLayer[actor.MapLayerName].Remove(actor);
				MapActorsByMapLayerBehavior[actor.MapLayerBehavior].Remove(actor);
				MapActorsByGameObject.Remove(actor.GameObject);

				foreach (BoundingBoxProperties boundingBoxProperties in actor.ActorProperties.BoundingBoxes) {
					MapActorsByBoundingBox.Remove(boundingBoxProperties);
				}
			}
		}

		/// <summary>
		/// Gets the complete Actor Catalog
		/// </summary>
		/// <returns>The complete List of Actors</returns>
		public static List<Actor> GetActorCatalog() {
			return MapActorList;
		}

		/// <summary>
		/// Get an Actor by its Index in the Catalog
		/// </summary>
		/// <param name="index">Index of the Actor</param>
		/// <returns>The matching Actor</returns>
		public static Actor GetActorFromCatalogByIndex(int index) {
			return index >= MapActorList.Count || index < 0
				? MapActorList[index]
				: null;
		}

		/// <summary>
		/// Get an Actor by its GameObject
		/// </summary>
		/// <param name="gameObject">The Actor's Game Object</param>
		/// <returns>The matching Actor</returns>
		public static Actor GetActorFromCatalogByGameObject(GameObject gameObject) {
			return MapActorsByGameObject.ContainsKey(gameObject)
				? MapActorsByGameObject[gameObject]
				: null;
		}

		/// <summary>
		/// Get an Actor by its BoundingBox
		/// </summary>
		/// <param name="boundingBoxProperties">The Actor's Bounding Box</param>
		/// <returns>The matching Actor</returns>
		public static Actor GetActorFromCatalogByBoundingBox(BoundingBoxProperties boundingBoxProperties) {
			return MapActorsByBoundingBox.ContainsKey(boundingBoxProperties)
				? MapActorsByBoundingBox[boundingBoxProperties]
				: null;
		}

		/// <summary>
		/// Get all Actors by their Name as defined in the Actor Studio
		/// </summary>
		/// <param name="name">The name of the Actor</param>
		/// <returns>A List of all matching Actors</returns>
		public static List<Actor> GetActorsFromCatalogByName(string name) {
			return MapActorsByName.ContainsKey(name)
				? MapActorsByName[name]
				: null;
		}

		/// <summary>
		/// Get all Actors by their MapId
		/// </summary>
		/// <param name="mapId">The MapId as set in the Map Editor for an Actor</param>
		/// <returns>A List of all matching Actors</returns>
		public static List<Actor> GetActorsFromCatalogByMapId(string mapId) {
			return MapActorsByMapId.ContainsKey(mapId)
				? MapActorsByMapId[mapId]
				: null;
		}

		/// <summary>
		/// Get all Actors by their MapLayer
		/// </summary>
		/// <param name="mapLayer"></param>
		/// <returns>A List of all matching Actors</returns>
		public static List<Actor> GetActorsFromCatalogByMapLayer(string mapLayer) {
			return MapActorsByMapLayer.ContainsKey(mapLayer)
				? MapActorsByMapLayer[mapLayer]
				: null;
		}

		/// <summary>
		/// Get all Actors by their MapLayerBehavior
		/// </summary>
		/// <param name="mapLayerBehavior"></param>
		/// <returns>List of all matching Actors</returns>
		public static List<Actor> GetActorsFromCatalogByMapLayerBehavior(MapLayerBehavior mapLayerBehavior) {
			return MapActorsByMapLayerBehavior.ContainsKey(mapLayerBehavior)
				? MapActorsByMapLayerBehavior[mapLayerBehavior]
				: null;
		}

		/// <summary>
		/// Moves an Actor from one MapLayer to another
		/// </summary>
		/// <param name="actor">The Actor to move</param>
		/// <param name="newMapLayer">The new MapLayer to put the Actor on</param>
		public static void MoveActorToMapLayer(Actor actor, string newMapLayer) {
			RemoveActorFromCatalogs(actor);

			actor.SetMapLayerBehavior(MapLayersByName[newMapLayer].MapLayerBehavior);
			AddActorToCatalog(actor);

			GameObject newMapLayerContainer = MapLayersByName[newMapLayer].ActorContainer;
			Vector3 relativeLocation = actor.Transform.localPosition;
			actor.Transform.parent = newMapLayerContainer.transform;
			actor.Transform.localPosition = relativeLocation;
		}

		/// <summary>
		/// Get a MapLayerBehavior by the Layer Name as defined in the Map Editor
		/// </summary>
		/// <param name="mapLayerName">The name of the Map Layer</param>
		/// <returns>The MapLayerBehavior of the specified MapLayer</returns>
		public static MapLayerBehavior GetMapLayerBehaviorByName(string mapLayerName) {
			return MapLayersByName.ContainsKey(mapLayerName)
				? MapLayersByName[mapLayerName].MapLayerBehavior
				: null;
		}
	}
}