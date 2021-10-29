using Boomerang2DFramework.Framework.AudioManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class BackgroundMusicStop : ActorEvent {
		public BackgroundMusicStop(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			AudioManager.StopBackgroundMusic();
		}
	}
}