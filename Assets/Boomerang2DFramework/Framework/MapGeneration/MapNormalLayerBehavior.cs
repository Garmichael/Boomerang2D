using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.MapGeneration {
	public class MapNormalLayerBehavior : MapLayerBehavior {
		
		public void Initialize(MapLayerProperties properties) {
			Properties = properties;
			Name = properties.Name;
			TileChunkDimensions = MapManager.ActiveMapProperties.ChunkDimensions;

			BuildLayerTiles();
			BuildLayerActors();
			BuildLayerPrefabs();
			BuildCamera();
		}

		private void BuildLayerTiles() {
			GameObject tilesContainer = new GameObject("Tiles");
			tilesContainer.transform.parent = transform;
			tilesContainer.transform.localPosition = new Vector3(0, 0, 0);

			Tileset tileSet = new Tileset(Properties.Tileset);
			List<string> reversedTileRows = new List<string>(Properties.TileRows);
			float yPosition = 1f;

			foreach (string mapRow in reversedTileRows) {
				List<string> cells = mapRow.Split(',').ToList();
				float xPosition = 0f;
				foreach (string mapCel in cells) {
					GameObject newTile = tileSet.BuildTile(mapCel, Properties.UseColliders);

					if (newTile != null) {
						newTile.transform.parent = tilesContainer.transform;

						newTile.transform.localPosition = new Vector3(
							xPosition * (tileSet.TilesetProperties.TileSize / GameProperties.PixelsPerUnit),
							MapManager.ActiveMapProperties.Height - yPosition *
							(tileSet.TilesetProperties.TileSize / GameProperties.PixelsPerUnit),
							0
						);

						AddTileToChunkData(newTile);
					}

					xPosition++;
				}

				yPosition++;
			}

			PlaceTilesInChunks();
		}

		private void BuildLayerActors() {
			GameObject actorContainer = new GameObject("Actors");
			actorContainer.transform.parent = transform;
			actorContainer.transform.localPosition = new Vector3(0, 0, 0);

			foreach (MapActorPlacementProperties mapActorProperties in Properties.Actors) {
				if (!BoomerangDatabase.ActorJsonDatabaseEntries.ContainsKey(mapActorProperties.Actor)) {
					continue;
				}

				string json = BoomerangDatabase.ActorJsonDatabaseEntries[mapActorProperties.Actor].text;

				if (json == null) {
					continue;
				}

				ActorProperties actorProperties = JsonUtility.FromJson<ActorProperties>(json);

				Vector3 spawnPosition = new Vector3(
					mapActorProperties.Position.x * GameProperties.PixelSize +
					actorProperties.SpriteWidth * GameProperties.PixelSize / 2,
					MapManager.ActiveMapProperties.Height -
					(mapActorProperties.Position.y * GameProperties.PixelSize +
					 actorProperties.SpriteHeight * GameProperties.PixelSize / 2f),
					-0.5f
				);

				Actor actorScript = Boomerang2D.ActorManager.GetActorFromPool(
					actorProperties,
					mapActorProperties.ActorInstanceProperties,
					actorContainer.transform,
					spawnPosition,
					this,
					Name,
					mapActorProperties.MapId
				);

				for (int i = 0; i < actorScript.ActorProperties.StatsFloats.Count; i++) {
					if (i < mapActorProperties.ActorDefaultStatsFloats.Count &&
					    mapActorProperties.ActorDefaultStatsFloats[i].MapOverride) {
						actorScript.ActorProperties.StatsFloats[i].Value =
							mapActorProperties.ActorDefaultStatsFloats[i].InitialValue;
					}
				}

				for (int i = 0; i < actorScript.ActorProperties.StatsBools.Count; i++) {
					if (i < mapActorProperties.ActorDefaultStatsBools.Count &&
					    mapActorProperties.ActorDefaultStatsBools[i].MapOverride) {
						actorScript.ActorProperties.StatsBools[i].Value =
							mapActorProperties.ActorDefaultStatsBools[i].InitialValue;
					}
				}

				for (int i = 0; i < actorScript.ActorProperties.StatsStrings.Count; i++) {
					if (i < mapActorProperties.ActorDefaultStatsStrings.Count &&
					    mapActorProperties.ActorDefaultStatsStrings[i].MapOverride) {
						actorScript.ActorProperties.StatsStrings[i].Value =
							mapActorProperties.ActorDefaultStatsStrings[i].InitialValue;
					}
				}
			}
		}

		private void BuildLayerPrefabs() {
			GameObject prefabContainer = new GameObject("Prefabs");
			prefabContainer.transform.parent = transform;
			prefabContainer.transform.localPosition = new Vector3(0, 0, 0);

			foreach (MapPrefabPlacementProperties placementProperties in Properties.Prefabs) {
				if (!BoomerangDatabase.MapPrefabDatabaseEntries.ContainsKey(placementProperties.Prefab)) {
					continue;
				}

				GameObject prefabObject = BoomerangDatabase.MapPrefabDatabaseEntries[placementProperties.Prefab];
				GameObject instance = GameObject.Instantiate(prefabObject, new Vector3(0, 0, 0), Quaternion.identity);
				instance.transform.parent = prefabContainer.transform;

				Vector3 position = new Vector3(
					BoomerangUtils.RoundToPixelPerfection(
						placementProperties.Position.x / GameProperties.PixelsPerUnit
					),
					MapManager.ActiveMapProperties.Height - BoomerangUtils.RoundToPixelPerfection(
						placementProperties.Position.y / GameProperties.PixelsPerUnit
					),
					placementProperties.DistanceAwayFromCamera
				);
				instance.transform.localPosition = position;
			}
		}

		public override void UpdateLayerCameraPosition() {
			if (!Boomerang2D.MainCameraController.CurrentView) {
				return;
			}

			Vector3 localPosition = Boomerang2D.MainCameraController.transform.localPosition;
			Vector3 layerCameraNewPosition = new Vector3(
				localPosition.x,
				localPosition.y,
				LayerCameraContainer.transform.localPosition.z
			);

			float verticalExtent = Boomerang2D.MainCameraController.OrthographicSize;
			float horizontalExtent = verticalExtent * Boomerang2D.MainCameraController.Aspect;

			layerCameraNewPosition.x = BoomerangUtils.MinValue(layerCameraNewPosition.x, horizontalExtent);
			layerCameraNewPosition.y = BoomerangUtils.MinValue(layerCameraNewPosition.y, verticalExtent);

			LayerCameraContainer.transform.localPosition = layerCameraNewPosition;
			LayerCameraContainer.transform.rotation = Boomerang2D.MainCameraController.transform.rotation;

			foreach (KeyValuePair<Vector2, ChunkData> tileChunk in Chunks) {
				Vector3 tileChunkPosition = tileChunk.Value.ChunkContainer.transform.localPosition;
				Vector2 cameraVisibleTileCount = LayerCameraController.VisibleTileCount;

				float leftSideOfChunk = tileChunkPosition.x;
				float rightSideOfChunk = tileChunkPosition.x + TileChunkDimensions.x;
				float bottomSideOfChunk = tileChunkPosition.y;
				float topSideOfChunk = tileChunkPosition.y + TileChunkDimensions.y;

				float leftSideOfCamera = layerCameraNewPosition.x - cameraVisibleTileCount.x;
				float rightSideOfCamera = layerCameraNewPosition.x + cameraVisibleTileCount.x;
				float bottomSideOfCamera = layerCameraNewPosition.y - cameraVisibleTileCount.y;
				float topSideOfCamera = layerCameraNewPosition.y + cameraVisibleTileCount.y;

				bool chunkIsActive =
					rightSideOfChunk >= leftSideOfCamera &&
					leftSideOfChunk <= rightSideOfCamera &&
					topSideOfChunk >= bottomSideOfCamera &&
					bottomSideOfChunk <= topSideOfCamera;

				tileChunk.Value.ChunkContainer.SetActive(chunkIsActive);
			}
		}
	}
}