using Boomerang2DFramework.Framework.AudioManagement;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class BackgroundMusicPlay : GameEvent {
		private BackgroundMusicPlayProperties MyProperties => (BackgroundMusicPlayProperties) Properties;

		public BackgroundMusicPlay(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			AudioManager.PlayBackgroundMusic(MyProperties.MusicName);
		}
	}
}