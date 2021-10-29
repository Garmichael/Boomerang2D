using Boomerang2DFramework.Framework.AudioManagement;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class BackgroundMusicStop : GameEvent {
		private BackgroundMusicStopProperties MyProperties => (BackgroundMusicStopProperties) Properties;

		public BackgroundMusicStop(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			AudioManager.StopBackgroundMusic();
		}
	}
}