using UnityEngine;

namespace Boomerang2DFramework.Framework.MapGeneration.PropertyClasses {
	/// <summary>
	/// JSON definition for the properties for placed prefabs on the map
	/// </summary>
	[System.Serializable]
	public class MapPrefabPlacementProperties {
		public string Prefab;
		public Vector2Int Position;
		public float DistanceAwayFromCamera;
	}
}