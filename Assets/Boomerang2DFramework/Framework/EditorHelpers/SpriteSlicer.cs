using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Boomerang2DFramework.Framework.EditorHelpers {
	public static class SpriteSlicer {
#if UNITY_EDITOR
		public static void SliceSprite(string texturePath, int sizeX, int sizeY) {
			TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;

			Texture2D sourceTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D));
			if (textureImporter == null) {
				return;
			}

			textureImporter.textureType = TextureImporterType.Sprite;
			textureImporter.spriteImportMode = SpriteImportMode.Multiple;
			textureImporter.spritePixelsPerUnit = GameProperties.PixelsPerUnit;
			textureImporter.filterMode = FilterMode.Point;
			textureImporter.wrapMode = TextureWrapMode.Clamp;
			textureImporter.maxTextureSize = 2048;
			textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
			textureImporter.crunchedCompression = false;
			textureImporter.compressionQuality = 100;
			textureImporter.isReadable = true;
			textureImporter.textureShape = TextureImporterShape.Texture2D;
			textureImporter.npotScale = TextureImporterNPOTScale.None;

			AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);			
			EditorUtility.SetDirty(textureImporter);
			
			if (textureImporter == null) {
				return;
			}

			List<SpriteMetaData> spriteMetaDatas = new List<SpriteMetaData>();
			int frameNumber = 0;
			for (int j = sourceTexture.height; j > 0; j -= sizeY) {
				for (int i = 0; i < sourceTexture.width; i += sizeX) {
					SpriteMetaData spriteMetaData = new SpriteMetaData {
						name = sourceTexture.name + "_" + frameNumber,
						rect = new Rect(i, j - sizeY, sizeX, sizeY),
						alignment = 0,
						pivot = new Vector2(0f, 0f)
					};

					spriteMetaDatas.Add(spriteMetaData);
					frameNumber++;
				}
			}
			
			textureImporter.spritesheet = spriteMetaDatas.ToArray();
			AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
		}

		public static void SliceRegions(Texture2D sourceTexture, List<Rect> regions) {
			string name = sourceTexture.name;
			string texturePath = AssetDatabase.GetAssetPath(sourceTexture);
			int nameNumber = 0;

			TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;

			if (textureImporter == null) {
				return;
			}

			textureImporter.spritePixelsPerUnit = GameProperties.PixelsPerUnit;
			textureImporter.filterMode = FilterMode.Point;
			textureImporter.wrapMode = TextureWrapMode.Clamp;
			textureImporter.maxTextureSize = 2048;
			textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
			textureImporter.crunchedCompression = false;
			textureImporter.compressionQuality = 100;
			textureImporter.spriteImportMode = SpriteImportMode.Multiple;
			textureImporter.isReadable = true;

			List<SpriteMetaData> spriteMetaDatas = new List<SpriteMetaData>();

			foreach (Rect region in regions) {
				SpriteMetaData spriteMetaData = new SpriteMetaData {
					name = name + "_" + nameNumber,
					rect = region,
					alignment = 0,
					pivot = new Vector2(0f, 0f)
				};

				spriteMetaDatas.Add(spriteMetaData);
				nameNumber++;
			}

			textureImporter.spritesheet = spriteMetaDatas.ToArray();
			AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
		}
#endif
	}
}