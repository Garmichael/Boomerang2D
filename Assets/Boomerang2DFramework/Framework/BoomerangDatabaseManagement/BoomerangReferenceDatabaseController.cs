using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace Boomerang2DFramework.Framework.BoomerangDatabaseManagement {
	public class BoomerangReferenceDatabaseController {
		private BoomerangReferenceDatabase _reference;


#if UNITY_EDITOR
		public void PopulateDatabaseReferenceContainer() {
			AssetDatabase.Refresh();
			BuildRequiredDirectories();
			BuildRequiredFiles();
			GetReferenceContainer();

			if (_reference != null) {
				PopulateGameFlagData();
				PopulateDialogContentItems();
				PopulateAudioData();
				PopulateTextureData();
				PopulateActorData();
				PopulateWeaponData();
				PopulateMapData();
				PopulateTilesetData();
				PopulateTileColliderData();
				PopulateMapPrefabsData();
				PopulateShaderData();
				PopulateParticleEffectData();
				PopulateBitmapFontData();
				PopulateHudObjectData();

				BoomerangDatabase.IndexContent();

				Debug.Log("Boomerang 2D Database Rebuilt at " + System.DateTime.Now);

				if (!Application.isPlaying) {
					string[] path = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path.Split(char.Parse("/"));

					bool sceneIsSaved = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
						UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene(), string.Join("/", path)
					);

					if (!sceneIsSaved) {
						Debug.Log("There was an error saving the Scene with the new Database. Please Save Manually.");
					}
				} else {
					Debug.LogWarning("Saving a Boomerang2D Asset while the preview is running will not save the Scene.");
				}
			}
		}

		private void BuildRequiredDirectories() {
			List<string> folders = new() {
				GameProperties.ActorContentDirectory,
				GameProperties.AudioContentDirectory,
				GameProperties.TextureBuiltInDirectory,
				GameProperties.TextureCustomDirectory,
				GameProperties.WeaponContentDirectory,
				GameProperties.MapContentDirectory,
				GameProperties.TilesetContentDirectory,
				GameProperties.TileCollidersCustomContentDirectory,
				GameProperties.SpriteShadersCustomDirectory,
				GameProperties.ParticleEffectContentDirectory,
				GameProperties.BitmapFontsContentDirectory,
				GameProperties.HudObjectsContentDirectory,
				GameProperties.MapPrefabsContentDirectory,
				GameProperties.DialogContentContentDirectory,
				GameProperties.CustomActorStateClassNs,
				GameProperties.CustomCameraStateClassNs,
				GameProperties.CustomActorTriggerClassNs,
				GameProperties.CustomActorFilterClassNs,
				GameProperties.CustomActorEventsClassNs,
				GameProperties.CustomGameEventsClassNs,
				GameProperties.CustomWeaponStrikeClassNs,
				GameProperties.CustomUiElementsPropertiesClassNs,
				GameProperties.CustomUiElementsBehaviorClassNs,
				GameProperties.CustomUiElementsEditorClassNs,
				GameProperties.CustomHudObjectTriggerClassNs
			};

			foreach (string folder in folders) {
				string normalizedFolderName = folder.Replace(".", "/");
				List<string> fullPath = normalizedFolderName.Split('/').ToList();
				if (fullPath[0] != "Assets") {
					fullPath.Insert(0, "Assets");
				}

				for (int i = 0; i < fullPath.Count - 1; i++) {
					string pathToHere = string.Join("/", fullPath.Take(i + 1));

					if (!AssetDatabase.IsValidFolder(pathToHere + "/" + fullPath[i + 1])) {
						AssetDatabase.CreateFolder(pathToHere, fullPath[i + 1]);
						AssetDatabase.Refresh();
					}
				}
			}
		}

		private void BuildRequiredFiles() {
			Debug.Log("Building Database..");
			Dictionary<string, string> files = new() {
				{ GameProperties.DialogContentContentFile, "{}" }
			};

			foreach (KeyValuePair<string, string> file in files) {
				bool fileExists = (TextAsset) AssetDatabase.LoadAssetAtPath(file.Key, typeof(TextAsset))  != null;

				if (!fileExists) {
					File.WriteAllText(file.Key, file.Value);
					AssetDatabase.Refresh();
				}
			}
		}

		private void GetReferenceContainer() {
			GameObject startGameObject = GameObject.Find(GameProperties.InitializingGameObjectName);
			if (startGameObject == null) {
				return;
			}

			_reference = startGameObject.GetComponent<BoomerangReferenceDatabase>();
		}

		private void PopulateGameFlagData() {
			_reference.GameFlagsJson =
				(TextAsset) AssetDatabase.LoadAssetAtPath(GameProperties.GameFlagContentFile, typeof(TextAsset));
		}

		private void PopulateDialogContentItems() {
			_reference.DialogContentJson =
				(TextAsset) AssetDatabase.LoadAssetAtPath(GameProperties.DialogContentContentFile, typeof(TextAsset));
		}

		private void PopulateAudioData() {
			List<AudioClip> data = new();
			_reference.AudioMixer =
				(AudioMixer) AssetDatabase.LoadAssetAtPath(GameProperties.AudioMixerAssetFile, typeof(AudioMixer));

			string[] fileNames = AssetDatabase.FindAssets("t:AudioClip", new[] { GameProperties.AudioContentDirectory });
			foreach (string fileName in fileNames) {
				data.Add((AudioClip) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(fileName),
					typeof(AudioClip)));
			}

			_reference.AudioClips = data;
		}

		private void PopulateTextureData() {
			List<Texture> data = new();

			string[] fileNames = AssetDatabase.FindAssets(
				"t:Texture", new[] {
					GameProperties.TextureBuiltInDirectory,
					GameProperties.TextureCustomDirectory,
				});
			foreach (string fileName in fileNames) {
				data.Add((Texture) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(fileName),
					typeof(Texture)));
			}

			_reference.Textures = data;
		}

		private void PopulateActorData() {
			List<TextAsset> jsonData = new();
			List<BoomerangDatabase.SpriteDatabaseEntry> spriteData = new();

			string[] jsonFileNames =
				AssetDatabase.FindAssets("t:TextAsset", new[] { GameProperties.ActorContentDirectory });

			foreach (string fileName in jsonFileNames) {
				TextAsset json =
					(TextAsset) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(fileName),
						typeof(TextAsset));
				jsonData.Add(json);
			}

			foreach (TextAsset actorJson in jsonData) {
				string path = GameProperties.ActorContentDirectory + "/" + actorJson.name + ".png";
				Sprite[] actorSprites =
					AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().ToArray();
				spriteData.Add(new BoomerangDatabase.SpriteDatabaseEntry {
					Name = actorJson.name,
					Sprites = actorSprites
				});
			}

			_reference.ActorJsons = jsonData;
			_reference.ActorSprites = spriteData;
		}

		private void PopulateWeaponData() {
			List<TextAsset> data = new();

			string[] jsonFileNames =
				AssetDatabase.FindAssets("t:TextAsset", new[] { GameProperties.WeaponContentDirectory });

			foreach (string fileName in jsonFileNames) {
				TextAsset json =
					(TextAsset) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(fileName),
						typeof(TextAsset));
				data.Add(json);
			}

			_reference.Weapons = data;
		}

		private void PopulateMapData() {
			List<TextAsset> data = new();

			string[] jsonFileNames =
				AssetDatabase.FindAssets("t:TextAsset", new[] { GameProperties.MapContentDirectory });

			foreach (string fileName in jsonFileNames) {
				TextAsset json =
					(TextAsset) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(fileName),
						typeof(TextAsset));
				data.Add(json);
			}

			_reference.Maps = data;
		}

		private void PopulateTilesetData() {
			List<TextAsset> jsonData = new();
			List<BoomerangDatabase.SpriteDatabaseEntry> spriteData = new();

			string[] jsonFileNames =
				AssetDatabase.FindAssets("t:TextAsset", new[] { GameProperties.TilesetContentDirectory });

			foreach (string fileName in jsonFileNames) {
				TextAsset json =
					(TextAsset) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(fileName),
						typeof(TextAsset));
				jsonData.Add(json);
			}

			foreach (TextAsset tilesetJson in jsonData) {
				string path = GameProperties.TilesetContentDirectory + "/" + tilesetJson.name + ".png";
				Sprite[] tilesetSprites =
					AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().ToArray();
				spriteData.Add(new BoomerangDatabase.SpriteDatabaseEntry {
					Name = tilesetJson.name,
					Sprites = tilesetSprites
				});
			}

			_reference.TilesetJsons = jsonData;
			_reference.TilesetSprites = spriteData;
		}

		private void PopulateTileColliderData() {
			List<GameObject> data = new();

			string[] prefabFileNames = AssetDatabase.FindAssets(
				"t:GameObject", new[] {
					GameProperties.TileCollidersBuiltInDirectory,
					GameProperties.TileCollidersCustomContentDirectory
				});

			foreach (string fileName in prefabFileNames) {
				GameObject collider =
					(GameObject) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(fileName),
						typeof(GameObject));
				data.Add(collider);
			}

			_reference.TileColliders = data;
		}

		private void PopulateMapPrefabsData() {
			List<GameObject> data = new();

			string[] prefabFileNames = AssetDatabase.FindAssets(
				"t:GameObject", new[] {
					GameProperties.MapPrefabsContentDirectory
				});

			foreach (string fileName in prefabFileNames) {
				GameObject prefab =
					(GameObject) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(fileName),
						typeof(GameObject));
				data.Add(prefab);
			}

			_reference.MapPrefabs = data;
		}

		private void PopulateShaderData() {
			List<Shader> data = new() {
				Shader.Find("Unlit/Transparent"),
				Shader.Find("Sprites/Default"),
				Shader.Find("Sprites/Diffuse"),
				Shader.Find("Sprites/Mask")
			};

			string[] jsonFileNames = AssetDatabase.FindAssets(
				"t:Shader", new[] {
					GameProperties.SpriteShadersBuiltInDirectory,
					GameProperties.SpriteShadersCustomDirectory
				});

			foreach (string fileName in jsonFileNames) {
				Shader shader =
					(Shader) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(fileName), typeof(Shader));
				data.Add(shader);
			}

			_reference.Shaders = data;
		}

		private void PopulateParticleEffectData() {
			List<GameObject> data = new();

			string[] particleEffects = AssetDatabase.FindAssets(
				"t:GameObject", new[] {
					GameProperties.ParticleEffectContentDirectory
				});

			foreach (string fileName in particleEffects) {
				GameObject particleEffect =
					(GameObject) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(fileName),
						typeof(GameObject));
				data.Add(particleEffect);
			}

			_reference.ParticleEffects = data;
		}

		private void PopulateBitmapFontData() {
			List<TextAsset> jsonData = new();
			List<BoomerangDatabase.SpriteDatabaseEntry> spriteData = new();

			string[] jsonFileNames =
				AssetDatabase.FindAssets("t:TextAsset", new[] { GameProperties.BitmapFontsContentDirectory });

			foreach (string fileName in jsonFileNames) {
				TextAsset json =
					(TextAsset) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(fileName),
						typeof(TextAsset));
				jsonData.Add(json);
			}

			foreach (TextAsset bitmapFontJson in jsonData) {
				string path = GameProperties.BitmapFontsContentDirectory + "/" + bitmapFontJson.name + ".png";
				Sprite[] bitmapFontSprites =
					AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Sprite>().ToArray();
				spriteData.Add(new BoomerangDatabase.SpriteDatabaseEntry {
					Name = bitmapFontJson.name,
					Sprites = bitmapFontSprites
				});
			}

			_reference.BitmapFontJsons = jsonData;
			_reference.BitmapFontSprites = spriteData;
		}

		private void PopulateHudObjectData() {
			List<TextAsset> jsonData = new();
			string[] jsonFileNames =
				AssetDatabase.FindAssets("t:TextAsset", new[] { GameProperties.HudObjectsContentDirectory });

			foreach (string fileName in jsonFileNames) {
				TextAsset json =
					(TextAsset) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(fileName),
						typeof(TextAsset));
				jsonData.Add(json);
			}

			_reference.HudObjectJsons = jsonData;
		}
#endif
	}
}