using Boomerang2DFramework.Framework.AudioManagement;

namespace Boomerang2DFramework.Framework.Actors.ActorEvents.Events {
	public class PlaySoundEffect : ActorEvent {
		private PlaySoundEffectProperties MyProperties => (PlaySoundEffectProperties) Properties;

		public PlaySoundEffect(ActorEventProperties properties) {
			Properties = properties;
		}

		public override void ApplyOutcome(Actor targetActor, Actor sourceActor) {
			AudioManager.PlayOnce(MyProperties.SoundEffectName);
		}
	}
}