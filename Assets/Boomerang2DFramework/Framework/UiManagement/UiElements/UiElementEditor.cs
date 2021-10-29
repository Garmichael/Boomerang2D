using System;
using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.EditorHelpers;
using Boomerang2DFramework.Framework.MapGeneration.PropertyClasses;
using UnityEngine;

namespace Boomerang2DFramework.Framework.UiManagement.UiElements {
	/// <summary>
	/// The Base Class used by the HudObject Editor for an Element
	/// </summary>
	public class UiElementEditor {
#if UNITY_EDITOR
		public virtual string RenderPropertiesForm(string propertiesJson, Type propertiesType) {
			SuperForms.Label("Override RenderPropertiesForm()");
			return "{}";
		}

		public virtual void RenderPreview(int renderScale, string propertiesJson, Type propertiesType) {
			SuperForms.Label("Override RenderPreview()");
		}

		protected List<Texture2D> LoadTilesetTextures(TilesetProperties tilesetProperties) {
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

		protected List<Color32[]> LoadTilesetPixelDatas(List<Texture2D> textures) {
			return textures.Select(texture => texture.GetPixels32()).ToList();
		}
#endif
	}
}