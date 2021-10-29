using Boomerang2DFramework.Framework.AudioManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class BackgroundMusicPlay : ActorEvent {
		private BackgroundMusicPlayProperties MyProperties => (BackgroundMusicPlayProperties) Properties;

		public BackgroundMusicPlay(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			AudioManager.PlayBackgroundMusic(MyProperties.MusicName);
		}
	}
}