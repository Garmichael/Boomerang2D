using System;
using System.Collections.Generic;

namespace Boomerang2DFramework.Framework.GameFlagManagement {
	[Serializable]
	public class FlagList {
		public List<FloatFlag> FloatFlags;
		public List<StringFlag> StringFlags;
		public List<BoolFlag> BoolFlags;

		public Dictionary<string, float> HashFloatFlags() {
			Dictionary<string, float> flagsLookup = new Dictionary<string, float>();

			foreach (FloatFlag flag in FloatFlags) {
				if (flagsLookup.ContainsKey(flag.Key)) {
					flagsLookup[flag.Key] = flag.Value;
				} else {
					flagsLookup.Add(flag.Key, flag.Value);
				}
			}

			return flagsLookup;
		}

		public Dictionary<string, bool> HashBoolFlags() {
			Dictionary<string, bool> flagsLookup = new Dictionary<string, bool>();

			foreach (BoolFlag flag in BoolFlags) {
				if (flagsLookup.ContainsKey(flag.Key)) {
					flagsLookup[flag.Key] = flag.Value;
				} else {
					flagsLookup.Add(flag.Key, flag.Value);
				}
			}

			return flagsLookup;
		}

		public Dictionary<string, string> HashStringFlags() {
			Dictionary<string, string> flagsLookup = new Dictionary<string, string>();

			foreach (StringFlag flag in StringFlags) {
				if (flagsLookup.ContainsKey(flag.Key)) {
					flagsLookup[flag.Key] = flag.Value;
				} else {
					flagsLookup.Add(flag.Key, flag.Value);
				}
			}

			return flagsLookup;
		}
	}
}