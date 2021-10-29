using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Boomerang2DFramework.Framework.Actors.ActorStateProperties.AnimationProperties {
	/// <summary>
	/// JSON definition for a State Animation
	/// </summary>
	[System.Serializable]
	public class StatePropertiesAnimation {
		public string Name;

		/// <summary>
		/// Binds the animation to the global time.
		/// </summary>
		/// <remarks>
		/// When disabled, animations will start at the first frame when the State begins. When enabled, the animation
		/// will start on a frame calculated by the time elapsed since the game started. This allows all animated tiles
		/// and actors to be in sync, regardless of when they started their individual lifespans. 
		/// </remarks>
		public bool BoundToGlobalTimeManager;

		/// <summary>
		/// If enabled, the animation will loop forever. If disabled, the animation will defer to the FixedLoopCount
		/// property.
		/// </summary>
		public bool IndefinitelyLoops = true;

		/// <summary>
		/// Referenced when IndefinitelyLoops is disabled. This will define how many times the animation will repeat
		/// while in this State.
		/// </summary>
		/// <remarks>
		/// This count resets whenever the State is transitioned into again.
		/// </remarks>
		public int FixedLoopCount;

		/// <summary>
		/// When the animation finishes its final loop, this is the frame the animation display.
		/// </summary>
		public int FinalFrameAfterLoops;

		/// <summary>
		/// When enabled, the animation will start on the same frame number that the previous animation left off on,
		/// even if the previous animation was in another State.
		/// </summary>
		public bool StartOnExistingSpriteFrame;

		/// <summary>
		/// The total duration of the animation, calculated automatically.
		/// </summary>
		public float TotalDuration { get; private set; }

		/// <summary>
		/// When set, the animation's start point and duration is re-scaled to match the current playing point and
		/// duration of the specified weapon
		/// </summary>
		public bool BoundToWeaponDuration;
		
		/// <summary>
		/// The Weapon name referenced when BoundToWeaponDuration is enabled
		/// </summary>
		public string WeaponBoundTo;

		/// <summary>
		/// Collection of Properties for each Frame of the Animation
		/// </summary>
		public List<StatePropertiesAnimationFrame> AnimationFrames = new List<StatePropertiesAnimationFrame>();

		/// <summary>
		/// Triggers that determine if this Animation should play when multiple animations are applied to the
		/// same State.
		/// </summary>
		public List<StatePropertiesAnimationCondition> AnimationConditions =
			new List<StatePropertiesAnimationCondition>();

		/// <summary>
		/// Must be manually called to generate the total duration of an animation
		/// </summary>
		public void CalculateTotalDuration() {
			switch (AnimationFrames.Count) {
				case 0:
					Debug.LogWarning("Animation Has No Frames: " + Name);
					break;
				case 1:
					TotalDuration = 1;
					break;
				default:
					TotalDuration = AnimationFrames.Sum(frame => frame.Duration);
					break;
			}
		}
	}
}