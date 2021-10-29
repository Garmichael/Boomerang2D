using System.Collections.Generic;
using Boomerang2DFramework.Framework.Actors.PropertyClasses;
using UnityEngine;

namespace Boomerang2DFramework.Framework.MapGeneration.PropertyClasses {
	/// <summary>
	/// JSON definition for the properties for placed actors on the map
	/// </summary>
	[System.Serializable]
	public class MapActorPlacementProperties {
		public string Actor;
		public string MapId;
		public Vector2Int Position;
		public ActorInstanceProperties ActorInstanceProperties;
		public List<FloatStatProperties> ActorDefaultStatsFloats;
		public List<BoolStatProperties> ActorDefaultStatsBools;
		public List<StringStatProperties> ActorDefaultStatsStrings;
	}
}