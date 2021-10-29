using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Boomerang2DFramework.Framework.BoomerangDatabaseManagement {
	public static class BoomerangDatabase {
		private static GameObject _frameworkManagerGameObject = null;
		private static BoomerangReferenceDatabaseController _referenceDatabaseController;

		public static TextAsset GameFlagsStore;
		public static TextAsset DialogContentStore;
		public static AudioMixer AudioMixer;
		public static readonly Dictionary<string, TextAsset> ActorJsonDatabaseEntries = new Dictionary<string, TextAsset>();
		public static readonly Dictionary<string, Sprite[]> ActorSpriteDatabaseEntries = new Dictionary<string, Sprite[]>();
		public static readonly Dictionary<string, AudioClip> AudioClipDatabaseEntries = new Dictionary<string, AudioClip>();
		public static readonly Dictionary<string, Texture> TextureDatabaseEntries = new Dictionary<string, Texture>();
		public static readonly Dictionary<string, TextAsset> WeaponDatabaseEntries = new Dictionary<string, TextAsset>();
		public static readonly Dictionary<string, TextAsset> MapDatabaseEntries = new Dictionary<string, TextAsset>();
		public static readonly Dictionary<string, TextAsset> TilesetJsonDatabaseEntries = new Dictionary<string, TextAsset>();
		public static readonly Dictionary<string, GameObject> TileColliderDatabaseEntries = new Dictionary<string, GameObject>();
		public static readonly Dictionary<string, GameObject> MapPrefabDatabaseEntries = new Dictionary<string, GameObject>();
		public static readonly Dictionary<string, Shader> ShaderDatabaseEntries = new Dictionary<string, Shader>();
		public static readonly Dictionary<string, GameObject> ParticleEffectEntries = new Dictionary<string, GameObject>();
		public static readonly Dictionary<string, TextAsset> BitmapFontJsonEntries = new Dictionary<string, TextAsset>();
		public static readonly Dictionary<string, Sprite[]> BitmapFontSpriteEntries = new Dictionary<string, Sprite[]>();
		public static readonly Dictionary<string, TextAsset> HudObjectEntries = new Dictionary<string, TextAsset>();
		public static readonly Dictionary<string, Sprite[]> TilesetSpriteDatabaseEntries = new Dictionary<string, Sprite[]>();

		[Serializable]
		public struct SpriteDatabaseEntry {
			public string Name;
			public Sprite[] Sprites;
		}

#if UNITY_EDITOR
		[MenuItem("Tools/Boomerang2D/Build Reference Database", false, 51)]
		private static void BuildReferenceDatabase() {
			PopulateDatabase();
		}

		public static void PopulateDatabase() {
			if (_referenceDatabaseController == null) {
				_referenceDatabaseController = new BoomerangReferenceDatabaseController();
			}

			_referenceDatabaseController?.PopulateDatabaseReferenceContainer();
		}
#endif

		static BoomerangDatabase() {
			IndexContent();
		}
		
		public static void IndexContent() {
			if (_frameworkManagerGameObject == null) {
				_frameworkManagerGameObject = GameObject.Find(GameProperties.InitializingGameObjectName);
			}

			if (_frameworkManagerGameObject != null) {
				BoomerangReferenceDatabase reference =
					_frameworkManagerGameObject.GetComponent<BoomerangReferenceDatabase>();

				IndexGameFlags(reference);
				IndexDialogContent(reference);
				IndexAudio(reference);
				IndexTextures(reference);
				IndexActors(reference);
				IndexWeapons(reference);
				IndexMaps(reference);
				IndexTilesets(reference);
				IndexTileColliders(reference);
				IndexMapPrefabs(reference);
				IndexShaders(reference);
				IndexParticleEffects(reference);
				IndexBitmapFonts(reference);
				IndexHudObjects(reference);
			}
		}

		private static void IndexGameFlags(BoomerangReferenceDatabase reference) {
			GameFlagsStore = reference.GameFlagsJson;
		}

		private static void IndexDialogContent(BoomerangReferenceDatabase reference) {
			DialogContentStore = reference.DialogContentJson;
		}

		private static void IndexAudio(BoomerangReferenceDatabase reference) {
			AudioClipDatabaseEntries.Clear();
			AudioMixer = reference.AudioMixer;

			foreach (AudioClip audioClip in reference.AudioClips) {
				if (audioClip) {
					AudioClipDatabaseEntries.Add(audioClip.name, audioClip);
				}
			}
		}

		private static void IndexTextures(BoomerangReferenceDatabase reference) {
			TextureDatabaseEntries.Clear();

			foreach (Texture texture in reference.Textures) {
				if (texture) {
					TextureDatabaseEntries.Add(texture.name, texture);
				}
			}
		}

		private static void IndexActors(BoomerangReferenceDatabase reference) {
			ActorJsonDatabaseEntries.Clear();
			ActorSpriteDatabaseEntries.Clear();

			foreach (TextAsset actorDatabaseEntry in reference.ActorJsons) {
				if (actorDatabaseEntry) {
					if (!ActorJsonDatabaseEntries.ContainsKey(actorDatabaseEntry.name)) {
						ActorJsonDatabaseEntries.Add(actorDatabaseEntry.name, actorDatabaseEntry);
					} else {
						ActorJsonDatabaseEntries[actorDatabaseEntry.name] = actorDatabaseEntry;
					}
				}
			}

			foreach (SpriteDatabaseEntry actorSpritesEntry in reference.ActorSprites) {
				if (!ActorSpriteDatabaseEntries.ContainsKey(actorSpritesEntry.Name)) {
					ActorSpriteDatabaseEntries.Add(actorSpritesEntry.Name, actorSpritesEntry.Sprites);
				} else {
					ActorSpriteDatabaseEntries[actorSpritesEntry.Name] = actorSpritesEntry.Sprites;
				}
			}
		}

		private static void IndexWeapons(BoomerangReferenceDatabase reference) {
			WeaponDatabaseEntries.Clear();

			foreach (TextAsset weaponDatabaseEntry in reference.Weapons) {
				if (weaponDatabaseEntry) {
					if (!WeaponDatabaseEntries.ContainsKey(weaponDatabaseEntry.name)) {
						WeaponDatabaseEntries.Add(weaponDatabaseEntry.name, weaponDatabaseEntry);
					} else {
						WeaponDatabaseEntries[weaponDatabaseEntry.name] = weaponDatabaseEntry;
					}
				}
			}
		}

		private static void IndexMaps(BoomerangReferenceDatabase reference) {
			MapDatabaseEntries.Clear();

			foreach (TextAsset mapDatabaseEntry in reference.Maps) {
				if (mapDatabaseEntry) {
					if (!MapDatabaseEntries.ContainsKey(mapDatabaseEntry.name)) {
						MapDatabaseEntries.Add(mapDatabaseEntry.name, mapDatabaseEntry);
					} else {
						MapDatabaseEntries[mapDatabaseEntry.name] = mapDatabaseEntry;
					}
				}
			}
		}

		private static void IndexTilesets(BoomerangReferenceDatabase reference) {
			TilesetJsonDatabaseEntries.Clear();
			TilesetSpriteDatabaseEntries.Clear();

			foreach (TextAsset tilesetDatabaseEntry in reference.TilesetJsons) {
				if (tilesetDatabaseEntry) {
					if (!TilesetJsonDatabaseEntries.ContainsKey(tilesetDatabaseEntry.name)) {
						TilesetJsonDatabaseEntries.Add(tilesetDatabaseEntry.name, tilesetDatabaseEntry);
					} else {
						TilesetJsonDatabaseEntries[tilesetDatabaseEntry.name] = tilesetDatabaseEntry;
					}
				}
			}

			foreach (SpriteDatabaseEntry tilesetSpriteEntry in reference.TilesetSprites) {
				if (!TilesetSpriteDatabaseEntries.ContainsKey(tilesetSpriteEntry.Name)) {
					TilesetSpriteDatabaseEntries.Add(tilesetSpriteEntry.Name, tilesetSpriteEntry.Sprites);
				} else {
					TilesetSpriteDatabaseEntries[tilesetSpriteEntry.Name] = tilesetSpriteEntry.Sprites;
				}
			}
		}

		private static void IndexTileColliders(BoomerangReferenceDatabase reference) {
			TileColliderDatabaseEntries.Clear();

			foreach (GameObject tileColliderDatabaseEntry in reference.TileColliders) {
				if (tileColliderDatabaseEntry) {
					if (!TileColliderDatabaseEntries.ContainsKey(tileColliderDatabaseEntry.name)) {
						TileColliderDatabaseEntries.Add(tileColliderDatabaseEntry.name, tileColliderDatabaseEntry);
					} else {
						TileColliderDatabaseEntries[tileColliderDatabaseEntry.name] = tileColliderDatabaseEntry;
					}
				}
			}
		}

		private static void IndexMapPrefabs(BoomerangReferenceDatabase reference) {
			MapPrefabDatabaseEntries.Clear();

			foreach (GameObject mapPrefabDatabaseEntry in reference.MapPrefabs) {
				if (mapPrefabDatabaseEntry) {
					if (!MapPrefabDatabaseEntries.ContainsKey(mapPrefabDatabaseEntry.name)) {
						MapPrefabDatabaseEntries.Add(mapPrefabDatabaseEntry.name, mapPrefabDatabaseEntry);
					} else {
						MapPrefabDatabaseEntries[mapPrefabDatabaseEntry.name] = mapPrefabDatabaseEntry;
					}
				}
			}
		}

		private static void IndexShaders(BoomerangReferenceDatabase reference) {
			ShaderDatabaseEntries.Clear();

			foreach (Shader shaderDatabaseEntry in reference.Shaders) {
				if (shaderDatabaseEntry) {
					if (!ShaderDatabaseEntries.ContainsKey(shaderDatabaseEntry.name)) {
						ShaderDatabaseEntries.Add(shaderDatabaseEntry.name, shaderDatabaseEntry);
					} else {
						ShaderDatabaseEntries[shaderDatabaseEntry.name] = shaderDatabaseEntry;
					}
				}
			}
		}

		private static void IndexParticleEffects(BoomerangReferenceDatabase reference) {
			ParticleEffectEntries.Clear();

			foreach (GameObject particleEffectDatabaseEntry in reference.ParticleEffects) {
				if (particleEffectDatabaseEntry) {
					if (!ParticleEffectEntries.ContainsKey(particleEffectDatabaseEntry.name)) {
						ParticleEffectEntries.Add(particleEffectDatabaseEntry.name, particleEffectDatabaseEntry);
					} else {
						ParticleEffectEntries[particleEffectDatabaseEntry.name] = particleEffectDatabaseEntry;
					}
				}
			}
		}

		private static void IndexBitmapFonts(BoomerangReferenceDatabase reference) {
			BitmapFontJsonEntries.Clear();
			BitmapFontSpriteEntries.Clear();

			foreach (TextAsset bitmapFontJson in reference.BitmapFontJsons) {
				if (bitmapFontJson) {
					if (!BitmapFontJsonEntries.ContainsKey(bitmapFontJson.name)) {
						BitmapFontJsonEntries.Add(bitmapFontJson.name, bitmapFontJson);
					} else {
						BitmapFontJsonEntries[bitmapFontJson.name] = bitmapFontJson;
					}
				}
			}

			foreach (SpriteDatabaseEntry bitmapSpritesEntry in reference.BitmapFontSprites) {
				if (!BitmapFontSpriteEntries.ContainsKey(bitmapSpritesEntry.Name)) {
					BitmapFontSpriteEntries.Add(bitmapSpritesEntry.Name, bitmapSpritesEntry.Sprites);
				} else {
					BitmapFontSpriteEntries[bitmapSpritesEntry.Name] = bitmapSpritesEntry.Sprites;
				}
			}
		}

		private static void IndexHudObjects(BoomerangReferenceDatabase reference) {
			HudObjectEntries.Clear();

			foreach (TextAsset hudObjectEntry in reference.HudObjectJsons) {
				if (hudObjectEntry) {
					if (!HudObjectEntries.ContainsKey(hudObjectEntry.name)) {
						HudObjectEntries.Add(hudObjectEntry.name, hudObjectEntry);
					} else {
						HudObjectEntries[hudObjectEntry.name] = hudObjectEntry;
					}
				}
			}
		}
	}
}