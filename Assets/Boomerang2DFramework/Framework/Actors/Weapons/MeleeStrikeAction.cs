namespace Boomerang2DFramework.Framework.Actors.Weapons {
	/// <summary>
	/// JSON definition for a Melee Strike's Action 
	/// </summary>
	[System.Serializable]
	public class MeleeStrikeAction {
		public float Duration;
		public string EaseMode;
		public object StartState;
		public object EndState;
		public string StartStateProperties = "{}";
		public string EndStateProperties = "{}";
	}
}