using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.GameFlagManagement;
using Boomerang2DFramework.Framework.MapGeneration;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.UiManagement.UiElements.Properties;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements.Behaviors {
	public class GraphicGridTile {
		public GameObject GameObject;
		public TileBehavior TileBehavior;
	}

	public class GraphicGridBehavior : UiElementBehavior {
		private GraphicGrid MyProperties => (GraphicGrid) Properties;
		private GameObject _container;
		private Tileset _tileset;

		private bool _lastIsActiveState;
		private int _lastCurrentValue;
		private int _lastMaxValue;

		private readonly List<GraphicGridTile> _pooledGameObjects = new List<GraphicGridTile>();

		public override void Initialize(
			UiElementProperties properties,
			string hudObjectParent,
			GameObject hudObjectContainer,
			HudObjectBehavior hudObjectBehavior,
			Rect hudDimensions,
			string contentId = ""
		) {
			base.Initialize(properties, hudObjectParent, hudObjectContainer, hudObjectBehavior, hudDimensions, contentId);
			_tileset = new Tileset(MyProperties.Tileset);

			_container = new GameObject("Container");
			_container.transform.parent = hudObjectContainer.transform;
			_container.transform.localPosition = Vector3.zero;
		}

		public void Update() {
			int currentCount = MyProperties.CurrentValueSource == ValueSource.GameFlagFloat
				? (int) GameFlags.GetFloatFlag(MyProperties.CurrentValueGameFlag)
				: MyProperties.StaticCurrentValue;

			int maxCount = MyProperties.MaxValueSource == ValueSource.GameFlagFloat
				? (int) GameFlags.GetFloatFlag(MyProperties.MaxValueGameFlag)
				: MyProperties.StaticMaxValue;

			if (MyProperties.UsesEmptyStamp) {
				currentCount = BoomerangUtils.MaxValue(currentCount, maxCount);
			}

			bool shouldReRender = _lastIsActiveState != MyProperties.IsActive ||
			                      _lastCurrentValue != currentCount ||
			                      _lastMaxValue != maxCount;

			if (shouldReRender) {
				Render(currentCount, maxCount);
				_lastCurrentValue = currentCount;
				_lastMaxValue = maxCount;
				_lastIsActiveState = MyProperties.IsActive;
			}
		}

		private void Render(int currentCount, int maxCount) {
			TopOffPools(currentCount, maxCount);
			ResetPools();

			int tileSize = _tileset.TilesetProperties.TileSize;
			float realTileSize = tileSize / GameProperties.PixelsPerUnit;

			int totalElements = MyProperties.UsesEmptyStamp
				? maxCount
				: currentCount;

			int totalStampsTall = Mathf.CeilToInt((float) totalElements / MyProperties.ItemsPerRow);

			Vector2 realStampSize = new Vector2(
				MyProperties.StampSize.x * realTileSize,
				MyProperties.StampSize.y * realTileSize
			);

			Vector2 realElementSpacing = new Vector2(
				BoomerangUtils.RoundToPixelPerfection(MyProperties.Spacing.x / GameProperties.PixelsPerUnit),
				BoomerangUtils.RoundToPixelPerfection(MyProperties.Spacing.y / GameProperties.PixelsPerUnit)
			);

			int poolIndex = 0;
			int row = totalStampsTall - 1;
			int column = 0;

			for (int stampItem = 0; stampItem < maxCount; stampItem++) {
				if (stampItem > 0 && stampItem % MyProperties.ItemsPerRow == 0) {
					row--;
					column = 0;
				}

				Vector2 position = new Vector2(
					column * realStampSize.x + column * realElementSpacing.x,
					row * realStampSize.y + row * realElementSpacing.y
				);

				PlaceStamp(stampItem, currentCount, position, realTileSize, poolIndex);
				poolIndex += MyProperties.StampSize.x * MyProperties.StampSize.y;
				column++;
			}

			float fullElementWidth = realStampSize.x * MyProperties.ItemsPerRow + realElementSpacing.x * (MyProperties.ItemsPerRow - 1);
			float fullElementHeight = realStampSize.y * totalStampsTall + realElementSpacing.y * (totalStampsTall - 1);

			PositionEntireElement(fullElementWidth, fullElementHeight);
		}

		private void TopOffPools(int currentCount, int maxCount) {
			int totalTilesForFilled = currentCount * MyProperties.StampSize.x * MyProperties.StampSize.y;
			int totalTilesForEmpty = maxCount * MyProperties.StampSize.x * MyProperties.StampSize.y;

			while (_pooledGameObjects.Count < totalTilesForFilled + totalTilesForEmpty) {
				_pooledGameObjects.Add(BuildTile("Tile"));
			}
		}

		private GraphicGridTile BuildTile(string tileName) {
			GameObject tileGameObject = new GameObject(tileName);
			tileGameObject.transform.parent = _container.transform;
			TileBehavior tileBehavior = tileGameObject.AddComponent<TileBehavior>();
			TileProperties tilePropertiesProperties = _tileset.TilesetProperties.TilesLookup.FirstOrDefault().Value;
			tileBehavior.SetProperties(tilePropertiesProperties, _tileset.TileSprites);

			GameObject sprite = new GameObject {name = "Sprite"};
			sprite.transform.parent = tileGameObject.transform;
			sprite.transform.localPosition = new Vector3(
				_tileset.TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2,
				_tileset.TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2,
				0f
			);

			sprite.AddComponent<SpriteRenderer>();

			return new GraphicGridTile {
				GameObject = tileGameObject,
				TileBehavior = tileBehavior
			};
		}

		private void ResetPools() {
			foreach (GraphicGridTile backgroundTile in _pooledGameObjects) {
				backgroundTile.TileBehavior.SetProperties(null, _tileset.TileSprites);
			}
		}

		private void PositionEntireElement(float fullElementWidth, float fullElementHeight) {
			Vector2 realPositionOffset = new Vector2(
				BoomerangUtils.RoundToPixelPerfection(MyProperties.Position.x / GameProperties.PixelsPerUnit),
				BoomerangUtils.RoundToPixelPerfection(MyProperties.Position.y / GameProperties.PixelsPerUnit)
			);


			Vector2 origin = new Vector2(
				-GameProperties.UnitsWide / 2 + realPositionOffset.x,
				GameProperties.UnitsHigh / 2 - realPositionOffset.y - fullElementHeight
			);

			if (MyProperties.OriginCorner == OriginCorner.TopRight || MyProperties.OriginCorner == OriginCorner.BottomRight) {
				origin.x = GameProperties.UnitsWide / 2 - realPositionOffset.x - fullElementWidth;
			}

			if (MyProperties.OriginCorner == OriginCorner.BottomLeft || MyProperties.OriginCorner == OriginCorner.BottomRight) {
				origin.y = -GameProperties.UnitsHigh / 2 + realPositionOffset.y;
			}

			_container.transform.localPosition = origin;
		}

		private void PlaceStamp(int stampItem, int currentValue, Vector2 position, float realTileSize, int poolIndex) {
			int stampIndex = stampItem < currentValue
				? MyProperties.IsActive
					? MyProperties.FilledActiveStampIndex
					: MyProperties.FilledStampIndex
				: MyProperties.IsActive
					? MyProperties.EmptyActiveStampIndex
					: MyProperties.EmptyStampIndex;

			TilesetEditorStamp stamp = _tileset.TilesetProperties.Stamps[stampIndex];
			List<List<int>> parsedStamp = BoomerangUtils.BuildMappedObjectForPlacedStamp(stamp, MyProperties.StampSize.x, MyProperties.StampSize.x);
			parsedStamp.Reverse();

			for (int j = 0; j < parsedStamp.Count; j++) {
				List<int> row = parsedStamp[j];

				for (int i = 0; i < row.Count; i++) {
					int tileId = row[i];

					if (tileId > 0 && _tileset.TilesetProperties.TilesLookup.ContainsKey(tileId)) {
						float x = position.x + i * realTileSize;
						float y = position.y + j * realTileSize;

						_pooledGameObjects[poolIndex].TileBehavior.SetProperties(
							_tileset.TilesetProperties.TilesLookup[tileId],
							_tileset.TileSprites
						);

						_pooledGameObjects[poolIndex].GameObject.transform.localPosition = new Vector3(x, y, 0);
					}

					poolIndex++;
				}
			}
		}
	}
}