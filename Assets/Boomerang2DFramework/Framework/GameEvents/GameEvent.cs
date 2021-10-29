namespace Boomerang2DFramework.Framework.GameEvents {
	public class GameEvent {
		protected GameEventProperties Properties;
		public float StartTime;
		public bool HasExecuted;

		public virtual void ApplyOutcome() { }
	}
}