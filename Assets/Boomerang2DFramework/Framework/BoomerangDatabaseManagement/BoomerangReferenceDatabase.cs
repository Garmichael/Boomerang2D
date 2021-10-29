using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Boomerang2DFramework.Framework.BoomerangDatabaseManagement {
	public class BoomerangReferenceDatabase : MonoBehaviour {
		public TextAsset GameFlagsJson;
		public TextAsset DialogContentJson;
		public AudioMixer AudioMixer;
		public List<AudioClip> AudioClips = new List<AudioClip>();
		public List<Texture> Textures = new List<Texture>();
		public List<TextAsset> ActorJsons = new List<TextAsset>();
		public List<BoomerangDatabase.SpriteDatabaseEntry> ActorSprites = new List<BoomerangDatabase.SpriteDatabaseEntry>();
		public List<TextAsset> Weapons = new List<TextAsset>();
		public List<TextAsset> Maps = new List<TextAsset>();
		public List<TextAsset> TilesetJsons = new List<TextAsset>();
		public List<BoomerangDatabase.SpriteDatabaseEntry> TilesetSprites = new List<BoomerangDatabase.SpriteDatabaseEntry>();
		public List<GameObject> TileColliders = new List<GameObject>();
		public List<GameObject> MapPrefabs = new List<GameObject>();
		public List<Shader> Shaders = new List<Shader>();
		public List<GameObject> ParticleEffects = new List<GameObject>();
		public List<TextAsset> BitmapFontJsons = new List<TextAsset>();
		public List<BoomerangDatabase.SpriteDatabaseEntry> BitmapFontSprites = new List<BoomerangDatabase.SpriteDatabaseEntry>();
		public List<TextAsset> HudObjectJsons = new List<TextAsset>();
	}
}