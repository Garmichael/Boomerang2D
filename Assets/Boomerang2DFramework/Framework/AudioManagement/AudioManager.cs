using System;
using System.Collections.Generic;
using System.Linq;
using Boomerang2DFramework.Framework.Actors.ActorStateProperties.SoundEffectProperties;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using Boomerang2DFramework.Framework.GlobalTimeManagement;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Boomerang2DFramework.Framework.AudioManagement {
	public static class AudioManager {
		private static readonly List<AudioSource> BackgroundMusicChannels;
		private static readonly List<AudioSource> SfxChannels;
		private static int _currentBackgroundChannel;
		private static readonly AudioMixerSnapshot BackgroundChannelAMax;
		private static readonly AudioMixerSnapshot BackgroundChannelBMax;

		static AudioManager() {
			GameObject container = GameObject.Find(GameProperties.InitializingGameObjectName);

			AudioMixer audioMixer = BoomerangDatabase.AudioMixer;
			BackgroundMusicChannels = new List<AudioSource> {
				container.AddComponent<AudioSource>(),
				container.AddComponent<AudioSource>()
			};
			BackgroundMusicChannels[0].outputAudioMixerGroup = audioMixer.FindMatchingGroups("Background Music A")[0];
			BackgroundMusicChannels[1].outputAudioMixerGroup = audioMixer.FindMatchingGroups("Background Music B")[0];
			BackgroundMusicChannels[0].loop = true;
			BackgroundMusicChannels[1].loop = true;
			_currentBackgroundChannel = 0;

			BackgroundChannelAMax = audioMixer.FindSnapshot("BackgroundAMax");
			BackgroundChannelBMax = audioMixer.FindSnapshot("BackgroundBMax");

			SfxChannels = new List<AudioSource>();
			for (int i = 0; i < 100; i++) {
				AudioSource newChannel = container.AddComponent<AudioSource>();
				SfxChannels.Add(newChannel);
				newChannel.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SoundEffects")[0];
			}
		}

		public static bool IsPlayingBackgroundMusic() {
			return BackgroundMusicChannels[_currentBackgroundChannel].isPlaying;
		}

		public static void PlayBackgroundMusic(string name, float volume = 1f) {
			if (!BoomerangDatabase.AudioClipDatabaseEntries.ContainsKey(name)) {
				Debug.LogWarning("Audio " + name + "does not exist in the Catalog");
				return;
			}

			if (BackgroundMusicChannels[_currentBackgroundChannel].clip == BoomerangDatabase.AudioClipDatabaseEntries[name] &&
			    BackgroundMusicChannels[_currentBackgroundChannel].isPlaying
			) {
				return;
			}

			BackgroundMusicChannels[_currentBackgroundChannel].clip = BoomerangDatabase.AudioClipDatabaseEntries[name];
			BackgroundMusicChannels[_currentBackgroundChannel].volume = volume;
			BackgroundMusicChannels[_currentBackgroundChannel].Play();
		}

		public static void StopBackgroundMusic() {
			BackgroundMusicChannels[_currentBackgroundChannel].Stop();
		}

		public static void FadeOutBackgroundMusic(float fadeOutDuration) {
			if (_currentBackgroundChannel == 0) {
				_currentBackgroundChannel = 1;
				BackgroundMusicChannels[1].Stop();
				BackgroundChannelBMax.TransitionTo(fadeOutDuration);
			} else {
				_currentBackgroundChannel = 0;
				BackgroundMusicChannels[0].Stop();
				BackgroundChannelAMax.TransitionTo(fadeOutDuration);
			}
		}

		public static void FadeInBackgroundMusic(float fadeOutDuration) {
			if (_currentBackgroundChannel == 0) {
				BackgroundMusicChannels[1].Stop();
				BackgroundChannelAMax.TransitionTo(fadeOutDuration);
			} else {
				BackgroundMusicChannels[0].Stop();
				BackgroundChannelBMax.TransitionTo(fadeOutDuration);
			}
		}

		public static void CrossFadeBackgroundMusicTo(string name, float crossFadeDuration) {
			if (BackgroundMusicChannels[_currentBackgroundChannel].clip != null &&
			    BackgroundMusicChannels[_currentBackgroundChannel].clip.name == name
			) {
				return;
			}

			if (_currentBackgroundChannel == 0) {
				_currentBackgroundChannel = 1;
				BackgroundMusicChannels[_currentBackgroundChannel].clip = BoomerangDatabase.AudioClipDatabaseEntries[name];
				BackgroundMusicChannels[_currentBackgroundChannel].Play();
				BackgroundChannelBMax.TransitionTo(crossFadeDuration);
			} else {
				_currentBackgroundChannel = 0;
				BackgroundMusicChannels[_currentBackgroundChannel].clip = BoomerangDatabase.AudioClipDatabaseEntries[name];
				BackgroundMusicChannels[_currentBackgroundChannel].Play();
				BackgroundChannelAMax.TransitionTo(crossFadeDuration);
			}
		}

		public static void PlayOnce(string name, float volume = 1f) {
			if (!BoomerangDatabase.AudioClipDatabaseEntries.ContainsKey(name)) {
				LogError(AudioManagerErrorCode.NotInCatalog);
				return;
			}

			AudioSource selectedAudioSource = SfxChannels.FirstOrDefault(audioSource => !audioSource.isPlaying);

			if (selectedAudioSource == null) {
				return;
			}

			selectedAudioSource.PlayOneShot(BoomerangDatabase.AudioClipDatabaseEntries[name], volume);
		}

		public static void PlayOnceFromPool(List<string> pool, bool isRandom, float volume = 1f) {
			string name = isRandom ? pool[Random.Range(0, pool.Count)] : pool[0];

			if (!BoomerangDatabase.AudioClipDatabaseEntries.ContainsKey(name)) {
				LogError(AudioManagerErrorCode.NotInCatalog);
				return;
			}

			AudioSource selectedAudioSource = SfxChannels.FirstOrDefault(audioSource => !audioSource.isPlaying);

			if (selectedAudioSource == null) {
				return;
			}

			selectedAudioSource.PlayOneShot(BoomerangDatabase.AudioClipDatabaseEntries[name], volume);
		}

		public static void PlayXTimes(string name, int timesToPlay, float volume = 1f) {
			if (!BoomerangDatabase.AudioClipDatabaseEntries.ContainsKey(name)) {
				LogError(AudioManagerErrorCode.NotInCatalog);
				return;
			}

			AudioSource selectedAudioSource = SfxChannels.FirstOrDefault(audioSource => !audioSource.isPlaying);

			if (selectedAudioSource == null) {
				return;
			}

			for (int i = 0; i < timesToPlay; i++) {
				GlobalTimeManager.PerformAfter(BoomerangDatabase.AudioClipDatabaseEntries[name].length * i,
					() => { selectedAudioSource.PlayOneShot(BoomerangDatabase.AudioClipDatabaseEntries[name], volume); });
			}
		}

		public static void PlayXTimesFromPool(List<string> pool, int timesToPlay, bool isRandom, float volume = 1f) {
			List<string> soundEffectsToPlay = new List<string>();

			for (int i = 0; i < timesToPlay; i++) {
				if (isRandom) {
					soundEffectsToPlay.Add(pool[Random.Range(0, pool.Count)]);
				} else {
					int targetedIndex = i;
					while (targetedIndex >= pool.Count) {
						targetedIndex -= pool.Count;
					}

					soundEffectsToPlay.Add(pool[targetedIndex]);
				}
			}

			for (int i = 0; i < timesToPlay; i++) {
				string name = soundEffectsToPlay[i];

				if (!BoomerangDatabase.AudioClipDatabaseEntries.ContainsKey(name)) {
					LogError(AudioManagerErrorCode.NotInCatalog);
					return;
				}

				AudioSource selectedAudioSource = SfxChannels.FirstOrDefault(audioSource => !audioSource.isPlaying);

				if (selectedAudioSource == null) {
					return;
				}

				float startTime = 0;
				for (int j = 0; j < i; j++) {
					startTime += BoomerangDatabase.AudioClipDatabaseEntries[soundEffectsToPlay[j]].length;
				}

				GlobalTimeManager.PerformAfter(startTime, () => { selectedAudioSource.PlayOneShot(BoomerangDatabase.AudioClipDatabaseEntries[name], volume); });
			}
		}

		public static LoopingAudioEffect PlayLoop(StatePropertiesSoundEffect soundEffect) {
			return PlayLoop(soundEffect.SoundEffectPool, soundEffect.RandomOrder, soundEffect.ImmediateKillOnExitState, soundEffect.Volume);
		}

		public static LoopingAudioEffect PlayLoop(string soundEffect, bool isRandom, bool immediatelyDie, float volume = 1f) {
			return PlayLoop(new List<string> {soundEffect}, isRandom, immediatelyDie, volume);
		}

		public static LoopingAudioEffect PlayLoop(List<string> pool, bool isRandom, bool immediatelyDie, float volume = 1f) {
			AudioSource selectedAudioSource = null;
			foreach (AudioSource audioSource in SfxChannels) {
				if (audioSource.isPlaying) {
					continue;
				}

				selectedAudioSource = audioSource;
			}

			if (selectedAudioSource == null) {
				return new LoopingAudioEffect();
			}

			LoopingAudioEffect newLoopingAudioEffect = new LoopingAudioEffect {
				AudioSource = selectedAudioSource,
				Pool = pool,
				IsRandom = isRandom,
				Volume = volume,
				ImmediatelyDie = immediatelyDie,
				ReadyToDie = false,
				CurrentPlayingItemIndex = 0
			};

			ContinueLoop(newLoopingAudioEffect);
			return newLoopingAudioEffect;
		}

		private static void ContinueLoop(LoopingAudioEffect loopingAudioEffect) {
			string name = loopingAudioEffect.IsRandom
				? loopingAudioEffect.Pool[Random.Range(0, loopingAudioEffect.Pool.Count)]
				: loopingAudioEffect.Pool[loopingAudioEffect.CurrentPlayingItemIndex];

			loopingAudioEffect.AudioSource.PlayOneShot(BoomerangDatabase.AudioClipDatabaseEntries[name], loopingAudioEffect.Volume);

			GlobalTimeManager.PerformAfter(BoomerangDatabase.AudioClipDatabaseEntries[name].length, () => {
				if (!loopingAudioEffect.ReadyToDie) {
					loopingAudioEffect.CurrentPlayingItemIndex++;
					if (loopingAudioEffect.CurrentPlayingItemIndex >= loopingAudioEffect.Pool.Count) {
						loopingAudioEffect.CurrentPlayingItemIndex = 0;
					}

					ContinueLoop(loopingAudioEffect);
				}
			});
		}

		public static void StopLoop(LoopingAudioEffect loopingAudioEffect) {
			loopingAudioEffect.ReadyToDie = true;
			if (loopingAudioEffect.ImmediatelyDie) {
				loopingAudioEffect.AudioSource.Stop();
			}
		}

		public static AudioSource PlayClassicLoop(string name, float volume = 1f) {
			if (!BoomerangDatabase.AudioClipDatabaseEntries.ContainsKey(name)) {
				LogError(AudioManagerErrorCode.NotInCatalog);
				return null;
			}

			AudioSource selectedAudioSource = null;

			foreach (AudioSource audioSource in SfxChannels) {
				if (audioSource.isPlaying) {
					continue;
				}

				audioSource.clip = BoomerangDatabase.AudioClipDatabaseEntries[name];
				audioSource.loop = true;
				audioSource.volume = volume;
				audioSource.Play();
				selectedAudioSource = audioSource;
				break;
			}

			return selectedAudioSource;
		}

		public static void StopClassicLoop(AudioSource audioSource) {
			audioSource.loop = false;
			audioSource.Stop();
		}

		private enum AudioManagerErrorCode {
			NotInCatalog
		}

		private static void LogError(AudioManagerErrorCode code) {
			if (code == AudioManagerErrorCode.NotInCatalog) {
				throw new Exception(
					"Audio does not exist in the Catalog. Use Boomerang.AudioManager.AddAudio() to register an Audio File in the Catalog"
				);
			}
		}
	}
}