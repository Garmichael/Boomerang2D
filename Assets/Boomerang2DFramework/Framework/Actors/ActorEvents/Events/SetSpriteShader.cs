using System.Collections.Generic;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class SetSpriteShader : ActorEvent {
		private SetSpriteShaderProperties MyProperties => (SetSpriteShaderProperties) Properties;

		public SetSpriteShader(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			if (targetActor?.ActorProperties == null) {
				return;
			}
			
			if (BoomerangDatabase.ShaderDatabaseEntries.ContainsKey(MyProperties.ShaderName)) {
				Material material = new Material(BoomerangDatabase.ShaderDatabaseEntries[MyProperties.ShaderName]);
				Dictionary<string, float> floats = MyProperties.FloatsProperties.Dictionary;
				Dictionary<string, int> ints = MyProperties.IntsProperties.Dictionary;
				Dictionary<string, Color> colors = MyProperties.ColorsProperties.Dictionary;
				Dictionary<string, Texture> textures = MyProperties.TexturesProperties.Dictionary;

				if (floats != null) {
					foreach (KeyValuePair<string, float> property in floats) {
						material.SetFloat(property.Key, property.Value);
					}
				}

				if (ints != null) {
					foreach (KeyValuePair<string, int> property in ints) {
						material.SetInt(property.Key, property.Value);
					}
				}

				if (colors != null) {
					foreach (KeyValuePair<string, Color> property in colors) {
						material.SetColor(property.Key, property.Value);
					}
				}

				if (textures != null) {
					foreach (KeyValuePair<string, Texture> property in textures) {
						material.SetTexture(property.Key, property.Value);
					}
				}

				targetActor.SpriteRenderer.material = material;
				material.SetFloat(Shader.PropertyToID("_StartTime"), Time.time);
			}
		}
	}
}