using System.Collections.Generic;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.Weapons {
	/// <summary>
	/// JSON definition for Weapon Properties
	/// </summary>
	[System.Serializable]
	public class WeaponProperties {
		/// <summary>
		/// The Weapon's Name
		/// </summary>
		public string Name;
		
		/// <summary>
		/// A collection of Melee Strikes
		/// </summary>
		/// <remarks>
		/// A weapon does nothing on its own. It's made up of MeleeStrikes, each with their own properties. When the 
		/// weapon fires, it fires its MeleeStrikes and when they're all finished, the weapon is finished firing.
		/// </remarks>
		public List<MeleeStrikeProperties> MeleeStrikes = new List<MeleeStrikeProperties>();

		public Vector2 EditorBoundingDimensions = new Vector2(1,1);
		public int EditorDrawScale = 100;
		[System.NonSerialized] public Sprite EditorSprite;
		public Color EditorGuideLineColor = new Color(0.8f, 0, 0.8f, 0.5f);
		public Color EditorWeaponLineColor = new Color(0f, 0.8f, 0f, 1);
		public float EditorCrossLength = 15f;
		public bool EditorShowBounding = true;
		
		public float TotalWeaponDuration {
			get {
				float totalWeaponDuration = 0;

				foreach (MeleeStrikeProperties meleeStrikeProperties in MeleeStrikes) {
					if (meleeStrikeProperties.StartTime + meleeStrikeProperties.TotalTime > totalWeaponDuration) {
						totalWeaponDuration = meleeStrikeProperties.StartTime + meleeStrikeProperties.TotalTime;
					}
				}

				return totalWeaponDuration;
			}
		}
	}
}