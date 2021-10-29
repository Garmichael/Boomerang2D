using System.Collections.Generic;
using System.IO;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework.GameFlagManagement {
	public static class GameFlags {
		private static FlagList _flagList;
		private static readonly Dictionary<string, float> FloatFlags;
		private static readonly Dictionary<string, bool> BoolFlags;
		private static readonly Dictionary<string, string> StringFlags;

		static GameFlags() {
			FlagList flagList = JsonUtility.FromJson<FlagList>(BoomerangDatabase.GameFlagsStore.text);

			FloatFlags = flagList.HashFloatFlags();
			BoolFlags = flagList.HashBoolFlags();
			StringFlags = flagList.HashStringFlags();
		}

		public static void SaveGameFlags() {
			List<FloatFlag> floatFlags = new List<FloatFlag>();
			List<BoolFlag> boolFlags = new List<BoolFlag>();
			List<StringFlag> stringFlags = new List<StringFlag>();

			foreach (KeyValuePair<string, float> flag in FloatFlags) {
				floatFlags.Add(new FloatFlag {
					Key = flag.Key,
					Value = flag.Value
				});
			}

			foreach (KeyValuePair<string, bool> flag in BoolFlags) {
				boolFlags.Add(new BoolFlag {
					Key = flag.Key,
					Value = flag.Value
				});
			}

			foreach (KeyValuePair<string, string> flag in StringFlags) {
				stringFlags.Add(new StringFlag {
					Key = flag.Key,
					Value = flag.Value
				});
			}

			FlagList newFlagList = new FlagList {
				BoolFlags = boolFlags,
				FloatFlags = floatFlags,
				StringFlags = stringFlags
			};

			File.WriteAllText(GameProperties.GameFlagContentFile, JsonUtility.ToJson(newFlagList, true));
		}

		public static void SetFloatFlag(string key, float value) {
			if (FloatFlags.ContainsKey(key)) {
				FloatFlags[key] = value;
			} else {
				FloatFlags.Add(key, value);
			}
		}

		public static float GetFloatFlag(string key) {
			return FloatFlags.ContainsKey(key)
				? FloatFlags[key]
				: 0;
		}

		public static void SetBoolFlag(string key, bool value) {
			if (BoolFlags.ContainsKey(key)) {
				BoolFlags[key] = value;
			} else {
				BoolFlags.Add(key, value);
			}
		}

		public static bool GetBoolFlag(string key) {
			return BoolFlags.ContainsKey(key) && BoolFlags[key];
		}

		public static void SetStringFlag(string key, string value) {
			if (StringFlags.ContainsKey(key)) {
				StringFlags[key] = value;
			} else {
				StringFlags.Add(key, value);
			}
		}

		public static string GetStringFlag(string key) {
			return StringFlags.ContainsKey(key)
				? StringFlags[key]
				: "";
		}
	}
}