using System.Collections.Generic;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Boomerang2DFramework.Framework.MapGeneration {
	public class MapDepthLayerBehavior : MapLayerBehavior {
		private Rect _layerBounds = Rect.zero;
		private Vector3 _realPosition = Vector3.zero;
		private int _timeAlive;

		public void Initialize(MapLayerProperties properties) {
			Properties = properties;
			Name = properties.Name;
			_timeAlive = 0;

			BuildCamera();
			BuildLayerTiles();
		}

		private void BuildLayerTiles() {
			GameObject tilesContainer = new GameObject("Tiles");
			tilesContainer.transform.parent = transform;
			tilesContainer.transform.localPosition = new Vector3(0, 0, 0);

			Tileset tileSet = new Tileset(Properties.Tileset);
			TilesetEditorStamp stamp = tileSet.TilesetProperties.Stamps[Properties.DepthLayerStampId];

			List<List<int>> parsedStamp = BoomerangUtils.BuildMappedObjectForPlacedStamp(
				stamp,
				Properties.DepthLayerStampDimensions.x,
				Properties.DepthLayerStampDimensions.y
			);

			parsedStamp.Reverse();

			int tileSize = tileSet.TilesetProperties.TileSize;

			Vector2Int pixelsDimensionsOfScreen = new Vector2Int(
				GameProperties.RenderDimensionsWidth,
				GameProperties.RenderDimensionsHeight
			);

			Vector2Int tilesThatFit = new Vector2Int(
				Mathf.CeilToInt(pixelsDimensionsOfScreen.x / (float) tileSize),
				Mathf.CeilToInt(pixelsDimensionsOfScreen.y / (float) tileSize)
			);

			Vector2Int stampsThatFit = new Vector2Int(
				Mathf.CeilToInt(tilesThatFit.x / (float) Properties.DepthLayerStampDimensions.x),
				Mathf.CeilToInt(tilesThatFit.y / (float) Properties.DepthLayerStampDimensions.y)
			);

			Vector2 stampUnitDimensions = new Vector2(
				Properties.DepthLayerStampDimensions.x * GameProperties.PixelSize * tileSize,
				Properties.DepthLayerStampDimensions.y * GameProperties.PixelSize * tileSize
			);

			int totalPanes = 1;
			if (Properties.DepthLayerRepeatX) {
				totalPanes *= 2;
			} else {
				stampsThatFit.x = 1;
			}

			if (Properties.DepthLayerRepeatY) {
				totalPanes *= 2;
			} else {
				stampsThatFit.y = 1;
			}

			Vector2 paneDimensions = new Vector2(
				stampsThatFit.x * stampUnitDimensions.x,
				stampsThatFit.y * stampUnitDimensions.y
			);

			List<GameObject> panes = new List<GameObject>();

			for (int paneIndex = 0; paneIndex < totalPanes; paneIndex++) {
				GameObject pane = new GameObject("depthLayerPane");
				pane.transform.parent = tilesContainer.transform;

				panes.Add(pane);

				for (int stampIndexX = 0; stampIndexX < stampsThatFit.x; stampIndexX++) {
					for (int stampIndexY = 0; stampIndexY < stampsThatFit.y; stampIndexY++) {
						for (int j = 0; j < parsedStamp.Count; j++) {
							List<int> row = parsedStamp[j];
							for (int i = 0; i < row.Count; i++) {
								int tileId = row[i];
								if (tileId > 0 && tileSet.TilesetProperties.TilesLookup.ContainsKey(tileId)) {
									GameObject newTile = tileSet.BuildTile(tileId.ToString(), false);

									if (newTile != null) {
										newTile.transform.parent = pane.transform;

										Vector3 position = new Vector3(
											i * (tileSize / GameProperties.PixelsPerUnit),
											j * (tileSize / GameProperties.PixelsPerUnit),
											0
										);

										position += new Vector3(
											stampUnitDimensions.x * stampIndexX,
											stampUnitDimensions.y * stampIndexY,
											0
										);

										newTile.transform.localPosition = position;
									}
								}
							}
						}
					}
				}
			}

			if (Properties.DepthLayerRepeatX && Properties.DepthLayerRepeatY) {
				panes[0].transform.localPosition = new Vector3(0, -paneDimensions.y, 0);
				panes[1].transform.localPosition = new Vector3(paneDimensions.x, -paneDimensions.y, 0);
				panes[2].transform.localPosition = new Vector3(0, -paneDimensions.y - paneDimensions.y, 0);
				panes[3].transform.localPosition =
					new Vector3(paneDimensions.x, -paneDimensions.y - paneDimensions.y, 0);
			} else if (Properties.DepthLayerRepeatX && !Properties.DepthLayerRepeatY) {
				panes[0].transform.localPosition = new Vector3(0, -paneDimensions.y, 0);
				panes[1].transform.localPosition = new Vector3(paneDimensions.x, -paneDimensions.y, 0);
				_layerBounds = Rect.zero;
			} else if (!Properties.DepthLayerRepeatX && Properties.DepthLayerRepeatY) {
				panes[0].transform.localPosition = new Vector3(0, -paneDimensions.y, 0);
				panes[1].transform.localPosition = new Vector3(0, -paneDimensions.y - paneDimensions.y, 0);
				_layerBounds = Rect.zero;
			} else {
				panes[0].transform.localPosition = new Vector3(0, -paneDimensions.y, 0);
			}

			_layerBounds = new Rect(
				0,
				0,
				paneDimensions.x * (Properties.DepthLayerRepeatX ? 2 : 1),
				-paneDimensions.y * (Properties.DepthLayerRepeatY ? 2 : 1)
			);

			_realPosition = new Vector3(
				_layerBounds.width / 2,
				_layerBounds.height / 2,
				LayerCameraContainer.transform.localPosition.z
			);
		}

		public override void UpdateLayerCameraPosition() {
			_timeAlive++;

			if (Properties.DepthLayerScrollMode == DepthLayerScrollMode.Autoscroll) {
				AutoScroll();
			} else if (Properties.DepthLayerScrollMode == DepthLayerScrollMode.Parallax) {
				ParallaxScroll();
			}

			LayerCameraContainer.transform.localPosition = BoomerangUtils.RoundToPixelPerfection(_realPosition);
		}

		private void ParallaxScroll() {
			Vector2 originPoint = GetOriginPoint();
			Vector2 thisOriginPoint = GetLayerOriginPoint();
			Vector2 cameraOriginPoint = GetViewOriginPoint();
			Vector2 distanceFromOriginPoint = originPoint - cameraOriginPoint;
			Vector2 parallaxedDistance = distanceFromOriginPoint * -Properties.DepthLayerScrollSpeed;

			_realPosition = (Vector3) thisOriginPoint + new Vector3(
				                parallaxedDistance.x,
				                parallaxedDistance.y,
				                _realPosition.z
			                );

			if (Properties.DepthLayerRepeatX) {
				while (_realPosition.x - LayerCameraController.VisibleTileCount.x / 2 < 0) {
					_realPosition += new Vector3(_layerBounds.width / 2, 0, 0);
				}

				while (_realPosition.x + LayerCameraController.VisibleTileCount.x / 2 > _layerBounds.width) {
					_realPosition -= new Vector3(_layerBounds.width / 2, 0, 0);
				}
			}

			if (Properties.DepthLayerRepeatY) {
				while (_realPosition.y + LayerCameraController.VisibleTileCount.y / 2 > 0) {
					_realPosition += new Vector3(0, _layerBounds.height / 2, 0);
				}

				while (_realPosition.y - LayerCameraController.VisibleTileCount.y / 2 < _layerBounds.height) {
					_realPosition -= new Vector3(0, _layerBounds.height / 2, 0);
				}
			}
		}

		private Vector2 GetOriginPoint() {
			Vector2 originPoint = Vector2.zero;

			if (Properties.DepthLayerOriginCorner == DepthLayerOriginCorner.BottomLeft) {
				originPoint = Properties.DepthLayerOrigin == DepthLayerOrigin.Map
					? Vector2.zero
					: new Vector2(
						Boomerang2D.MainCameraController.CurrentView.LeftExtent,
						Boomerang2D.MainCameraController.CurrentView.BottomExtent
					);
			} else if (Properties.DepthLayerOriginCorner == DepthLayerOriginCorner.TopLeft) {
				originPoint = Properties.DepthLayerOrigin == DepthLayerOrigin.Map
					? new Vector2(0, MapManager.ActiveMapProperties.Height)
					: new Vector2(
						Boomerang2D.MainCameraController.CurrentView.LeftExtent,
						Boomerang2D.MainCameraController.CurrentView.TopExtent
					);
			} else if (Properties.DepthLayerOriginCorner == DepthLayerOriginCorner.BottomRight) {
				originPoint = Properties.DepthLayerOrigin == DepthLayerOrigin.Map
					? new Vector2(MapManager.ActiveMapProperties.Width, 0)
					: new Vector2(
						Boomerang2D.MainCameraController.CurrentView.RightExtent,
						Boomerang2D.MainCameraController.CurrentView.BottomExtent
					);
			} else if (Properties.DepthLayerOriginCorner == DepthLayerOriginCorner.TopRight) {
				originPoint = Properties.DepthLayerOrigin == DepthLayerOrigin.Map
					? new Vector2(MapManager.ActiveMapProperties.Width, MapManager.ActiveMapProperties.Height)
					: new Vector2(
						Boomerang2D.MainCameraController.CurrentView.RightExtent,
						Boomerang2D.MainCameraController.CurrentView.TopExtent
					);
			}

			return originPoint;
		}

		private Vector2 GetViewOriginPoint() {
			Vector2 originPoint = Vector2.zero;

			if (Properties.DepthLayerOriginCorner == DepthLayerOriginCorner.BottomLeft) {
				originPoint = (Vector2) Boomerang2D.MainCameraController.RealPosition +
				              new Vector2(-GameProperties.UnitsWide / 2, -GameProperties.UnitsHigh / 2);
			} else if (Properties.DepthLayerOriginCorner == DepthLayerOriginCorner.TopLeft) {
				originPoint = (Vector2) Boomerang2D.MainCameraController.RealPosition +
				              new Vector2(-GameProperties.UnitsWide / 2, GameProperties.UnitsHigh / 2);
			} else if (Properties.DepthLayerOriginCorner == DepthLayerOriginCorner.BottomRight) {
				originPoint = (Vector2) Boomerang2D.MainCameraController.RealPosition +
				              new Vector2(GameProperties.UnitsWide / 2, -GameProperties.UnitsHigh / 2);
			} else if (Properties.DepthLayerOriginCorner == DepthLayerOriginCorner.TopRight) {
				originPoint = (Vector2) Boomerang2D.MainCameraController.RealPosition +
				              new Vector2(GameProperties.UnitsWide / 2, GameProperties.UnitsHigh / 2);
			}

			return originPoint;
		}

		private Vector2 GetLayerOriginPoint() {
			Vector2 originPoint = Vector2.zero;

			Debug.DrawLine(
				new Vector3(_layerBounds.x, _layerBounds.y, 0),
				new Vector3(_layerBounds.x + _layerBounds.width, _layerBounds.y, 0),
				Color.yellow);

			Debug.DrawLine(
				new Vector3(_layerBounds.x, _layerBounds.y, 0),
				new Vector3(_layerBounds.x, _layerBounds.y + _layerBounds.height, 0),
				Color.yellow);

			Debug.DrawLine(
				new Vector3(_layerBounds.x + _layerBounds.width, _layerBounds.y, 0),
				new Vector3(_layerBounds.x + _layerBounds.width, _layerBounds.y + _layerBounds.height, 0),
				Color.yellow);

			Debug.DrawLine(
				new Vector3(_layerBounds.x, _layerBounds.y + _layerBounds.height, 0),
				new Vector3(_layerBounds.x + _layerBounds.width, _layerBounds.y + _layerBounds.height, 0),
				Color.yellow);


			if (Properties.DepthLayerOriginCorner == DepthLayerOriginCorner.BottomLeft) {
				originPoint = new Vector2(_layerBounds.x, _layerBounds.y + _layerBounds.height) +
				              new Vector2(
					              LayerCameraController.VisibleTileCount.x / 2,
					              LayerCameraController.VisibleTileCount.y / 2
				              );
			} else if (Properties.DepthLayerOriginCorner == DepthLayerOriginCorner.TopLeft) {
				originPoint = new Vector2(_layerBounds.x, _layerBounds.y) +
				              new Vector2(
					              LayerCameraController.VisibleTileCount.x / 2,
					              -LayerCameraController.VisibleTileCount.y / 2);
			} else if (Properties.DepthLayerOriginCorner == DepthLayerOriginCorner.BottomRight) {
				originPoint = new Vector2(_layerBounds.x + _layerBounds.width, _layerBounds.y + _layerBounds.height) -
				              new Vector2(
					              LayerCameraController.VisibleTileCount.x / 2,
					              -LayerCameraController.VisibleTileCount.y / 2);
			} else if (Properties.DepthLayerOriginCorner == DepthLayerOriginCorner.TopRight) {
				originPoint = new Vector2(_layerBounds.x + _layerBounds.width, _layerBounds.y) -
				              new Vector2(
					              LayerCameraController.VisibleTileCount.x / 2,
					              LayerCameraController.VisibleTileCount.y / 2
				              );
			}

			return originPoint;
		}

		private void AutoScroll() {
			Vector2 depthLayerCameraOriginPoint = GetLayerOriginPoint();

			Vector2 scrollDistance = GameProperties.PixelSize * _timeAlive * Properties.DepthLayerScrollSpeed;

			Vector3 offset = new(
				scrollDistance.x,
				scrollDistance.y,
				_realPosition.z
			);

			_realPosition = (Vector3) depthLayerCameraOriginPoint + offset;

			if (Properties.DepthLayerRepeatX) {
				_realPosition.x = depthLayerCameraOriginPoint.x + scrollDistance.x;

				while (_realPosition.x - LayerCameraController.VisibleTileCount.x / 2 < 0) {
					_realPosition += new Vector3(_layerBounds.width / 2, 0, 0);
				}

				while (_realPosition.x + LayerCameraController.VisibleTileCount.x / 2 > _layerBounds.width) {
					_realPosition -= new Vector3(_layerBounds.width / 2, 0, 0);
				}
			}

			if (Properties.DepthLayerRepeatY) {
				_realPosition.y = depthLayerCameraOriginPoint.y + scrollDistance.y;

				while (_realPosition.y + LayerCameraController.VisibleTileCount.y / 2 > 0) {
					_realPosition += new Vector3(0, _layerBounds.height / 2, 0);
				}

				while (_realPosition.y - LayerCameraController.VisibleTileCount.y / 2 < _layerBounds.height) {
					_realPosition -= new Vector3(0, _layerBounds.height / 2, 0);
				}
			}
		}
	}
}