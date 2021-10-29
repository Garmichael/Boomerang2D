using System;
using System.Collections.Generic;
using Boomerang2DFramework.Framework.BoomerangDatabaseManagement;
using UnityEngine;

namespace Boomerang2DFramework.Framework {
	/// <summary>
	/// A Dictionary with a String Key with String Values that can be serialized into JSON
	/// </summary>
	[Serializable]
	public class SerializableStringDictionary {
		public List<string> Keys = new List<string>();
		public List<string> Values = new List<string>();

		public Dictionary<string, Texture> Dictionary {
			get {
				Dictionary<string, Texture> dictionary = new Dictionary<string, Texture>();

				for (int index = 0; index < Keys.Count; index++) {
					if (!dictionary.ContainsKey(Keys[index]) &&
					    BoomerangDatabase.TextureDatabaseEntries.ContainsKey(Values[index])
					) {
						dictionary.Add(Keys[index], BoomerangDatabase.TextureDatabaseEntries[Values[index]]);
					}
				}

				return dictionary;
			}
		}
	}

	/// <summary>
	/// A Dictionary with a String Key with Float Values that can be serialized into JSON
	/// </summary>
	[Serializable]
	public class SerializableFloatDictionary {
		public List<string> Keys = new List<string>();
		public List<float> Values = new List<float>();

		public Dictionary<string, float> Dictionary {
			get {
				Dictionary<string, float> dictionary = new Dictionary<string, float>();

				for (int index = 0; index < Keys.Count; index++) {
					if (!dictionary.ContainsKey(Keys[index])) {
						dictionary.Add(Keys[index], Values[index]);
					}
				}

				return dictionary;
			}
		}
	}

	/// <summary>
	/// A Dictionary with a String Key with Bool Values that can be serialized into JSON
	/// </summary>
	[Serializable]
	public class SerializableBoolDictionary {
		public List<string> Keys = new List<string>();
		public List<bool> Values = new List<bool>();

		public Dictionary<string, bool> Dictionary {
			get {
				Dictionary<string, bool> dictionary = new Dictionary<string, bool>();

				for (int index = 0; index < Keys.Count; index++) {
					if (!dictionary.ContainsKey(Keys[index])) {
						dictionary.Add(Keys[index], Values[index]);
					}
				}

				return dictionary;
			}
		}
	}

	/// <summary>
	/// A Dictionary with a String Key with Int Values that can be serialized into JSON
	/// </summary>
	[Serializable]
	public class SerializableIntDictionary {
		public List<string> Keys = new List<string>();
		public List<int> Values = new List<int>();

		public Dictionary<string, int> Dictionary {
			get {
				Dictionary<string, int> dictionary = new Dictionary<string, int>();

				for (int index = 0; index < Keys.Count; index++) {
					if (!dictionary.ContainsKey(Keys[index])) {
						dictionary.Add(Keys[index], Values[index]);
					}
				}

				return dictionary;
			}
		}
	}

	/// <summary>
	/// A Dictionary with a String Key with Color Values that can be serialized into JSON
	/// </summary>
	[Serializable]
	public class SerializableColorDictionary {
		public List<string> Keys = new List<string>();
		public List<Color> Values = new List<Color>();

		public Dictionary<string, Color> Dictionary {
			get {
				Dictionary<string, Color> dictionary = new Dictionary<string, Color>();

				for (int index = 0; index < Keys.Count; index++) {
					if (!dictionary.ContainsKey(Keys[index])) {
						dictionary.Add(Keys[index], Values[index]);
					}
				}

				return dictionary;
			}
		}
	}

	public enum InputType {
		None,
		Axis,
		Button,
		Key
	}

	public enum ValueComparison {
		LessThan,
		LessThanOrEqual,
		Equal,
		GreaterThanOrEqual,
		GreaterThan
	}

	public enum MathOperation {
		Plus,
		Minus,
		Divide,
		Multiply,
		Square,
		Root,
		Modulo
	}
	
	public enum RenderDisplayMode {
		PixelPerfect,
		Fit,
		Stretch
	}
}