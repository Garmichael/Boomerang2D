using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.Actors.ActorStateProperties.SoundEffectProperties {
	/// <summary>
	/// JSON definition for an Actor's State's Sound Effects
	/// </summary>
	[System.Serializable]
	public class StatePropertiesSoundEffect {
		/// <summary>
		/// A list of Sound Effects that can be played in this State, referenced by name
		/// </summary>
		public List<string> SoundEffectPool = new List<string>();
		
		/// <summary>
		/// The Volume of the sound effects played by this state
		/// </summary>
		public float Volume = 1;
		
		/// <summary>
		/// If enabled, the sound effects loop indefinitely
		/// </summary>
		public bool LoopEffect = false;
		
		/// <summary>
		/// If LoopEffect is not enabled, then this controls how many times to repeat
		/// </summary>
		public int PlayCount = 1;
		
		/// <summary>
		/// When enabled, will randomly play sound effects from the pool. When disabled, the sound effects will play
		/// sequentially in order 
		/// </summary>
		public bool RandomOrder = false;
		
		/// <summary>
		/// How many seconds to wait after entering this state before playing the sound effects
		/// </summary>
		public float StartTime = 0;
		
		/// <summary>
		/// Enable this when the sound effects are supposed to be playing. Disable it when they're done playing.
		/// </summary>
		public bool HasStarted = false;
		
		/// <summary>
		/// When enabled, the sound effect will immediately stop mid-playing, when the State is transitioned out of.
		/// When disabled, the sound effect continues to play when the State is transitioned out of.
		/// </summary>
		/// <remarks>
		/// This does not affect looping sound effects. The loops will always cease after the currently playing effect
		/// finishes, whether its cut off or not. 
		/// </remarks>
		public bool ImmediateKillOnExitState = true;
	}
}