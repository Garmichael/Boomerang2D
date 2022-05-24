using Boomerang2DFramework.Framework.MapGeneration;
using UnityEngine;

namespace Boomerang2DFramework.Framework {
	/// <summary>
	/// Example of a Start Game script.
	/// First, we always Initialize the Framework.
	/// Then, you can start executing commands. The Easiest is to build a map made in the Map Editor
	/// </summary>
	public class StartDemoGame : MonoBehaviour {
		private void Start() {
			Boomerang2D.InitializeFramework();
			MapManager.LoadMap("demoTown");
		}
	}
}