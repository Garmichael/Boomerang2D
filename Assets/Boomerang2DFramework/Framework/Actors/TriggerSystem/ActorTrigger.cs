using Boomerang2DFramework.Framework.StateManagement;

namespace Boomerang2DFramework.Framework.Actors.TriggerSystem {
	/// <summary>
	/// The Base Class for All ActorTriggers
	/// </summary>
	[System.Serializable]
	public class ActorTrigger {
		public ActorTrigger() { }

		/// <summary>
		/// The Trigger's Properties
		/// </summary>
		/// <param name="actorTriggerProperties"></param>
		public ActorTrigger(ActorTriggerProperties actorTriggerProperties) { }

		/// <summary>
		/// Execute to determine if the conditions of the trigger were met.
		/// The Main Logic for the Trigger belongs here
		/// </summary>
		/// <param name="actor">The Actor the State lives on</param>
		/// <param name="state">The State the Trigger lives on</param>
		/// <returns>A true/false if the conditions are met</returns>
		public virtual bool IsTriggered(Actor actor, State state) {
			return false;
		}
	}
}