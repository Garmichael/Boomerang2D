using System.Collections.Generic;
using UnityEngine;

namespace Boomerang2DFramework.Framework.AudioManagement {
	public class LoopingAudioEffect {
		public AudioSource AudioSource;
		public List<string> Pool;
		public bool IsRandom;
		public float Volume;
		public bool ImmediatelyDie;
		public int CurrentPlayingItemIndex;
		public bool ReadyToDie;
	}
}