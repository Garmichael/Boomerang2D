using System.Collections.Generic;
using System.IO;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Editor.ActorStudio {
	public partial class ActorStudio : EditorWindow {
		private void LoadActors() {
			_allActorProperties.Clear();

			foreach (KeyValuePair<string, TextAsset> actorJson in BoomerangDatabase.ActorJsonDatabaseEntries) {
				ActorProperties actorProperties = JsonUtility.FromJson<ActorProperties>(actorJson.Value.text);
				_allActorProperties.Add(actorProperties);
			}
		}

		private void LoadWeapons() {
			_allWeapons.Clear();

			foreach (KeyValuePair<string, TextAsset> weaponJson in BoomerangDatabase.WeaponDatabaseEntries) {
				_allWeapons.Add(weaponJson.Key);
			}
		}

		private void LoadParticleEffects() {
			_allParticleEffects.Clear();

			foreach (KeyValuePair<string, GameObject> particleEffect in BoomerangDatabase.ParticleEffectEntries) {
				_allParticleEffects.Add(particleEffect.Key);
			}
		}

		private void SaveActor() {
			File.WriteAllText(GameProperties.ActorContentDirectory + "/" + ActiveActor.Name + ".json", JsonUtility.ToJson(ActiveActor, true));
			AssetDatabase.Refresh();
			_fileHasChanged = false;
		}

		private void ImportImage(string importedImagePath) {
			string fileExtension = Path.GetExtension(importedImagePath);
			string fileName = ActiveActor.Name + fileExtension;
			string assetPath = GameProperties.ActorContentDirectory + "/" + fileName;

			if (File.Exists(assetPath)) {
				FileUtil.DeleteFileOrDirectory(assetPath);
				AssetDatabase.Refresh();
			}

			FileUtil.ReplaceFile(importedImagePath, assetPath);
			AssetDatabase.Refresh();
			
			SpriteSlicer.SliceSprite(assetPath, (int) ActiveActor.SpriteWidth, (int) ActiveActor.SpriteHeight);
			AssetDatabase.Refresh();
			
			BoomerangDatabase.PopulateDatabase();
			BoomerangDatabase.IndexContent();

			GenerateTexturesForSprite();
			SaveActor();
		}

		private void CreateActor(string actorName) {
			if (actorName.Trim() != "") {
				ActorProperties newActorProperties = new ActorProperties {
					Name = actorName.Trim()
				};

				string fileName = GameProperties.ActorContentDirectory + "/" + actorName + ".json";

				File.WriteAllText(fileName, JsonUtility.ToJson(newActorProperties, true));
				AssetDatabase.Refresh();
				BoomerangDatabase.PopulateDatabase();
				OnEnable();
			}
		}

		private void RenameActor(string newName) {
			if (ActiveActor == null) {
				return;
			}

			string oldName = ActiveActor.Name;

			AssetDatabase.DeleteAsset(GameProperties.ActorContentDirectory + "/" + oldName + ".json");
			AssetDatabase.DeleteAsset(GameProperties.ActorContentDirectory + "/" + oldName + ".png");

			ActiveActor.Name = newName;
			SaveActor();

			OnEnable();
		}

		private void DeleteActor() {
			if (ActiveActor == null) {
				return;
			}

			AssetDatabase.DeleteAsset(GameProperties.ActorContentDirectory + "/" + ActiveActor.Name + ".json");
			AssetDatabase.DeleteAsset(GameProperties.ActorContentDirectory + "/" + ActiveActor.Name + ".png");

			AssetDatabase.Refresh();
			BoomerangDatabase.PopulateDatabase();
			
			OnEnable();
		}

		private void GenerateTexturesForSprite() {
			if (ActiveActor == null) {
				return;
			}

			Sprite[] actorSprites = BoomerangDatabase.ActorSpriteDatabaseEntries[ActiveActor.Name];
			_activeActorTextures = new List<Texture2D>();

			if (actorSprites == null) {
				return;
			}

			foreach (Sprite sprite in actorSprites) {
				Texture2D croppedTexture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
				Color[] pixels = sprite.texture.GetPixels(
					(int) sprite.rect.x,
					(int) sprite.rect.y,
					(int) sprite.rect.width,
					(int) sprite.rect.height,
					0
				);

				croppedTexture.SetPixels(pixels, 0);
				croppedTexture.wrapMode = TextureWrapMode.Clamp;
				croppedTexture.filterMode = FilterMode.Point;
				croppedTexture.Apply();
				_activeActorTextures.Add(croppedTexture);
			}
		}

		private void OnSelectNewActor() {
			GenerateTexturesForSprite();
			_previousIndexForActiveActor = _indexForActiveActor;
			_indexForActiveState = 0;
			_indexForActiveAnimation = 0;
			_indexForNewModTrigger = 0;
			_indexForNewTransitionTrigger = 0;
		}
	}
}