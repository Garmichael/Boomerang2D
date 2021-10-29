namespace Boomerang2DFramework.Framework.Actors.ActorEvents {
	public class ActorEvent {
		protected ActorEventProperties Properties;
		public float StartTime;
		public bool HasExecuted;
		public bool AffectFilteredActors;

		/// <summary>
		/// Executes the Event's Logic
		/// </summary>
		/// <param name="targetActor">The Actor the script affects</param>
		/// <param name="sourceActor">The Actor that executes the event</param>
		public virtual void ApplyOutcome(Actor targetActor, Actor sourceActor) { }
	}
}