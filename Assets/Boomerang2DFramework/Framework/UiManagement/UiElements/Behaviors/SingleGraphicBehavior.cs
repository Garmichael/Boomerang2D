using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.MapGeneration;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.UiManagement.UiElements.Properties;
using Boomerang2DFramework.Framework.Utilities;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements.Behaviors {
	public class SingleGraphicBehavior : UiElementBehavior {
		private SingleGraphic MyProperties => (SingleGraphic) Properties;
		private GameObject _container;

		public override void Initialize(
			UiElementProperties properties,
			string hudObjectParent,
			GameObject hudObjectContainer,
			HudObjectBehavior hudObjectBehavior,
			Rect hudDimensions,
			string contentId = ""
		) {
			base.Initialize(properties, hudObjectParent, hudObjectContainer, hudObjectBehavior, hudDimensions, contentId);

			_container = new GameObject("Container");
			_container.transform.parent = hudObjectContainer.transform;
			_container.transform.localPosition = Vector3.zero;

			Render();
		}

		private void Render() {
			if (ContentId == "") {
				ContentId = MyProperties.ContentId;
			}

			string tilesetToUse = MyProperties.UsesPortraitContent
				? UiManager.DialogContentStore[ContentId].SpeakerPortraitTileset
				: MyProperties.Tileset;

			Tileset tileset = new Tileset(tilesetToUse);

			int tileSize = tileset.TilesetProperties.TileSize;
			float realTileSize = tileSize / GameProperties.PixelsPerUnit;

			Vector2 realStampSize = new Vector2(
				MyProperties.StampSize.x * realTileSize,
				MyProperties.StampSize.y * realTileSize
			);

			PlaceStampIntoContainer(tileset, realTileSize);
			PositionContainer(realStampSize);
		}

		private void PlaceStampIntoContainer(Tileset tileset, float realTileSize) {
			int stampIndex = MyProperties.UsesPortraitContent
				? UiManager.DialogContentStore[ContentId].SpeakerPortraitStampIndex
				: MyProperties.StampIndex;

			TilesetEditorStamp stamp = tileset.TilesetProperties.Stamps[stampIndex];
			List<List<int>> parsedStamp = BoomerangUtils.BuildMappedObjectForPlacedStamp(stamp, MyProperties.StampSize.x, MyProperties.StampSize.y);
			parsedStamp.Reverse();

			for (int j = 0; j < parsedStamp.Count; j++) {
				List<int> row = parsedStamp[j];
				for (int i = 0; i < row.Count; i++) {
					int tileId = row[i];
					if (tileId > 0 && tileset.TilesetProperties.TilesLookup.ContainsKey(tileId)) {
						GraphicGridTile newGridTile = BuildTile(tileset);
						newGridTile.TileBehavior.SetProperties(
							tileset.TilesetProperties.TilesLookup[tileId],
							tileset.TileSprites
						);

						newGridTile.GameObject.transform.localPosition = new Vector3(
							i * realTileSize,
							j * realTileSize,
							0
						);
					}
				}
			}
		}

		private GraphicGridTile BuildTile(Tileset tileset) {
			GameObject tileGameObject = new GameObject("Tile");
			tileGameObject.transform.parent = _container.transform;
			TileBehavior tileBehavior = tileGameObject.AddComponent<TileBehavior>();
			TileProperties tilePropertiesProperties = tileset.TilesetProperties.TilesLookup.FirstOrDefault().Value;
			tileBehavior.SetProperties(tilePropertiesProperties, tileset.TileSprites);

			GameObject sprite = new GameObject {name = "Sprite"};
			sprite.transform.parent = tileGameObject.transform;
			sprite.transform.localPosition = new Vector3(
				tileset.TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2,
				tileset.TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2,
				0f
			);

			sprite.AddComponent<SpriteRenderer>();

			return new GraphicGridTile {
				GameObject = tileGameObject,
				TileBehavior = tileBehavior
			};
		}

		private void PositionContainer(Vector2 realStampSize) {
			Vector2 containerPosition = new Vector2(
				BoomerangUtils.RoundToPixelPerfection(MyProperties.Position.x / GameProperties.PixelsPerUnit),
				BoomerangUtils.RoundToPixelPerfection(MyProperties.Position.y / GameProperties.PixelsPerUnit)
			);

			Vector2 origin = new Vector2(
				-GameProperties.UnitsWide / 2 + containerPosition.x,
				GameProperties.UnitsHigh / 2 - containerPosition.y - realStampSize.y
			);

			if (MyProperties.OriginCorner == OriginCorner.TopRight || MyProperties.OriginCorner == OriginCorner.BottomRight) {
				origin.x = GameProperties.UnitsWide / 2 - containerPosition.x - realStampSize.x;
			}

			if (MyProperties.OriginCorner == OriginCorner.BottomLeft || MyProperties.OriginCorner == OriginCorner.BottomRight) {
				origin.y = -GameProperties.UnitsHigh / 2 + containerPosition.y;
			}

			_container.transform.localPosition = origin;
		}
	}
}