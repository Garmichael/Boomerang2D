using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Boomerang2DFramework.Framework.MapGeneration {
	public class Tileset {
		public readonly TilesetProperties TilesetProperties;
		public readonly Sprite[] TileSprites;

		public Tileset(string tilesetName) {
			TextAsset json = BoomerangDatabase.TilesetJsonDatabaseEntries[tilesetName];
			TileSprites = BoomerangDatabase.TilesetSpriteDatabaseEntries[tilesetName];

			TilesetProperties = JsonUtility.FromJson<TilesetProperties>(json.text);
			TilesetProperties.PopulateLookupTable();
		}

		public GameObject BuildTile(string tileSlot, bool usesColliders) {
			int tileId = int.Parse(tileSlot);

			if (tileId == 0 || !TilesetProperties.TilesLookup.ContainsKey(tileId)) {
				return null;
			}

			TileProperties tilePropertiesProperties = TilesetProperties.TilesLookup[tileId];

			GameObject newTile = new GameObject {name = "Tile"};
			TileBehavior newTileBehavior = newTile.AddComponent<TileBehavior>();
			newTileBehavior.SetProperties(tilePropertiesProperties, TileSprites);

			if (usesColliders && ShouldAttachColliders(tilePropertiesProperties)) {
				if (tilePropertiesProperties.SolidOnTop &&
				    tilePropertiesProperties.SolidOnBottom &&
				    tilePropertiesProperties.SolidOnLeft &&
				    tilePropertiesProperties.SolidOnRight
				) {
					AttachCollider(newTile, tilePropertiesProperties, "Solid");
					if (newTileBehavior.Properties.Flags.IndexOf("Solid") == -1) {
						newTileBehavior.Properties.Flags.Add("Solid");
					}
				} else if (!tilePropertiesProperties.SolidOnTop &&
				           !tilePropertiesProperties.SolidOnBottom &&
				           !tilePropertiesProperties.SolidOnLeft &&
				           !tilePropertiesProperties.SolidOnRight
				) {
					AttachCollider(newTile, tilePropertiesProperties, "NonSolidTile");
				} else {
					if (tilePropertiesProperties.SolidOnTop) {
						AttachCollider(newTile, tilePropertiesProperties, "SolidOnTop");
						if (newTileBehavior.Properties.Flags.IndexOf("SolidOnTop") == -1) {
							newTileBehavior.Properties.Flags.Add("SolidOnTop");
						}
					}

					if (tilePropertiesProperties.SolidOnBottom) {
						AttachCollider(newTile, tilePropertiesProperties, "SolidOnBottom");
						if (newTileBehavior.Properties.Flags.IndexOf("SolidOnBottom") == -1) {
							newTileBehavior.Properties.Flags.Add("SolidOnBottom");
						}
					}

					if (tilePropertiesProperties.SolidOnLeft) {
						AttachCollider(newTile, tilePropertiesProperties, "SolidOnLeft");
						if (newTileBehavior.Properties.Flags.IndexOf("SolidOnLeft") == -1) {
							newTileBehavior.Properties.Flags.Add("SolidOnLeft");
						}
					}

					if (tilePropertiesProperties.SolidOnRight) {
						AttachCollider(newTile, tilePropertiesProperties, "SolidOnRight");
						if (newTileBehavior.Properties.Flags.IndexOf("SolidOnRight") == -1) {
							newTileBehavior.Properties.Flags.Add("SolidOnRight");
						}
					}
				}
			}

			AttachSprite(newTile);

			MapManager.AddTileToCatalog(newTile, newTileBehavior);
			return newTile;
		}

		private bool ShouldAttachColliders(TileProperties tilePropertiesProperties) {
			return tilePropertiesProperties.UsesCollider &&
			       tilePropertiesProperties.CollisionShape != null;
		}

		private void AttachCollider(GameObject tile, TileProperties tileProperties, string layer) {
			GameObject collider = GetColliderInstance(tileProperties);

			if (layer.ToLower() != "none") {
				collider.layer = LayerMask.NameToLayer(layer);
			}

			collider.transform.parent = tile.transform;
			SetColliderPositioning(collider, tileProperties);
		}

		private GameObject GetColliderInstance(TileProperties tileProperties) {
			return Object.Instantiate(
				BoomerangDatabase.TileColliderDatabaseEntries[tileProperties.CollisionShape],
				new Vector3(0, 0, 0), Quaternion.identity
			);
		}

		private void SetColliderPositioning(GameObject collider, TileProperties tileProperties) {
			collider.transform.localPosition = new Vector3(
				TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2 + tileProperties.CollisionOffset.x,
				TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2 - tileProperties.CollisionOffset.y,
				0f
			);

			collider.transform.rotation = Quaternion.Euler(0f, 0f, -tileProperties.CollisionRotation);

			Vector3 localScale = new Vector3(
				tileProperties.CollisionFlippedX ? -1 : 1,
				tileProperties.CollisionFlippedY ? -1 : 1,
				200
			);

			localScale = new Vector3(
				localScale.x * (TilesetProperties.TileSize / GameProperties.PixelsPerUnit),
				localScale.y * (TilesetProperties.TileSize / GameProperties.PixelsPerUnit),
				localScale.z
			);
			collider.transform.localScale = localScale;
		}

		private void AttachSprite(GameObject tile) {
			GameObject sprite = new GameObject {name = "Sprite"};
			sprite.transform.parent = tile.transform;
			sprite.transform.localPosition = new Vector3(
				TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2,
				TilesetProperties.TileSize / GameProperties.PixelsPerUnit / 2,
				0f
			);

//			sprite.transform.localScale = new Vector3(
//				-1 * sprite.transform.localScale.x,
//				sprite.transform.localScale.y,
//				sprite.transform.localScale.z
//			);
//
//			sprite.transform.localScale = new Vector3(
//				sprite.transform.localScale.x,
//				-1 * sprite.transform.localScale.y,
//				sprite.transform.localScale.z
//			);

			sprite.AddComponent<SpriteRenderer>();
		}
	}
}