using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.Camera;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.MapGeneration {
	/// <summary>
	/// The MonoBehavior for MapLayers
	/// </summary>
	/// <remarks>
	/// This is responsible for managing its own layer camera position and has functionality for adjusting its
	/// properties, shaders, as well as some utility methods
	/// </remarks>
	public class MapLayerBehavior : MonoBehaviour {
		public string Name;
		protected GameObject LayerCameraContainer;
		protected LayerCameraController LayerCameraController;
		protected MapLayerProperties Properties;

		protected Vector2 TileChunkDimensions;
		protected Dictionary<Vector2, ChunkData> Chunks = new Dictionary<Vector2, ChunkData>();

		protected struct ChunkData {
			public int X;
			public int Y;
			public GameObject ChunkContainer;
			public List<GameObject> ChunkTiles;
		}

		public void SetLayerShader(
			string shader,
			Dictionary<string, float> floats = null,
			Dictionary<string, int> ints = null,
			Dictionary<string, Color> colors = null,
			Dictionary<string, Texture> textures = null
		) {
			LayerCameraController.SetShader(shader, floats, ints, colors, textures);
		}

		protected void BuildCamera() {
			LayerCameraContainer = new GameObject("LayerCamera");
			LayerCameraController = LayerCameraContainer.AddComponent<LayerCameraController>();
			LayerCameraController.RegisterMapLayerCamera(Name, this);
			LayerCameraController.SetShader(Properties.Shader);
			LayerCameraContainer.transform.parent = transform;
			LayerCameraContainer.transform.localPosition = new Vector3(0, 0, -1f);
		}

		public virtual void UpdateLayerCameraPosition() { }

		public virtual Dictionary<Directions, float> GetCameraEdges() {
			Vector2 position = LayerCameraContainer.transform.localPosition;
			float halfHeight = Boomerang2D.MainCameraController.OrthographicSize;
			float halfWidth = halfHeight * Boomerang2D.MainCameraController.Aspect;

			return new Dictionary<Directions, float> {
				{Directions.Up, BoomerangUtils.RoundToPixelPerfection(position.y + halfHeight)},
				{Directions.Right, BoomerangUtils.RoundToPixelPerfection(position.x + halfWidth)},
				{Directions.Down, BoomerangUtils.RoundToPixelPerfection(position.y - halfHeight)},
				{Directions.Left, BoomerangUtils.RoundToPixelPerfection(position.x - halfWidth)}
			};
		}

		public virtual bool PointIsInActiveChunk(Vector2 point) {
			Vector2 chunkPosition = new Vector2(Mathf.FloorToInt(point.x / TileChunkDimensions.x),
				Mathf.FloorToInt(point.y / TileChunkDimensions.y));
			Vector3 cameraPosition = LayerCameraContainer.transform.localPosition;
			Vector2 cameraDimensions = LayerCameraController.VisibleTileCount;

			float leftSideOfChunk = chunkPosition.x * TileChunkDimensions.x;
			float rightSideOfChunk = leftSideOfChunk + +TileChunkDimensions.x;
			float bottomSideOfChunk = chunkPosition.y * TileChunkDimensions.y;
			float topSideOfChunk = bottomSideOfChunk + TileChunkDimensions.y;

			float leftSideOfCamera = cameraPosition.x - cameraDimensions.x / 2f;
			float rightSideOfCamera = cameraPosition.x + cameraDimensions.x / 2f;
			float bottomSideOfCamera = cameraPosition.y - cameraDimensions.y / 2f;
			float topSideOfCamera = cameraPosition.y + cameraDimensions.y / 2f;

			return rightSideOfChunk >= leftSideOfCamera &&
			       leftSideOfChunk <= rightSideOfCamera &&
			       topSideOfChunk >= bottomSideOfCamera &&
			       bottomSideOfChunk <= topSideOfCamera;
		}
		
		protected void AddTileToChunkData(GameObject tile) {
			Vector3 localPosition = tile.transform.localPosition;
			int tileChunkX = Mathf.FloorToInt(localPosition.x / TileChunkDimensions.x);
			int tileChunkY = Mathf.FloorToInt(localPosition.y / TileChunkDimensions.y);

			if (!Chunks.ContainsKey(new Vector2(tileChunkX, tileChunkY))) {
				Chunks[new Vector2(tileChunkX, tileChunkY)] = new ChunkData {
					X = tileChunkX,
					Y = tileChunkY,
					ChunkContainer = null,
					ChunkTiles = new List<GameObject>()
				};
			}

			Chunks[new Vector2(tileChunkX, tileChunkY)].ChunkTiles.Add(tile);
		}
		
		protected void PlaceTilesInChunks() {
			Dictionary<Vector2, ChunkData> newChunkDatas = new Dictionary<Vector2, ChunkData>();

			foreach (KeyValuePair<Vector2, ChunkData> tileChunk in Chunks) {
				GameObject newChunk = new GameObject {name = "TileChunk"};
				newChunk.transform.parent = gameObject.transform.Find("Tiles").transform;
				newChunk.transform.localPosition = new Vector3(
					tileChunk.Key.x * TileChunkDimensions.x,
					tileChunk.Key.y * TileChunkDimensions.y,
					0f
				);

				foreach (GameObject tile in tileChunk.Value.ChunkTiles) {
					tile.transform.parent = newChunk.transform;
				}

				newChunkDatas.Add(tileChunk.Key, new ChunkData {
					X = tileChunk.Value.X,
					Y = tileChunk.Value.Y,
					ChunkContainer = newChunk,
					ChunkTiles = tileChunk.Value.ChunkTiles
				});
			}

			Chunks = newChunkDatas;
		}
	}
}