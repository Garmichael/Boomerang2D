using System.Collections.Generic;
using System.Linq;

namespace Boomerang2DFramework.Framework.Actors.Weapons {
	/// <summary>
	/// JSON definition for a Melee Strike
	/// </summary>
	[System.Serializable]
	public class MeleeStrikeProperties {
		public string StrikeTypeClass;
		public string StrikeTypePropertiesClass;
		
		public float StartTime;
		public List<MeleeStrikeAction> Actions;

		public float TotalTime {
			get { return Actions.Sum(action => action.Duration); }
		}
	}
}