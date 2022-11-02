using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.EditorObjects;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using Boomerang2DFramework.Framework.Utilities;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.MapEditor {
	public partial class MapEditor : EditorWindow {
		private void LoadData() {
			_allActorProperties.Clear();

			foreach (KeyValuePair<string, TextAsset> actor in BoomerangDatabase.ActorJsonDatabaseEntries) {
				TextAsset json = BoomerangDatabase.ActorJsonDatabaseEntries[actor.Key];
				ActorProperties newActorProperties = JsonUtility.FromJson<ActorProperties>(json.text);
				_allActorProperties.Add(newActorProperties.Name, newActorProperties);
			}

			_allActorTextures.Clear();

			foreach (KeyValuePair<string, ActorProperties> actorProperties in _allActorProperties) {
				_allActorTextures.Add(actorProperties.Key, FetchActorTexture(actorProperties.Value));
			}

			_allMaps.Clear();

			foreach (KeyValuePair<string, TextAsset> actorJson in BoomerangDatabase.MapDatabaseEntries) {
				MapProperties mapProperties = JsonUtility.FromJson<MapProperties>(actorJson.Value.text);
				_allMaps.Add(mapProperties);
			}

			_allTileSets.Clear();

			foreach (KeyValuePair<string, TextAsset> actorJson in BoomerangDatabase.TilesetJsonDatabaseEntries) {
				TilesetProperties tilesetProperties = JsonUtility.FromJson<TilesetProperties>(actorJson.Value.text);
				tilesetProperties.PopulateLookupTable();

				List<Texture2D> tilesetTextures = LoadTilesetTextures(tilesetProperties);
				List<Color32[]> tilesetPixelData = LoadTilesetPixelDatas(tilesetTextures);
				List<Texture2D> tilesetStampTextures = BuildTilesetStampTextures(tilesetProperties, tilesetPixelData);

				_allTileSets.Add(tilesetProperties.Name, new TileSetData {
					Properties = tilesetProperties,
					Textures = tilesetTextures,
					PixelDatas = tilesetPixelData,
					StampTextures = tilesetStampTextures
				});
			}
		}

		private Texture2D FetchActorTexture(ActorProperties actorProperties) {
			Texture2D textureToUse;

			if (actorProperties.States.Count > 0 &&
			    actorProperties.States[0].Animations.Count > 0 &&
			    actorProperties.States[0].Animations[0].AnimationFrames.Count > 0
			) {
				int spriteFrameIndex = actorProperties.States[0].Animations[0].AnimationFrames[0].SpriteFrame;
				Sprite[] actorSprites = BoomerangDatabase.ActorSpriteDatabaseEntries[actorProperties.Name];

				if (actorSprites.Length > spriteFrameIndex) {
					Sprite sprite = actorSprites[spriteFrameIndex];
					Texture2D croppedTexture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
					Color[] pixels = sprite.texture.GetPixels((int) sprite.rect.x,
						(int) sprite.rect.y,
						(int) sprite.rect.width,
						(int) sprite.rect.height, 0);

					croppedTexture.SetPixels(pixels, 0);
					croppedTexture.wrapMode = TextureWrapMode.Clamp;
					croppedTexture.filterMode = FilterMode.Point;
					croppedTexture.Apply();

					textureToUse = croppedTexture;
				} else {
					textureToUse = new Texture2D(1, 1);
					Color32[] texColors = {new Color32(0, 0, 0, 0)};
					textureToUse.SetPixels32(texColors);
					textureToUse.Apply();
				}
			} else {
				textureToUse = new Texture2D(1, 1);
				Color32[] texColors = {new Color32(0, 0, 0, 0)};
				textureToUse.SetPixels32(texColors);
				textureToUse.Apply();
			}

			return textureToUse;
		}

		private List<Texture2D> BuildTilesetStampTextures(TilesetProperties tilesetProperties, IReadOnlyList<Color32[]> tilesetPixelData) {
			List<Texture2D> tilesetStampTextures = new List<Texture2D>();

			foreach (TilesetEditorStamp tilesetEditorStamp in tilesetProperties.Stamps) {
				List<List<int>> parsedStamp = BoomerangUtils.ParseTilesetEditorStamp(tilesetEditorStamp);
				Texture2D stampTexture = new Texture2D(
					tilesetEditorStamp.Width * tilesetProperties.TileSize,
					tilesetEditorStamp.Height * tilesetProperties.TileSize
				) {
					wrapMode = TextureWrapMode.Clamp,
					filterMode = FilterMode.Point
				};

				_transparentTileGraphicPixelData = new Color32[tilesetProperties.TileSize * tilesetProperties.TileSize];
				for (int i = 0; i < _transparentTileGraphicPixelData.Length; i++) {
					_transparentTileGraphicPixelData[i] = new Color32(0, 0, 0, 0);
				}

				for (int i = 0; i < tilesetEditorStamp.Height; i++) {
					for (int j = 0; j < tilesetEditorStamp.Width; j++) {
						int value = parsedStamp[i][j];
						bool tileTextureExists = tilesetProperties.TilesLookup.ContainsKey(value);

						Color32[] pixelDataToUse = value == 0
							? _transparentTileGraphicPixelData
							: tileTextureExists
								? tilesetPixelData[tilesetProperties.TilesLookup[value].AnimationFrames[0]]
								: _missingTileGraphicPixelData;

						stampTexture.SetPixels32(
							j * tilesetProperties.TileSize,
							(tilesetEditorStamp.Height - 1 - i) * tilesetProperties.TileSize,
							tilesetProperties.TileSize,
							tilesetProperties.TileSize,
							pixelDataToUse
						);
					}
				}

				stampTexture.Apply();
				tilesetStampTextures.Add(stampTexture);
			}

			return tilesetStampTextures;
		}

		private List<Texture2D> LoadTilesetTextures(TilesetProperties tilesetProperties) {
			List<Texture2D> tilesetTextures = new List<Texture2D>();
			Sprite[] tileSprites = BoomerangDatabase.TilesetSpriteDatabaseEntries[tilesetProperties.Name];

			if (tileSprites == null) {
				return new List<Texture2D>();
			}

			foreach (Sprite sprite in tileSprites) {
				Texture2D croppedTexture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
				Color[] pixels = sprite.texture.GetPixels((int) sprite.rect.x,
					(int) sprite.rect.y,
					(int) sprite.rect.width,
					(int) sprite.rect.height, 0);

				croppedTexture.SetPixels(pixels, 0);
				croppedTexture.wrapMode = TextureWrapMode.Clamp;
				croppedTexture.filterMode = FilterMode.Point;
				croppedTexture.Apply();
				tilesetTextures.Add(croppedTexture);
			}

			return tilesetTextures;
		}

		private List<Color32[]> LoadTilesetPixelDatas(List<Texture2D> textures) {
			return textures.Select(texture => texture.GetPixels32()).ToList();
		}

		private void SaveMap() {
			if (ActiveMap == null) {
				return;
			}

			if (_tileEditingMode == TileEditingMode.PaintingBrush) {
				_tileEditingMode = TileEditingMode.NoneSelected;
				ApplyBrushMatteAsEditorObject(_brushEditorObjectBeingEdited);
			}

			BuildCsVMapFromTileEditorObjects();

			File.WriteAllText(GameProperties.MapContentDirectory + "/" + ActiveMap.Name + ".json", JsonUtility.ToJson(ActiveMap, true));

			AssetDatabase.Refresh();
			BoomerangDatabase.PopulateDatabase();
			OnEnable();
		}

		private void BuildCsVMapFromTileEditorObjects() {
			foreach (MapLayerProperties mapLayer in ActiveMap.Layers) {
				int tileSize = _allTileSets[mapLayer.Tileset].Properties.TileSize;
				float scale = GameProperties.PixelsPerUnit / tileSize;
				int width = Mathf.CeilToInt(ActiveMap.Width * scale);
				int height = Mathf.CeilToInt(ActiveMap.Height * scale);
				List<List<int>> parsedMap = BuildEmptyParsedMap(width, height);

				foreach (TileEditorObject editorObject in mapLayer.TileEditorObjects) {
					switch (editorObject.TileEditorType) {
						case TileEditorObjectType.Tile: {
							for (int x = 0; x < editorObject.Width; x++) {
								for (int y = 0; y < editorObject.Height; y++) {
									bool isInMap = BoomerangUtils.IndexInRange(parsedMap, y + editorObject.Y) &&
									               BoomerangUtils.IndexInRange(parsedMap[0], x + editorObject.X);

									if (isInMap) {
										parsedMap[y + editorObject.Y][x + editorObject.X] = editorObject.Id;
									}
								}
							}

							break;
						}

						case TileEditorObjectType.Stamp: {
							TileSetData tilesetData = _allTileSets[mapLayer.Tileset];
							TilesetEditorStamp tilesetEditorStamp = tilesetData.Properties.Stamps[editorObject.Id];

							editorObject.ParsedBrushMapDefinition = BoomerangUtils.BuildMappedObjectForPlacedStamp(
								tilesetEditorStamp,
								editorObject.Width,
								editorObject.Height
							);

							for (int y = 0; y < editorObject.ParsedBrushMapDefinition.Count; y++) {
								for (int x = 0; x < editorObject.ParsedBrushMapDefinition[y].Count; x++) {
									bool isInMap = y + editorObject.Y >= 0 &&
									               y + editorObject.Y < ActiveMap.Height &&
									               x + editorObject.X >= 0 &&
									               x + editorObject.X < ActiveMap.Width;

									if (isInMap && editorObject.ParsedBrushMapDefinition[y][x] > 0) {
										parsedMap[y + editorObject.Y][x + editorObject.X] = editorObject.ParsedBrushMapDefinition[y][x];
									}
								}
							}

							break;
						}

						case TileEditorObjectType.Brush: {
							editorObject.ParsedBrushMapDefinition = ParseBrushEditorObjectDefinition(editorObject);

							for (int y = 0; y < editorObject.ParsedBrushMapDefinition.Count; y++) {
								for (int x = 0; x < editorObject.ParsedBrushMapDefinition[y].Count; x++) {
									bool isInMap = y + editorObject.Y >= 0 &&
									               y + editorObject.Y < ActiveMap.Height &&
									               x + editorObject.X >= 0 &&
									               x + editorObject.X < ActiveMap.Width;

									if (isInMap) {
										int tileToPaint = GetPaintbrushTileId(
											_allTileSets[mapLayer.Tileset].Properties.Brushes[editorObject.Id],
											editorObject,
											x,
											y
										);

										if (tileToPaint > 0) {
											parsedMap[y + editorObject.Y][x + editorObject.X] = tileToPaint;
										}
									}
								}
							}

							break;
						}

						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				mapLayer.TileRows = BuildMapLayerTileRows(parsedMap);
			}
		}

		private List<List<int>> BuildEmptyParsedMap(int width, int height) {
			List<List<int>> parsedMap = new List<List<int>>();
			List<int> emptyRow = new List<int>();

			for (int i = 0; i < width; i++) {
				emptyRow.Add(0);
			}

			for (int i = 0; i < height; i++) {
				parsedMap.Add(new List<int>(emptyRow));
			}

			return parsedMap;
		}

		private List<string> BuildMapLayerTileRows(List<List<int>> parsedMap) {
			List<string> tileRows = new List<string>();

			foreach (List<int> row in parsedMap) {
				tileRows.Add(string.Join(",", row));
			}

			return tileRows;
		}

		private bool ShouldReloadData() {
			bool shouldReload = false;

			foreach (KeyValuePair<string, TileSetData> tileset in _allTileSets) {
				foreach (Texture2D texture2D in tileset.Value.Textures) {
					if (texture2D == null) {
						shouldReload = true;
						break;
					}
				}

				if (shouldReload) {
					break;
				}
			}

			return shouldReload;
		}

		private bool ShouldBuildMapEditorTextures() {
			return ActiveMap != null &&
			       (_mapEditingTransparentBackground == null ||
			        _mapEditingTransparentBackground.width != ActiveMap.Width * (int) GameProperties.PixelsPerUnit ||
			        _mapEditingTransparentBackground.height != ActiveMap.Height * (int) GameProperties.PixelsPerUnit ||
			        _missingTileGraphic == null ||
			        _missingTileGraphicPixelData == null);
		}

		private void BuildMapEditorTextures() {
			const int baseTileSize = 16;
			int width = ActiveMap.Width * baseTileSize;
			int height = ActiveMap.Height * baseTileSize;

			_mapEditingTransparentBackground = new Texture2D(width, height) {
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point
			};

			for (int x = 0; x < ActiveMap.Width; x++) {
				for (int y = 0; y < ActiveMap.Height; y++) {
					_mapEditingTransparentBackground.SetPixels32(
						x * 16, y * 16, 16, 16,
						SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.TransparentTile].GetPixels32()
					);
				}
			}

			_mapEditingTransparentBackground.Apply();

			_viewBorderTexture = SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.BoundingBox];
			_missingTileGraphic = SuperFormsStyles.GuiTextures[SuperFormsStyles.B2DEditorTextures.Rays];
			_missingTileGraphicPixelData = _missingTileGraphic.GetPixels32();
		}

		private void OnMapChange() {
			BuildMapEditorTextures();
			_previousSelectedMapIndex = _selectedMapIndex;
		}

		private static bool ShouldBuildTextureForTileEditorObject(TileEditorObject tileEditorEditorObject, string tileSet) {
			return tileEditorEditorObject.CachedEditorTexture == null ||
			       tileEditorEditorObject.CachedEditorTextureInfo.Width != tileEditorEditorObject.Width ||
			       tileEditorEditorObject.CachedEditorTextureInfo.Height != tileEditorEditorObject.Height ||
			       tileEditorEditorObject.CachedEditorTextureInfo.Id != tileEditorEditorObject.Id ||
			       tileEditorEditorObject.CachedEditorTextureInfo.TileSet != tileSet;
		}

		private void BuildTextureForTileEditorObject(TileEditorObject tileEditorEditorObject, string tileset) {
			int tileSize = _allTileSets[tileset].Properties.TileSize;

			Texture2D texture2D = new Texture2D(tileEditorEditorObject.Width * tileSize, tileEditorEditorObject.Height * tileSize) {
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point
			};

			TileSetData tilesetData = _allTileSets[tileset];
			bool tileTextureExists = tilesetData.Properties.TilesLookup.ContainsKey(tileEditorEditorObject.Id);

			for (int i = 0; i < tileEditorEditorObject.Width; i++) {
				for (int j = 0; j < tileEditorEditorObject.Height; j++) {
					texture2D.SetPixels32(
						i * tileSize, j * tileSize, tileSize, tileSize,
						tileTextureExists
							? _allTileSets[tileset].PixelDatas[_allTileSets[tileset].Properties.TilesLookup[tileEditorEditorObject.Id].AnimationFrames[0]]
							: _missingTileGraphicPixelData
					);
				}
			}

			texture2D.Apply();

			tileEditorEditorObject.CachedEditorTexture = texture2D;

			tileEditorEditorObject.CachedEditorTextureInfo = new CachedTileEditorObjectInfo {
				Width = tileEditorEditorObject.Width,
				Height = tileEditorEditorObject.Height,
				Id = tileEditorEditorObject.Id,
				TileSet = tileset
			};
		}

		private void BuildTextureForStampObject(TileEditorObject stampEditorObject, string tileset) {
			int tileSize = _allTileSets[tileset].Properties.TileSize;

			Texture2D texture2D = new Texture2D(stampEditorObject.Width * tileSize, stampEditorObject.Height * tileSize) {
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point
			};

			TileSetData tilesetData = _allTileSets[tileset];
			
			if (!BoomerangUtils.IndexInRange(tilesetData.Properties.Stamps, stampEditorObject.Id)) {
				return;
			}
			
			TilesetEditorStamp tilesetEditorStamp = tilesetData.Properties.Stamps[stampEditorObject.Id];
 
			List<List<int>> mappedStampObject = BoomerangUtils.BuildMappedObjectForPlacedStamp(
				tilesetEditorStamp,
				stampEditorObject.Width,
				stampEditorObject.Height
			);

			_transparentTileGraphicPixelData = new Color32[tileSize * tileSize];
			for (int i = 0; i < _transparentTileGraphicPixelData.Length; i++) {
				_transparentTileGraphicPixelData[i] = new Color32(0, 0, 0, 0);
			}

			for (int j = 0; j < mappedStampObject.Count; j++) {
				for (int i = 0; i < mappedStampObject[j].Count; i++) {
					int value = mappedStampObject[j][i];
					bool tileTextureExists = tilesetData.Properties.TilesLookup.ContainsKey(value);

					Color32[] pixelDataToUse = value == 0
						? _transparentTileGraphicPixelData
						: tileTextureExists
							? _allTileSets[tileset].PixelDatas[_allTileSets[tileset].Properties.TilesLookup[value].AnimationFrames[0]]
							: _missingTileGraphicPixelData;

					texture2D.SetPixels32(
						i * tileSize, (stampEditorObject.Height - 1 - j) * tileSize, tileSize, tileSize,
						pixelDataToUse
					);
				}
			}

			texture2D.Apply();

			stampEditorObject.ParsedBrushMapDefinition = mappedStampObject;
			stampEditorObject.CachedEditorTexture = texture2D;

			stampEditorObject.CachedEditorTextureInfo = new CachedTileEditorObjectInfo {
				Width = stampEditorObject.Width,
				Height = stampEditorObject.Height,
				Id = stampEditorObject.Id,
				TileSet = tileset
			};
		}

		private void BuildTextureForBrushObject(TileEditorObject brushEditorObject, string tileset) {
			brushEditorObject.CachedEditorTexture = BuildPaintbrushMatteTexture(brushEditorObject, tileset);
		}

		private Texture2D BuildPaintbrushMatteTexture(TileEditorObject brushEditorObject, string tileset) {
			TileSetData brushTileset = _allTileSets[tileset];
			int tileSize = brushTileset.Properties.TileSize;

			if (brushEditorObject.ParsedBrushMapDefinition == null || brushEditorObject.ParsedBrushMapDefinition.Count == 0) {
				brushEditorObject.ParsedBrushMapDefinition = ParseBrushEditorObjectDefinition(brushEditorObject);
			}

			List<List<int>> map = brushEditorObject.ParsedBrushMapDefinition;

			Texture2D brushTexture = new Texture2D(brushEditorObject.Width * tileSize, brushEditorObject.Height * tileSize) {
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point
			};

			_transparentTileGraphicPixelData = new Color32[tileSize * tileSize];
			for (int i = 0; i < _transparentTileGraphicPixelData.Length; i++) {
				_transparentTileGraphicPixelData[i] = new Color32(0, 0, 0, 0);
			}

			for (int y = 0; y < map.Count; y++) {
				for (int x = 0; x < map[y].Count; x++) {
					if (map[y][x] == 1) {
						int tileToPaint = GetPaintbrushTileId(brushTileset.Properties.Brushes[brushEditorObject.Id], brushEditorObject, x, y);

						if (tileToPaint > 0) {
							int textureId = brushTileset.Properties.TilesLookup[tileToPaint].AnimationFrames[0];
							Color32[] pixelData = brushTileset.PixelDatas[textureId];

							brushTexture.SetPixels32(
								x * tileSize, (brushEditorObject.Height - 1 - y) * tileSize, tileSize, tileSize,
								pixelData
							);
						} else {
							brushTexture.SetPixels32(
								x * tileSize, (brushEditorObject.Height - 1 - y) * tileSize, tileSize, tileSize,
								_transparentTileGraphicPixelData
							);
						}
					} else {
						brushTexture.SetPixels32(
							x * tileSize, (brushEditorObject.Height - 1 - y) * tileSize, tileSize, tileSize,
							_transparentTileGraphicPixelData
						);
					}
				}
			}

			brushTexture.Apply();

			return brushTexture;
		}

		private List<List<int>> ParseBrushEditorObjectDefinition(TileEditorObject brushEditorObject) {
			List<List<int>> parsedDefinition = new List<List<int>>();

			int width = brushEditorObject.Width;

			string[] cells = brushEditorObject.BrushMapDefinition.Split(',');

			List<int> currentRowData = new List<int>();

			for (int i = 0; i < cells.Length; i++) {
				int value;
				currentRowData.Add(int.TryParse(cells[i], out value) ? value : 0);

				if ((i + 1) % width == 0) {
					parsedDefinition.Add(currentRowData);
					currentRowData = new List<int>();
				}
			}

			return parsedDefinition;
		}

		private int GetPaintbrushTileId(TilesetEditorBrush brush, TileEditorObject editorObject, int x, int y) {
			
			
			if (editorObject.ParsedBrushMapDefinition[y][x] == 0) {
				return 0;
			}

			int height = editorObject.ParsedBrushMapDefinition.Count;
			int width = editorObject.ParsedBrushMapDefinition[0].Count;
			int bits = 0;

			bool topRight = y > 0 && x < width - 1 && editorObject.ParsedBrushMapDefinition[y - 1][x + 1] == 1;
			bool topMiddle = y > 0 && editorObject.ParsedBrushMapDefinition[y - 1][x] == 1;
			bool topLeft = y > 0 && x > 0 && editorObject.ParsedBrushMapDefinition[y - 1][x - 1] == 1;

			bool leftMiddle = x > 0 && editorObject.ParsedBrushMapDefinition[y][x - 1] == 1;
			bool rightMiddle = x < width - 1 && editorObject.ParsedBrushMapDefinition[y][x + 1] == 1;

			bool bottomLeft = y < height - 1 && x > 0 && editorObject.ParsedBrushMapDefinition[y + 1][x - 1] == 1;
			bool bottomMiddle = y < height - 1 && editorObject.ParsedBrushMapDefinition[y + 1][x] == 1;
			bool bottomRight = y < height - 1 && x < width - 1 && editorObject.ParsedBrushMapDefinition[y + 1][x + 1] == 1;

			if (editorObject.BrushTreatEdgeLikeSolid) {
				if (y + editorObject.Y == ActiveMap.Height - 1) {
					bottomLeft = true;
					bottomMiddle = true;
					bottomRight = true;
				}

				if (y - editorObject.Y == 0) {
					topLeft = true;
					topMiddle = true;
					topRight = true;
				}

				if (x + editorObject.X == ActiveMap.Width - 1) {
					topRight = true;
					rightMiddle = true;
					bottomRight = true;
				}

				if (x - editorObject.X == 0) {
					topLeft = true;
					leftMiddle = true;
					bottomLeft = true;
				}
			}

			if (rightMiddle) {
				bits += 1;
			}

			if (bottomRight && bottomMiddle && rightMiddle) {
				bits += 2;
			}

			if (bottomMiddle) {
				bits += 4;
			}

			if (bottomLeft && bottomMiddle && leftMiddle) {
				bits += 8;
			}

			if (leftMiddle) {
				bits += 16;
			}

			if (topLeft && topMiddle && leftMiddle) {
				bits += 32;
			}

			if (topMiddle) {
				bits += 64;
			}

			if (topRight && topMiddle && rightMiddle) {
				bits += 128;
			}

			Dictionary<int, int> mappedBitToTile = new Dictionary<int, int> {
				//9-slice
				{7, 0}, {31, 1}, {28, 2}, {199, 3}, {255, 4}, {124, 5}, {193, 6}, {241, 7}, {112, 8}, {0, 9}, {5, 10},
				//narrow-path
				{20, 11}, {65, 12}, {80, 13}, {68, 14}, {17, 15}, {1, 16}, {16, 17}, {64, 18}, {4, 19},
				//3-way intersections
				{23, 20}, {29, 21}, {21, 22}, {209, 23}, {113, 24}, {81, 25}, {71, 26}, {197, 27}, {69, 28}, {92, 29}, {116, 30}, {84, 31},
				//4-way intersections
				{223, 32}, {127, 33}, {247, 34}, {253, 35}, {95, 36}, {125, 37}, {215, 38}, {245, 39}, {221, 40}, {119, 41}, {87, 42}, {93, 43}, {213, 44},
				{117, 45}, {85, 46}
			};

			if (brush.ParsedDefinitions == null || brush.ParsedDefinitions.Count == 0) {
				brush.ParsedDefinitions = GetParsedBrushDefinitions(brush);
			}

			return mappedBitToTile.ContainsKey(bits)
				? brush.ParsedDefinitions[mappedBitToTile[bits]]
				: brush.ParsedDefinitions[4];
		}

		private Dictionary<int, int> GetParsedBrushDefinitions(TilesetEditorBrush brush) {
			Dictionary<int, int> brushDefinitions = new Dictionary<int, int>();

			if (string.IsNullOrEmpty(brush.Definitions)) {
				for (int i = 0; i < 48; i++) {
					brushDefinitions.Add(i, 0);
				}
			} else {
				string[] definitions = brush.Definitions.Split(';');

				foreach (string definition in definitions) {
					string[] parsedDefinition = definition.Split(':');

					if (parsedDefinition.Length == 2) {
						int key, value;

						brushDefinitions.Add(
							int.TryParse(parsedDefinition[0], out key) ? key : 0,
							int.TryParse(parsedDefinition[1], out value) ? value : 0
						);
					}
				}
			}

			return brushDefinitions;
		}

		private void ApplyBrushMatteAsEditorObject(TileEditorObject brushObject) {
			int tileSize = ActiveTileset.Properties.TileSize;

			List<List<int>> map = brushObject.ParsedBrushMapDefinition;

			int y = CropBrushMatteVertically(map);
			int x = CropBrushMatteHorizontally(map);

			if (map.Count == 0) {
				return;
			}

			int height = map.Count;
			int width = map[0].Count;

			string brushMapDefinition = "";

			for (int i = 0; i < map.Count; i++) {
				List<int> row = map[i];
				brushMapDefinition += string.Join(",", row);

				if (i < map.Count - 1) {
					brushMapDefinition += ",";
				}
			}

			Texture2D cachedTexture = new Texture2D(width * tileSize, height * tileSize) {
				wrapMode = TextureWrapMode.Clamp,
				filterMode = FilterMode.Point
			};

			Color[] pixels = _brushEditorObjectBeingEdited.CachedEditorTexture.GetPixels(
				x * tileSize,
				_brushEditorObjectBeingEdited.CachedEditorTexture.height - y * tileSize - height * tileSize,
				width * tileSize,
				height * tileSize
			);

			cachedTexture.SetPixels(pixels);
			cachedTexture.Apply();

			ActiveMapLayer.TileEditorObjects.Add(new TileEditorObject {
				TileEditorType = TileEditorObjectType.Brush,
				Id = _brushEditorObjectBeingEdited.Id,
				X = x,
				Y = y,
				Height = height,
				Width = width,
				BrushTreatEdgeLikeSolid = brushObject.BrushTreatEdgeLikeSolid,
				BrushMapDefinition = brushMapDefinition,
				ParsedBrushMapDefinition = new List<List<int>>(map),
				CachedEditorTexture = cachedTexture,
				CachedEditorTextureInfo = new CachedTileEditorObjectInfo {
					TileSet = ActiveMapLayer.Tileset,
					Id = 0,
					Width = width,
					Height = height
				}
			});
		}

		private int CropBrushMatteVertically(List<List<int>> map) {
			int rowsToDeleteFromStart = 0;

			for (int i = 0; i < map.Count; i++) {
				bool deleteThisRow = true;

				foreach (int cell in map[i]) {
					if (cell != 0) {
						deleteThisRow = false;
						break;
					}
				}

				if (!deleteThisRow) {
					rowsToDeleteFromStart = i;
					break;
				}
			}

			for (int i = 0; i < rowsToDeleteFromStart; i++) {
				map.RemoveAt(0);
			}

			for (int i = map.Count - 1; i >= 0; i--) {
				List<int> row = map[i];
				bool deleteThisRow = true;

				foreach (int cell in row) {
					if (cell != 0) {
						deleteThisRow = false;
						break;
					}
				}

				if (deleteThisRow) {
					map.RemoveAt(map.Count - 1);
				} else {
					break;
				}
			}

			return rowsToDeleteFromStart;
		}

		private int CropBrushMatteHorizontally(List<List<int>> map) {
			if (map.Count == 0) {
				return 0;
			}

			int earliestXIndex = map[0].Count;
			int latestXIndex = 0;

			foreach (List<int> row in map) {
				for (int i = 0; i < row.Count; i++) {
					int cell = row[i];
					if (cell > 0) {
						if (i < earliestXIndex) {
							earliestXIndex = i;
						}

						if (i > latestXIndex) {
							latestXIndex = i;
						}
					}
				}
			}

			foreach (List<int> row in map) {
				int cellsToDrop = row.Count - latestXIndex - 1;

				for (int i = 0; i < cellsToDrop; i++) {
					row.RemoveAt(row.Count - 1);
				}

				for (int i = 0; i < earliestXIndex; i++) {
					row.RemoveAt(0);
				}
			}

			return earliestXIndex;
		}
	}
}