using Boomerang2DFramework.Framework.AudioManagement;

namespace Boomerang2DFramework.Framework.GameEvents.Events {
	public class PlaySoundEffect : GameEvent {
		private PlaySoundEffectProperties MyProperties => (PlaySoundEffectProperties) Properties;

		public PlaySoundEffect(GameEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome() {
			AudioManager.PlayOnce(MyProperties.SoundEffectName);
		}
	}
}